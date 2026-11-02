using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // Fire burning along the element's bottom edge: overlapping tongues played from a flipbook of simulated flame, a back row
    // taller and deeper red behind a brighter front row, a flickering heat glow at the base and embers drifting up off it (above
    // the element too, so give it room). The flipbook comes from USS: `--hud-flame-flipbook: url(...)` (a texture with straight
    // alpha, frames left to right and top to bottom) and `--hud-flame-grid` (frames per side, default 8). Non-interactive; reduced
    // motion (HudFx.Reduced) holds one still frame without embers. Usable from UXML: <ar:HudFlame class="my-flame" />.
    [UxmlElement]
    public partial class HudFlame : VisualElement
    {
        private static readonly CustomStyleProperty<Texture2D> FlipbookProperty = new CustomStyleProperty<Texture2D>("--hud-flame-flipbook");
        private static readonly CustomStyleProperty<int> GridProperty = new CustomStyleProperty<int>("--hud-flame-grid");
        private const float Fps = 30, Overlap = .4f;
        private static Texture2D dot;
        private static int seeds;

        private Texture2D flipbook;
        private int grid = 8;
        private readonly int seed = ++seeds * 7919;
        private readonly IVisualElementScheduledItem timer;
        private bool still;

        // A soft round dot over 64 px, so the dynamic atlas never takes it and the mesh UVs stay the texture's own.
        private static Texture2D Dot
        {
            get
            {
                if (dot != null) return dot;
                const int n = 96; dot = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                var px = new Color32[n * n];
                for (var y = 0; y < n; y++)
                    for (var x = 0; x < n; x++)
                    {
                        var d = new Vector2((x + .5f) / n - .5f, (y + .5f) / n - .5f).magnitude * 2;
                        px[y * n + x] = new Color32(255, 255, 255, (byte)(255 * Mathf.Pow(Mathf.Clamp01(1 - d), 2)));
                    }
                dot.SetPixels32(px); dot.Apply(false, true); return dot;
            }
        }

        public HudFlame()
        {
            AddToClassList("ar-flame"); pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(e =>
            {
                if (e.customStyle.TryGetValue(FlipbookProperty, out var texture)) flipbook = texture;
                if (e.customStyle.TryGetValue(GridProperty, out var cells)) grid = Mathf.Clamp(cells, 1, 16);
                MarkDirtyRepaint();
            });
            timer = schedule.Execute(Tick).Every(33); timer.Pause();
            RegisterCallback<AttachToPanelEvent>(_ => timer.Resume());
            RegisterCallback<DetachFromPanelEvent>(_ => timer.Pause());
        }

        // Reduced motion paints once and then stays still until it is turned off again.
        private void Tick()
        {
            if (HudFx.Reduced && still) return;
            still = HudFx.Reduced; MarkDirtyRepaint();
        }

        private float Hash(int i, int salt) => Mathf.Repeat(Mathf.Sin((i + seed) * 12.9898f + salt * 78.233f) * 43758.547f, 1);

        private void Draw(MeshGenerationContext ctx)
        {
            float w = contentRect.width, h = contentRect.height;
            if (w < 4 || h < 4) return;
            var time = HudFx.Reduced ? 1.7f : Time.unscaledTime;
            var flicker = .82f + .18f * Mathf.PerlinNoise(time * 5.3f, seed * .01f);

            // Heat on the base: a row of soft dots, flickering.
            var glows = Mathf.Max(2, Mathf.CeilToInt(w / (h * .9f)));
            var mesh = ctx.Allocate(glows * 4, glows * 6, Dot); var n = 0;
            for (var i = 0; i < glows; i++)
            {
                float x = (i + .5f) * w / glows, size = h * 1.3f;
                Quad(mesh, ref n, new Rect(x - size * .7f, h - size * .45f, size * 1.4f, size * .7f), new Rect(0, 0, 1, 1), new Color(1, .42f, .1f, .34f * flicker * Taper(x, w)));
            }

            if (flipbook != null)
            {
                // Two rows of tongues, back (taller, redder, dimmer) then front; each plays the flipbook from its own frame and mirrors at random.
                var count = Mathf.Max(2, Mathf.CeilToInt(w / (h * Overlap)));
                mesh = ctx.Allocate(count * 2 * 4, count * 2 * 6, flipbook); n = 0;
                for (var row = 0; row < 2; row++)
                    for (var i = 0; i < count; i++)
                    {
                        var k = row * 97 + i;
                        float x = (i + .5f + (Hash(k, 1) - .5f) * .6f) * w / count, taper = Taper(x, w);
                        var size = h * (row == 0 ? 1.15f + .3f * Hash(k, 2) : .78f + .3f * Hash(k, 3)) * Mathf.Lerp(.45f, 1, taper);
                        var frame = Mathf.FloorToInt(time * Fps * (.85f + .3f * Hash(k, 4)) + Hash(k, 5) * grid * grid) % (grid * grid);
                        float cell = 1f / grid, u = frame % grid * cell, v = 1 - (frame / grid + 1) * cell;
                        var uv = Hash(k, 6) < .5f ? new Rect(u, v, cell, cell) : new Rect(u + cell, v, -cell, cell);
                        var tint = row == 0 ? new Color(1, .62f, .45f, .62f * flicker) : new Color(1, .97f, .92f, .95f * (.9f + .1f * flicker));
                        // The flipbook's flames stand on the cell's bottom edge with a little margin; sink them below the base line.
                        Quad(mesh, ref n, new Rect(x - size / 2, h - size * .96f, size, size), uv, tint);
                    }
            }

            if (HudFx.Reduced) return;
            // Embers: born on the base, rising and swaying, white-gold cooling to red as they fade.
            var embers = Mathf.Clamp(Mathf.RoundToInt(w / 9), 6, 48);
            mesh = ctx.Allocate(embers * 4, embers * 6, Dot); n = 0;
            for (var i = 0; i < embers; i++)
            {
                float life = 1.1f + 1.3f * Hash(i, 11), age = Mathf.Repeat(time + Hash(i, 12) * life, life) / life;
                var born = Mathf.Floor((time + Hash(i, 12) * life) / life);
                float x0 = Hash(i + (int)born * 131, 13) * w, rise = h * (1.2f + 1.4f * Hash(i, 14));
                var x = x0 + Mathf.Sin(age * 6.3f * (1 + Hash(i, 15)) + i) * h * .12f;
                var y = h * (.85f - .2f * Hash(i, 16)) - rise * age;
                var size = Mathf.Max(1.2f, h * .035f) * (1.3f - age * .6f);
                var heat = Color.Lerp(new Color(1, .88f, .5f), new Color(1, .32f, .04f), age);
                heat.a = Mathf.Clamp01(Mathf.Clamp01(age * 8) * (1 - age) * 1.8f * (.6f + .4f * Mathf.PerlinNoise(time * 9 + i, 0))) * Taper(x0, w);
                Quad(mesh, ref n, new Rect(x - size, y - size, size * 2, size * 2), new Rect(0, 0, 1, 1), heat);
            }
        }

        // Shorter, fainter fire towards both ends, so the line burns out instead of stopping at a cut.
        private static float Taper(float x, float w) { var edge = Mathf.Min(x, w - x) / Mathf.Max(1, Mathf.Min(w * .18f, 60)); return Mathf.SmoothStep(.25f, 1, Mathf.Clamp01(edge)); }

        // One textured quad; `start` counts the allocation's vertices, which its indices address.
        private static void Quad(MeshWriteData mesh, ref int start, Rect at, Rect uv, Color tint)
        {
            Color32 c = tint;
            mesh.SetNextVertex(new Vertex { position = new Vector3(at.xMin, at.yMax, Vertex.nearZ), uv = new Vector2(uv.xMin, uv.yMin), tint = c });
            mesh.SetNextVertex(new Vertex { position = new Vector3(at.xMin, at.yMin, Vertex.nearZ), uv = new Vector2(uv.xMin, uv.yMax), tint = c });
            mesh.SetNextVertex(new Vertex { position = new Vector3(at.xMax, at.yMin, Vertex.nearZ), uv = new Vector2(uv.xMax, uv.yMax), tint = c });
            mesh.SetNextVertex(new Vertex { position = new Vector3(at.xMax, at.yMax, Vertex.nearZ), uv = new Vector2(uv.xMax, uv.yMin), tint = c });
            mesh.SetNextIndex((ushort)start); mesh.SetNextIndex((ushort)(start + 1)); mesh.SetNextIndex((ushort)(start + 2));
            mesh.SetNextIndex((ushort)start); mesh.SetNextIndex((ushort)(start + 2)); mesh.SetNextIndex((ushort)(start + 3));
            start += 4;
        }
    }
}
