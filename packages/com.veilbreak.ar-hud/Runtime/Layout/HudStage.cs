using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // A fixed-height logical artboard scaled to the screen, so screens are laid out in their mockups' own pixel units
    // (844 x 390 on phones, a 1440 x 900 board on desktops) and anchored to edges with the device safe area added. The stage
    // carries a `mobile` or `desktop` class, so USS can restyle a screen per board, and `.only-mobile` / `.only-desktop` blocks
    // (Base.uss) show on one board alone. Phones are wide (≥ 1.9:1) or, on a mobile platform, physically short screens; 16:9
    // and 16:10 monitors are desktop, including Retina ones (a 1080-point window at 304 dpi measures "3.5 inches" and would
    // otherwise pass for a phone).
    // In UXML its children go on the board: <ar:HudStage><ar:HudAnchor left="24" top="10">…</ar:HudAnchor></ar:HudStage>. The
    // HudVisor (the visor chrome behind the panels) sits under the board in the stage's own hierarchy.
    [UxmlElement]
    public partial class HudStage : VisualElement
    {
        public const float MobileHeight = 390, DesktopHeight = 900;
        public readonly VisualElement Board = new VisualElement();
        public readonly HudVisor Visor;
        private bool mirror;
        // Left-handed layout: every anchor's left and right offsets swap.
        [UxmlAttribute] public bool Mirror { get => mirror; set { mirror = value; Fit(); } }
        public float Scale { get; private set; } = 1;
        public bool Mobile { get => mobile; private set => MobileBoard = mobile = value; }
        // Until its first layout a stage assumes the last laid-out board, so a screen rebuilt in place starts on the right one (code that
        // branches on Mobile while building, and the .mobile/.desktop classes) instead of drawing the phone board for a frame.
        private bool mobile = MobileBoard;
        // The last laid-out board, for scene code that frames around the UI without a reference to the stage.
        public static bool MobileBoard { get; private set; } = true;
        public event Action Resized;
        public override VisualElement contentContainer => Board;
        private readonly List<(VisualElement element, float? left, float? top, float? right, float? bottom)> placed = new List<(VisualElement, float?, float?, float?, float?)>();
        private Rect insets;

        public HudStage()
        {
            AddToClassList("stage"); pickingMode = PickingMode.Ignore;
            EnableInClassList("mobile", mobile); EnableInClassList("desktop", !mobile);
            Board.AddToClassList("board"); Board.pickingMode = PickingMode.Ignore; Board.style.transformOrigin = new TransformOrigin(0, 0);
            hierarchy.Add(Board);
            Visor = new HudVisor(this); hierarchy.Add(Visor);
            RegisterCallback<GeometryChangedEvent>(_ => Fit());
        }

        public float Width => Board.layout.width;
        public float Height => Board.layout.height;

        public void Fit()
        {
            float w = layout.width, h = layout.height;
            if (float.IsNaN(w) || w <= 0 || h <= 0) return;
            var device = Application.isPlaying;
            Mobile = w / h >= 1.9f || device && Application.isMobilePlatform && Screen.dpi > 0 && Screen.height / Screen.dpi < 4.2f;
            Scale = Math.Min(h / (Mobile ? MobileHeight : DesktopHeight), w / 640f);
            EnableInClassList("mobile", Mobile); EnableInClassList("desktop", !Mobile);
            Board.style.width = w / Scale; Board.style.height = h / Scale;
            Board.style.scale = new StyleScale(new Scale(new Vector3(Scale, Scale, 1)));
            // Device safe area in board units; Rect stores four independent inset amounts here, not min/max coordinates.
            if (device)
            {
                var safe = Screen.safeArea; var k = h / Mathf.Max(1, Screen.height) / Scale; var kx = w / Mathf.Max(1, Screen.width) / Scale;
                insets = new Rect(safe.xMin * kx, (Screen.height - safe.yMax) * k, (Screen.width - safe.xMax) * kx, safe.yMin * k);
            }
            else insets = new Rect();
            foreach (var anchor in Board.Query<HudAnchor>().ToList()) { var (l, t, r, b) = anchor.Offsets(Mobile); Apply((anchor, l, t, r, b)); }
            foreach (var p in placed) Apply(p);
            Visor.MarkDirtyRepaint();
            Resized?.Invoke();
        }

        // Anchors a code-built element to board edges; left/right swap when the left-handed layout is on.
        public T Place<T>(T element, float? left = null, float? top = null, float? right = null, float? bottom = null) where T : VisualElement
        {
            if (element.parent == null) Board.Add(element);
            element.style.position = Position.Absolute;
            placed.RemoveAll(p => p.element == element);
            var entry = (element, left, top, right, bottom);
            placed.Add(entry); Apply(entry);
            return element;
        }

        private void Apply((VisualElement element, float? left, float? top, float? right, float? bottom) p)
        {
            var (l, r) = Mirror ? (p.right, p.left) : (p.left, p.right);
            var s = p.element.style;
            s.position = Position.Absolute;
            s.left = l.HasValue ? l.Value + insets.xMin : StyleKeyword.Auto;
            s.right = r.HasValue ? r.Value + insets.width : StyleKeyword.Auto;
            s.top = p.top.HasValue ? p.top.Value + insets.yMin : StyleKeyword.Auto;
            s.bottom = p.bottom.HasValue ? p.bottom.Value + insets.height : StyleKeyword.Auto;
        }

        // Viewport (0..1, y up) to board coordinates. The camera and the panel both cover the whole target (the screen, or the
        // texture a panel draws into), so the viewport maps straight onto the panel's root rather than through the screen size.
        public Vector2 FromViewport(Vector3 viewport)
        {
            var tree = panel?.visualTree;
            if (tree == null) return Vector2.zero;
            var r = tree.worldBound;
            return Board.WorldToLocal(new Vector2(r.xMin + viewport.x * r.width, r.yMin + (1 - viewport.y) * r.height));
        }

        // Board coordinates to screen pixels (y up), for 3D picking.
        public Vector2 ToScreen(Vector2 boardPoint) => panel == null ? Vector2.zero : HudInput.PanelToScreen(panel, Board.LocalToWorld(boardPoint));
    }

    // A block pinned to the stage's edges in board units (empty = auto). The stage adds the safe area and mirrors
    // left/right for the left-handed layout, so screens are authored once in UI Builder. On the desktop board, an anchor
    // with any d-left / d-top / d-right / d-bottom set takes all four edges from those instead (empty = auto).
    [UxmlElement]
    public partial class HudAnchor : VisualElement
    {
        private string left = "", top = "", right = "", bottom = "";
        [UxmlAttribute] public string Left { get => left; set { left = value; Refit(); } }
        [UxmlAttribute] public string Top { get => top; set { top = value; Refit(); } }
        [UxmlAttribute] public string Right { get => right; set { right = value; Refit(); } }
        [UxmlAttribute] public string Bottom { get => bottom; set { bottom = value; Refit(); } }
        private string dLeft = "", dTop = "", dRight = "", dBottom = "";
        [UxmlAttribute("d-left")] public string DLeft { get => dLeft; set { dLeft = value; Refit(); } }
        [UxmlAttribute("d-top")] public string DTop { get => dTop; set { dTop = value; Refit(); } }
        [UxmlAttribute("d-right")] public string DRight { get => dRight; set { dRight = value; Refit(); } }
        [UxmlAttribute("d-bottom")] public string DBottom { get => dBottom; set { dBottom = value; Refit(); } }
        public HudAnchor() { AddToClassList("anchor"); pickingMode = PickingMode.Ignore; style.position = Position.Absolute; }
        internal (float?, float?, float?, float?) Offsets(bool mobile) =>
            mobile || (dLeft + dTop + dRight + dBottom).Length == 0 ? (Offset(left), Offset(top), Offset(right), Offset(bottom)) : (Offset(dLeft), Offset(dTop), Offset(dRight), Offset(dBottom));
        internal float? Offset(string value) => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : (float?)null;
        private void Refit() => GetFirstAncestorOfType<HudStage>()?.Fit();
    }
}
