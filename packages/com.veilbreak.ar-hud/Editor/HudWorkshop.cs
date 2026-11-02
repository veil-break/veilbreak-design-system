using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud.Editor
{
    /// <summary>
    /// Live theme editing over the sample gallery, laid out on a <see cref="HudStage"/> with its chrome and the kit. It opens on
    /// <see cref="HudTheme.Default"/> / <see cref="HudEffects.Default"/> (an app's installed profiles) or temporary ones, and
    /// previews with <see cref="PreviewStyles"/> (an app can install its own sheets; the package's Hud.uss, Base.uss and Kit.uss
    /// with com.veilbreak.gui's Tokens.uss otherwise). A <see cref="HudTheme.Generated"/> theme is shown read-only: edit its source and regenerate, or save a copy.
    /// </summary>
    public sealed class HudWorkshop : EditorWindow
    {
        /// <summary>The sheets the preview attaches, in cascade order; null or empty uses the package's Hud.uss, gui's Tokens.uss, Base.uss and Kit.uss.</summary>
        public static System.Func<StyleSheet[]> PreviewStyles { get; set; }
        private HudTheme theme;
        private HudEffects effects;
        private HudTheme localTheme;
        private HudEffects localEffects;
        private HudView view;
        private IVisualElementScheduledItem refresh;
        private int themeRevision = -1, effectsRevision = -1;
        // Resolved through the package registry so the path is right however the package is installed.
        /// <summary>The design-token sheet the kit reads: com.veilbreak.gui owns the tokens.</summary>
        internal const string GuiTokens = "Packages/com.veilbreak.gui/Runtime/Styles/Tokens.uss";
        internal static StyleSheet Tokens() => AssetDatabase.LoadAssetAtPath<StyleSheet>(GuiTokens);
        internal static string PackagePath => UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(HudTheme).Assembly).assetPath + "/";
        [MenuItem("Veilbreak/AR HUD/Theme Workshop")]
        public static void Open() => GetWindow<HudWorkshop>("AR HUD Workshop");
        public void CreateGUI()
        {
            view?.Dispose(); refresh?.Pause(); rootVisualElement.Clear();
            minSize = new Vector2(850, 540);
            if (!localTheme) { localTheme = CreateInstance<HudTheme>(); localTheme.hideFlags = HideFlags.HideAndDontSave; }
            if (!localEffects) { localEffects = CreateInstance<HudEffects>(); localEffects.hideFlags = HideFlags.HideAndDontSave; }
            if (!theme) theme = HudTheme.Default ? HudTheme.Default : localTheme;
            if (!effects) effects = HudEffects.Default ? HudEffects.Default : localEffects;
            var toolbar = new Toolbar();
            var preset = new EnumField(HudPalette.CyanVisor);
            preset.RegisterValueChangedCallback(evt =>
            {
                if (!theme || theme.Generated) return; // generated themes follow their source, never a preset
                Undo.RecordObject(theme, "Apply HUD preset"); theme.ApplyPreset((HudPalette)evt.newValue); EditorUtility.SetDirty(theme); Apply();
            });
            toolbar.Add(new Label("Palette ")); toolbar.Add(preset);
            toolbar.Add(new ToolbarButton(() => view.Navigation.Show("menu")) { text = "Menu" });
            toolbar.Add(new ToolbarButton(() => view.Navigation.Show("battle")) { text = "Battle" });
            toolbar.Add(new ToolbarButton(() => view.Navigation.Show("loadout")) { text = "Loadout" });
            toolbar.Add(new ToolbarButton(SaveProfiles) { text = "Save profiles as assets" });
            rootVisualElement.Add(toolbar);
            var body = new VisualElement(); body.style.flexDirection = FlexDirection.Row; body.style.flexGrow = 1;
            rootVisualElement.Add(body);
            var preview = new VisualElement(); preview.style.flexGrow = 1; preview.style.backgroundColor = new Color(.025f, .035f, .05f);
            preview.style.overflow = Overflow.Hidden; body.Add(preview);
            view = new HudView(Styles());
            preview.Add(view.Root); HudGallery.Build(view, new HudModel());
            var settings = new ScrollView(); settings.style.width = 290; settings.style.flexShrink = 0; body.Add(settings);
            var themeField = new ObjectField("Theme") { objectType = typeof(HudTheme), value = theme };
            var effectsField = new ObjectField("Effects") { objectType = typeof(HudEffects), value = effects };
            settings.Add(themeField); settings.Add(effectsField);
            settings.Add(new HelpBox("Select project assets to edit them live (Undo supported), or save the temporary profiles. Preview uses the runtime UI tree; its dark background is editor-only.", HelpBoxMessageType.Info));
            var generated = new HelpBox("", HelpBoxMessageType.Warning); settings.Add(generated);
            var inspectors = new VisualElement(); settings.Add(inspectors);
            void Inspect()
            {
                var locked = theme.Generated;
                generated.text = locked ? $"{theme.name} is generated from {theme.generatedFrom}: edit that source and regenerate it, or save a copy to edit here." : "";
                generated.style.display = locked ? DisplayStyle.Flex : DisplayStyle.None;
                preset.SetEnabled(!locked);
                var themeInspector = new InspectorElement(theme); themeInspector.SetEnabled(!locked);
                inspectors.Clear(); inspectors.Add(themeInspector); inspectors.Add(new InspectorElement(effects)); Apply();
            }
            themeField.RegisterValueChangedCallback(evt => { theme = evt.newValue as HudTheme; if (!theme) theme = localTheme; Inspect(); });
            effectsField.RegisterValueChangedCallback(evt => { effects = evt.newValue as HudEffects; if (!effects) effects = localEffects; Inspect(); });
            Inspect();
            refresh = rootVisualElement.schedule.Execute(() =>
            {
                if (theme && effects && (theme.Revision != themeRevision || effects.Revision != effectsRevision)) Apply();
            }).Every(100);
        }
        private static StyleSheet[] Styles()
        {
            var styles = PreviewStyles?.Invoke();
            if (styles != null && styles.Length > 0) return styles;
            StyleSheet Sheet(string name) => AssetDatabase.LoadAssetAtPath<StyleSheet>(PackagePath + "Runtime/Styles/" + name);
            return new[] { Sheet("Hud.uss"), Tokens(), Sheet("Base.uss"), Sheet("Kit.uss") };
        }

        private void Apply()
        {
            if (view == null || !theme || !effects) return;
            view.Apply(theme, effects); themeRevision = theme.Revision; effectsRevision = effects.Revision;
        }
        private void SaveProfiles()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save theme", "VisorTheme", "asset", "Choose where to store the theme. A matching effects asset is created beside it.");
            if (string.IsNullOrEmpty(path)) return;
            // Never overwrite existing user profiles.
            var savedTheme = Instantiate(theme); savedTheme.hideFlags = HideFlags.None; savedTheme.generatedFrom = ""; // the copy is the user's own
            var savedEffects = Instantiate(effects); savedEffects.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(savedTheme, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.CreateAsset(savedEffects, AssetDatabase.GenerateUniqueAssetPath(path.Replace(".asset", "Effects.asset")));
            AssetDatabase.SaveAssets(); EditorGUIUtility.PingObject(savedTheme);
        }
        private void OnDisable()
        {
            refresh?.Pause(); view?.Dispose(); view = null;
            if (localTheme) DestroyImmediate(localTheme);
            if (localEffects) DestroyImmediate(localEffects);
        }
    }
}
