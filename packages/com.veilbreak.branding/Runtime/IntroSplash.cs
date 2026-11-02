using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Veilbreak.Gui;




namespace Veilbreak.Branding
{
    // Boot intro that replaces Unity's splash (assets/splash-animation, "Veilbreak Splash v2"): VEIL/BREAK fades in while a
    // hexagon draws clockwise behind it, the blade whips in and locks through the L and B, then the impact (hit-stop, an inverted
    // frame, the world splitting along the cut, a light beam, shockwave hexes, sparks) breaks the mark apart; it holds, then glitches
    // out with an RGB split and tears. The slicing sound (Resources/VeilbreakSplashSlice.wav, Tools~/splash_sound.py) starts
    // SliceLead seconds early so its cut lands on the impact; it never plays when the strike is skipped, pinned or reduced. Drawn with Painter2D in the design's 1920 × 1080 space (covering the screen, centred); the SVG clip paths
    // become half-plane polygon clipping. A tap or any key (or gamepad button) skips to the exit; those inputs are consumed, so a key
    // pressed during the intro never reaches the menu behind it. Reduced motion shows the broken mark without the strike and fades out.
    public sealed class IntroSplash
    {
        // Scenes (authored starts): Veil 1 s, Strike .18 s, Break 1.3 s, Hold 1.3 s, Exit .55 s.
        private const float Strike = 1f, Break = 1.18f, Hold = 2.48f, Exit = 3.78f, Total = 4.33f, Fade = .4f;
        private static readonly Color Ground = VbTokens.SplashGround, Ink = VbTokens.Text, Pink = VbTokens.SignalPink, Teal = VbTokens.Ally;
        private static readonly Color Solid = Ink; // the design's chosen blade colour ("ink")

        private readonly VisualElement overlay, ambient, vignette, grain, grain2;
        private readonly IVisualElementScheduledItem ticker;
        private readonly bool reduced;
        private readonly float volume;
        private Action ended;
        private bool sliced;
        private float start = -1;
        private readonly float seed = UnityEngine.Random.Range(0, 1000);
        private float shift, t;

        // True while the intro is up (attached to a panel; the menu's navigation stands by while its own-input modal is open). Tied to the live overlay, so a
        // play session stopped mid-intro cannot leave it stuck when domain reload is off.
        private static IntroSplash current;
        public static bool Active => current != null && current.overlay.panel != null;
        private VisualElement tree;
        // Draws the intro over `host` (a host with navigation opens `host` as a modal with its own input, so its navigation stands by)
        // and calls `ended` once when the intro finishes, is skipped through, stopped or detached, so the host can close that modal.
        public static void Play(VisualElement host, bool reducedMotion, float volume = 1, Action ended = null) { Stop(); new IntroSplash(host, reducedMotion, volume, ended); }
        // Seconds of the slice clip before its cut (keep equal to SLICE_LEAD in Tools~/splash_sound.py).
        public const float SliceLead = .24f;
        // The cut's sound: the package's clip unless a host supplies its own (null: silent).
        private static AudioClip slice; private static bool sliceLoaded;
        public static AudioClip SliceSound { get { if (!sliceLoaded) { sliceLoaded = true; slice = Resources.Load<AudioClip>("VeilbreakSplashSlice"); } return slice; } set { sliceLoaded = true; slice = value; } }
        // The moment (intro seconds) the slice clip starts: SliceLead before the blade locks and the mark breaks.
        public static float SliceStart => Break - SliceLead;
        // Editor captures (PlayCapture) pin the intro's clock to one moment of the timeline; null runs it in real time.
        public static float? Pinned { get; set; }
        public static void Stop(){var splash=current;if(splash==null)return;splash.ticker.Pause();splash.End();splash.overlay.RemoveFromHierarchy();}

