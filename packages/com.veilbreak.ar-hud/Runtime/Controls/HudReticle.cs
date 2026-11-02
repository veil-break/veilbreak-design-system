using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    [UxmlElement]
    public partial class HudReticle : VisualElement, IHudThemed
    {
        private Color color = Color.cyan;
        private float spread = 12;
        [UxmlAttribute] public float Spread { get => spread; set { spread = Mathf.Clamp(value, 2, 40); MarkDirtyRepaint(); } }
        public HudReticle()
        {
            AddToClassList("ar-reticle"); pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }
        public void ApplyTheme(HudTheme value) { color = value.accent; MarkDirtyRepaint(); }
        private void Draw(MeshGenerationContext ctx)
        {
            var c = contentRect.center;
            for (int i = 0; i < 4; i++)
            {
                var d = new Vector2(Mathf.Cos(i * Mathf.PI / 2), Mathf.Sin(i * Mathf.PI / 2));
                HudPaint.Line(ctx.painter2D, c + d * spread, c + d * (spread + 8), color);
            }
            HudPaint.Arc(ctx.painter2D, c, 2, 0, 359.9f, color, 2);
        }
    }
}
