using System;
using UnityEngine;

namespace Veilbreak.Rendering
{
    // The transient glitch: big moments kick it and it falls back to nothing within a fraction of a second. The host turns the
    // amount into the weight of a Volume that holds a Glitch override (Changed), so ScreenGlitchFeature draws it through the
    // Volume stack, and anything else that follows the glitch reads Amount. Reduced motion (Still) holds it at 0; with flashes
    // disabled (NoFlashes) ScreenGlitchFeature skips the pass altogether.
    public static class ScreenGlitch
    {
        private static float amount;
        public static bool Still, NoFlashes;
        public static float Amount => amount;

        // Raised with the amount on every Tick and Clear; the host sets its glitch volume's weight from it.
        public static event Action<float> Changed;

        public static void Kick(float strength) { if (!Still) amount = Mathf.Max(amount, Mathf.Clamp01(strength)); }

        public static void Tick(float dt)
        {
            amount = Still ? 0 : Mathf.MoveTowards(amount, 0, dt * (1.6f + amount * 2.5f));
            Changed?.Invoke(amount);
        }

        public static void Clear() { amount = 0; Changed?.Invoke(0); }
    }
}
