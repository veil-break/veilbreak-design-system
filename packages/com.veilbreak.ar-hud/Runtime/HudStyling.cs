using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// Themes the <see cref="HudView.ThemedClass"/> subtrees under a root from a <see cref="HudTheme"/>: text colour, size and face
    /// on the themed element, the ar-muted / ar-danger / ar-power / ar-heading roles, minimum target height and surface on buttons,
    /// and every <see cref="IHudThemed"/> control. Elements outside a themed subtree are left to their style sheets.
    /// </summary>
    public static class HudStyling
    {
        public const string HighContrastClass = "ar-high-contrast";

        public static void Apply(VisualElement root, HudTheme theme)
        {
            if (!theme || root == null) return;
            root.Query<VisualElement>(className: HudView.ThemedClass).ForEach(scope =>
            {
                scope.style.color = theme.foreground; scope.style.fontSize = theme.fontSize;
                scope.style.unityFont = theme.bodyFont;
                scope.EnableInClassList(HighContrastClass, theme.highContrast);
                scope.Query<VisualElement>().ForEach(element => ApplyElement(element, theme));
            });
        }

        private static void ApplyElement(VisualElement element, HudTheme theme)
        {
            if (element is IHudThemed control) control.ApplyTheme(theme);
            if (element.ClassListContains("ar-muted")) element.style.color = theme.muted;
            if (element.ClassListContains("ar-danger")) element.style.color = theme.danger;
            if (element.ClassListContains("ar-power")) element.style.color = theme.power;
            if (element.ClassListContains("ar-heading"))
            {
                element.style.color = theme.accent;
                element.style.unityFont = theme.headingFont ? theme.headingFont : theme.bodyFont;
            }
            if (element is Button && !element.ClassListContains(HudShape.ClassName))
            {
                element.style.minHeight = theme.targetHeight;
                element.style.color = theme.foreground;
                element.style.backgroundColor = theme.surface;
            }
        }
    }
}
