using System;
using UnityEngine;

namespace Veilbreak.Feedback
{
    /// <summary>
    /// A pooled one-shot effect prefab instance. Its root particle system's Stop Action is Callback, so it hands itself back
    /// to its pool when it finishes instead of being destroyed on a timer. A prefab without a root system falls back to a
    /// fixed lifetime.
    /// </summary>
    public sealed class PooledEffect : MonoBehaviour
    {
        public const float FallbackSeconds = 2;

        private ParticleSystem root;
        private Action<PooledEffect> release;
        private float until;

        public static PooledEffect Create(GameObject prefab, Transform parent, Action<PooledEffect> release)
        {
            var go = Instantiate(prefab, parent); go.SetActive(false);
            var fx = go.AddComponent<PooledEffect>(); fx.release = release; fx.root = go.GetComponent<ParticleSystem>();
            if (fx.root != null) { var main = fx.root.main; main.loop = false; main.playOnAwake = false; main.stopAction = ParticleSystemStopAction.Callback; }
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true)) { var m = ps.main; m.prewarm = false; }
            return fx;
        }

        public void Play(Vector3 at)
        {
            transform.SetPositionAndRotation(at, Quaternion.identity);
            until = Time.time + FallbackSeconds;
            if (root != null) root.Play(true);
            else foreach (var ps in GetComponentsInChildren<ParticleSystem>()) ps.Play(true);
        }

        private void Update() { if (root == null && Time.time >= until) release(this); }
        private void OnParticleSystemStopped() => release(this);
    }
}
