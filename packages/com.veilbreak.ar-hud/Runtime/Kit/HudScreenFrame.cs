using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // The decorative chrome of a screen's own corners: a cut across the top-left and the bottom-right corner plus a thin edge
    // strip under the first, and a dark plate behind a side rail (solid at the screen edge, fading out toward the scene). Place
    // it as the first child of a HudStage board (with the HudFx.Static class, so it never fades in); it mirrors with the stage's
    // left-handed layout. Colours/sizes (board units) come from USS: --frame-fill, --frame-cut, --frame-cut-2, --frame-strip,
    // --frame-veil, --frame-veil-width (Base.uss .screen-frame).
    [UxmlElement]
    public partial class HudScreenFrame : VisualElement
    {
        private static readonly CustomStyleProperty<Color> FillProperty = new CustomStyleProperty<Color>("--frame-fill");
        private static readonly CustomStyleProperty<float> CutProperty = new CustomStyleProperty<float>("--frame-cut");
        private static readonly CustomStyleProperty<float> Cut2Property = new CustomStyleProperty<float>("--frame-cut-2");
        private static readonly CustomStyleProperty<float> StripProperty = new CustomStyleProperty<float>("--frame-strip");
        private static readonly CustomStyleProperty<Color> VeilProperty = new CustomStyleProperty<Color>("--frame-veil");
        private static readonly CustomStyleProperty<float> VeilWidthProperty = new CustomStyleProperty<float>("--frame-veil-width");
        private Color fill = new Color(.23f, .18f, .31f), veil = new Color(.024f, .016f, .055f, .88f); private float cut = 29, cut2 = 34, strip = 1.2f, veilWidth = 133;
        public HudScreenFrame()
        {
            AddToClassList("screen-frame"); pickingMode = PickingMode.Ignore; generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                if (customStyle.TryGetValue(FillProperty, out var f)) fill = f;
                if (customStyle.TryGetValue(CutProperty, out var c)) cut = c;
                if (customStyle.TryGetValue(Cut2Property, out var c2)) cut2 = c2;
                if (customStyle.TryGetValue(StripProperty, out var s)) strip = s;
                if (customStyle.TryGetValue(VeilProperty, out var v)) veil = v;
                if (customStyle.TryGetValue(VeilWidthProperty, out var vw)) veilWidth = vw;
                MarkDirtyRepaint();
            });
            RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
        }

        private void Draw(MeshGenerationContext context)
        {
            float w = layout.width, h = layout.height; if (w <= 0 || h <= 0) return;
            var mirror = GetFirstAncestorOfType<HudStage>()?.Mirror ?? false;
            var p = context.painter2D; p.fillColor = fill;
            float X(float x) => mirror ? w - x : x;
            // The rail's dark plate: solid over the first half of its width, then a linear fade into the scene.
            if (veil.a > 0 && veilWidth > 0)
            {
                var x1 = Mathf.Min(w, veilWidth); var clear = new Color(veil.r, veil.g, veil.b, 0);
                p.fillGradient = HudShape.Linear(veil, clear, new Vector2(X(x1 * .5f), 0), new Vector2(X(x1), 0));
                p.BeginPath(); p.MoveTo(new Vector2(X(0), 0)); p.LineTo(new Vector2(X(x1), 0)); p.LineTo(new Vector2(X(x1), h)); p.LineTo(new Vector2(X(0), h)); p.ClosePath(); p.Fill();
                p.fillColor = fill;
            }
            void Shape(params Vector2[] points)
            {
                p.BeginPath(); p.MoveTo(new Vector2(X(points[0].x), points[0].y));
                for (var i = 1; i < points.Length; i++) p.LineTo(new Vector2(X(points[i].x), points[i].y));
                p.ClosePath(); p.Fill();
            }
            Shape(new Vector2(0, 0), new Vector2(cut, 0), new Vector2(0, cut));
            if (strip > 0) Shape(new Vector2(0, cut), new Vector2(strip, cut), new Vector2(strip, cut + 81), new Vector2(0, cut + 81));
            Shape(new Vector2(w, h), new Vector2(w - cut2, h), new Vector2(w, h - cut2));
        }
    }

}
