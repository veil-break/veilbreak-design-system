using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // The screen chrome every HudStage carries: a HudChrome (faint rails, side brackets, optional scanlines and sweep) laid out
    // in board units under the board, so panels cover it. Its theme and effects come from the HudView that hosts the stage
    // (HudView.Apply), or HudTheme.Default / HudEffects.Default outside a host (UI Builder, edit-mode previews); the shared
    // assets are never written. Its edge interference follows Glitch and rests with HudFx.Reduced (or the profile's own
    // reduced motion). Only the newest attached chrome shows, so an overlay stage (a result sheet over a HUD) does not stack a
    // second one. While a Lens is installed and active, the eye focus (0..1 from the top-left) eases after the pointer, drives
    // the chrome's parallax and the lens filter goes on the panel's root (the top of this panel's tree), rebuilt only when the
    // focus has moved or the quality level changed; without one the eye rests at the centre.
    public sealed class HudVisor : VisualElement
    {
        private static readonly List<HudVisor> Live = new List<HudVisor>();

        // The chrome's edge interference (0..0.25, like HudEffects.glitch); null keeps the effects profile's own value.
        public static Func<float> Glitch { get; set; }
        // A full-panel filter for the eye focus (0..1 from the top-left); null, or a null result, leaves the panel unfiltered.
        public static Func<Vector2, List<FilterFunction>> Lens { get; set; }
        // Whether the installed Lens applies now (a quality tier, say); null means whenever one is installed.
        public static Func<bool> LensActive { get; set; }
        private static bool Lensing => Lens != null && (LensActive == null || LensActive());
        // Editor captures pin where the eye rests (0..1 from the top-left); null follows the pointer.
        public static Vector2? PinnedFocus { get; set; }

        private readonly HudStage stage;
        private readonly HudChrome chrome = new HudChrome();
        private HudTheme theme;
        private HudEffects effects;
        private bool reduced; private float glitch = -1;
        // Where the eye rests (0..1 from the top-left): the pointer, eased; the centre until a pointer moves.
        private static Vector2 focus = new Vector2(.5f, .5f), applied = new Vector2(-1, -1);
#if !AR_HUD_INPUT_SYSTEM
        private static Vector2 target = new Vector2(.5f, .5f);
#endif
        private IVisualElementScheduledItem tracking;
#if !AR_HUD_INPUT_SYSTEM
        private VisualElement tree;
#endif

        public HudChrome Chrome => chrome;

        public HudVisor(HudStage stage)
        {
            this.stage = stage;
            AddToClassList("visor"); pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute; style.left = style.top = style.right = style.bottom = 0;
            chrome.style.position = Position.Absolute; chrome.style.left = chrome.style.top = 0;
            // Under the board, so panels and readouts cover its lines.
            chrome.style.transformOrigin = new TransformOrigin(0, 0); stage.hierarchy.Insert(0, chrome);
            RegisterCallback<GeometryChangedEvent>(_ => Fit());
            RegisterCallback<AttachToPanelEvent>(e =>
            {
                Live.Remove(this); Live.Add(this); Refresh(); QualitySettings.activeQualityLevelChanged += OnQuality;
                var view = HudView.Of(this); Configure(view?.Theme ? view.Theme : HudTheme.Default, view?.Effects ? view.Effects : HudEffects.Default);
#if !AR_HUD_INPUT_SYSTEM
                tree = e.destinationPanel.visualTree; tree.RegisterCallback<PointerMoveEvent>(OnPointer, TrickleDown.TrickleDown);
#endif
                tracking = schedule.Execute(Track).Every(16);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                tracking?.Pause(); Live.Remove(this); Refresh(); QualitySettings.activeQualityLevelChanged -= OnQuality;
#if !AR_HUD_INPUT_SYSTEM
                tree?.UnregisterCallback<PointerMoveEvent>(OnPointer, TrickleDown.TrickleDown); tree = null;
#endif
            });
        }

        // The palette and profile the chrome draws with (HudView.Apply calls this for the visors it hosts).
        public void Configure(HudTheme palette, HudEffects profile)
        {
            theme = palette; effects = profile; glitch = -1; chrome.Configure(theme, effects); TrackChrome();
        }

        // The chrome draws in board units, like the screens under it.
        private void Fit()
        {
            var k = stage.Scale > 0 ? stage.Scale : 1;
            chrome.style.width = layout.width / k; chrome.style.height = layout.height / k;
            chrome.style.scale = new StyleScale(new Scale(new Vector3(k, k, 1)));
        }

        private void OnQuality(int from, int to) { applied = new Vector2(-1, -1); ApplyFilter(); }

#if !AR_HUD_INPUT_SYSTEM
        // Without the Input System the eye follows the panel's own pointer events.
        private void OnPointer(PointerMoveEvent e)
        {
            var size = tree?.layout.size ?? Vector2.zero; if (!(size.x > 0 && size.y > 0)) return;
            target = new Vector2(Mathf.Clamp01(e.position.x / size.x), Mathf.Clamp01(e.position.y / size.y));
        }
#endif

        // The newest visor shows; the rest hide until it detaches.
        private static void Refresh()
        {
            for (var i = 0; i < Live.Count; i++) Live[i].visible = Live[i].chrome.visible = i == Live.Count - 1;
            applied = new Vector2(-1, -1);
            if (Live.Count > 0) Live[Live.Count - 1].ApplyFilter();
        }

        // The eye follows the pointer (eased): the chrome's parallax, and the lens filter, rebuilt only when the focus has moved.
        private void Track()
        {
            if (Live.Count == 0 || Live[Live.Count - 1] != this) return;
            TrackChrome();
            if (!Lensing) return;
            if (PinnedFocus.HasValue) focus = PinnedFocus.Value;
            else
            {
#if AR_HUD_INPUT_SYSTEM
                // The screen pointer, not the panel's (a lens bends the panel's pointer positions).
                var pointer = UnityEngine.InputSystem.Pointer.current;
                if (pointer == null || Screen.width <= 0 || Screen.height <= 0) return;
                var at = pointer.position.ReadValue(); var target = new Vector2(at.x / Screen.width, 1 - at.y / Screen.height);
#endif
                focus = Vector2.Lerp(focus, target, Mathf.Clamp01(Time.unscaledDeltaTime * 10));
            }
            if ((focus - applied).sqrMagnitude > .003f * .003f) ApplyFilter();
        }

        private void TrackChrome()
        {
            if (effects == null) return;
            var still = HudFx.Reduced || effects.reducedMotion;
            var amount = still ? 0 : Glitch != null ? Glitch() : effects.glitch;
            if (reduced != still || Mathf.Abs(glitch - amount) > .002f || amount == 0 && glitch != 0)
            {
                reduced = still; glitch = amount; chrome.Override(reduced, glitch);
            }
            chrome.SetPointer(new Vector2(focus.x * 2 - 1, focus.y * 2 - 1));
        }

        // The panel's root: the top of this panel's tree under the visual tree, so overlays outside the stage bend too.
        private VisualElement Root()
        {
            if (panel == null) return null;
            VisualElement top = stage;
            while (top.parent != null && top.parent != panel.visualTree) top = top.parent;
            return top;
        }

        private void ApplyFilter()
        {
            var root = Root(); if (root == null) return;
            var filter = Lensing ? Lens(focus) : null;
            root.style.filter = filter != null ? new StyleList<FilterFunction>(filter) : StyleKeyword.Null;
            applied = focus;
        }
    }
}
