using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Veilbreak.Rendering.Tests
{
    public sealed class RenderingTests
    {
        private float barrel;
        private int firstQuality;

        [SetUp]
        public void Remember() { barrel = VisorLens.Barrel; firstQuality = VisorLens.FirstQuality; }

        [TearDown]
        public void Restore()
        {
            VisorLens.Barrel = barrel; VisorLens.FirstQuality = firstQuality;
            ScreenGlitch.Still = false; ScreenGlitch.Clear();
        }

        // The barrel keeps the corners, magnifies the centre by 1 + k, inverts through Shown and is the identity at 0.
        [Test]
        public void VisorLensIsOneInvertibleMapping()
        {
            var size = new Vector2(1920, 1080); const float k = .1f;
            foreach (var corner in new[] { Vector2.zero, size, new Vector2(size.x, 0), new Vector2(0, size.y) })
                Assert.That(Vector2.Distance(VisorLens.Source(corner, size, k), corner), Is.LessThan(1e-3f), "corner " + corner);
            var near = VisorLens.Source(size / 2 + new Vector2(10, 0), size, k) - size / 2;
            Assert.That(near.x, Is.EqualTo(10 / (1 + k)).Within(1e-3f), "centre magnification");
            foreach (var p in new[] { new Vector2(100, 900), new Vector2(1700, 120), new Vector2(960, 40), new Vector2(30, 540) })
                Assert.That(Vector2.Distance(VisorLens.Shown(VisorLens.Source(p, size, k), size, k), p), Is.LessThan(.05f), "round trip " + p);
            Assert.That(VisorLens.Source(new Vector2(30, 540), size, 0), Is.EqualTo(new Vector2(30, 540)), "identity when off");
        }

        // The host's tier gate: below FirstQuality the lens is off whatever the barrel.
        [Test]
        public void VisorLensFollowsTheHostConfiguration()
        {
            VisorLens.Barrel = 0; VisorLens.FirstQuality = 0;
            Assert.That(VisorLens.Active, Is.False, "barrel 0 is the identity");
            VisorLens.Barrel = .1f;
            Assert.That(VisorLens.Strength, Is.EqualTo(.1f), "on from the first quality level");
            VisorLens.FirstQuality = QualitySettings.GetQualityLevel() + 1;
            Assert.That(VisorLens.Strength, Is.EqualTo(0), "flat below FirstQuality");
        }

        // A kick decays to nothing, reports every step through Changed, and reduced motion holds it at zero.
        [Test]
        public void ScreenGlitchDecaysAndReportsItsAmount()
        {
            var reported = -1f;
            void Report(float amount) => reported = amount;
            ScreenGlitch.Changed += Report;
            try
            {
                ScreenGlitch.Kick(2);
                Assert.That(ScreenGlitch.Amount, Is.EqualTo(1), "kicks clamp to 1");
                ScreenGlitch.Tick(.1f);
                Assert.That(reported, Is.EqualTo(ScreenGlitch.Amount).And.LessThan(1).And.GreaterThan(0), "decays and reports");
                for (var i = 0; i < 20; i++) ScreenGlitch.Tick(.1f);
                Assert.That(reported, Is.EqualTo(0), "settles within two seconds");
                ScreenGlitch.Still = true; ScreenGlitch.Kick(1);
                Assert.That(ScreenGlitch.Amount, Is.EqualTo(0), "reduced motion ignores kicks");
            }
            finally { ScreenGlitch.Changed -= Report; }
        }

        // The overrides stay inactive at their defaults, so a profile that carries them draws nothing until raised.
        [Test]
        public void OverridesAreInactiveByDefault()
        {
            var streaks = ScriptableObject.CreateInstance<LensStreaks>();
            var glitch = ScriptableObject.CreateInstance<Glitch>();
            try
            {
                Assert.That(streaks.IsActive(), Is.False);
                Assert.That(glitch.IsActive(), Is.False);
                Assert.That(Shader.Find("Hidden/Veilbreak/Streaks"), Is.Not.Null);
                Assert.That(Shader.Find("Veilbreak/Glitch"), Is.Not.Null);
                Assert.That(Shader.Find("Hidden/Veilbreak/VisorLens"), Is.Not.Null);
            }
            finally { Object.DestroyImmediate(streaks); Object.DestroyImmediate(glitch); }
        }
    }
}
