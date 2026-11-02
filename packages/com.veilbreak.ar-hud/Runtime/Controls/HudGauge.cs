using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    [UxmlElement]
    public partial class HudGauge : VisualElement, IHudThemed
    {
        private float amount = .72f;
        private int segments = 36;
        private readonly Label readout;
        private HudTheme theme;
        [UxmlAttribute, CreateProperty]
        public float Value
        {
            get => amount;
            set
            {
                amount = float.IsNaN(value) ? 0 : Mathf.Clamp01(value);
                if (readout != null) readout.text = $"{amount * 100:0}%";
                MarkDirtyRepaint();
            }
        }
        [UxmlAttribute] public int Segments { get => segments; set { segments = Mathf.Clamp(value, 4, 96); MarkDirtyRepaint(); } }
        public HudGauge()
        {
            AddToClassList("ar-gauge"); pickingMode = PickingMode.Ignore;
            readout = new Label(); readout.AddToClassList("ar-gauge__value");
            readout.pickingMode = PickingMode.Ignore; Add(readout); Value = amount;
            generateVisualContent += Draw;
        }
        public void ApplyTheme(HudTheme value) { theme = value; readout.style.color = value.foreground; MarkDirtyRepaint(); }
        private void Draw(MeshGenerationContext ctx)
        {
            var center = contentRect.center;
            var radius = Mathf.Min(contentRect.width, contentRect.height) * .5f - 8;
            var accent = theme ? theme.accent : Color.cyan;
            for (int i = 0; i < segments; i++)
            {
                float a = 140 + 260f * i / segments;
                HudPaint.Arc(ctx.painter2D, center, radius, a, a + 260f / segments - 2,
                    HudPaint.Alpha(accent, i < Mathf.CeilToInt(amount * segments) ? 1 : .18f), 4);
            }
            HudPaint.Arc(ctx.painter2D, center, radius - 9, 140, 400, HudPaint.Alpha(accent, .22f), 1);
        }
    }
}
