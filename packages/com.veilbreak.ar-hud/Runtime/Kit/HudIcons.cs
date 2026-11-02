using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // An icon set as direct references: every screen uses its glyphs and they always ship together, so the app references one
    // library and installs it at boot (Active) instead of each HudIcon loading from Resources. Callers name a glyph by its
    // kebab-case id ("skill-aria-needle-storm", "lock"); the file is its PascalCase form ("SkillAriaNeedleStorm", "Lock").
    [CreateAssetMenu(menuName = "Veilbreak/AR HUD/Icon Library", fileName = "IconLibrary")]
    public sealed class HudIcons : ScriptableObject
    {
        public VectorImage[] icons = new VectorImage[0];
        private Dictionary<string, VectorImage> byName;

        // Installed by the app before the first screen loads.
        public static HudIcons Active { get; set; }

        private void OnEnable() => byName = null;

        public VectorImage Find(string name)
        {
            if (byName == null)
            {
                byName = new Dictionary<string, VectorImage>();
                foreach (var icon in icons) if (icon != null && !byName.ContainsKey(icon.name)) byName[icon.name] = icon;
            }
            return byName.TryGetValue(FileName(name), out var found) ? found : null;
        }

        // Kebab-case id to file name: words split on anything but ASCII letters and digits (underscores stay separators), each
        // word capitalised, an all-caps word title-cased, adjacent number groups kept apart with an underscore.
        public static string FileName(string name)
        {
            var result = new StringBuilder();
            foreach (var segment in name.Split('_'))
            {
                var part = new StringBuilder(); var word = new StringBuilder();
                void Flush()
                {
                    if (word.Length == 0) return;
                    var w = word.ToString(); word.Clear();
                    int letters = 0, upper = 0;
                    foreach (var c in w) if (char.IsLetter(c)) { letters++; if (char.IsUpper(c)) upper++; }
                    w = letters > 1 && upper == letters ? w.Substring(0, 1) + w.Substring(1).ToLowerInvariant() : char.ToUpperInvariant(w[0]) + w.Substring(1);
                    if (part.Length > 0 && char.IsDigit(part[part.Length - 1]) && char.IsDigit(w[0])) part.Append('_');
                    part.Append(w);
                }
                foreach (var c in segment) if (c < 128 && char.IsLetterOrDigit(c)) word.Append(c); else Flush();
                Flush();
                if (part.Length == 0) continue;
                if (result.Length > 0) result.Append('_');
                result.Append(part);
            }
            return result.ToString();
        }

#if UNITY_EDITOR
        private static HudIcons editorLibrary;
        // UI Builder and edit-mode previews: the project's first icon library.
        internal static HudIcons EditorLibrary()
        {
            if (editorLibrary != null) return editorLibrary;
            var found = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(HudIcons));
            return editorLibrary = found.Length == 0 ? null : UnityEditor.AssetDatabase.LoadAssetAtPath<HudIcons>(UnityEditor.AssetDatabase.GUIDToAssetPath(found[0]));
        }
#endif
    }
}