        private IntroSplash(VisualElement host, bool reducedMotion, float volume, Action ended)
        {
            reduced = reducedMotion; this.volume = Mathf.Clamp01(volume); this.ended = ended;
            overlay = new VisualElement { name = "intro-splash" }; overlay.AddToClassList("intro"); Fill(overlay);
            overlay.style.backgroundColor = Ground;
            overlay.Add(ambient = Layer(Radial(false))); ambient.style.unityBackgroundImageTintColor = Solid;
            var canvas = Layer(null); canvas.generateVisualContent += Draw; overlay.Add(canvas);
            overlay.Add(vignette = Layer(Radial(true)));
            // Film grain: two layers of the noise tile at unrelated sizes, mirrored and re-rolled independently every frame, so no
            // tile or drift pattern reads; faint, so it textures the ground without showing as noise.
            overlay.Add(grain = Layer(Noise())); grain.style.opacity = .02f;
            overlay.Add(grain2 = Layer(Noise())); grain2.style.opacity = .016f; grain2.style.scale = new Scale(new Vector3(-1, 1, 1));
            foreach (var g in new[] { grain, grain2 }) g.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
            overlay.RegisterCallback<PointerDownEvent>(_ => SkipToExit());
            overlay.RegisterCallback<DetachFromPanelEvent>(_ => End());
            current = this; host.Add(overlay);overlay.focusable=true;overlay.Focus();
            // With nothing focused, keyboard and navigation events go to the panel's root, so listen there until the intro ends.
            if (overlay.panel != null) Listen(overlay.panel); else overlay.RegisterCallbackOnce<AttachToPanelEvent>(e => { if (current == this) Listen(e.destinationPanel); });
            ticker = overlay.schedule.Execute(() => Tick(canvas)).Every(16);
        }

        private static VisualElement Layer(Texture2D image)
        {
            var layer = new VisualElement(); layer.AddToClassList("intro-layer"); Fill(layer); layer.pickingMode = PickingMode.Ignore;
            if (image != null) layer.style.backgroundImage = image;
            return layer;
        }

        // Root siblings cannot depend on a cloned screen's stylesheet scope.
        private static void Fill(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left = element.style.right = element.style.top = element.style.bottom = 0;
        }

        private void Listen(IPanel panel)
        {
            tree = panel.visualTree;
            tree.RegisterCallback<KeyDownEvent>(Skip, TrickleDown.TrickleDown); tree.RegisterCallback<NavigationMoveEvent>(Skip, TrickleDown.TrickleDown);
            tree.RegisterCallback<NavigationSubmitEvent>(Skip, TrickleDown.TrickleDown); tree.RegisterCallback<NavigationCancelEvent>(Skip, TrickleDown.TrickleDown);
        }

        private void Skip(EventBase e) { SkipToExit(); e.StopPropagation(); tree?.panel?.focusController?.IgnoreEvent(e); }
        private void SkipToExit() { if (t < Exit) shift += Exit - t; }

        // Starts the slicing sound once, as the blade whips in; a skip past the cut, a pinned capture or Reduced Motion (no strike) stays silent.
        private void Slice()
        {
            if (sliced || t < SliceStart) return;
            sliced = true;
            if (Pinned != null || reduced || t > Break || volume <= 0) return;
            var clip = SliceSound; if (clip == null) return;
            var voice = new GameObject("Veilbreak splash slice") { hideFlags = HideFlags.HideAndDontSave };
            var source = voice.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0; source.ignoreListenerPause = true;
            source.PlayOneShot(clip, volume); UnityEngine.Object.Destroy(voice, clip.length + .1f);
        }

        private void End()
        {
            if (current == this) current = null;
            var done = ended; ended = null; done?.Invoke();
            if (tree == null) return;
            tree.UnregisterCallback<KeyDownEvent>(Skip, TrickleDown.TrickleDown); tree.UnregisterCallback<NavigationMoveEvent>(Skip, TrickleDown.TrickleDown);
            tree.UnregisterCallback<NavigationSubmitEvent>(Skip, TrickleDown.TrickleDown); tree.UnregisterCallback<NavigationCancelEvent>(Skip, TrickleDown.TrickleDown);
            tree = null;
        }

