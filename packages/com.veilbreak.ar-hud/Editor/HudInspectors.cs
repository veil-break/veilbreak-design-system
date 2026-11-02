using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud.Editor
{
    [CustomEditor(typeof(HudTheme)), CanEditMultipleObjects]
    public sealed class HudThemeInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new Button(HudWorkshop.Open) { text = "Open live theme workshop" });
            var generated = targets.OfType<HudTheme>().FirstOrDefault(t => t.Generated);
            var fields = new VisualElement(); InspectorElement.FillDefaultInspector(fields, serializedObject, this);
            if (generated)
            {
                // Generated themes follow their source: edits here would be overwritten on the next sync.
                root.Add(new HelpBox($"Generated from {generated.generatedFrom}. Edit that source and regenerate it, or save a copy from the workshop to edit.", HelpBoxMessageType.Warning));
                fields.SetEnabled(false);
            }
            root.Add(fields); return root;
        }
    }
    [CustomEditor(typeof(HudEffects)), CanEditMultipleObjects]
    public sealed class HudEffectsInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new HelpBox("Effects decorate the visor only. Reduced Motion stops animation and parallax. High Contrast on the theme removes all chrome.", HelpBoxMessageType.Info));
            InspectorElement.FillDefaultInspector(root, serializedObject, this); return root;
        }
    }
    [CustomEditor(typeof(HudHost)), CanEditMultipleObjects]
    public sealed class HudHostInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new HelpBox("Builds a HudView into this PanelRenderer's root: screens register with its navigation from the Ready event (HudView.Navigation.Register, Show, OpenModal). The panel theme should import Hud.uss (RuntimeTheme.tss does); Styles go on the view root. Theme and effects fall back to HudTheme.Default / HudEffects.Default. A Layout replaces the gallery.", HelpBoxMessageType.Info));
            InspectorElement.FillDefaultInspector(root, serializedObject, this); return root;
        }
    }
}
