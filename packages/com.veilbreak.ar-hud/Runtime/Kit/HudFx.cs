using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // UI Toolkit effects: staggered screen entry, a glitch hand-off, CTA sheen and pulse halos, spark bursts, shake, flash and
    // slam titles, all driven by one tween helper on the element scheduler (unscaled time). Reduced (the player's reduced
    // motion) turns movement into plain fades and also stills HudPress and HudVisor; NoFlash (a "disable flashes" setting)
    // caps flashes to a soft glow. Styles: Base.uss (fx-*). Board children with the Static class (a screen's corner chrome)
    // or HudNavigation.Modal (scrims) never fade in.
    public static class HudFx
    {
        public const string Static = "fx-static";
        public static bool Reduced, NoFlash;
        private static Texture2D glow, sheen, spark;
        private static readonly System.Random random = new System.Random();

        public static Texture2D Glow => glow != null ? glow : glow = Make(64, 64, (x, y) => Mathf.Pow(Mathf.Clamp01(1 - new Vector2(x - .5f, y - .5f).magnitude * 2), 2));
        private static Texture2D Sheen => sheen != null ? sheen : sheen = Make(64, 4, (x, _) => Mathf.Pow(Mathf.Clamp01(1 - Mathf.Abs(x - .5f) * 2), 2.5f));
        private static Texture2D Spark => spark != null ? spark : spark = Make(32, 32, (x, y) =>
        {
            float dx = Mathf.Abs(x - .5f) * 2, dy = Mathf.Abs(y - .5f) * 2;
            return Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(1 - dx), 6) * Mathf.Clamp01(1 - dy * 4) + Mathf.Pow(Mathf.Clamp01(1 - dy), 6) * Mathf.Clamp01(1 - dx * 4) + Mathf.Pow(Mathf.Clamp01(1 - new Vector2(dx, dy).magnitude), 3));
        });
        private static Texture2D Make(int w, int h, Func<float, float, float> alpha)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var px = new Color[w * h];
            for (var y = 0; y < h; y++) for (var x = 0; x < w; x++) px[y * w + x] = new Color(1, 1, 1, alpha((x + .5f) / w, (y + .5f) / h));
            tex.SetPixels(px); tex.Apply(); return tex;
        }

        public static float OutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
        public static float OutBack(float t) { const float c = 1.70158f; t -= 1; return 1 + (c + 1) * t * t * t + c * t * t; }
        private static float Range(float a, float b) => a + (float)random.NextDouble() * (b - a);

        // Runs apply(0..1) every frame from `delay` for `duration` seconds, then done. Pausing the returned item cancels it.
        public static IVisualElementScheduledItem Tween(VisualElement el, float delay, float duration, Action<float> apply, Action done = null)
        {
            var start = Time.unscaledTime + delay; var finished = false; IVisualElementScheduledItem item = null;
            item = el.schedule.Execute(() =>
            {
                if (finished) return;
                var now = Time.unscaledTime; if (now < start) return;
                var t = duration <= 0 ? 1 : Mathf.Clamp01((now - start) / duration);
                apply(t);
                if (t >= 1) { finished = true; item.Pause(); done?.Invoke(); }
            }).Every(0);
            return item;
        }

        // A new screen's placed blocks rise a little and fade in, one shortly after another.
        public static void Enter(VisualElement board)
        {
            var i = 0;
            foreach (var child in board.Children())
            {
                if (child.ClassListContains(HudNavigation.Modal) || child.ClassListContains(Static)) continue; // static chrome (the screen's corner cuts) never fades in
                // One short beat per block (dur-ui), capped so a busy screen is settled in under half a second.
                var el = child; var delay = Mathf.Min(i++ * .03f, .15f); el.style.opacity = 0;
                Tween(el, delay, Reduced ? .2f : .28f, t =>
                {
                    el.style.opacity = Reduced ? t : Mathf.Clamp01(t * 1.4f);
                    if (!Reduced) el.style.translate = new Translate(0, (1 - OutCubic(t)) * 8);
                }, () => { el.style.opacity = StyleKeyword.Null; el.style.translate = StyleKeyword.Null; });
            }
        }

        // A light band sweeps across the button every few seconds.
        public static void AddSheen(VisualElement button, float every = 3.6f)
        {
            if (Reduced) return;
            var clip = new VisualElement { pickingMode = PickingMode.Ignore }; clip.AddToClassList("fx-clip");
            var band = new VisualElement { pickingMode = PickingMode.Ignore }; band.AddToClassList("fx-sheen");
            band.style.backgroundImage = Sheen; clip.Add(band); button.Add(clip);
            button.schedule.Execute(() => { if (button.enabledInHierarchy) Tween(band, 0, .85f, t => band.style.left = Length.Percent(-60 + 190 * OutCubic(t))); })
                .Every((long)(every * 1000)).StartingIn((long)Range(300, 1800));
        }

        // A breathing coloured halo behind an element (a sibling, so the element's own paint stays on top).
        public static void Pulse(VisualElement el, Color color)
        {
            var halo = new VisualElement { pickingMode = PickingMode.Ignore }; halo.AddToClassList("fx-halo");
            halo.style.backgroundImage = Glow; halo.style.unityBackgroundImageTintColor = color;
            el.RegisterCallback<AttachToPanelEvent>(_ => { if (halo.parent == null && el.parent != null) el.parent.Insert(el.parent.IndexOf(el), halo); });
            el.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var r = el.layout; halo.style.left = r.x - 26; halo.style.top = r.y - 22; halo.style.width = r.width + 52; halo.style.height = r.height + 44;
            });
            el.schedule.Execute(() =>
            {
                var a = Reduced ? .45f : .3f + .35f * (.5f + .5f * Mathf.Sin(Time.unscaledTime * 3.4f));
                halo.style.opacity = el.enabledInHierarchy ? a : 0;
            }).Every(0);
        }

        // Sparks fly out from a point (layer-local coordinates) with drag and gravity.
        public static void Burst(VisualElement layer, Vector2 at, Color color, int count, float speed = 420)
        {
            if (Reduced) return;
            var parts = new List<(VisualElement e, Vector2 p, Vector2 v, float life, float spin, float size)>();
            for (var i = 0; i < count; i++)
            {
                var e = new VisualElement { pickingMode = PickingMode.Ignore }; e.AddToClassList("fx-spark");
                e.style.backgroundImage = i % 3 == 0 ? Spark : Glow; e.style.unityBackgroundImageTintColor = Color.Lerp(color, Color.white, Range(0, .45f));
                var a = Range(0, Mathf.PI * 2); var v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed * Range(.25f, 1);
                parts.Add((e, at, v, Range(.5f, 1.15f), Range(-360, 360), Range(5, i % 3 == 0 ? 18 : 10))); layer.Add(e);
            }
            var start = Time.unscaledTime; var last = start; IVisualElementScheduledItem item = null;
            item = layer.schedule.Execute(() =>
            {
                var now = Time.unscaledTime; var dt = Mathf.Min(.05f, now - last); last = now; var alive = false;
                for (var i = 0; i < parts.Count; i++)
                {
                    var (e, p, v, life, spin, size) = parts[i]; var age = now - start;
                    if (age >= life) { e.RemoveFromHierarchy(); continue; }
                    alive = true; v *= Mathf.Exp(-2.4f * dt); v.y += 300 * dt; p += v * dt; parts[i] = (e, p, v, life, spin, size);
                    var k = 1 - age / life;
                    e.style.left = p.x - size / 2; e.style.top = p.y - size / 2; e.style.width = size; e.style.height = size;
                    e.style.opacity = k; e.style.rotate = new Rotate(spin * age);
                }
                if (!alive) item.Pause();
            }).Every(0);
        }

        public static void Shake(VisualElement el, float amplitude = 9, float duration = .45f)
        {
            if (Reduced) return;
            Tween(el, 0, duration, t => { var k = (1 - t) * amplitude; el.style.translate = t >= 1 ? new Translate(0, 0) : new Translate(Range(-k, k), Range(-k, k)); });
        }

        // A screen hand-off after a failing CRT picture: the board's blocks flicker in through a few frames of
        // sideways jumps under sliding scan bands and dropped macroblocks (small squares in swapped holo colours), settling
        // within ~0.5 s. Reduced motion fades instead; NoFlash keeps the bars faint.
        public static void Glitch(VisualElement board)
        {
            var blocks = new List<VisualElement>();
            foreach (var child in board.Children()) if (!child.ClassListContains(HudNavigation.Modal) && !child.ClassListContains(Static)) blocks.Add(child);
            if (Reduced) { foreach (var b in blocks) { var el = b; el.style.opacity = 0; Tween(el, 0, .2f, t => el.style.opacity = t); } return; }
            var veil = new VisualElement { pickingMode = PickingMode.Ignore }; veil.AddToClassList("fx-glitch"); board.Add(veil);
            var bars = new List<VisualElement>();
            for (var i = 0; i < 7; i++) { var bar = new VisualElement { pickingMode = PickingMode.Ignore }; bar.AddToClassList("fx-glitch-bar"); veil.Add(bar); bars.Add(bar); }
            var squares = new List<VisualElement>();
            for (var i = 0; i < 14; i++) { var sq = new VisualElement { pickingMode = PickingMode.Ignore }; sq.AddToClassList("fx-glitch-bar"); veil.Add(sq); squares.Add(sq); }
            Color teal = new Color(.24f, .91f, .75f), pink = new Color(.93f, .3f, 1);
            var frame = -1;
            Color violet = new Color(.55f, .4f, 1);
            Tween(board, 0, .5f, t =>
            {
                // Re-roll the tear about every other frame, so it stutters like a signal rather than animating smoothly.
                var step = Mathf.FloorToInt(t * 14); if (step == frame) return; frame = step;
                var calm = OutCubic(t);
                foreach (var b in blocks)
                {
                    b.style.opacity = Range(0, 1) < .35f + calm * .65f ? 1 : Range(0, .3f);
                    b.style.translate = new Translate(Range(-1, 1) * 14 * (1 - calm), 0);
                }
                foreach (var bar in bars)
                {
                    var on = Range(0, 1) > .3f + calm * .6f; bar.style.display = on ? DisplayStyle.Flex : DisplayStyle.None; if (!on) continue;
                    var c = Range(0, 1) < .5f ? teal : pink;
                    bar.style.top = Length.Percent(Range(0, 98)); bar.style.height = Range(1, 9);
                    bar.style.left = Length.Percent(Range(-20, 30)); bar.style.width = Length.Percent(Range(40, 110));
                    bar.style.backgroundColor = new Color(c.r, c.g, c.b, (NoFlash ? .08f : .28f) * (1 - calm));
                }
                foreach (var sq in squares)
                {
                    var on = Range(0, 1) > .45f + calm * .55f; sq.style.display = on ? DisplayStyle.Flex : DisplayStyle.None; if (!on) continue;
                    var c = Range(0, 1) < .33f ? teal : Range(0, 1) < .5f ? pink : violet; var size = Range(8, 26);
                    sq.style.left = Length.Percent(Range(0, 97)); sq.style.top = Length.Percent(Range(0, 96)); sq.style.width = size * Range(1, 2.5f); sq.style.height = size;
                    sq.style.backgroundColor = new Color(c.g, c.b, c.r, (NoFlash ? .1f : .45f) * (1 - calm));
                }
            }, () =>
            {
                veil.RemoveFromHierarchy();
                foreach (var b in blocks) { b.style.opacity = StyleKeyword.Null; b.style.translate = StyleKeyword.Null; }
            });
        }

        // Full-layer colour flash; with flashes disabled it becomes a slow, faint glow.
        public static void Flash(VisualElement layer, Color color, float peak = .7f)
        {
            var f = new VisualElement { pickingMode = PickingMode.Ignore }; f.AddToClassList("fx-flash");
            f.style.backgroundImage = Glow; f.style.unityBackgroundImageTintColor = color; f.style.backgroundColor = new Color(color.r, color.g, color.b, .35f);
            layer.Add(f);
            var max = NoFlash ? Mathf.Min(peak, .3f) : peak; var length = NoFlash ? 1.1f : .65f;
            Tween(f, 0, length, t => f.style.opacity = max * (t < .12f ? t / .12f : 1 - OutCubic((t - .12f) / .88f)), f.RemoveFromHierarchy);
        }

        // A big title that slams in, holds and fades (the kit's `disp` face).
        public static void Slam(VisualElement layer, string text, Color color)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore }; label.AddToClassList("disp"); label.AddToClassList("fx-slam");
            label.style.color = color; layer.Add(label);
            Tween(label, 0, 1.6f, t =>
            {
                var s = Reduced ? 1 : t < .2f ? Mathf.Lerp(2.4f, 1, OutBack(t / .2f)) : 1 + (t - .2f) * .06f;
                label.style.scale = new Scale(new Vector2(s, s));
                label.style.opacity = t < .12f ? t / .12f : t > .75f ? 1 - (t - .75f) / .25f : 1;
            }, label.RemoveFromHierarchy);
        }
    }
}
