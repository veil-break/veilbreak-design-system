using Veilbreak.Gui;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// Animates the difference between two builds of the same UI, for screens that are rebuilt on every change. Capture the
    /// tree before tearing it down, build the new one, then Play: elements are matched by their path of names (or type and
    /// sibling index), and a changed class list replays from the old classes one frame later (so USS transitions and painted
    /// controls ease into the new state), a changed number counts to its new value, other changed text fades in (labels with
    /// the <see cref="Decode"/> class decode left to right instead), a resized bar grows, and an element that is new or newly
    /// shown fades in. Only named elements slide when they move. Removed elements simply go. Reduced motion keeps colour and
    /// opacity changes only. Labels with a data binding are left to their binding. The engine is <see cref="VbRebuild"/> in
    /// com.veilbreak.gui; this class keeps the HUD's class names (<see cref="Skip"/>, <see cref="Decode"/>).
    /// </summary>
    public static class HudMotion
    {
        public const string Skip = "ar-no-motion", Decode = "ar-decode";
        public const float Duration = VbRebuild.Duration;

        public sealed class Snapshot
        {
            internal readonly RebuildSnapshot inner;
            internal Snapshot(RebuildSnapshot inner) => this.inner = inner;
            public int Count => inner.Count;
            public bool Contains(string key) => inner.Contains(key);
        }

        public static Snapshot Capture(VisualElement scope) => new Snapshot(VbRebuild.Capture(scope, Skip));

        public static void Play(VisualElement scope, Snapshot before, bool reduced)
        {
            if (before != null) VbRebuild.Play(scope, before.inner, reduced, Decode);
        }
    }
}
