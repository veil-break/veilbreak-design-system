using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Gui;

namespace Veilbreak.ARHud
{
    /// <summary>The HUD's radial gauge. The dial, value rules and read-out text are gui's <see cref="VbGauge"/>; this element keeps the
    /// <c>ar-gauge</c> classes, the themed colours and its UXML name. It paints with <see cref="VbGauge.Draw"/> instead of deriving from
    /// it so <see cref="Value"/> keeps its own data-binding property bag.</summary>
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
                amount = VbGauge.Clamp(value);
                if (readout != null) readout.text = VbGauge.Percent(amount);
                MarkDirtyRepaint();
            }
        }
        [UxmlAttribute] public int Segments { get => segments; set { segments = Mathf.Clamp(value, VbGauge.MinSegments, VbGauge.MaxSegments); MarkDirtyRepaint(); } }
        public HudGauge()
        {
            AddToClassList("ar-gauge"); pickingMode = PickingMode.Ignore;
            readout = new Label(); readout.AddToClassList("ar-gauge__value");
            readout.pickingMode = PickingMode.Ignore; Add(readout); Value = amount;
            generateVisualContent += Draw;
        }
        public void ApplyTheme(HudTheme value) { theme = value; readout.style.color = value.foreground; MarkDirtyRepaint(); }
        private void Draw(MeshGenerationContext ctx) => VbGauge.Draw(ctx.painter2D, contentRect, amount, segments, theme ? theme.accent : Color.cyan);
    }
}
