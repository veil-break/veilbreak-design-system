using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// Animates the difference between two builds of the same UI, for screens that are rebuilt on every change. Capture the
    /// tree before tearing it down, build the new one, then Play: elements are matched by their path of names (or type and
    /// sibling index), and a changed class list replays from the old classes one frame later (so USS transitions and painted
    /// controls ease into the new state), a changed number counts to its new value, other changed text fades in (labels with
    /// the <see cref="Decode"/> class decode left to right instead), a resized bar grows, and an element that is new or newly
    /// shown fades in. Only named elements slide when they move: an unnamed one is matched by its index, so after a list
    /// changes it would slide from another row's place. Removed elements simply go. Reduced motion keeps colour and opacity
    /// changes only. Labels with a data binding are left to their binding.
    /// </summary>
    public static class HudMotion
    {
        public const string Skip = "ar-no-motion", Decode = "ar-decode";
        public const float Duration = .18f;

        public sealed class Snapshot
        {
            internal readonly Dictionary<string, State> states = new Dictionary<string, State>();
            public int Count => states.Count;
            public bool Contains(string key) => states.ContainsKey(key);
        }

        internal sealed class State
        {
            public HashSet<string> classes; public string text; public Rect bound; public bool shown, named; public StyleLength width, height;
        }

        private static readonly Regex Number = new Regex(@"-?\d[\d,]*(\.\d+)?", RegexOptions.CultureInvariant);
        private const string Glyphs = "#%&*+/<=>?[]^_{}|~0123456789ABCDEF";
        private static readonly System.Random Random = new System.Random();

        public static Snapshot Capture(VisualElement scope)
        {
            var snapshot = new Snapshot(); if (scope == null) return snapshot;
            var origin = scope.worldBound.position;
            Walk(scope, "", (element, key) =>
            {
                var bound = element.worldBound;
                snapshot.states[key] = new State
                {
                    classes = new HashSet<string>(element.GetClasses()), text = (element as TextElement)?.text,
                    bound = new Rect(bound.position - origin, bound.size), shown = Shown(element), named = Named(element),
                    width = element.style.width, height = element.style.height,
                };
            });
            return snapshot;
        }

        public static void Play(VisualElement scope, Snapshot before, bool reduced)
        {
            if (scope == null || before == null || before.Count == 0) return;
            var matched = new List<(VisualElement element, State state)>(); var fresh = new List<VisualElement>();
            Walk(scope, "", (element, key) =>
            {
                if (before.states.TryGetValue(key, out var state)) { matched.Add((element, state)); return; }
                // Only the outermost new element of a subtree fades; its parent must have existed before.
                var parent = element.hierarchy.parent; var parentKey = key.Substring(0, Math.Max(0, key.LastIndexOf('/')));
                if (parent != null && (parent == scope || before.states.ContainsKey(parentKey))) fresh.Add(element);
            });
            foreach (var (element, state) in matched)
            {
                Replay(element, state.classes);
                if (element is TextElement label && state.text != null && label.text != state.text && !Bound(label)) Text(label, state.text, reduced);
                Size(element, state, reduced);
            }
            var order = 0;
            foreach (var element in fresh) FadeIn(element, order++, reduced);
            // Positions and visibility need the new layout: one pass after the first layout of the rebuilt tree.
            var anchor = scope.hierarchy.childCount > 0 ? scope.hierarchy[0] : scope;
            anchor.RegisterCallbackOnce<GeometryChangedEvent>(_ => Settle(scope, matched, reduced));
        }

        private static void Settle(VisualElement scope, List<(VisualElement element, State state)> matched, bool reduced)
        {
            var origin = scope.worldBound.position; var moved = new HashSet<VisualElement>(); var order = 0;
            foreach (var (element, state) in matched)
            {
                if (element.panel == null) continue;
                var shown = Shown(element);
                if (shown && !state.shown) { if (!Under(element, moved)) { moved.Add(element); FadeIn(element, order++, reduced); } continue; }
                if (reduced || !shown || !state.shown || !state.named || !Named(element) || Under(element, moved)) continue;
                var now = element.worldBound; var delta = state.bound.position - (now.position - origin);
                if (delta.sqrMagnitude < 2.25f || float.IsNaN(delta.x)) continue;
                var k = element.layout.width > 0 ? now.width / element.layout.width : 1; if (k <= 0 || float.IsNaN(k)) k = 1;
                moved.Add(element); Slide(element, delta / k, element.childCount == 0 && now.width > 0 && now.height > 0 ? new Vector2(state.bound.width / now.width, state.bound.height / now.height) : Vector2.one);
            }
        }

        private static bool Under(VisualElement element, HashSet<VisualElement> set)
        {
            for (var p = element.hierarchy.parent; p != null; p = p.hierarchy.parent) if (set.Contains(p)) return true;
            return false;
        }

        // The old classes for one frame, then the new ones: transitions run from the old look. Only the difference is put back,
        // so classes another system toggles meanwhile (focus marks, say) are kept.
        private static void Replay(VisualElement element, HashSet<string> old)
        {
            var added = new List<string>(); var removed = new List<string>();
            foreach (var c in element.GetClasses()) if (!old.Contains(c)) added.Add(c);
            foreach (var c in old) if (!element.ClassListContains(c)) removed.Add(c);
            if (added.Count == 0 && removed.Count == 0) return;
            foreach (var c in added) element.RemoveFromClassList(c);
            foreach (var c in removed) element.AddToClassList(c);
            element.schedule.Execute(() => { foreach (var c in removed) element.RemoveFromClassList(c); foreach (var c in added) element.AddToClassList(c); });
        }

        private static void Size(VisualElement element, State state, bool reduced)
        {
            void Grow(StyleLength from, StyleLength to, Action<StyleLength> set)
            {
                if (from.keyword != StyleKeyword.Undefined || to.keyword != StyleKeyword.Undefined) return;
                var a = from.value; var b = to.value; if (a.unit != b.unit || Mathf.Approximately(a.value, b.value)) return;
                set(from);
                Tween(element, reduced ? Duration * .5f : Duration * 1.6f, t => set(new Length(Mathf.Lerp(a.value, b.value, t), b.unit)), () => set(to));
            }
            Grow(state.width, element.style.width, v => element.style.width = v);
            Grow(state.height, element.style.height, v => element.style.height = v);
        }

        private static void Slide(VisualElement element, Vector2 from, Vector2 scale)
        {
            var stretch = Mathf.Abs(scale.x - 1) > .01f || Mathf.Abs(scale.y - 1) > .01f;
            if (stretch) element.style.transformOrigin = new TransformOrigin(0, 0);
            Tween(element, Duration * 1.4f, t =>
            {
                var u = 1 - t; element.style.translate = new Translate(from.x * u, from.y * u);
                if (stretch) element.style.scale = new Scale(new Vector3(Mathf.Lerp(scale.x, 1, t), Mathf.Lerp(scale.y, 1, t), 1));
            }, () =>
            {
                element.style.translate = StyleKeyword.Null;
                if (stretch) { element.style.scale = StyleKeyword.Null; element.style.transformOrigin = StyleKeyword.Null; }
            });
        }

        private static void FadeIn(VisualElement element, int order, bool reduced)
        {
            element.style.opacity = 0; var delay = Mathf.Min(order * .015f, .09f);
            Tween(element, Duration * 1.4f, t =>
            {
                element.style.opacity = t;
                if (!reduced) element.style.translate = new Translate(0, (1 - t) * 6);
            }, () => { element.style.opacity = StyleKeyword.Null; if (!reduced) element.style.translate = StyleKeyword.Null; }, delay);
        }

        // A changed number counts from the old value when the rest of the text is unchanged; other text fades in, or decodes left to
        // right on a label that opts in with the Decode class.
        private static void Text(TextElement label, string old, bool reduced)
        {
            var target = label.text; if (reduced || string.IsNullOrEmpty(target)) return;
            var a = Number.Matches(old); var b = Number.Matches(target);
            if (a.Count == 1 && b.Count == 1 && Number.Replace(old, "#") == Number.Replace(target, "#")
                && double.TryParse(a[0].Value.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out var from)
                && double.TryParse(b[0].Value.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out var to))
            {
                var match = b[0]; var decimals = match.Groups[1].Success ? match.Groups[1].Value.Length - 1 : 0; var grouped = match.Value.Contains(",");
                var format = (grouped ? "#,0" : "0") + (decimals > 0 ? "." + new string('0', decimals) : "");
                Write(label, target, Duration * 2.2f, t => target.Substring(0, match.Index) + (from + (to - from) * t).ToString(format, CultureInfo.InvariantCulture) + target.Substring(match.Index + match.Length));
                return;
            }
            if (!label.ClassListContains(Decode))
            {
                label.style.opacity = .2f;
                Tween(label, Duration, t => label.style.opacity = .2f + .8f * t, () => label.style.opacity = StyleKeyword.Null);
                return;
            }
            Write(label, target, Duration * 1.8f, t =>
            {
                var shown = new StringBuilder(target.Length); var settled = t * (target.Length + 3) - 3;
                for (var i = 0; i < target.Length; i++)
                    shown.Append(i < settled || char.IsWhiteSpace(target[i]) ? target[i] : Glyphs[Random.Next(Glyphs.Length)]);
                return shown.ToString();
            });
        }

        // Writes frames into the label until done; stops early if someone else sets the text meanwhile.
        private static void Write(TextElement label, string target, float duration, Func<float, string> frame)
        {
            var last = label.text = frame(0);
            Tween(label, duration, t =>
            {
                if (label.text != last) return false;
                label.text = last = frame(1 - (1 - t) * (1 - t)); return true;
            }, () => { if (label.text == last) label.text = target; });
        }

        private static void Tween(VisualElement element, float duration, Action<float> apply, Action done, float delay = 0) =>
            Tween(element, duration, t => { apply(t); return true; }, done, delay);

        // Runs apply(eased 0..1) on the element's scheduler in unscaled time; apply returning false cancels without `done`.
        private static void Tween(VisualElement element, float duration, Func<float, bool> apply, Action done, float delay = 0)
        {
            var start = Time.realtimeSinceStartup + delay; IVisualElementScheduledItem item = null;
            item = element.schedule.Execute(() =>
            {
                var now = Time.realtimeSinceStartup; if (now < start) return;
                var t = Mathf.Clamp01((now - start) / Mathf.Max(.001f, duration));
                if (!apply(1 - Mathf.Pow(1 - t, 3))) { item.Pause(); return; }
                if (t >= 1) { item.Pause(); done?.Invoke(); }
            }).Every(0);
        }

        private static bool Bound(VisualElement element) => element.HasBinding("text");
        private static bool Named(VisualElement element) => !string.IsNullOrEmpty(element.name) && !element.name.StartsWith("unity-", StringComparison.Ordinal);

        private static bool Shown(VisualElement element)
        {
            for (var e = element; e != null; e = e.hierarchy.parent) if (e.resolvedStyle.display == DisplayStyle.None) return false;
            return element.resolvedStyle.visibility != Visibility.Hidden;
        }

        // Depth-first, in tree order; a path segment is the element's own name (numbered when a sibling shares it), else its
        // type and index among its siblings.
        private static void Walk(VisualElement scope, string path, Action<VisualElement, string> visit)
        {
            Dictionary<string, int> seen = null;
            for (var i = 0; i < scope.hierarchy.childCount; i++)
            {
                var child = scope.hierarchy[i]; if (child.ClassListContains(Skip)) continue;
                string segment;
                if (!string.IsNullOrEmpty(child.name) && !child.name.StartsWith("unity-", StringComparison.Ordinal))
                {
                    seen ??= new Dictionary<string, int>(); seen.TryGetValue(child.name, out var count); seen[child.name] = ++count;
                    segment = count == 1 ? child.name : child.name + "@" + count;
                }
                else segment = child.GetType().Name + "#" + i;
                var key = path + "/" + segment; visit(child, key); Walk(child, key, visit);
            }
        }
    }
}
