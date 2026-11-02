using System;
using UnityEngine.UIElements;
using Veilbreak.Gui;

namespace Veilbreak.ARHud
{
    // Pointer feedback for every Button on a panel: a hover/focus glow that traces the button's own outline (its HudShape chamfer when
    // it has one) and a small press sink. The behaviour is gui's VbPressGlow; this adapter gives it the HUD's vocabulary: the `--neon`
    // custom property, HudNavigation's `kbd` class (keyboard and gamepad focus light their button once the focus frames are on) and
    // HudFx.Reduced (keeps the glow, drops the sink). HudNavigation hooks it once on the panel's visual tree, so screen rebuilds never
    // drop it, and runs it while the navigation is Enabled.
    public sealed class HudPress : IDisposable
    {
        private readonly VbPressGlow glow;

        public HudPress(VisualElement tree) =>
            glow = new VbPressGlow(tree, new VbPressGlowOptions { NeonProperty = "--neon", KeyboardClass = HudNavigation.KeyboardClass, Still = () => HudFx.Reduced });

        public void Dispose() => glow.Dispose();
    }
}
