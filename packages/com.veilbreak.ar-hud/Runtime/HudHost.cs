using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// Owns a <see cref="PanelRenderer"/>'s UI: builds a <see cref="HudView"/> into the panel's root and themes it. PanelRenderer
    /// (UIDocument's successor) hands its root out only through its reload callback, once the panel has built it (on the next panel
    /// update, not synchronously) and again whenever it is rebuilt; each new root gets a new view and raises <see cref="Ready"/>.
    /// Screens register with the view's <see cref="HudView.Navigation"/> from <see cref="Ready"/> (or from <see cref="View"/> when
    /// it is already up). With no <see cref="Layout"/> it can show the sample gallery. The theme and effects fall back to
    /// <see cref="HudTheme.Default"/> / <see cref="HudEffects.Default"/>, then to built-in profiles. <see cref="ScreenToPanel"/>, when
    /// installed, becomes the panel's screen-to-panel mapping (a lens that bends what the panel shows bends pointer input with it).
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(PanelRenderer))]
    public sealed class HudHost : MonoBehaviour
    {
        [SerializeField, Tooltip("Palette and effects colours for the visor chrome and themed controls; HudTheme.Default, then a built-in theme, when empty.")] private HudTheme theme;
        [SerializeField, Tooltip("Visor chrome and decorative animation; HudEffects.Default, then a built-in profile, when empty.")] private HudEffects effects;
        [SerializeField, Tooltip("Style sheets on the view root: the package's Hud.uss when the panel's theme does not import it, plus Base.uss and Kit.uss for the gallery.")] private StyleSheet[] styles = new StyleSheet[0];
        [SerializeField, Tooltip("Optional UXML cloned into the content; replaces the gallery when assigned.")] private VisualTreeAsset layout;
        [SerializeField, Tooltip("Show the sample gallery when no layout is assigned.")] private bool showGallery = true;

        public HudTheme Theme { get => theme; set { theme = value; restyle = true; } }
        public HudEffects Effects { get => effects; set { effects = value; restyle = true; } }
        public StyleSheet[] Styles { get => styles; set { styles = value ?? new StyleSheet[0]; Rebuild(); } }
        public VisualTreeAsset Layout { get => layout; set { layout = value; Rebuild(); } }
        public bool ShowGallery { get => showGallery; set { showGallery = value; Rebuild(); } }
        public HudView View { get; private set; }
        public HudModel Model { get; } = new HudModel();
        /// <summary>A new view is up (the first root, or a rebuilt panel): register screens and show the first one.</summary>
        public event Action<HudView> Ready;
        /// <summary>Installed by the app: the screen-to-panel mapping every host's panel uses (null leaves the panel's own).</summary>
        public static Func<Vector2, Vector2> ScreenToPanel
        {
            get => screenToPanel;
            set { screenToPanel = value; foreach (var host in Live) host.MapInput(); }
        }
        private static Func<Vector2, Vector2> screenToPanel;
        private static readonly System.Collections.Generic.List<HudHost> Live = new System.Collections.Generic.List<HudHost>();

        private PanelRenderer panel;
        private PanelRenderer.VersionedUIReloadCallback reload;
        private VisualElement panelRoot;
        private HudTheme fallbackTheme;
        private HudEffects fallbackEffects;
        private HudTheme lastTheme;
        private HudEffects lastEffects;
        private int themeRevision = -1, effectsRevision = -1;
        private bool restyle;

        private void OnEnable()
        {
            panel = GetComponent<PanelRenderer>();
            Live.Add(this); MapInput();
            reload ??= (_, root, _) => { if (root != panelRoot || View == null) Build(root); };
            panel.RegisterUIReloadCallback(reload);
        }

        private void OnDisable()
        {
            Live.Remove(this);
            if (panel != null && reload != null) panel.UnregisterUIReloadCallback(reload);
            ReleaseView(); panelRoot = null;
            DestroyOwned(fallbackTheme); DestroyOwned(fallbackEffects); fallbackTheme = null; fallbackEffects = null;
        }

        private void OnValidate() => restyle = true;

        private void MapInput()
        {
            var map = screenToPanel;
            if (map != null && panel != null && panel.panelSettings != null) panel.panelSettings.SetScreenToPanelSpaceFunction(v => map(v));
        }

        /// <summary>Builds the view again on the current root (after changing <see cref="Styles"/>, <see cref="Layout"/> or the gallery).</summary>
        public void Rebuild() { if (panelRoot != null && isActiveAndEnabled) Build(panelRoot); }

        // Live theme edits (the workshop, the inspector) restyle the view.
        private void Update()
        {
            if (View == null) return;
            var palette = Palette; var profile = Profile;
            if (restyle || palette != lastTheme || profile != lastEffects || themeRevision != palette.Revision || effectsRevision != profile.Revision) Restyle();
        }

        private HudTheme Palette
        {
            get
            {
                if (theme) return theme;
                if (HudTheme.Default) return HudTheme.Default;
                if (!fallbackTheme) { fallbackTheme = ScriptableObject.CreateInstance<HudTheme>(); fallbackTheme.hideFlags = HideFlags.HideAndDontSave; }
                return fallbackTheme;
            }
        }

        private HudEffects Profile
        {
            get
            {
                if (effects) return effects;
                if (HudEffects.Default) return HudEffects.Default;
                if (!fallbackEffects) { fallbackEffects = ScriptableObject.CreateInstance<HudEffects>(); fallbackEffects.hideFlags = HideFlags.HideAndDontSave; }
                return fallbackEffects;
            }
        }

        private void Restyle()
        {
            restyle = false; lastTheme = Palette; lastEffects = Profile;
            View.Apply(lastTheme, lastEffects); themeRevision = lastTheme.Revision; effectsRevision = lastEffects.Revision;
        }

        private void Build(VisualElement root)
        {
            ReleaseView(); panelRoot = root;
            if (root == null) return;
            View = new HudView(styles); root.Add(View.Root); View.Root.dataSource = Model;
            if (layout) layout.CloneTree(View.Content);
            else if (showGallery) HudGallery.Build(View, Model);
            Restyle();
            Ready?.Invoke(View);
            View?.RefreshTheme();
        }

        private void ReleaseView()
        {
            View?.Dispose(); View = null; themeRevision = effectsRevision = -1; lastTheme = null; lastEffects = null;
        }

        private static void DestroyOwned(UnityEngine.Object asset)
        {
            if (!asset) return;
            if (Application.isPlaying) Destroy(asset); else DestroyImmediate(asset);
        }
    }
}
