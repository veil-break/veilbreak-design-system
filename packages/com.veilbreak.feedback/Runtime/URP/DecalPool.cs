using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

namespace Veilbreak.Feedback
{
    /// <summary>
    /// Impact decals: pooled URP Decal Projectors projected straight down so sculpted relief receives them, fading out over
    /// the last third of their life. The oldest fades first when the cap is reached. The host picks the cap for its quality
    /// tier (<see cref="Initialize"/>) and the draw distance for its camera, so marks never pop.
    /// </summary>
    public sealed class DecalPool : MonoBehaviour
    {
        public const int DefaultCapacity = 128;
        public const float Depth = 3;

        /// <summary>Projector draw distance in metres; match the camera's farthest view of the ground.</summary>
        public float DrawDistance = 42;
        /// <summary>The time source the fades run on; tests and replays can substitute their own.</summary>
        public Func<float> Clock = () => Time.time;

        private ObjectPool<DecalProjector> pool;
        private readonly List<(DecalProjector decal, float born, float life)> live = new List<(DecalProjector, float, float)>();
        private readonly System.Random random = new System.Random(5);

        public int Capacity { get; private set; }
        public int Live => live.Count;

        public void Initialize(int capacity)
        {
            Clear();
            pool?.Dispose();
            Capacity = Mathf.Max(1, capacity);
            pool = new ObjectPool<DecalProjector>(Create, d => d.gameObject.SetActive(true), d => d.gameObject.SetActive(false), DestroyDecal, false, Capacity, Capacity);
        }

        private DecalProjector Create()
        {
            var go = new GameObject("decal"); go.transform.SetParent(transform, false);
            var d = go.AddComponent<DecalProjector>();
            d.drawDistance = DrawDistance; d.fadeScale = .9f; d.startAngleFade = 60; d.endAngleFade = 85;
            return d;
        }

        private static void DestroyDecal(DecalProjector d)
        {
            if (Application.isPlaying) Destroy(d.gameObject); else DestroyImmediate(d.gameObject);
        }

        public void Place(Material material, Vector3 at, float size, float life)
        {
            if (material == null || size <= 0) return;
            if (pool == null) Initialize(DefaultCapacity);
            if (live.Count >= Capacity) { pool.Release(live[0].decal); live.RemoveAt(0); }
            var d = pool.Get(); d.material = material;
            var spin = Quaternion.AngleAxis((float)random.NextDouble() * 360, Vector3.down);
            d.transform.SetPositionAndRotation(at + Vector3.up * (Depth * .4f), spin * Quaternion.LookRotation(Vector3.down, Vector3.forward));
            d.size = new Vector3(size, size, Depth); d.pivot = new Vector3(0, 0, Depth / 2); d.fadeFactor = 1;
            live.Add((d, Clock(), Mathf.Max(.1f, life)));
        }

        /// <summary>Fades and retires decals for the given clock time (<see cref="Clock"/> drives it every frame).</summary>
        public void Tick(float now)
        {
            for (var i = live.Count - 1; i >= 0; i--)
            {
                var (decal, born, life) = live[i]; var t = (now - born) / life;
                if (t >= 1) { pool.Release(decal); live.RemoveAt(i); continue; }
                decal.fadeFactor = Fade(t);
            }
        }

        /// <summary>Opacity at a fraction of a decal's life: full for the first two thirds, then a smooth fade.</summary>
        public static float Fade(float t) => t < .66f ? 1 : 1 - Mathf.SmoothStep(0, 1, (t - .66f) / .34f);

        private void Update() => Tick(Clock());

        public void Clear()
        {
            foreach (var (decal, _, _) in live) pool.Release(decal);
            live.Clear();
        }

        private void OnDestroy() => pool?.Dispose();
    }
}
