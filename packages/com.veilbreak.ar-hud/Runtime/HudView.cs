using System;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// One HUD tree: the root (<c>.ar-root</c>, filling its parent) holds the content (<c>.ar-content</c>, where screens build),
    /// then the modals and toasts its <see cref="Navigation"/> opens. Independent of scenes and game state. Screen chrome and the
    /// safe area belong to the <see cref="HudStage"/>s the screens are laid out on; their <see cref="HudVisor"/>s draw with this
    /// view's theme and effects. <see cref="Apply"/> also themes the <see cref="ThemedClass"/> subtrees (the gallery, themed
    /// controls such as <see cref="HudPanel"/>); everything else keeps its own style sheets' look.
    /// </summary>
    public sealed class HudView : IDisposable
    {
        /// <summary>Subtrees <see cref="HudStyling"/> themes from the <see cref="HudTheme"/> (inline colours and fonts, themed controls).</summary>
        public const string ThemedClass = "ar-themed";
        public VisualElement Root { get; }
        public VisualElement Content { get; }
        public HudNavigation Navigation { get; }
        public HudTheme Theme { get; private set; }
        public HudEffects Effects { get; private set; }

        public HudView(params StyleSheet[] styles)
        {
            Root = new VisualElement { name = "ar-hud", pickingMode = PickingMode.Ignore, userData = this };
            Root.AddToClassList("ar-root");
            if (styles != null) foreach (var sheet in styles) if (sheet) Root.styleSheets.Add(sheet);
            Content = new VisualElement { name = "ar-content", pickingMode = PickingMode.Ignore };
            Content.AddToClassList("ar-content"); Root.Add(Content);
            Navigation = new HudNavigation(Root, Content);
        }

        /// <summary>The view hosting <paramref name="element"/>, or null outside one (UI Builder, a bare panel).</summary>
        public static HudView Of(VisualElement element)
        {
            for (var e = element; e != null; e = e.hierarchy.parent) if (e.userData is HudView view && view.Root == e) return view;
            return null;
        }

        public void Apply(HudTheme palette, HudEffects profile)
        {
            Theme = palette; Effects = profile;
            RefreshTheme();
            Root.Query<HudVisor>().ForEach(visor => visor.Configure(palette ? palette : HudTheme.Default, profile ? profile : HudEffects.Default));
        }

        /// <summary>Themes the <see cref="ThemedClass"/> subtrees again (after building themed content).</summary>
        public void RefreshTheme() => HudStyling.Apply(Root, Theme);

        public void Dispose()
        {
            Navigation.Dispose();
            Root.RemoveFromHierarchy(); // detachment pauses the visors' and controls' schedulers
            Root.userData = null;
        }
    }
}