        private void Tick(VisualElement canvas)
        {
            overlay.BringToFront();
            var rect = canvas.contentRect;
            if (!(rect.width > 0 && rect.height > 0)) return;
            if (start < 0) start = Time.unscaledTime;
            if (UnityEngine.InputSystem.Keyboard.current?.anyKey.wasPressedThisFrame == true) SkipToExit(); // any key, not only navigation ones
            t = Pinned ?? Time.unscaledTime - start + shift;
            if (Pinned == null && t >= (reduced ? Exit + Fade : Total)) { ticker.Pause(); End(); overlay.RemoveFromHierarchy(); return; }
            Slice();
            Layout(rect);
            canvas.MarkDirtyRepaint();
        }

        // ---------- the mark: part geometry from the design (logo units), every keyframe pose a pure translation ----------
        private sealed class Part { public string id; public bool blade; public Vector2[][] rings; public Func<float, float> fx, fy; }


        // Keyframe poses: start, 25 %, 50 % (blade locked), Artboard-4 (broken); the blade starts 300 units up its own line.
        private static readonly Vector2 K25Text = new Vector2(0, -8.23f), K25Blade = new Vector2(-77.997f, 108), K50Text = new Vector2(1.652f, -4.585f), K50Blade = new Vector2(-116.356f, 180);
        private static readonly Dictionary<string, Vector2> Broken = new Dictionary<string, Vector2>
        {
            ["V"] = new Vector2(-.207f, -2.297f), ["E1"] = new Vector2(-.207f, -2.297f), ["I"] = new Vector2(-.207f, -2.297f), ["Lstem"] = new Vector2(-.207f, -2.297f),
            ["Lfoot"] = new Vector2(-1.934f, .063f), ["Bstem"] = new Vector2(-8.793f, 18.503f),
            ["Bbowl"] = new Vector2(.207f, 13.519f), ["R"] = new Vector2(.207f, 13.519f), ["E2"] = new Vector2(.207f, 13.519f), ["A"] = new Vector2(.207f, 13.519f), ["K"] = new Vector2(.207f, 13.519f),
            ["s1"] = new Vector2(-144.207f, 234.288f), ["s2"] = new Vector2(-137.518f, 222.176f), ["s3"] = new Vector2(-147.201f, 222.997f), ["s4"] = new Vector2(-147.201f, 222.997f),
        };
        private const float Pre = 300;
        private static readonly Vector2 Up = new Vector2(.5855f, -.8107f);
        private static readonly float[] Knots = { 0, Pre, Pre + 133.2f, Pre + 214.78f, Pre + 268.3f };
        private static readonly float D = Knots[4], D50 = Knots[3];
        // Locked blade tips (logo units), the cut direction and its normal.
        private static readonly Vector2 TipB = new Vector2(203.29f, 448.291f), TipT = new Vector2(335.296f, 225), U = new Vector2(.5089f, -.8608f), N = new Vector2(.8608f, .5089f);

        private static Part[] parts;
        private static Part[] Parts => parts ??= MarkGeometry.Parts.Select(p =>
        {
            var keys = new[] { p.blade ? Up * Pre : Vector2.zero, Vector2.zero, p.blade ? K25Blade : K25Text, p.blade ? K50Blade : K50Text, Broken[p.id] };
            return new Part { id = p.id, blade = p.blade, rings = MarkGeometry.Rings(p.d), fx = Monotone(Knots, keys.Select(k => k.x).ToArray()), fy = Monotone(Knots, keys.Select(k => k.y).ToArray()) };
        }).ToArray();

