using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Gui;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// Chamfered vector shapes for any element, styled from USS custom properties so a screen's stylesheet owns every colour.
    /// UI Toolkit has no clip-path, so the outline is painted; the element's content box and hit area stay its rectangle.
    /// The painter is gui's <see cref="VbVectorShape"/>; this class is the HUD's vocabulary for it: the bare custom properties
    /// below and the <c>ar-shape</c> class. All properties are optional; <c>.ar-shape</c> resets them in Base.uss, because custom
    /// properties inherit into nested shapes:
    ///   --cut (chamfer size) --flip (1: cut top-right/bottom-left instead of top-left/bottom-right)
    ///   --fill [--fill-to --fill-dir (degrees, 0 = left to right, 90 = top to bottom)] --stroke --stroke-width --glow
    ///   --bracket [--bracket-2 (the second corner) --bracket-mask (1 TL, 2 TR, 4 BR, 8 BL; 0 = the two square corners)
    ///   --bracket-len --bracket-w --bracket-out (px outside the outline; negative = flush inside)]
    ///   --ring [--ring-gap]: a focus frame, 2px in --ring after a 2px gap in --ring-gap.
    ///   Glass: --sheen (top-lit: fades to clear at half height) --scan [--scan-pitch] (scanlines inside the outline)
    ///   --halo (projector bleed: two soft strokes under --stroke).
    /// A change of look (hover, selection, a class HudMotion replays) blends over <see cref="Blend"/> seconds.
    /// </summary>
    public static class HudShape
    {
        public const string ClassName = "ar-shape";
        public const float Blend = .2f;

        private static readonly VbVectorVocabulary HudLook = new VbVectorVocabulary("--", ClassName, Blend);

        /// <summary>The chamfered outline grown by <paramref name="inset"/> px, in local coordinates. A plain element gives its rectangle.</summary>
        public static Vector2[] Outline(VisualElement element, float inset) => VbVectorShape.Outline(element, inset);

        /// <summary>The outline of a <paramref name="w"/> × <paramref name="h"/> shape with chamfer <paramref name="c"/>, grown by <paramref name="inset"/>.</summary>
        public static Vector2[] Outline(bool flip, float w, float h, float c, float inset) => VbVectorShape.Outline(flip, w, h, c, inset);

        /// <summary>A linear fill that keeps both colours' alpha (FillGradient.MakeLinearGradient(Color, Color, ...) drops it).</summary>
        public static FillGradient Linear(Color from, Color to, Vector2 start, Vector2 end) => VbVectorShape.Linear(from, to, start, end);

        /// <summary>Paints <paramref name="element"/> as a shape from its USS custom properties and returns it.</summary>
        public static T Attach<T>(T element) where T : VisualElement => VbVectorShape.Attach(element, HudLook);
    }

    /// <summary>A chamfered panel for UXML (<c>&lt;ar:HudFrame class="..."&gt;</c>), painted by <see cref="HudShape"/> from USS.</summary>
    [UxmlElement]
    public partial class HudFrame : VisualElement
    {
        public const string ClassName = "ar-frame";
        public HudFrame() { AddToClassList(ClassName); HudShape.Attach(this); }
    }

    /// <summary>A chamfered button for UXML (<c>&lt;ar:HudButton class="..."&gt;</c>), painted by <see cref="HudShape"/> from USS.</summary>
    [UxmlElement]
    public partial class HudButton : Button
    {
        public const string ClassName = "ar-press";
        public HudButton() : this(null) { }
        public HudButton(System.Action clicked) : base(clicked) { AddToClassList(ClassName); HudShape.Attach(this); }
    }
}
