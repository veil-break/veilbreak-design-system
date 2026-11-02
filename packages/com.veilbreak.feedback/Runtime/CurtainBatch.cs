using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Veilbreak.Feedback
{
    /// <summary>
    /// Vertical light curtains standing on ground outlines: every <see cref="Add"/> appends one quad strip per edge, from the
    /// outline up <c>height</c> metres, into one mesh that draws in a single call. Presenters rebuild it every frame
    /// (Begin, Add per outline, End); builders bake a static one with <see cref="Bake"/>. uv.x is metres along the outline,
    /// uv.y 0..1 up. The host supplies the curtain material; its shader reads the shared <see cref="PhaseProperty"/>.
    /// </summary>
    public sealed class CurtainBatch
    {
        /// <summary>Global float a curtain shader drifts its rays by (see <see cref="Phase"/>).</summary>
        public static readonly int PhaseProperty = Shader.PropertyToID("_CurtainPhase");

        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<int> triangles = new List<int>();
        private readonly Mesh mesh;
        private readonly MeshRenderer renderer;

        public CurtainBatch(Transform parent, Material material, string name = "curtains")
        {
            mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 }; mesh.MarkDynamic();
            if (parent == null) return;
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false; renderer.lightProbeUsage = LightProbeUsage.Off;
        }

        /// <summary>The geometry built since the last <see cref="Begin"/> (or the last <see cref="End"/>).</summary>
        public Mesh Mesh => mesh;
        public int Strips => triangles.Count / 6;

        /// <summary>Advances the shared ray drift (0 holds it still under reduced motion).</summary>
        public static void Phase(float time) => Shader.SetGlobalFloat(PhaseProperty, time);

        public void Begin() { vertices.Clear(); colors.Clear(); uvs.Clear(); triangles.Clear(); }

        /// <summary>
        /// One strip per segment of <paramref name="points"/> (closing the loop when asked); the top edge is transparent,
        /// so colour only needs to be right at the floor. Points are world space (or the batch's space for baked meshes).
        /// </summary>
        public void Add(IReadOnlyList<Vector3> points, int count, bool loop, float height, Color color)
        {
            if (count < 2 || height <= 0 || color.maxColorComponent <= .002f) return;
            var along = 0f; var segments = loop ? count : count - 1;
            for (var i = 0; i < segments; i++)
            {
                Vector3 a = points[i], b = points[(i + 1) % count]; var length = Vector3.Distance(a, b);
                if (length < 1e-4f) continue;
                var start = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(a + Vector3.up * height); vertices.Add(b + Vector3.up * height);
                for (var k = 0; k < 4; k++) colors.Add(color);
                uvs.Add(new Vector2(along, 0)); uvs.Add(new Vector2(along + length, 0)); uvs.Add(new Vector2(along, 1)); uvs.Add(new Vector2(along + length, 1));
                triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1); triangles.Add(start + 1); triangles.Add(start + 2); triangles.Add(start + 3);
                along += length;
            }
        }

        public void End()
        {
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0, false);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 4000); // rebuilt every frame and drawn in world space: never cull it
            if (renderer != null) renderer.enabled = vertices.Count > 0;
        }

        /// <summary>A finished copy of the current geometry with tight bounds, for builders to save as an asset.</summary>
        public Mesh Bake(string name)
        {
            End(); var copy = Object.Instantiate(mesh); copy.name = name; copy.RecalculateBounds(); return copy;
        }
    }
}
