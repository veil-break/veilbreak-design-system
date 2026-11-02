using UnityEngine;

namespace Veilbreak.ARHud
{
    public enum HudPalette { CyanVisor, Veilbreak, Amber, HighContrast }

    [CreateAssetMenu(menuName = "Veilbreak/AR HUD/Theme", fileName = "HudTheme")]
    public sealed class HudTheme : ScriptableObject
    {
        [Header("Semantic palette")]
        public Color surface = new Color(0.025f, 0.065f, 0.09f, 0.86f);
        public Color foreground = new Color(0.9f, 0.98f, 1f);
        public Color muted = new Color(0.66f, 0.8f, 0.84f);
        public Color accent = new Color(0.28f, 0.94f, 0.95f);
        public Color danger = new Color(0.95f, 0.32f, 0.55f);
        public Color warning = new Color(1f, 0.76f, 0.35f);
        public Color power = new Color(0.75f, 0.52f, 0.99f);
        [Header("Typography (optional; Unity default when empty)")]
        public Font bodyFont;
        public Font headingFont;
        [Range(12, 28)] public float fontSize = 16;
        [Header("Geometry in panel points")]
        [Range(0, 24)] public float cornerCut = 10;
        [Range(1, 3)] public float lineWidth = 1;
        [Range(8, 32)] public float spacing = 16;
        [Range(44, 88)] public float targetHeight = 48;
        public bool highContrast;
        [Tooltip("Set when a tool writes this theme from another source (a design-token file, say): edit that source and regenerate; the workshop shows the theme read-only.")]
        public string generatedFrom = "";
        public bool Generated => !string.IsNullOrEmpty(generatedFrom);
        /// <summary>The theme a <see cref="HudHost"/>, <see cref="HudVisor"/> or preview without its own uses (installed by the app at boot and by its editor code for previews).</summary>
        public static HudTheme Default { get; set; }
        public int Revision { get; private set; }
        public void NotifyChanged() => Revision++;
        private void OnValidate() => NotifyChanged();

        public void ApplyPreset(HudPalette palette)
        {
            highContrast = palette == HudPalette.HighContrast;
            surface = Hex("071117"); surface.a = .86f;
            foreground = Hex("e6faff"); muted = Hex("a8ccd6");
            accent = Hex("47f0f2"); danger = Hex("f0509a");
            warning = Hex("ffc35a"); power = Hex("c084fc");
            if (palette == HudPalette.Veilbreak)
            {
                surface = Hex("0e0914"); surface.a = .86f;
                foreground = Hex("faeef4"); muted = Color.Lerp(surface, foreground, .78f);
                accent = Hex("1dd9a0");
            }
            if (palette == HudPalette.Amber) accent = Hex("ffc35a");
            if (highContrast)
            {
                surface = Color.black; foreground = muted = Color.white;
                accent = Hex("4dffc4"); danger = Hex("ff8cc2"); power = Hex("d6a6ff");
            }
            NotifyChanged();
        }
        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out var color);
            return color;
        }
    }
}
