using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>Non-interactive vector decoration; never distorts hit targets or text. HudVisor hosts one per stage.</summary>
    public sealed class HudChrome : VisualElement
    {
        private HudTheme theme;
        private HudEffects effects;
        // Runtime state laid over the profile without writing the shared asset: the player's reduced motion and the live interference.
        private bool? reducedOverride;
        private float? glitchOverride;
        private bool Reduced => reducedOverride ?? effects.reducedMotion;
        private float GlitchAmount => glitchOverride ?? effects.glitch;
        private readonly IVisualElementScheduledItem timer;
        private Vector2 pointer;
        private float phase;
        public HudChrome()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("ar-chrome");
            generateVisualContent += Draw;
            timer = schedule.Execute(Tick).Every(33);
            timer.Pause();
            RegisterCallback<AttachToPanelEvent>(_ => ConfigureTimer());
            RegisterCallback<DetachFromPanelEvent>(_ => timer.Pause());
        }
        public void Configure(HudTheme palette, HudEffects profile)
        {
            theme = palette; effects = profile; ConfigureTimer(); MarkDirtyRepaint();
        }
        /// <summary>Reduced motion and interference for this chrome only (null: the profile's own values); the asset is never written.</summary>
        public void Override(bool? reducedMotion, float? glitch)
        {
            reducedOverride = reducedMotion; glitchOverride = glitch; ConfigureTimer(); MarkDirtyRepaint();
        }
        public void SetPointer(Vector2 normalized)
        {
            pointer = new Vector2(Mathf.Clamp(normalized.x, -1, 1), Mathf.Clamp(normalized.y, -1, 1));
            if (effects && !Reduced && theme && !theme.highContrast) MarkDirtyRepaint();
        }
        private void ConfigureTimer()
        {
            timer.Pause();
            if (panel == null || !effects || !theme || Reduced || theme.highContrast) return;
            if (effects.pulse == 0 && effects.sweep == 0 && GlitchAmount == 0) return;
            timer.Every(1000 / Mathf.Clamp(effects.framesPerSecond, 10, 60)); timer.Resume();
        }
        private void Tick() { phase = Time.realtimeSinceStartup; MarkDirtyRepaint(); }
        private void Draw(MeshGenerationContext ctx)
        {
            if (!theme || !effects || theme.highContrast) return;
            float w = contentRect.width, h = contentRect.height;
            if (w < 20 || h < 20) return;
            var p = ctx.painter2D;
            bool moving = !Reduced;
            var offset = moving ? pointer * effects.parallax : Vector2.zero;
            float alpha = Mathf.Clamp01(effects.edgeOpacity * (1 - (moving ? effects.pulse * (.5f + .5f * Mathf.Sin(phase * 2)) : 0)));
            for (int pass = 2; pass >= 0; pass--)
            {
                float opacity = pass == 0 ? alpha : alpha * effects.glow * .12f;
                p.lineWidth = pass == 0 ? theme.lineWidth + 1 : 3 + pass * 4;
                p.strokeColor = HudPaint.Alpha(theme.accent, opacity);
                for (int edge = 0; edge < 2; edge++)
                {
                    float y = edge == 0 ? 16 : h - 16;
                    float bend = (edge == 0 ? 1 : -1) * effects.curvature * h * .13f;
                    p.BeginPath(); p.MoveTo(new Vector2(24, y + bend) + offset);
                    p.BezierCurveTo(new Vector2(w * .25f, y) + offset, new Vector2(w * .75f, y) + offset,
                        new Vector2(w - 24, y + bend) + offset); p.Stroke();
                }
            }
            // Side brackets complete the visor silhouette without covering the sightline.
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? 18 : w - 18;
                float direction = side == 0 ? 1 : -1;
                p.lineWidth = theme.lineWidth;
                p.strokeColor = HudPaint.Alpha(theme.accent, alpha * .6f);
                p.BeginPath(); p.MoveTo(new Vector2(x + direction * 20, h * .18f) + offset);
                p.LineTo(new Vector2(x, h * .24f) + offset);
                p.LineTo(new Vector2(x, h * .76f) + offset);
                p.LineTo(new Vector2(x + direction * 20, h * .82f) + offset); p.Stroke();
                for (int tick = 0; tick < 9; tick++)
                {
                    float y = h * (.38f + tick * .03f);
                    HudPaint.Line(p, new Vector2(x, y) + offset,
                        new Vector2(x + direction * (tick == 4 ? 12 : 5), y) + offset,
                        HudPaint.Alpha(theme.accent, alpha * .6f));
                }
            }
            // The top tick ruler follows the edge opacity like the rest of the chrome (at 0 the chrome is gone entirely).
            for (int i = 0; i < 17 && alpha > 0; i++)
            {
                float x = w * (.25f + i * .03125f);
                HudPaint.Line(p, new Vector2(x, 28), new Vector2(x, i % 4 == 0 ? 38 : 33), HudPaint.Alpha(theme.accent, alpha * .6f));
            }
            for (float y = 0; y < h; y += Mathf.Max(4, effects.scanlineSpacing))
                HudPaint.Line(p, new Vector2(0, y), new Vector2(w, y), HudPaint.Alpha(theme.accent, effects.scanlineOpacity));
            if (!moving) return;
            float sweepY = (phase * 35) % h;
            if (effects.sweep > 0) HudPaint.Line(p, new Vector2(0, sweepY), new Vector2(w, sweepY), HudPaint.Alpha(theme.accent, effects.sweep), 2);
            // Local edge interference, never full-screen flashes or text jitter.
            if (GlitchAmount > 0 && Mathf.Sin(phase * 7) > .97f)
                HudPaint.Line(p, new Vector2(24, h * .72f), new Vector2(w * .15f, h * .72f), HudPaint.Alpha(theme.accent, GlitchAmount), 3);
        }
    }
}
