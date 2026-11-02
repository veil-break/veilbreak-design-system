using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // Glyphs from the installed HudIcons library (VectorImages on a 24-unit grid) tinted with the element's text colour; a few
    // kinds are also stroked here, used when the library has no file for them. Usable from UXML: <ar:HudIcon kind="team" size="20" />.
    [UxmlElement]
    public partial class HudIcon : VisualElement
    {
        private static readonly Dictionary<string, string> Files = new Dictionary<string, string>
        { ["auto"] = "automation", ["replay"] = "replays", ["plus"] = "add", ["minus"] = "remove", ["signal"] = "connection", ["rank"] = "ranked", ["rift"] = "rifts" };
        private string kind = "diamond";
        private float size = 20;
        private bool vector;
        [UxmlAttribute] public string Kind { get => kind; set { kind = value; Load(); MarkDirtyRepaint(); } }
        [UxmlAttribute] public float Size { get => size; set { size = value; style.width = value; style.height = value; } }
        public HudIcon() : this("diamond") { }
        public HudIcon(string kind, float size = 20)
        {
            Kind = kind; Size = size; AddToClassList("icon"); pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            // Glyph files are white ink, tinted with the resolved text colour (stroked kinds repaint in it) whenever styles change (hover,
            // state; Base.uss gives .icon the --icon property so the event reaches it), and again when a colour transition ends: the
            // restyle that starts one resolves the colour it starts from.
            RegisterCallback<CustomStyleResolvedEvent>(_ => Tint());
            RegisterCallback<TransitionEndEvent>(_ => Tint());
        }
        private void Tint() { if (vector) style.unityBackgroundImageTintColor = resolvedStyle.color; else MarkDirtyRepaint(); }
        private void Load()
        {
            var file = kind == "diamond" ? null : Library()?.Find(Files.TryGetValue(kind, out var name) ? name : kind);
            vector = file != null;
            style.backgroundImage = vector ? new StyleBackground(file) : new StyleBackground(StyleKeyword.None);
        }
        // The app installs its library at boot (HudIcons.Active); UI Builder and edit-mode previews use the project's library.
        private static HudIcons Library()
        {
#if UNITY_EDITOR
            if (HudIcons.Active == null) return HudIcons.EditorLibrary();
#endif
            return HudIcons.Active;
        }
        private void Draw(MeshGenerationContext context)
        {
            if (vector) return;
            var p = context.painter2D; var s = layout.width / 24f; var color = resolvedStyle.color;
            p.strokeColor = color; p.fillColor = color; p.lineWidth = 1.8f * s; p.lineJoin = LineJoin.Miter;
            Vector2 V(float x, float y) => new Vector2(x * s, y * s);
            void Line(params float[] xy) { p.BeginPath(); p.MoveTo(V(xy[0], xy[1])); for (var i = 2; i < xy.Length; i += 2) p.LineTo(V(xy[i], xy[i + 1])); p.Stroke(); }
            void Circle(float x, float y, float r) { p.BeginPath(); p.Arc(V(x, y), r * s, 0, 360); p.Stroke(); }
            switch (kind)
            {
                case "team": Circle(8, 8, 3); Circle(16, 8, 3); Line(2, 20, 4, 15, 8, 14, 12, 15, 14, 20); Line(14, 14, 19, 15, 22, 20); break;
                case "auto": Line(5, 7, 19, 7, 19, 19, 5, 19, 5, 7); Line(12, 3, 12, 7); Line(9, 16, 15, 16); Line(9, 11.5f, 9.5f, 11.5f); Line(14.5f, 11.5f, 15, 11.5f); break;
                case "studio": Line(12, 3, 20, 7.5f, 20, 16.5f, 12, 21, 4, 16.5f, 4, 7.5f, 12, 3); Line(12, 12, 20, 7.5f); Line(12, 12, 12, 21); Line(12, 12, 4, 7.5f); break;
                case "replay": p.BeginPath(); p.Arc(V(12, 12), 9 * s, 200, 520); p.Stroke(); Line(3, 3, 3, 8, 8, 8); Line(10, 9, 15, 12, 10, 15, 10, 9); break;
                case "settings": Circle(12, 12, 3); Line(12, 2, 12, 5); Line(12, 19, 12, 22); Line(2, 12, 5, 12); Line(19, 12, 22, 12); Line(4.9f, 4.9f, 7, 7); Line(17, 17, 19.1f, 19.1f); Line(4.9f, 19.1f, 7, 17); Line(17, 7, 19.1f, 4.9f); break;
                case "back": Line(15, 4, 8, 12, 15, 20); break;
                case "plus": Line(12, 4, 12, 20); Line(4, 12, 20, 12); break;
                case "minus": Line(5, 12, 19, 12); break;
                case "close": Line(6, 6, 18, 18); Line(18, 6, 6, 18); break;
                case "signal": Line(3, 20, 5, 20); Line(9, 20, 9, 15); Line(14, 20, 14, 10); Line(19, 20, 19, 4); break;
                case "rank": Line(12, 1, 23, 12, 12, 23, 1, 12, 12, 1); p.BeginPath(); p.MoveTo(V(12, 7)); p.LineTo(V(17, 12)); p.LineTo(V(12, 17)); p.LineTo(V(7, 12)); p.ClosePath(); p.Fill(); break;
                case "diamond": p.BeginPath(); p.MoveTo(V(12, 2)); p.LineTo(V(22, 12)); p.LineTo(V(12, 22)); p.LineTo(V(2, 12)); p.ClosePath(); p.Fill(); break;
            }
        }
    }

}
