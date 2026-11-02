using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Veilbreak.Rendering
{
    // Per-renderer material values that keep the renderer in the SRP Batcher. A
    // MaterialPropertyBlock opts its renderer out of SRP batching, so in play mode the values go onto material instances, which
    // share the source's shader variant and still batch:
    //   Shared  one instance per source material and value set, reused by every renderer that shows it (a palette worn by four
    //           characters is one material) and counted: each holder hands it back through Return, and the last return destroys it
    //   Owned   an instance only this renderer draws, for values that change every frame (animated plates); Discard
    //           drops it with the renderer
    // Edit-mode previews (ExecuteAlways) write a property block instead, so serialized materials are never replaced; no player
    // build reaches that path.
    public sealed class MaterialPaint
    {
        private static readonly Dictionary<string, (Material material, int holders)> Cache = new Dictionary<string, (Material, int)>();
        private static readonly Dictionary<Material, string> Keys = new Dictionary<Material, string>();
        private static readonly HashSet<Material> Instances = new HashSet<Material>();
        private static MaterialPropertyBlock block;
        private readonly List<(int id, Color value)> colors = new List<(int, Color)>();
        private readonly List<(int id, float value)> floats = new List<(int, float)>();

        public MaterialPaint Clear() { colors.Clear(); floats.Clear(); return this; }
        public MaterialPaint Color(int id, Color value) { colors.Add((id, value)); return this; }
        public MaterialPaint Float(int id, float value) { floats.Add((id, value)); return this; }

        // Paints material `index` of `renderer` (whose unpainted material is `source`) with the instance every renderer painting
        // `source` with these values shares, and returns it held for the caller (null in edit mode, which holds nothing).
        public Material Shared(Renderer renderer, int index, Material source)
        {
            if (Preview(renderer, index) || source == null) return null;
            var key = Key(source);
            if (!Cache.TryGetValue(key, out var entry) || entry.material == null)
            {
                entry = (new Material(source) { name = source.name + " (Painted)", hideFlags = HideFlags.DontSave }, 0);
                Write(entry.material); Keys[entry.material] = key;
            }
            Cache[key] = (entry.material, entry.holders + 1);
            Assign(renderer, index, entry.material);
            return entry.material;
        }

        // Hands back a material Shared returned; the last holder's return destroys it.
        public static void Return(Material material)
        {
            if (material == null || !Keys.TryGetValue(material, out var key)) return;
            var entry = Cache[key];
            if (entry.holders > 1) { Cache[key] = (entry.material, entry.holders - 1); return; }
            Cache.Remove(key); Keys.Remove(material); Object.Destroy(material);
        }

        // Paints material `index` of `renderer` with an instance only it draws, made from its current material on first use.
        public void Owned(Renderer renderer, int index = 0)
        {
            if (Preview(renderer, index)) return;
            var current = renderer.sharedMaterials[index];
            if (current == null) return;
            if (!Instances.Contains(current))
            {
                current = new Material(current) { name = current.name + " (Owned)", hideFlags = HideFlags.DontSave };
                Instances.Add(current); Assign(renderer, index, current);
            }
            Write(current);
        }

        // Destroys the owned instances `renderer` draws.
        public static void Discard(Renderer renderer)
        {
            if (renderer == null) return;
            foreach (var m in renderer.sharedMaterials) if (m != null && Instances.Remove(m)) Object.Destroy(m);
        }

        public static int SharedCount => Cache.Count;

        private bool Preview(Renderer renderer, int index)
        {
            if (Application.isPlaying) return false;
            block ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block, index);
            foreach (var (id, value) in colors) block.SetColor(id, value);
            foreach (var (id, value) in floats) block.SetFloat(id, value);
            renderer.SetPropertyBlock(block, index);
            return true;
        }

        private void Write(Material material)
        {
            foreach (var (id, value) in colors) material.SetColor(id, value);
            foreach (var (id, value) in floats) material.SetFloat(id, value);
        }

        private static void Assign(Renderer renderer, int index, Material material)
        {
            var materials = renderer.sharedMaterials;
            if (materials[index] == material) return;
            materials[index] = material; renderer.sharedMaterials = materials;
        }

        private string Key(Material source)
        {
            var key = new StringBuilder().Append(source.GetEntityId().ToString());
            foreach (var (id, c) in colors) key.Append('|').Append(id).Append(':').Append(c.r.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(c.g.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(c.b.ToString("R", CultureInfo.InvariantCulture)).Append(',').Append(c.a.ToString("R", CultureInfo.InvariantCulture));
            foreach (var (id, f) in floats) key.Append('|').Append(id).Append('=').Append(f.ToString("R", CultureInfo.InvariantCulture));
            return key.ToString();
        }
    }
}
