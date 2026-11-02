using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// Indeterminate loader: a wire icosahedron whose 30 edges light up one at a time along a traced path; each time an edge is
    /// fully lit the solid turns <see cref="TurnDegrees"/> counter-clockwise (about an axis tipped toward the viewer, so it reads
    /// as 3D), then the next edge starts. When every edge is lit the trace fades and begins again. Reduced motion keeps the
    /// trace and drops the turns. Lit edges use the theme accent (or the resolved USS colour when unthemed).
    /// </summary>
    [UxmlElement]
    public partial class HudLoader : VisualElement, IHudThemed
    {
        private static readonly Vector3[] Vertices = BuildVertices();
        /// <summary>The 30 edges as vertex index pairs, oriented and ordered along the lighting trace.</summary>
        public static readonly IReadOnlyList<(int from, int to)> Trace = BuildTrace(Vertices);
        // Tipped toward the viewer: the turn looks counter-clockwise on screen while the faces still swing through depth.
        private static readonly Vector3 Axis = new Vector3(.3f, .45f, 1).normalized;
        private static readonly Quaternion Tilt = Quaternion.Euler(-24, 18, 0);

        private float edgeSeconds = .24f, turnSeconds = .42f, turnDegrees = 60, fadeSeconds = .5f;
        private bool reducedMotion;
        private Color trace = new Color(.56f, .9f, 1f), wire = new Color(.56f, .9f, 1f, .16f);
        private bool themed;
        private readonly IVisualElementScheduledItem timer;
        // The cycle: light edge `lit` (progress 0..1), then turn, then the next edge; after the last edge, fade and restart.
        private int lit;
        private float phaseStart, angle, turnFrom;
        private enum Phase { Light, Turn, Fade }
        private Phase phase;

        [UxmlAttribute] public float EdgeSeconds { get => edgeSeconds; set => edgeSeconds = Mathf.Clamp(value, .05f, 2); }
        [UxmlAttribute] public float TurnSeconds { get => turnSeconds; set => turnSeconds = Mathf.Clamp(value, .05f, 2); }
        [UxmlAttribute] public float TurnDegrees { get => turnDegrees; set => turnDegrees = Mathf.Clamp(value, 0, 180); }
        /// <summary>Keeps the edge trace and drops the turns.</summary>
        [UxmlAttribute] public bool ReducedMotion { get => reducedMotion; set { reducedMotion = value; MarkDirtyRepaint(); } }
        /// <summary>Edges fully lit in the current cycle (0..30).</summary>
        public int Lit => lit;
        /// <summary>The solid's turn so far, in degrees (counter-clockwise on screen).</summary>
        public float Angle => angle;

        public HudLoader()
        {
            AddToClassList("ar-loader"); pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            timer = schedule.Execute(Tick).Every(16); timer.Pause();
            RegisterCallback<AttachToPanelEvent>(_ => { Restart(); timer.Resume(); });
            RegisterCallback<DetachFromPanelEvent>(_ => timer.Pause());
            RegisterCallback<CustomStyleResolvedEvent>(_ => { if (!themed) { trace = resolvedStyle.color; wire = HudPaint.Alpha(trace, .16f); MarkDirtyRepaint(); } });
        }

        public void ApplyTheme(HudTheme value)
        {
            themed = true; trace = value.accent; trace.a = 1; wire = HudPaint.Alpha(value.accent, value.highContrast ? .4f : .16f);
            MarkDirtyRepaint();
        }

        public void Restart() { lit = 0; phase = Phase.Light; phaseStart = Now; }

        private static float Now => Time.realtimeSinceStartup;

        /// <summary>Advances the cycle to <paramref name="now"/> (seconds); exposed for tests.</summary>
        public void Advance(float now)
        {
            // Long frames (a stalled editor) skip whole steps instead of jumping straight to the end of one.
            for (var guard = 0; guard < 8; guard++)
            {
                var t = now - phaseStart;
                if (phase == Phase.Light)
                {
                    if (t < edgeSeconds) return;
                    lit++; phaseStart += edgeSeconds;
                    if (lit >= Trace.Count) { phase = Phase.Fade; continue; }
                    if (!reducedMotion && turnDegrees > 0) { phase = Phase.Turn; turnFrom = angle; }
                }
                else if (phase == Phase.Turn)
                {
                    if (t < turnSeconds) { angle = turnFrom + turnDegrees * EaseSnap(t / turnSeconds); return; }
                    angle = Mathf.Repeat(turnFrom + turnDegrees, 360); phase = Phase.Light; phaseStart += turnSeconds;
                }
                else
                {
                    if (t < fadeSeconds) return;
                    lit = 0; phase = Phase.Light; phaseStart += fadeSeconds;
                }
            }
            phaseStart = now; // fell far behind: resume from here
        }

        private void Tick() { if (resolvedStyle.display == DisplayStyle.None || !visible) return; Advance(Now); MarkDirtyRepaint(); }

        // Cubic ease close to the tokens' ease-snap (0.2, 0.9, 0.1, 1): a quick lock-on that settles.
        private static float EaseSnap(float t) { t = Mathf.Clamp01(t); var u = 1 - t; return 1 - u * u * u * u; }

        private void Draw(MeshGenerationContext ctx)
        {
            var rect = contentRect; if (rect.width < 4 || rect.height < 4) return;
            var p = ctx.painter2D; var centre = rect.center; var radius = Mathf.Min(rect.width, rect.height) * .42f;
            var turn = Quaternion.AngleAxis(angle, Axis) * Tilt;
            var points = new Vector2[Vertices.Length]; var depth = new float[Vertices.Length];
            for (var i = 0; i < Vertices.Length; i++)
            {
                var v = turn * Vertices[i]; var k = 3.2f / (3.2f - v.z); // gentle perspective; z toward the viewer
                points[i] = centre + new Vector2(v.x, -v.y) * radius * k; depth[i] = v.z;
            }
            float Front(int a, int b) => Mathf.InverseLerp(-1, 1, (depth[a] + depth[b]) * .5f);
            p.lineCap = LineCap.Round; p.lineJoin = LineJoin.Round;
            foreach (var (a, b) in Trace) HudPaint.Line(p, points[a], points[b], HudPaint.Alpha(wire, wire.a * Mathf.Lerp(.45f, 1, Front(a, b))));

            var fade = phase == Phase.Fade ? 1 - Mathf.Clamp01((Now - phaseStart) / fadeSeconds) : 1;
            var current = phase == Phase.Light ? Mathf.Clamp01((Now - phaseStart) / edgeSeconds) : 0;
            var count = Mathf.Min(lit, Trace.Count);
            for (var i = 0; i <= count && i < Trace.Count; i++)
            {
                var (a, b) = Trace[i]; var amount = i < count ? 1 : current; if (amount <= 0) continue;
                var end = Vector2.Lerp(points[a], points[b], amount); var alpha = Mathf.Lerp(.4f, 1, Front(a, b)) * fade;
                HudPaint.Line(p, points[a], end, HudPaint.Alpha(trace, .16f * alpha), 5); // projector bleed
                HudPaint.Line(p, points[a], end, HudPaint.Alpha(trace, alpha), 1.6f);
                if (i == count && amount < 1) HudPaint.Arc(p, end, 1.6f, 0, 359.9f, HudPaint.Alpha(Color.Lerp(trace, Color.white, .6f), fade), 2.4f);
            }
        }

        private static Vector3[] BuildVertices()
        {
            var phi = (1 + Mathf.Sqrt(5)) / 2; var list = new List<Vector3>();
            foreach (var s in new[] { -1f, 1f }) foreach (var t in new[] { -1f, 1f })
            {
                list.Add(new Vector3(0, s, t * phi)); list.Add(new Vector3(s, t * phi, 0)); list.Add(new Vector3(t * phi, 0, s));
            }
            for (var i = 0; i < list.Count; i++) list[i] = list[i].normalized;
            return list.ToArray();
        }

        // Neighbours are the 30 closest pairs. The trace walks on from where the last edge ended, preferring the straightest
        // continuation; at a dead end it starts again at the nearest vertex that still has dark edges.
        private static List<(int, int)> BuildTrace(Vector3[] v)
        {
            var edges = new List<(int, int)>(); var shortest = float.MaxValue;
            for (var i = 0; i < v.Length; i++) for (var j = i + 1; j < v.Length; j++) shortest = Mathf.Min(shortest, Vector3.Distance(v[i], v[j]));
            for (var i = 0; i < v.Length; i++) for (var j = i + 1; j < v.Length; j++) if (Vector3.Distance(v[i], v[j]) < shortest * 1.01f) edges.Add((i, j));
            var left = new List<(int, int)>(edges); var trace = new List<(int, int)>(); int at = 0; var heading = Vector3.zero;
            while (left.Count > 0)
            {
                var best = -1; var score = float.MinValue;
                for (var k = 0; k < left.Count; k++)
                {
                    var (a, b) = left[k]; if (a != at && b != at) continue;
                    var to = a == at ? b : a; var s = Vector3.Dot(heading, (v[to] - v[at]).normalized);
                    if (s > score) { score = s; best = k; }
                }
                if (best < 0)
                {
                    var near = float.MaxValue;
                    foreach (var (a, b) in left) foreach (var c in new[] { a, b }) { var d = Vector3.Distance(v[at], v[c]); if (d < near) { near = d; best = c; } }
                    at = best; heading = Vector3.zero; continue;
                }
                var (e0, e1) = left[best]; var next = e0 == at ? e1 : e0;
                trace.Add((at, next)); heading = (v[next] - v[at]).normalized; at = next; left.RemoveAt(best);
            }
            return trace;
        }
    }
}
