using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Gui;

namespace Veilbreak.ARHud
{
    // The HUD's names for gui's Painter2D helpers (VbPaint).
    internal static class HudPaint
    {
        public static Color Alpha(Color color, float alpha) => VbPaint.Alpha(color, alpha);
        public static void Line(Painter2D p, Vector2 a, Vector2 b, Color color, float width = 1) => VbPaint.Line(p, a, b, color, width);
        public static void Arc(Painter2D p, Vector2 center, float radius, float start, float end, Color color, float width) => VbPaint.Arc(p, center, radius, start, end, color, width);
    }
    public interface IHudThemed { void ApplyTheme(HudTheme theme); }
}