        // Monotone cubic Hermite (Fritsch–Carlson) through the keyframes, as in the design.
        private static Func<float, float> Monotone(float[] xs, float[] ys)
        {
            int n = xs.Length; var dx = new float[n - 1]; var s = new float[n - 1]; var m = new float[n];
            for (var i = 0; i < n - 1; i++) { dx[i] = xs[i + 1] - xs[i]; s[i] = (ys[i + 1] - ys[i]) / dx[i]; }
            m[0] = s[0]; m[n - 1] = s[n - 2];
            for (var i = 1; i < n - 1; i++) m[i] = s[i - 1] * s[i] <= 0 ? 0 : (s[i - 1] + s[i]) / 2;
            for (var i = 0; i < n - 1; i++)
            {
                if (s[i] == 0) { m[i] = 0; m[i + 1] = 0; continue; }
                float a = m[i] / s[i], b = m[i + 1] / s[i], h = a * a + b * b;
                if (h > 9) { var k = 3 / Mathf.Sqrt(h); m[i] = k * a * s[i]; m[i + 1] = k * b * s[i]; }
            }
            return x =>
            {
                if (x <= xs[0]) return ys[0];
                if (x >= xs[n - 1]) return ys[n - 1];
                var i = 0; while (x > xs[i + 1]) i++;
                float h = dx[i], u = (x - xs[i]) / h, u2 = u * u, u3 = u2 * u;
                return (2 * u3 - 3 * u2 + 1) * ys[i] + (u3 - 2 * u2 + u) * h * m[i] + (-2 * u3 + 3 * u2) * ys[i + 1] + (u3 - u2) * h * m[i + 1];
            };
        }

        // ---------- frame ----------
        private static float Rnd(float a, float b = 0, float c = 0) { var x = Math.Sin(a * 127.1 + b * 311.7 + c * 74.7) * 43758.5453; return (float)(x - Math.Floor(x)); }
        private static float Spread(float a, float b = 0, float c = 0) => Rnd(a, b, c) * 2 - 1;
        private static float OutCubic(float x) { x -= 1; return x * x * x + 1; }
        private static float OutExpo(float x) => x >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
        private static float InOutSine(float x) => -(Mathf.Cos(Mathf.PI * x) - 1) / 2;

        private static readonly (float t, Vector2 dir, float dist, float len, float delay, float life, int col)[] Sparks = Enumerable.Range(0, 44).Select(i =>
        {
            float side = i % 2 == 1 ? 1 : -1, ang = Spread(i, 3) * .75f, c = Mathf.Cos(ang), s = Mathf.Sin(ang), along = Spread(i, 4) * .5f;
            Vector2 b = N * side, dir = new Vector2(b.x * c - b.y * s, b.x * s + b.y * c) + U * along;
            return (.15f + Rnd(i, 1) * .75f, dir.normalized, 90 + Rnd(i, 5) * 420, 14 + Rnd(i, 6) * 46, Rnd(i, 7) * .05f, .45f + Rnd(i, 8) * .55f, i % 4);
        }).ToArray();

        private float DAt(float time)
        {
            if (reduced) return D;
            if (time <= Strike) return 0;
            if (time < Break) return D50 * (time - Strike) / (Break - Strike);
            const float hitStop = .06f;
            return D50 + (D - D50) * OutExpo(Mathf.Clamp01((time - Break - hitStop) / (Hold - Break - hitStop)));
        }

        // Per-frame state shared by Layout (the textured layers) and Draw (the canvas), in design units.
        private float age, shake, punch, burst, split, beam, amb, lean, flash, S, k; private bool hit, inv; private Vector2 shakeOffset, origin, hp;

