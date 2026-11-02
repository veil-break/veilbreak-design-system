// Headless adapters for HudInput and HudStage only. They do not establish Unity API compatibility or native event delivery.
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2(0, 0);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public float magnitude => (float)System.Math.Sqrt(x * x + y * y);
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }
    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
        public float xMin => x;
        public float yMin => y;
        public float xMax => x + width;
        public float yMax => y + height;
        public static Rect MinMaxRect(float xMin, float yMin, float xMax, float yMax) => new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
    // The runner stands in for a device build: HudStage applies the safe area only while playing.
    public static class Application { public static bool isPlaying = true, isMobilePlatform = true; }
    public static class Debug { public static void LogWarning(object message) { } }
    public static class Screen { public static int width = 2400, height = 1080; public static float dpi = 420; public static Rect safeArea = new Rect(80, 24, 2320, 1056); }
    public static class Mathf
    {
        public static int RoundToInt(float v) => (int)Math.Round(v);
        public static int CeilToInt(float v) => (int)Math.Ceiling(v);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static float Min(float a, float b) => Math.Min(a, b);
        public static float Clamp01(float v) => Math.Clamp(v, 0, 1);
    }
}
namespace UnityEngine.UIElements
{
    public enum PickingMode { Position, Ignore }
    public enum Position { Relative, Absolute }
    public enum StyleKeyword { Auto }
    public struct StyleLength
    {
        public float value;
        public static implicit operator StyleLength(float v) => new StyleLength { value = v };
        public static implicit operator StyleLength(StyleKeyword _) => new StyleLength { value = float.NaN };
    }
    public struct TransformOrigin { public TransformOrigin(float x, float y) { } }
    public struct Scale { public UnityEngine.Vector3 value; public Scale(UnityEngine.Vector3 v) { value = v; } }
    public struct StyleScale { public Scale value; public StyleScale(Scale v) { value = v; } }
    public sealed class Style
    {
        public StyleLength width = float.NaN, height = float.NaN, left = float.NaN, top = float.NaN, right = float.NaN, bottom = float.NaN;
        public TransformOrigin transformOrigin;
        public StyleScale scale = new StyleScale(new Scale(new UnityEngine.Vector3(1, 1, 1)));
        public Position position;
    }
    public interface IPanel { VisualElement Pick(UnityEngine.Vector2 point); VisualElement visualTree { get; } }
    public sealed class TestPanel : IPanel
    {
        public UnityEngine.Vector2 origin, extent = new UnityEngine.Vector2(1200, 540), lastPick;
        public Func<UnityEngine.Vector2, VisualElement> pick = _ => null;
        public VisualElement Pick(UnityEngine.Vector2 point) { lastPick = point; return pick(point); }
        // The panel root covers the whole screen: origin to origin + extent in panel units.
        private readonly VisualElement root = new VisualElement();
        public VisualElement visualTree { get { root.layout = new UnityEngine.Rect(origin.x, origin.y, extent.x, extent.y); return root; } }
    }
    public static class RuntimePanelUtils
    {
        public static UnityEngine.Vector2 ScreenToPanel(IPanel p, UnityEngine.Vector2 s)
        {
            var panel = (TestPanel)p;
            return new UnityEngine.Vector2(panel.origin.x + s.x * panel.extent.x / UnityEngine.Screen.width,
                panel.origin.y + s.y * panel.extent.y / UnityEngine.Screen.height);
        }
    }
    public sealed class GeometryChangedEvent { }
    [AttributeUsage(AttributeTargets.Class)] public sealed class UxmlElementAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)] public sealed class UxmlAttributeAttribute : Attribute { public UxmlAttributeAttribute() { } public UxmlAttributeAttribute(string name) { } }
    public class VisualElement
    {
        private readonly List<VisualElement> children = new();
        private readonly HashSet<string> classes = new();
        private IPanel ownPanel;
        private UnityEngine.Rect bounds;
        public VisualElement parent { get; private set; }
        public IPanel panel { get => ownPanel ?? parent?.panel; set => ownPanel = value; }
        // Only the panel root's world bound is read (HudStage.FromViewport); a root's world bound is its layout.
        public UnityEngine.Rect worldBound => layout;
        public readonly Style style = new Style();
        public PickingMode pickingMode;
        public UnityEngine.Rect layout
        {
            get => new UnityEngine.Rect(bounds.x, bounds.y, float.IsNaN(style.width.value) ? bounds.width : style.width.value,
                float.IsNaN(style.height.value) ? bounds.height : style.height.value);
            set => bounds = value;
        }
        // Like Unity: Add() targets contentContainer (a HudStage's board), hierarchy.Add() the element itself.
        public virtual VisualElement contentContainer => this;
        public void Add(VisualElement child) => contentContainer.hierarchy.Add(child);
        public Hierarchy hierarchy => new Hierarchy(this);
        public readonly struct Hierarchy
        {
            private readonly VisualElement owner;
            public Hierarchy(VisualElement owner) { this.owner = owner; }
            public void Add(VisualElement child) { owner.children.Add(child); child.parent = owner; }
        }
        public IEnumerable<T> Query<T>() where T : VisualElement => (this is T self ? new[] { self } : Enumerable.Empty<T>()).Concat(children.SelectMany(c => c.Query<T>()));
        public T GetFirstAncestorOfType<T>() where T : VisualElement { for (var p = parent; p != null; p = p.parent) if (p is T t) return t; return null; }
        public IEnumerable<VisualElement> Children() => children;
        public void AddToClassList(string name) => classes.Add(name);
        public void EnableInClassList(string name, bool enable) { if (enable) classes.Add(name); else classes.Remove(name); }
        public bool ClassListContains(string name) => classes.Contains(name);
        public bool Contains(VisualElement child) => child == this || children.Any(c => c.Contains(child));
        public void RegisterCallback<T>(Action<T> callback) { }
        public void RemoveFromHierarchy() { parent?.children.Remove(this); parent = null; }
        public UnityEngine.Vector2 LocalToWorld(UnityEngine.Vector2 point)
        {
            var p = new UnityEngine.Vector2(point.x * style.scale.value.value.x + layout.x, point.y * style.scale.value.value.y + layout.y);
            return parent == null ? p : parent.LocalToWorld(p);
        }
        public UnityEngine.Vector2 WorldToLocal(UnityEngine.Vector2 point)
        {
            var p = parent == null ? point : parent.WorldToLocal(point);
            return new UnityEngine.Vector2((p.x - layout.x) / style.scale.value.value.x, (p.y - layout.y) / style.scale.value.value.y);
        }
    }
    public class Button : VisualElement { }
    public class ScrollView : VisualElement { }
    public class BaseField<T> : VisualElement { public const string ussClassName = "unity-base-field"; }
}

namespace Veilbreak.ARHud
{
    // The helmet-glass chrome HudStage hosts above its board; its drawing has no bearing on input or layout checks.
    public class HudVisor : UnityEngine.UIElements.VisualElement { public HudVisor(HudStage stage) { } public void MarkDirtyRepaint() { } }
}
