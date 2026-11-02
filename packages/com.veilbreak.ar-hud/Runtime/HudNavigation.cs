using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>A direction on the pad: arrows, WASD and the d-pad, in the layout's logical sense (Left/Right swap when <see cref="HudNavigation.Mirrored"/>).</summary>
    public enum HudPad { Left, Up, Right, Down }

    /// <summary>How a modal opened with <see cref="HudNavigation.OpenModal"/> behaves.</summary>
    [Flags]
    public enum HudModal
    {
        None = 0,
        /// <summary>The modal's layer carries the <see cref="HudNavigation.Modal"/> scrim class: it dims what is under it (Base.uss) and focus stays on it.</summary>
        Dim = 1,
        /// <summary>Escape / Cancel (or its back action) and showing another screen close it.</summary>
        Dismissible = 2,
        /// <summary>Focus moves into it when it opens (its autofocus element, else its first focusable).</summary>
        Focus = 4,
        /// <summary>The body handles its own keys and pointer (an intro, a loading cover): navigation stands by while it is open.</summary>
        OwnInput = 8,
        /// <summary>The modal's layer takes the pointer over the whole panel, so nothing under it can be pressed (without it, only the body picks).</summary>
        Block = 16,
        /// <summary>A dialog: dimmed, dismissible, focused and blocking.</summary>
        Dialog = Dim | Dismissible | Focus | Block,
    }

    /// <summary>
    /// Screens, modals, toasts and keyboard / gamepad focus for one <see cref="HudView"/>. The input callbacks live once on the
    /// panel's visual tree (events with nothing focused target it, and a screen rebuild never touches it): arrows, WASD and the
    /// d-pad move focus (geometric, or the screen's own <see cref="Route"/>), Tab and Shift+Tab cycle, Space presses (Enter and
    /// gamepad Submit already click a Button), Escape and Cancel close a dismissible modal or run the screen's <see cref="Back"/>
    /// action. With the Input System installed (AR_HUD_INPUT_SYSTEM), Space and <see cref="Hotkeys"/> are polled from it: as the
    /// only input backend its UI provider sends navigation, submit, cancel and pointers but never KeyDownEvents, so a key handler
    /// alone would only work in the editor. Built screens (<see cref="Register(string, Action{VisualElement})"/>) are rebuilt on
    /// every <see cref="Show"/>, so the focused element is remembered by name (or position) and refocused afterwards, falling back
    /// to the <see cref="AutoFocus"/> element, then the first focusable. While a <see cref="Modal"/> scrim is up (a modal opened with
    /// <see cref="HudModal.Dim"/>, or an element of a screen carrying the class) focus stays on what comes after it. The
    /// <see cref="KeyboardClass"/> class on the view (and on every element carrying style sheets) turns focus frames on only after
    /// keyboard or gamepad input, so mouse and touch never show them. <see cref="HudPress"/> (every button's hover glow) runs
    /// while the navigation is <see cref="Enabled"/>.
    /// </summary>
    public sealed class HudNavigation : IDisposable
    {
        public const string AutoFocus = "autofocus", Modal = "modal-scrim", KeyboardClass = "kbd", Skip = "no-nav";
        /// <summary>The element a screen shares with the next one (a nav bar): a change of screen animates only it (<see cref="HudMotion"/>).</summary>
        public const string Shared = "ar-shared";
        /// <summary>The class of every modal's layer (filling the view, above the content).</summary>
        public const string ModalClass = "ar-modal";
        // A layer focus stays inside (every modal but one with its own input).
        private const string ScopeClass = "ar-modal--scope";

        private sealed class Entry { public VisualElement element; public Action<VisualElement> build; }
        private sealed class Open { public VisualElement layer, body; public HudModal flags; public Action back; public VisualElement previous; }

        private readonly VisualElement root;
        private readonly VisualElement content;
        private readonly Dictionary<string, Entry> screens = new Dictionary<string, Entry>();
        private readonly List<Open> modals = new List<Open>();
        private readonly Dictionary<string, string> remembered = new Dictionary<string, string>();
        private Label toast;
        private IVisualElementScheduledItem toastTimer, poll;
        private VisualElement tree, last, moved;
        private string pending;
        private bool keyboard, disposed, enabled = true;
        private int cancelFrame = -1, tabFrame = -1;
        private Vector2 pointer;
        private HudPress press;

        /// <summary>The screen shown last (null before the first <see cref="Show"/>).</summary>
        public string Current { get; private set; }
        public bool HasModal => modals.Count > 0;
        public event Action<string> ScreenChanged;

        /// <summary>The current screen's back action for Escape / Cancel; null, or a null result, when there is none.</summary>
        public Func<Action> Back;
        /// <summary>A screen's own neighbours in logical directions; null (or a null result) falls back to geometry.</summary>
        public Func<VisualElement, HudPad, VisualElement> Route;
#if AR_HUD_INPUT_SYSTEM
        /// <summary>Keys that jump straight to a named element: a button is pressed, anything else takes focus.</summary>
        public Dictionary<UnityEngine.InputSystem.Key, string> Hotkeys;
#endif
        /// <summary>Left-handed layout: Left and Right swap before <see cref="Route"/> sees them.</summary>
        public bool Mirrored;
        /// <summary>Sound cues: "hover" when focus moves, "back" when Escape acts.</summary>
        public Action<string> Cue;
        /// <summary>The hub screen: coming back to it refocuses what was focused when it was left.</summary>
        public string Hub;
        /// <summary>Animate screen changes with <see cref="HudMotion"/>: an in-place rebuild diffs the whole content, a new screen its <see cref="Shared"/> element.</summary>
        public bool Animate;
        public bool ReducedMotion;

        public HudNavigation(VisualElement root, VisualElement content)
        {
            this.root = root; this.content = content;
            if (root.panel != null) Hook(root.panel);
            root.RegisterCallback<AttachToPanelEvent>(OnAttach);
            root.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        /// <summary>Off, the navigation ignores input, <see cref="Show"/> leaves focus alone and <see cref="HudPress"/> stands down (a battle HUD
        /// whose own keys rule until a sheet opens).</summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                if (!enabled) { press?.Dispose(); press = null; }
                else if (tree != null && press == null) press = new HudPress(tree);
            }
        }

        public bool KeyboardActive => keyboard;
        /// <summary>The focused target (a text input inside a field maps onto the field), or null.</summary>
        public VisualElement Focused => Resolve(Targets());

        // ---------- screens ----------
        /// <summary>A persistent screen: added to the content hidden, shown by <see cref="Show"/>.</summary>
        public void Register(string id, VisualElement screen)
        {
            Add(id, new Entry { element = screen });
            content.Add(screen); screen.style.display = DisplayStyle.None;
        }

        /// <summary>A built screen: every <see cref="Show"/> clears the content and builds it again into it.</summary>
        public void Register(string id, Action<VisualElement> build)
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            Add(id, new Entry { build = build });
        }

        public bool Has(string id) => id != null && screens.ContainsKey(id);

        private void Add(string id, Entry screen)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Screen id is required.", nameof(id));
            if (screens.ContainsKey(id)) throw new ArgumentException("Screen id already exists: " + id);
            screens.Add(id, screen);
        }

        /// <summary>Shows a screen (again): dismissible modals close, a built screen rebuilds, focus lands on the same element for an
        /// in-place rebuild, the remembered one on the hub, else the screen's <see cref="AutoFocus"/> element.</summary>
        public void Show(string id)
        {
            if (id == null || !screens.TryGetValue(id, out var next)) throw new ArgumentException("Unknown screen: " + id);
            var previous = Current; if (enabled) Leave(previous);
            for (var i = modals.Count - 1; i >= 0; i--) if ((modals[i].flags & HudModal.Dismissible) != 0) Close(modals[i], false);
            var same = previous == id;
            var before = Animate ? HudMotion.Capture(same ? content : content.Q(className: Shared)) : null;
            Current = id;
            if (next.build != null)
            {
                // Persistent screens stay (hidden); everything else in the content is the previous build.
                var kept = new HashSet<VisualElement>(screens.Values.Where(s => s.element != null).Select(s => s.element));
                foreach (var screen in kept) screen.style.display = DisplayStyle.None;
                foreach (var child in content.Children().ToList()) if (!kept.Contains(child)) child.RemoveFromHierarchy();
                next.build(content);
            }
            else
            {
                foreach (var screen in screens.Values) if (screen.element != null) screen.element.style.display = screen == next ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (before != null) HudMotion.Play(same ? content : content.Q(className: Shared), before, ReducedMotion);
            if (enabled) Arrive(id, previous);
            ScreenChanged?.Invoke(id);
        }

        // ---------- modals ----------
        /// <summary>Opens <paramref name="body"/> on a layer above the content (and below toasts). The layer carries the content's style
        /// sheets, so a body styled like the screens keeps their look and sheets of its own still win. It closes with
        /// <see cref="CloseModal"/>, when it is dismissed (<see cref="HudModal.Dismissible"/>: Escape runs <paramref name="back"/> if given)
        /// or when the body leaves the panel; focus then returns to what held it before.</summary>
        public VisualElement OpenModal(VisualElement body, HudModal flags = HudModal.Dialog, Action back = null)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            var layer = new VisualElement { name = "ar-modal", pickingMode = (flags & HudModal.Block) != 0 ? PickingMode.Position : PickingMode.Ignore };
            layer.AddToClassList(ModalClass);
            if ((flags & HudModal.OwnInput) == 0) layer.AddToClassList(ScopeClass);
            if ((flags & HudModal.Dim) != 0) layer.AddToClassList(Modal);
            for (var i = 0; i < content.styleSheets.count; i++) layer.styleSheets.Add(content.styleSheets[i]);
            layer.Add(body);
            var open = new Open { layer = layer, body = body, flags = flags, back = back, previous = tree?.panel?.focusController?.focusedElement as VisualElement };
            if (toast != null && toast.parent == root) root.Insert(root.IndexOf(toast), layer); else root.Add(layer);
            modals.Add(open);
            body.RegisterCallback<DetachFromPanelEvent>(OnModalDetached);
            Mark();
            if ((flags & HudModal.Focus) != 0) body.schedule.Execute(() => { if (modals.Contains(open)) Place(null); });
            return body;
        }

        /// <summary>Closes the topmost modal.</summary>
        public void CloseModal() { if (modals.Count > 0) Close(modals[modals.Count - 1], true); }

        private void OnModalDetached(DetachFromPanelEvent e)
        {
            var open = modals.FirstOrDefault(m => m.body == e.currentTarget);
            if (open != null && !disposed) Close(open, true);
        }

        private void Close(Open open, bool refocus)
        {
            if (!modals.Remove(open)) return;
            open.body.UnregisterCallback<DetachFromPanelEvent>(OnModalDetached);
            open.layer.RemoveFromHierarchy();
            if (!refocus || disposed || tree == null) return;
            if (open.previous != null && Takes(open.previous)) Focus(open.previous);
        }

        // ---------- toasts ----------
        public void Toast(string message, int milliseconds = 2800)
        {
            toastTimer?.Pause(); toast?.RemoveFromHierarchy();
            toast = new Label(message) { pickingMode = PickingMode.Ignore };
            toast.AddToClassList("ar-toast"); root.Add(toast);
            toastTimer = toast.schedule.Execute(() => { toast?.RemoveFromHierarchy(); toast = null; }).StartingIn(Math.Max(200, milliseconds));
        }

        public void Dispose()
        {
            disposed = true; poll?.Pause(); press?.Dispose(); press = null;
            foreach (var open in modals.ToList()) Close(open, false);
            toastTimer?.Pause(); toast?.RemoveFromHierarchy(); screens.Clear();
            root.UnregisterCallback<AttachToPanelEvent>(OnAttach); root.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            Unhook();
        }

        // ---------- panel hooks ----------
        private void OnAttach(AttachToPanelEvent e) => Hook(e.destinationPanel);
        private void OnDetach(DetachFromPanelEvent e) => Unhook();

        private void Hook(IPanel panel)
        {
            if (disposed || tree != null || panel == null) return;
            tree = panel.visualTree;
            tree.RegisterCallback<NavigationMoveEvent>(OnMove, TrickleDown.TrickleDown);
            tree.RegisterCallback<NavigationCancelEvent>(OnCancel, TrickleDown.TrickleDown);
            tree.RegisterCallback<KeyDownEvent>(OnKey, TrickleDown.TrickleDown);
            tree.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            tree.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            tree.RegisterCallback<FocusInEvent>(OnFocusIn, TrickleDown.TrickleDown);
            poll = root.schedule.Execute(Poll).Every(0);
            if (enabled) press = new HudPress(tree); // every button's hover glow lives with the navigation hooks
        }

        private void Unhook()
        {
            poll?.Pause(); poll = null; press?.Dispose(); press = null;
            if (tree == null) return;
            tree.UnregisterCallback<NavigationMoveEvent>(OnMove, TrickleDown.TrickleDown);
            tree.UnregisterCallback<NavigationCancelEvent>(OnCancel, TrickleDown.TrickleDown);
            tree.UnregisterCallback<KeyDownEvent>(OnKey, TrickleDown.TrickleDown);
            tree.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            tree.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            tree.UnregisterCallback<FocusInEvent>(OnFocusIn, TrickleDown.TrickleDown);
            tree = null;
        }

        // A modal with its own input (an intro) holds the keys while it is on the panel.
        private bool Standby { get { foreach (var m in modals) if ((m.flags & HudModal.OwnInput) != 0 && m.body.panel != null) return true; return false; } }
        private bool Live => !disposed && enabled && tree != null && !Standby;

        // ---------- focus targets ----------
        private static bool Shown(VisualElement e)
        {
            if (e.resolvedStyle.visibility == Visibility.Hidden) return false;
            for (var p = e; p != null; p = p.parent)
            {
                var inline = p.style.display;
                if (inline.keyword == StyleKeyword.Undefined && inline.value == DisplayStyle.None) return false;
                if (p.resolvedStyle.display == DisplayStyle.None) return false;
            }
            return true;
        }

        private static VisualElement Above(VisualElement e, string css) { for (var p = e.parent; p != null; p = p.parent) if (p.ClassListContains(css)) return p; return null; }

        /// <summary>Whether keyboard focus can land on <paramref name="e"/>: focusable, shown, enabled, not a scroll view or its scroller, not <see cref="Skip"/>.</summary>
        public static bool Takes(VisualElement e) => e != null && e.panel != null && e.focusable && !(e is ScrollView) && !e.ClassListContains(Skip) && Above(e, "unity-scroller") == null && e.enabledInHierarchy && Shown(e);

        // Focusable things in tree order, limited to what follows the last modal scrim (or modal layer); a field's inner input yields to the field.
        private List<VisualElement> Targets()
        {
            var nodes = root.Query<VisualElement>().ToList();
            var begin = nodes.FindLastIndex(n => n.ClassListContains(Modal) || n.ClassListContains(ScopeClass)) + 1;
            var all = new List<VisualElement>();
            for (var i = begin; i < nodes.Count; i++) if (Takes(nodes[i])) all.Add(nodes[i]);
            var fields = new HashSet<VisualElement>(all.Where(e => e.ClassListContains("unity-base-field")));
            return all.Where(e => { for (var p = e.parent; p != null; p = p.parent) if (fields.Contains(p)) return false; return true; }).ToList();
        }

        // The focused element, mapped onto the target that owns it (a text input inside a field, say).
        private VisualElement Resolve(List<VisualElement> all)
        {
            var f = tree?.panel?.focusController?.focusedElement as VisualElement;
            for (var p = f; p != null; p = p.parent) if (all.Contains(p)) return p;
            return null;
        }

        /// <summary>The stable handle for an element: its own (or its field's) name; empty when it has none.</summary>
        public static string NameOf(VisualElement e)
        {
            if (!string.IsNullOrEmpty(e.name) && !e.name.StartsWith("unity-")) return e.name;
            var field = Above(e, "unity-base-field");
            return field != null && !string.IsNullOrEmpty(field.name) ? field.name : "";
        }
        private static string KeyOf(VisualElement e, List<VisualElement> all) { var name = NameOf(e); return name.Length > 0 ? name : "#" + all.IndexOf(e); }
        private static VisualElement Find(string key, List<VisualElement> all)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (key[0] == '#') return int.TryParse(key.Substring(1), out var i) && i >= 0 && i < all.Count ? all[i] : null;
            return all.FirstOrDefault(e => NameOf(e) == key);
        }

        private VisualElement Initial(List<VisualElement> all, bool preferLast)
        {
            if (preferLast && last != null && all.Contains(last)) return last;
            return all.LastOrDefault(e => e.ClassListContains(AutoFocus)) ?? all.FirstOrDefault();
        }

        // Focus moved by navigation input: if Unity's own focus switching also acted on the same event, the next frame puts it back.
        private void Step(VisualElement to)
        {
            moved = to; Focus(to); Cue?.Invoke("hover");
            root.schedule.Execute(() => { if (tree != null && moved == to && to.panel != null && Resolve(Targets()) != to) Focus(to); });
        }

        /// <summary>Focuses <paramref name="e"/> if it <see cref="Takes"/> focus, scrolling its list to it.</summary>
        public void Focus(VisualElement e)
        {
            if (!Takes(e)) return;
            e.Focus();
            var list = e.GetFirstAncestorOfType<ScrollView>(); if (list != null && list.contentContainer.Contains(e)) list.ScrollTo(e);
        }

        // ---------- screen rebuilds ----------
        // Before a screen is torn down: remember what holds focus.
        private void Leave(string screen)
        {
            pending = null; if (string.IsNullOrEmpty(screen) || tree == null) return;
            var all = Targets(); var current = Resolve(all);
            if (current == null) return;
            pending = KeyOf(current, all); remembered[screen] = pending;
        }

        // After the new screen is built: the same element again for an in-place rebuild, the one that was focused when coming back
        // to the hub, else the screen's autofocus. A frame later it tries once more, in case styles had not resolved yet.
        private void Arrive(string screen, string previous)
        {
            if (tree == null) return;
            Mark();
            var want = screen == previous ? pending : Hub != null && screen == Hub && remembered.TryGetValue(screen, out var r) ? r : null;
            Place(want);
            root.schedule.Execute(() => { if (tree != null && Resolve(Targets()) == null) Place(want); }).ExecuteLater(60);
        }

        private void Place(string key)
        {
            var all = Targets(); var pick = Find(key, all) ?? Initial(all, false);
            if (pick != null) Focus(pick);
        }

        // ---------- events ----------
        private void SetKeyboard(bool on)
        {
            if (keyboard == on) return;
            keyboard = on; Mark();
        }
        // A style sheet's rules apply to its own subtree, so the flag goes on every element carrying sheets as well as on the view.
        private void Mark()
        {
            root.EnableInClassList(KeyboardClass, keyboard);
            foreach (var e in root.Query<VisualElement>().Where(e => e.styleSheets.count > 0).ToList()) e.EnableInClassList(KeyboardClass, keyboard);
        }

        // Stops the event here and keeps Unity's own focus switching (arrows, Tab) from also acting on it.
        private void Swallow(EventBase e) { e.StopPropagation(); tree?.panel?.focusController?.IgnoreEvent(e); }
        private bool PopupOpen() => tree.Q(className: "unity-base-dropdown") != null;
        private static bool Typing(VisualElement e) { for (var p = e; p != null; p = p.parent) if (p.ClassListContains("unity-base-text-field")) return true; return false; }
        private static bool Slides(VisualElement e) => e.ClassListContains("unity-base-slider");

        private void OnPointerDown(PointerDownEvent e) { if (!disposed && enabled) SetKeyboard(false); }
        private void OnPointerMove(PointerMoveEvent e)
        {
            if (disposed || !enabled || e.pointerType != UnityEngine.UIElements.PointerType.mouse || (e.position - (Vector3)pointer).sqrMagnitude < 36) return;
            pointer = e.position; SetKeyboard(false);
        }
        private void OnFocusIn(FocusInEvent e) { if (e.target is VisualElement v && v.panel != null) last = v; }

        private void OnMove(NavigationMoveEvent e)
        {
            if (!Live || e.direction == NavigationMoveEvent.Direction.None) return;
            SetKeyboard(true);
            if (PopupOpen()) return;
            var all = Targets(); var cur = Resolve(all);
            if (e.direction == NavigationMoveEvent.Direction.Next || e.direction == NavigationMoveEvent.Direction.Previous) { Swallow(e); Cycle(all, cur, e.direction == NavigationMoveEvent.Direction.Next); return; }
            if (cur != null && Typing(cur)) return;
            var dir = e.direction == NavigationMoveEvent.Direction.Left ? HudPad.Left : e.direction == NavigationMoveEvent.Direction.Right ? HudPad.Right : e.direction == NavigationMoveEvent.Direction.Up ? HudPad.Up : HudPad.Down;
            if (cur != null && Slides(cur) && (dir == HudPad.Left || dir == HudPad.Right)) return; // the slider changes its value
            Swallow(e);
            if (cur == null) { var start = Initial(all, true); if (start != null) Focus(start); return; }
            var logical = Mirrored && dir == HudPad.Left ? HudPad.Right : Mirrored && dir == HudPad.Right ? HudPad.Left : dir;
            var to = Route?.Invoke(cur, logical);
            if (to != null && !all.Contains(to)) to = null;
            to = to ?? Seek(cur, dir, all);
            if (to == null || to == cur) return;
            Step(to);
        }

        // Tab order is tree order, wrapping around.
        private void Cycle(List<VisualElement> all, VisualElement cur, bool forward)
        {
            if (all.Count == 0 || tabFrame == Time.frameCount) return;
            tabFrame = Time.frameCount;
            var i = cur == null ? (forward ? 0 : all.Count - 1) : (all.IndexOf(cur) + (forward ? 1 : all.Count - 1)) % all.Count;
            Step(all[i]);
        }

        // The nearest target past the focused one in `dir`, Android FocusFinder style: a nearer facing edge beats a sideways drift,
        // and a target overlapping the focused one's beam (its row for left/right, its column for up/down) beats any off-beam one,
        // always for sideways moves and for up/down unless the off-beam target lies entirely closer.
        private static VisualElement Seek(VisualElement origin, HudPad dir, List<VisualElement> all)
        {
            var a = origin.worldBound; VisualElement inBeam = null, offBeam = null; float inScore = float.MaxValue, offScore = float.MaxValue, inMajor = 0, offFar = 0;
            if (float.IsNaN(a.width) || a.width <= 0) return null;
            foreach (var c in all)
            {
                if (c == origin) continue;
                var b = c.worldBound; if (float.IsNaN(b.width) || b.width <= 0 || b.height <= 0) continue;
                // Candidates lie wholly past the focused one's leading edge (Android's isCandidate), not merely with a centre beyond it.
                var dx = b.center.x - a.center.x; var dy = b.center.y - a.center.y; float major, far, minor; bool beam;
                switch (dir)
                {
                    case HudPad.Right: if (!((a.xMin < b.xMin || a.xMax <= b.xMin) && a.xMax < b.xMax)) continue; major = Mathf.Max(0, b.xMin - a.xMax); far = b.xMax - a.xMax; minor = Mathf.Abs(dy); beam = b.yMax > a.yMin && b.yMin < a.yMax; break;
                    case HudPad.Left: if (!((a.xMax > b.xMax || a.xMin >= b.xMax) && a.xMin > b.xMin)) continue; major = Mathf.Max(0, a.xMin - b.xMax); far = a.xMin - b.xMin; minor = Mathf.Abs(dy); beam = b.yMax > a.yMin && b.yMin < a.yMax; break;
                    case HudPad.Down: if (!((a.yMin < b.yMin || a.yMax <= b.yMin) && a.yMax < b.yMax)) continue; major = Mathf.Max(0, b.yMin - a.yMax); far = b.yMax - a.yMax; minor = Mathf.Abs(dx); beam = b.xMax > a.xMin && b.xMin < a.xMax; break;
                    default: if (!((a.yMax > b.yMax || a.yMin >= b.yMax) && a.yMin > b.yMin)) continue; major = Mathf.Max(0, a.yMin - b.yMax); far = a.yMin - b.yMin; minor = Mathf.Abs(dx); beam = b.xMax > a.xMin && b.xMin < a.xMax; break;
                }
                var score = 13 * major * major + minor * minor;
                if (beam) { if (score < inScore) { inScore = score; inBeam = c; inMajor = major; } }
                else if (score < offScore) { offScore = score; offBeam = c; offFar = Mathf.Max(1, far); }
            }
            if (inBeam == null || offBeam == null) return inBeam ?? offBeam;
            return dir == HudPad.Left || dir == HudPad.Right || inMajor < offFar ? inBeam : offBeam;
        }

        private void OnKey(KeyDownEvent e)
        {
            if (!Live) return;
            switch (e.keyCode)
            {
                case KeyCode.None: return;
                case KeyCode.LeftShift: case KeyCode.RightShift: case KeyCode.LeftControl: case KeyCode.RightControl: case KeyCode.LeftAlt: case KeyCode.RightAlt: case KeyCode.LeftCommand: case KeyCode.RightCommand: return;
                case KeyCode.Escape: Escape(e); return;
            }
            SetKeyboard(true);
            if (PopupOpen()) return;
            var all = Targets(); var cur = Resolve(all);
            if (e.keyCode == KeyCode.Tab) { Swallow(e); Cycle(all, cur, !e.shiftKey); return; }
            if (cur != null && Typing(cur)) return;
#if AR_HUD_INPUT_SYSTEM
            // Space is polled; this only keeps a toggle's own Space handling (editor IMGUI events) from flipping it a second time.
            if (e.keyCode == KeyCode.Space && cur != null && (cur is Button || cur is Toggle)) Swallow(e);
#else
            if (e.keyCode == KeyCode.Space && cur != null && (cur is Button || cur is Toggle)) { Swallow(e); Press(cur); }
#endif
        }

        private void Poll()
        {
#if AR_HUD_INPUT_SYSTEM
            var keys = UnityEngine.InputSystem.Keyboard.current;
            if (!Live || keys == null || !keys.anyKey.wasPressedThisFrame || PopupOpen()) return;
            var all = Targets(); var cur = Resolve(all);
            if (cur != null && Typing(cur)) return;
            if (keys.spaceKey.wasPressedThisFrame && cur != null && (cur is Button || cur is Toggle)) { SetKeyboard(true); Press(cur); return; }
            if (Hotkeys == null) return;
            foreach (var pair in Hotkeys)
            {
                if (!keys[pair.Key].wasPressedThisFrame) continue;
                var to = all.FirstOrDefault(t => NameOf(t) == pair.Value);
                if (to == null) continue;
                SetKeyboard(true); if (to is Button) Press(to); else if (to != cur) Step(to);
                return;
            }
#endif
        }

        // Space has no Submit binding in the default UI actions, so it sends the Submit that Enter and the gamepad produce.
        private static void Press(VisualElement e) { using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = e; e.SendEvent(submit); } }

        private void OnCancel(NavigationCancelEvent e) => Escape(e);

        // Escape arrives as a key and as a Cancel; whichever comes first in a frame acts, the other is swallowed. A dismissible modal
        // closes (or runs its back action) before the screen's own back action is asked.
        private void Escape(EventBase e)
        {
            if (!Live) return;
            SetKeyboard(true);
            if (PopupOpen()) return;
            if (cancelFrame == Time.frameCount) { Swallow(e); return; }
            cancelFrame = Time.frameCount;
            var all = Targets(); var cur = Resolve(all);
            if (cur != null && Typing(cur)) { Swallow(e); cur.Blur(); var start = Initial(all, false); if (start != null && start != cur) Focus(start); return; }
            var top = modals.Count > 0 ? modals[modals.Count - 1] : null;
            if (top != null && (top.flags & HudModal.Dismissible) != 0)
            {
                Swallow(e); Cue?.Invoke("back");
                if (top.back != null) top.back(); else Close(top, true);
                return;
            }
            var action = Back?.Invoke();
            if (action == null) return;
            Swallow(e); Cue?.Invoke("back"); action();
        }
    }
}