        private void Layout(Rect rect)
        {
            age = t - Break; hit = !reduced && age >= 0; inv = hit && age < .045f;
            flash = hit && !inv ? .75f * Mathf.Exp(-(age - .045f) / .09f) : 0;
            shake = hit ? 18 * Mathf.Exp(-age / .14f) : 0; shakeOffset = new Vector2(shake * Mathf.Sin(age * 97), shake * .7f * Mathf.Sin(age * 73 + 1));
            punch = hit ? Mathf.Exp(-age / .18f) : 0; burst = hit ? Mathf.Exp(-age / .16f) : 0;
            split = hit ? 46 * Mathf.Exp(-age / .2f) : 0; beam = hit ? 14 * Mathf.Exp(-age / .12f) : 0; amb = hit ? .22f * Mathf.Exp(-age / .2f) : 0;
            lean = reduced || hit ? 0 : Mathf.Clamp01((t - (Strike - .35f)) / .35f); // the frame leans in just before the strike
            S = (1.8f + .15f * InOutSine(Mathf.Clamp01(t / Total))) * (1 + .07f * punch + .02f * lean);
            k = Mathf.Min(rect.width / 1050, rect.height / 1080); // cover; crop the rim, keep the mark centred
            origin = new Vector2((rect.width - 1920 * k) / 2, (rect.height - 1080 * k) / 2);
            var tb = Cam(TipB); var tt = Cam(TipT); hp = (tb + tt) / 2;

            ambient.style.display = amb > .01f && !inv ? DisplayStyle.Flex : DisplayStyle.None;
            if (amb > .01f) Place(ambient, hp, 620, amb);
            vignette.style.display = inv ? DisplayStyle.None : DisplayStyle.Flex;
            Place(vignette, new Vector2(960, 540), 1150 * 1.6f, .8f + .2f * lean);
            var frame = Mathf.Floor(t * 30) + seed;
            Roll(grain, frame, 0, 256); Roll(grain2, frame, 10, 181);
            if (reduced) overlay.style.opacity = 1 - Mathf.Clamp01((t - Exit) / Fade);
        }

