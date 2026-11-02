using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    [UxmlElement]
    public partial class HudPanel : VisualElement, IHudThemed
    {
        private readonly Label heading;
        private readonly VisualElement body;
        private HudTheme theme;
        public override VisualElement contentContainer => body;
        [UxmlAttribute] public string Title { get => heading.text; set => heading.text = value; }
        public HudPanel()
        {
            AddToClassList("ar-panel");
            heading = new Label("PANEL"); heading.AddToClassList("ar-heading");
            body = new VisualElement(); body.AddToClassList("ar-panel__body");
            hierarchy.Add(heading); hierarchy.Add(body);
            generateVisualContent += Draw;
        }
        public void ApplyTheme(HudTheme value)
        {
            theme = value;
            style.paddingLeft = style.paddingRight = value.spacing;
            style.paddingTop = style.paddingBottom = value.spacing;
            heading.style.color = value.accent;
            heading.style.unityFont = value.headingFont ? value.headingFont : value.bodyFont;
            MarkDirtyRepaint();
        }
        // The HudShape outline (top-left and bottom-right chamfers) one pixel inside the panel, filled and stroked in theme colours.
        private void Draw(MeshGenerationContext ctx)
        {
            float w = layout.width - 2, h = layout.height - 2;
            if (w <= 0 || h <= 0) return;
            var cut = Mathf.Clamp(theme ? theme.cornerCut : 10, 0, Mathf.Min(w, h) * .5f);
            var points = HudShape.Outline(false, layout.width, layout.height, cut, -1);
            var p = ctx.painter2D;
            p.BeginPath(); p.MoveTo(points[0]); for (var i = 1; i < points.Length; i++) p.LineTo(points[i]); p.ClosePath();
            p.fillColor = theme ? theme.surface : new Color(.025f, .065f, .09f, .86f); p.Fill();
            p.strokeColor = theme ? HudPaint.Alpha(theme.accent, theme.highContrast ? 1 : .45f) : Color.cyan;
            p.lineWidth = theme ? theme.lineWidth : 1; p.Stroke();
        }
    }
}
