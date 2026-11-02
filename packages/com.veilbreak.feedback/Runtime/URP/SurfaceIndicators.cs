using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Veilbreak.Feedback
{
    /// <summary>
    /// Ground plan lines (paths, rings, outlines) projected through the world depth with URP Decal Projectors, so they land on
    /// every surface, including buildings without physics colliders. One projector per segment, reused across frames
    /// (Begin, Draw per polyline, End). The host supplies the decal material template, the vertical span to project through
    /// (<see cref="Top"/>, <see cref="Bottom"/>), the segment cap for its tier and the tint palette.
    /// </summary>
    public sealed class SurfaceIndicators : IDisposable
    {
        /// <summary>Decal base-map property the generated mask textures are written to.</summary>
        public static readonly int BaseMapProperty = Shader.PropertyToID("Base_Map");

        /// <summary>
        /// Tint slots, picked by a line colour's dominant channel: green, red, blue, then neutral for anything balanced.
        /// </summary>
        public static readonly Color[] DefaultPalette = { new Color(.45f, 1, .78f), new Color(1, .35f, .65f), new Color(.8f, .65f, 1), new Color(.8f, .9f, 1) };

        public const int DefaultCapacity = 256;

        /// <summary>World heights the projectors span: above the highest and below the lowest surface lines can land on.</summary>
        public float Top = 128, Bottom = -16;

        private readonly Transform parent;
        private readonly List<DecalProjector> projectors = new List<DecalProjector>();
        private readonly Material[] materials;
        private readonly Texture2D[] textures;
        private readonly int capacity;
        private int used;

        public bool Available => materials != null;
        public int Capacity => capacity;
        public int Used => used;

        public SurfaceIndicators(Transform parent, Material template, int capacity = DefaultCapacity, Color[] palette = null)
        {
            this.parent = parent;
            this.capacity = Mathf.Max(0, capacity);
            if (template == null) return;
            var colors = palette != null && palette.Length == DefaultPalette.Length ? palette : DefaultPalette;
            materials = new Material[colors.Length]; textures = new Texture2D[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false) { name = "indicator mask", wrapMode = TextureWrapMode.Clamp };
                var pixels = new Color[64];
                for (var y = 0; y < 8; y++) for (var x = 0; x < 8; x++)
                { var color = colors[i]; color.a = x == 0 || x == 7 ? .15f : .9f; pixels[y * 8 + x] = color; }
                texture.SetPixels(pixels); texture.Apply(false, true); textures[i] = texture;
                materials[i] = new Material(template) { name = "surface indicator", enableInstancing = true };
                materials[i].SetTexture(BaseMapProperty, texture);
            }
        }

        /// <summary>The palette slot a line colour draws with (see <see cref="DefaultPalette"/>).</summary>
        public static int Slot(Color color) =>
            color.r > color.g * 1.2f ? 1 : color.b > color.g * 1.2f ? 2 : color.g > color.r * 1.2f ? 0 : 3;

        public void Begin() => used = 0;

        public void Draw(Vector3[] points, float width, bool loop, Color color)
        {
            if (!Available || points == null || points.Length < 2) return;
            var count = loop ? points.Length : points.Length - 1;
            var tint = materials[Slot(color)];
            for (var i = 0; i < count && used < capacity; i++)
            {
                var a = points[i]; var b = points[(i + 1) % points.Length];
                var along = new Vector3(b.x - a.x, 0, b.z - a.z);
                if (along.sqrMagnitude < .000001f) continue;
                if (used == projectors.Count)
                {
                    var go = new GameObject("surface indicator"); go.transform.SetParent(parent, false);
                    var decal = go.AddComponent<DecalProjector>(); decal.drawDistance = 180; decal.fadeScale = .95f;
                    decal.startAngleFade = 180; decal.endAngleFade = 180; projectors.Add(decal);
                }
                var d = projectors[used++]; d.enabled = true; d.material = tint;
                d.transform.SetPositionAndRotation(new Vector3((a.x + b.x) / 2, Top, (a.z + b.z) / 2), Quaternion.LookRotation(Vector3.down, along.normalized));
                d.size = new Vector3(Mathf.Max(.06f, width), along.magnitude + width * .5f, Top - Bottom);
                d.pivot = new Vector3(0, 0, (Top - Bottom) / 2);
                d.fadeFactor = Mathf.Clamp01(Mathf.Max(color.r, color.g, color.b));
            }
        }

        public void End() { for (var i = used; i < projectors.Count; i++) projectors[i].enabled = false; }

        public void Dispose()
        {
            foreach (var projector in projectors) if (projector != null) Destroy(projector.gameObject);
            projectors.Clear();
            if (materials == null) return;
            foreach (var material in materials) Destroy(material);
            foreach (var texture in textures) Destroy(texture);
        }

        private static void Destroy(UnityEngine.Object target)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(target); else UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