        // One grain layer's frame: a random tile size around `size` and a random offset within it.
        private static void Roll(VisualElement layer, float frame, float salt, float size)
        {
            var tile = size * (.85f + .3f * Rnd(frame, salt + 1, 3));
            layer.style.backgroundSize = new BackgroundSize(tile, tile);
            layer.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, Rnd(frame, salt + 2, 5) * tile);
            layer.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top, Rnd(frame, salt + 3, 7) * tile);
        }

        private void Place(VisualElement layer, Vector2 centre, float radius, float opacity)
        {
            var c = Map(centre); var r = radius * k;
            layer.style.left = c.x - r; layer.style.top = c.y - r; layer.style.width = 2 * r; layer.style.height = 2 * r; layer.style.right = StyleKeyword.Auto; layer.style.bottom = StyleKeyword.Auto;
            layer.style.opacity = opacity;
        }

        private Vector2 Cam(Vector2 p) => new Vector2(960 + shakeOffset.x, 540 + shakeOffset.y) + (p - new Vector2(320, 343)) * S;
        private Vector2 Map(Vector2 design) => origin + design * k;

        private void Draw(MeshGenerationContext context)
        {
            if (k <= 0) return;
            var painter = context.painter2D; var full = new Rect(0, 0, context.visualElement.contentRect.width, context.visualElement.contentRect.height);
            if (inv) Rect(painter, full, Ink);
            var d = DAt(t);
            var letters = OutCubic(Mathf.Clamp01((t - .1f) / .7f));
            var hexColor = inv ? Ground : Ink; var inkColor = inv ? Ground : Ink; var bladeColor = inv ? Ground : Solid;

            // Exit glitch (the previous intro's rules) or the impact's RGB split.
            var g = t - Exit; float overlayAlpha = 1, mainOn = 1; (float tx, float op)[] slices = null; (float op, Vector2 pink, Vector2 teal)? ghost = null;
            if (g >= 0 && !reduced)
            {
                var p = Mathf.Clamp01(g / (Total - Exit));
                overlayAlpha = p < .65f ? 1 : 1 - (p - .65f) / .35f;
                var j = Mathf.Floor(g / .055f); var inten = .35f + .65f * Mathf.Clamp01(j * .055f / (Total - Exit));
                if (Rnd(j, 1) >= .18f)
                {
                    mainOn = 0;
                    slices = Enumerable.Range(0, 8).Select(i => (Spread(j, i, 2) * (Rnd(j, i, 3) < .45f ? 100 : 14) * inten, Rnd(j, i, 4) < .22f * inten ? 0f : 1f)).ToArray();
                    var s = (4 + 16 * inten) * 1.8f;
                    ghost = (.85f, new Vector2(-s + Spread(j, 7) * 11, Spread(j, 8) * 5.4f), new Vector2(s + Spread(j, 9) * 11, Spread(j, 10) * 5.4f));
                }
            }
            else if (hit && burst > .02f && !inv) { var s = 3 + 34 * burst; ghost = (.85f * burst, new Vector2(-s, -s * .2f), new Vector2(s, s * .2f)); }

            // The world (hexagon + mark), split along the cut right after the impact: each half clipped to its side and pushed along the blade.
            var hexDraw = InOutSine(Mathf.Clamp01((t - .1f) / (Exit - .1f)));
            var hexR = 500 * (1 + .04f * InOutSine(Mathf.Clamp01(t / Total))) * (1 + .035f * punch);
            var hexCentre = new Vector2(960, 540) + shakeOffset * .5f;
            void World(Vector2 offset, List<(Vector2 n, float c)> clips)
            {
                var hex = Hex(hexCentre, hexR).Select(v => v + offset).ToList();
                Stroke(painter, hex, hexColor, .08f * letters * overlayAlpha, 1.5f, clips);
                var hexOp = (.5f + .5f * Mathf.Max(burst, flash)) * overlayAlpha;
                var drawn = Partial(hex, hexDraw);
                Stroke(painter, drawn, hexColor, .35f * .35f * hexOp, 16, clips); Stroke(painter, drawn, hexColor, .35f * .5f * hexOp, 7, clips); // the design's blurred glow
                Stroke(painter, drawn, hexColor, .7f * hexOp, 2, clips);
                if (hexDraw > .001f && hexDraw < .999f && clips == null)
                {
                    var tip = drawn[drawn.Count - 1];
                    Circle(painter, tip, 16, hexColor, .12f * hexOp); Circle(painter, tip, 9, hexColor, .2f * hexOp); Circle(painter, tip, 3.5f, hexColor, hexOp);
                }
                if (ghost.HasValue)
                {
                    Logo(painter, d, Pink, Pink, letters, false, offset + ghost.Value.pink, clips, ghost.Value.op * overlayAlpha);
                    Logo(painter, d, Teal, Teal, letters, false, offset + ghost.Value.teal, clips, ghost.Value.op * overlayAlpha);
                }
                var glow = t > Strike && !reduced ? (.45f + .55f * burst) * mainOn * overlayAlpha : 0;
                if (glow > 0) Logo(painter, d, inkColor, Solid, 1, true, offset, clips, glow * .35f, 14);
                if (!reduced)
                    for (var n = 1; n <= 6; n++) { var e = DAt(t - .012f * n); if (d - e > 2) Logo(painter, e, inkColor, bladeColor, 1, true, offset, clips, .5f / n * overlayAlpha); }
                if (mainOn > 0) Logo(painter, d, inkColor, bladeColor, letters, false, offset, clips, overlayAlpha);
                if (slices == null) return;
                float bandTop = 540 + (258 - 343) * S, bandH = (502 - 258) * S / 8;
                for (var i = 0; i < slices.Length; i++)
                {
                    if (slices[i].op <= 0) continue;
                    var band = new List<(Vector2, float)>(clips ?? new List<(Vector2, float)>()) { (Vector2.up, bandTop + i * bandH), (Vector2.down, -(bandTop + (i + 1) * bandH + .5f)) };
                    Logo(painter, d, Ink, Solid, letters, false, offset + new Vector2(slices[i].tx, 0), band, overlayAlpha);
                }
            }
            if (split > .5f)
                foreach (var side in new[] { 1f, -1f })
                    World((U * split * side + N * split * .22f * side), new List<(Vector2 n, float c)> { (N * side, Vector2.Dot(N * side, hp)) });
            else World(Vector2.zero, null);

            // Light beam along the cut, shockwave hexes and sparks.
            if (beam > .2f)
            {
                Vector2 a = hp - U * 3200, b = hp + U * 3200;
                Line(painter, a, b, Solid, .2f, beam * 7); Line(painter, a, b, Solid, .3f, beam * 3.5f);
                Line(painter, a - N * beam * .7f, b - N * beam * .7f, Pink, .8f, beam * .6f); Line(painter, a + N * beam * .7f, b + N * beam * .7f, Teal, .8f, beam * .6f);
                Line(painter, a, b, inv ? Ground : Ink, 1, beam);
            }
            if (hit)
            {
                for (var n = 0; n < 2; n++)
                {
                    var a = age - n * .07f; if (a <= 0 || a >= .8f) continue;
                    var q = a / .8f;
                    Stroke(painter, Hex(hexCentre, 140 + 1500 * OutExpo(q)), n == 1 ? Solid : inv ? Ground : Ink, (1 - q) * .85f, .8f + 7 * (1 - q), null);
                }
                Vector2 tb = Cam(TipB), tt = Cam(TipT);
                var colours = new[] { Ink, Solid, Pink, Teal };
                foreach (var s in Sparks)
                {
                    var a = age - s.delay; if (a <= 0 || a >= s.life) continue;
                    float q = a / s.life, e = OutExpo(q);
                    var head = tb + (tt - tb) * s.t + s.dir * s.dist * e; var tail = s.len * (1 - q) * (1 + 2 * (1 - e));
                    painter.lineCap = LineCap.Round; Line(painter, head, head - s.dir * tail, inv ? Ground : colours[s.col], 1 - q, 2.4f * (1 - q) + .6f); painter.lineCap = LineCap.Butt;
                }
            }
            if (flash > .01f) Rect(painter, full, new Color(Ink.r, Ink.g, Ink.b, flash));
        }

        // The mark at progress d. Blade-only layers (glow, motion trail) skip the letters; `halo` strokes a soft outline for the glow.
        private void Logo(Painter2D painter, float d, Color ink, Color blade, float textAlpha, bool bladeOnly, Vector2 offset, List<(Vector2 n, float c)> clips, float alpha, float halo = 0)
        {
            foreach (var part in Parts)
            {
                if (bladeOnly && !part.blade) continue;
                var a = alpha * (part.blade ? 1 : textAlpha); if (a <= .003f) continue;
                var at = new Vector2(part.fx(d), part.fy(d)); var colour = part.blade ? blade : ink;
                painter.BeginPath(); var any = false;
                foreach (var ring in part.rings)
                {
                    IList<Vector2> poly = ring.Select(p => Cam(p + at) + offset).ToList();
                    if (clips != null) foreach (var (n, c) in clips) poly = Clip(poly, n, c);
                    if (poly.Count < 3) continue;
                    painter.MoveTo(Map(poly[0])); for (var i = 1; i < poly.Count; i++) painter.LineTo(Map(poly[i])); painter.ClosePath(); any = true;
                }
                if (!any) continue;
                if (halo > 0) { painter.strokeColor = Alpha(colour, a * .5f); painter.lineWidth = halo * k; painter.lineJoin = LineJoin.Round; painter.Stroke(); painter.strokeColor = Alpha(colour, a); painter.lineWidth = halo * .5f * k; painter.Stroke(); }
                painter.fillColor = Alpha(colour, a); painter.Fill(FillRule.OddEven);
            }
        }

        // Sutherland–Hodgman against one half-plane (n·p ≥ c); even-odd rings stay correct ring by ring.
        private static IList<Vector2> Clip(IList<Vector2> poly, Vector2 n, float c)
        {
            var output = new List<Vector2>(poly.Count + 4);
            for (var i = 0; i < poly.Count; i++)
            {
                Vector2 a = poly[i], b = poly[(i + 1) % poly.Count]; float da = Vector2.Dot(n, a) - c, db = Vector2.Dot(n, b) - c;
                if (da >= 0) output.Add(a);
                if (da >= 0 != db >= 0) output.Add(a + (b - a) * (da / (da - db)));
            }
            return output;
        }

        // The hexagon from the upper-right vertex, clockwise (closed: 7 points).
        private static List<Vector2> Hex(Vector2 c, float r) => new[] { 300, 0, 60, 120, 180, 240, 300 }.Select(a => c + r * new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad))).ToList();

        private static List<Vector2> Partial(List<Vector2> closed, float fraction)
        {
            var result = new List<Vector2> { closed[0] }; var segments = closed.Count - 1; var reach = Mathf.Clamp01(fraction) * segments;
            for (var i = 0; i < segments && i < reach; i++) result.Add(Vector2.Lerp(closed[i], closed[i + 1], Mathf.Min(1, reach - i)));
            return result;
        }

        private void Stroke(Painter2D painter, List<Vector2> line, Color colour, float alpha, float width, List<(Vector2 n, float c)> clips)
        {
            if (alpha <= .003f || line.Count < 2) return;
            painter.strokeColor = Alpha(colour, alpha); painter.lineWidth = width * k; painter.lineJoin = LineJoin.Miter;
            painter.BeginPath(); var open = false;
            for (var i = 0; i < line.Count - 1; i++)
            {
                Vector2 a = line[i], b = line[i + 1]; var keep = true;
                if (clips != null)
                    foreach (var (n, c) in clips)
                    {
                        float da = Vector2.Dot(n, a) - c, db = Vector2.Dot(n, b) - c;
                        if (da < 0 && db < 0) { keep = false; break; }
                        if (da < 0) a += (b - a) * (da / (da - db));
                        else if (db < 0) b = a + (b - a) * (da / (da - db));
                    }
                if (!keep) { open = false; continue; }
                if (!open) painter.MoveTo(Map(a));
                painter.LineTo(Map(b)); open = clips == null; // clipped runs stroke segment by segment
            }
            painter.Stroke();
        }

        private void Line(Painter2D painter, Vector2 a, Vector2 b, Color colour, float alpha, float width)
        {
            painter.strokeColor = Alpha(colour, alpha); painter.lineWidth = width * k;
            painter.BeginPath(); painter.MoveTo(Map(a)); painter.LineTo(Map(b)); painter.Stroke();
        }

        private void Circle(Painter2D painter, Vector2 centre, float radius, Color colour, float alpha)
        {
            painter.fillColor = Alpha(colour, alpha);
            painter.BeginPath(); painter.Arc(Map(centre), radius * k, Angle.Degrees(0), Angle.Degrees(360)); painter.Fill();
        }

        private static void Rect(Painter2D painter, Rect r, Color colour)
        {
            painter.fillColor = colour; painter.BeginPath();
            painter.MoveTo(new Vector2(r.xMin, r.yMin)); painter.LineTo(new Vector2(r.xMax, r.yMin)); painter.LineTo(new Vector2(r.xMax, r.yMax)); painter.LineTo(new Vector2(r.xMin, r.yMax)); painter.ClosePath();
            painter.Fill();
        }

        private static Color Alpha(Color c, float a) => new Color(c.r, c.g, c.b, c.a * Mathf.Clamp01(a));

        // ---------- generated textures: radial falloffs (the ambient light and the vignette) and film grain ----------
        private static Texture2D ambientTexture, vignetteTexture, noiseTexture;

        // Ambient: white fading to clear at the rim. Vignette (drawn 1.6 × the design radius): clear inside 40 %, 85 % black at the radius and beyond.
        private static Texture2D Radial(bool vignette)
        {
            ref var cached = ref vignette ? ref vignetteTexture : ref ambientTexture;
            if (cached != null) return cached;
            const int size = 128; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var r = new Vector2(x + .5f - size / 2f, y + .5f - size / 2f).magnitude / (size / 2f);
                pixels[y * size + x] = vignette
                    ? new Color32(0, 0, 0, (byte)(255 * .85f * Mathf.Clamp01((r * 1.6f - .4f) / .6f)))
                    : new Color32(255, 255, 255, (byte)(255 * Mathf.Clamp01(1 - r)));
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return cached = texture;
        }

        private static Texture2D Noise()
        {
            if (noiseTexture != null) return noiseTexture;
            const int size = 256; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            var random = new System.Random(7); var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++) { var v = (byte)random.Next(256); pixels[i] = new Color32(v, v, v, 255); }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return noiseTexture = texture;
        }
    }
}
