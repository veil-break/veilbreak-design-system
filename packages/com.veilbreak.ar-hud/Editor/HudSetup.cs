using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud.Editor
{
    public static class HudSetup
    {
        // Where Create Demo writes its theme, effects and panel settings (an app's editor code can point it at its own UI folder).
        public static string AssetFolder = "Assets/ARHud";

        [MenuItem("Veilbreak/AR HUD/Create Demo in Scene")]
        public static void CreateDemo()
        {
            var folder = AssetFolder; EnsureFolder(folder);
            var theme = ScriptableObject.CreateInstance<HudTheme>();
            var effects = ScriptableObject.CreateInstance<HudEffects>();
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1440, 900);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = .5f;
            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(HudWorkshop.PackagePath + "Runtime/Styles/RuntimeTheme.tss");
            AssetDatabase.CreateAsset(theme, AssetDatabase.GenerateUniqueAssetPath(folder + "/Theme.asset"));
            AssetDatabase.CreateAsset(effects, AssetDatabase.GenerateUniqueAssetPath(folder + "/Effects.asset"));
            AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath(folder + "/PanelSettings.asset"));
            var go = new GameObject("AR HUD"); Undo.RegisterCreatedObjectUndo(go, "Create AR HUD");
            var panel = Undo.AddComponent<PanelRenderer>(go); panel.panelSettings = settings;
            var host = Undo.AddComponent<HudHost>(go);
            host.Theme = theme; host.Effects = effects;
            // RuntimeTheme.tss brings Hud.uss; the gallery's kit pieces need the kit sheets (and default token values) on the view.
            StyleSheet Sheet(string name) => AssetDatabase.LoadAssetAtPath<StyleSheet>(HudWorkshop.PackagePath + "Runtime/Styles/" + name);
            host.Styles = new[] { HudWorkshop.Tokens(), Sheet("Base.uss"), Sheet("Kit.uss") };
            host.Rebuild(); EditorUtility.SetDirty(host);
            EditorSceneManager.MarkSceneDirty(go.scene); AssetDatabase.SaveAssets(); Selection.activeGameObject = go;
        }
        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
