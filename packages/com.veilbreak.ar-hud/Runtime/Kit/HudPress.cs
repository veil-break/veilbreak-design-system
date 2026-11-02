using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // Pointer feedback for every Button on a panel. While hovered (or keyboard-focused once HudNavigation's `kbd` class is on),
    // a soft glow traces the button's own outline (its HudShape chamfer when it has one) and fades in over 0.22 s; pressing
    // sinks the button slightly and brightens the glow. The button never leans or moves its content. HudNavigation hooks it
    // once on the panel's visual tree, so screen rebuilds never drop it; the glow paints on one overlay above the document,
    // inside the lit button's top-level element. Reduced motion (HudFx.Reduced) keeps the glow without the sink. A button picks
    // its colour with the `--neon` custom property (holo cyan otherwise).
    public sealed class HudPress : IDisposable
    {
        private static readonly CustomStyleProperty<Color> NeonProperty = new CustomStyleProperty<Color>("--neon");
        private static readonly Color DefaultNeon = new Color(.35f, .88f, 1f);
        // Outer glow: a few thin rings on an exponential falloff, so the halo fades smoothly instead of in steps.
        private const int Rings = 4;

        private sealed class Lit { public Button button; public float level, press; }

        private readonly List<Lit> lit = new List<Lit>();
        private readonly VisualElement tree, layer;
        private readonly IVisualElementScheduledItem tick;
        private Button hovered;
        private bool down;

        public HudPress(VisualElement tree)
        {
            this.tree = tree;
            layer = new VisualElement { name = "button-fx", pickingMode = PickingMode.Ignore };
            layer.style.position = Position.Absolute; layer.style.left = 0; layer.style.top = 0; layer.style.right = 0; layer.style.bottom = 0;
            layer.generateVisualContent += Draw;
            tree.Add(layer);
            tree.RegisterCallback<PointerMoveEvent>(OnMove, TrickleDown.TrickleDown);
            tree.RegisterCallback<PointerDownEvent>(OnDown, TrickleDown.TrickleDown);
            tree.RegisterCallback<PointerUpEvent>(OnUp, TrickleDown.TrickleDown);
            tree.RegisterCallback<PointerCancelEvent>(OnCancel, TrickleDown.TrickleDown);
            tree.RegisterCallback<PointerLeaveEvent>(OnLeave);
            tick = tree.schedule.Execute(Tick).Every(0);
        }

        public void Dispose()
        {
            tick.Pause();
            tree.UnregisterCallback<PointerMoveEvent>(OnMove, TrickleDown.TrickleDown);
            tree.UnregisterCallback<PointerDownEvent>(OnDown, TrickleDown.TrickleDown);
            tree.UnregisterCallback<PointerUpEvent>(OnUp, TrickleDown.TrickleDown);
            tree.UnregisterCallback<PointerCancelEvent>(OnCancel, TrickleDown.TrickleDown);
            tree.UnregisterCallback<PointerLeaveEvent>(OnLeave);
            foreach (var l in lit) Release(l.button);
            lit.Clear(); layer.RemoveFromHierarchy();
        }

        private void OnMove(PointerMoveEvent e) => Track(e.target as VisualElement);
        private void OnDown(PointerDownEvent e) { Track(e.target as VisualElement); down = true; }
        private void OnUp(PointerUpEvent e) { down = false; if (e.pointerType != UnityEngine.UIElements.PointerType.mouse) hovered = null; }
        private void OnCancel(PointerCancelEvent e) { down = false; hovered = null; }
        private void OnLeave(PointerLeaveEvent e) { if (e.target == tree) { hovered = null; down = false; } }

        private static Button Owner(VisualElement e)
        {
            for (; e != null; e = e.parent) if (e is Button b) return b.enabledInHierarchy ? b : null;
            return null;
        }

        private Lit Find(Button b) { foreach (var l in lit) if (l.button == b) return l; return null; }

        private void Track(VisualElement target)
        {
            hovered = Owner(target);
            if (hovered != null && Find(hovered) == null) lit.Add(new Lit { button = hovered });
        }

        private void Tick()
        {
            // Keyboard and gamepad focus lights its button too (centred), once the focus frames are on.
            var target = hovered;
            if (target == null && tree.panel?.focusController?.focusedElement is Button focused && Keyboard(focused))
            { target = focused; if (Find(focused) == null) lit.Add(new Lit { button = focused }); }
            if (lit.Count == 0) return;
            var host = Host(target ?? lit[lit.Count - 1].button);
            if (host != null && layer.parent != host) host.Add(layer); else if (layer.parent != null && layer.parent.IndexOf(layer) != layer.parent.childCount - 1) layer.BringToFront();
            var dt = Time.unscaledDeltaTime; var still = HudFx.Reduced;
            for (var i = lit.Count - 1; i >= 0; i--)
            {
                var l = lit[i]; var b = l.button;
                var on = b == target && b.panel != null && b.enabledInHierarchy;
                l.level = Mathf.MoveTowards(l.level, on ? 1 : 0, dt / (on ? .22f : .3f));
                l.press = Mathf.MoveTowards(l.press, on && down ? 1 : 0, dt / .1f);
                if (b.panel == null || l.level <= 0) { Release(b); lit.RemoveAt(i); continue; }
                if (still) continue;
                b.style.scale = l.press > 0 ? new Scale(Vector3.one * (1 - .02f * l.press)) : StyleKeyword.Null;
            }
            layer.MarkDirtyRepaint();
        }

        // The button's top-level element under the panel's visual tree: the one HudVisor filters, so the glow bends with it.
        private VisualElement Host(VisualElement e)
        {
            if (e == null || e.panel == null) return null;
            while (e.parent != null && e.parent != tree) e = e.parent;
            return e.parent == tree ? e : null;
        }

        private static bool Keyboard(VisualElement e)
        {
            for (; e != null; e = e.parent) if (e.ClassListContains(HudNavigation.KeyboardClass)) return true;
            return false;
        }

        private static void Release(Button b) => b.style.scale = StyleKeyword.Null;

        private void Draw(MeshGenerationContext context)
        {
            var p = context.painter2D;
            foreach (var l in lit)
            {
                var b = l.button; if (b.panel != layer.panel || l.level <= .01f) continue;
                // Fade with the button: a hidden or glitching board takes its glow with it.
                var seen = 1f; for (VisualElement e = b; e != null; e = e.parent) seen *= e.resolvedStyle.opacity * (e.resolvedStyle.visibility == Visibility.Hidden ? 0 : 1);
                if (seen < .02f) continue;
                float w = b.layout.width, h = b.layout.height; if (w <= 0 || h <= 0) continue;
                var style = b.customStyle;
                var neon = style.TryGetValue(NeonProperty, out var c) ? c : DefaultNeon;
                var k = b.worldBound.width / w; // board px to panel px
                var flare = 1 + l.press * .6f; var level = l.level * seen;
                Vector2 Map(Vector2 local) => layer.WorldToLocal(b.LocalToWorld(local));
                void Path(float grow)
                {
                    var pts = HudShape.Outline(b, grow);
                    p.BeginPath(); p.MoveTo(Map(pts[0])); for (var i = 1; i < pts.Length; i++) p.LineTo(Map(pts[i])); p.ClosePath();
                }
                p.lineJoin = LineJoin.Miter; p.lineCap = LineCap.Butt;
                for (var r = Rings - 1; r >= 0; r--)
                {
                    Path(1 + r * 1.2f); p.lineWidth = 1.4f * k;
                    p.strokeColor = new Color(neon.r, neon.g, neon.b, Mathf.Min(1, .12f * Mathf.Exp(-r * .5f) * level * flare)); p.Stroke();
                }
                Path(.5f); p.lineWidth = 1 * k; p.strokeColor = new Color(neon.r, neon.g, neon.b, .55f * level * flare); p.Stroke();
            }
        }
    }
}
