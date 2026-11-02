using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.Gui
{
    /// <summary>
    /// Chamfered vector shapes for any element, painted with Painter2D and styled from USS custom properties, so a screen's stylesheet
    /// owns every colour. UI Toolkit has no clip-path, so the outline is painted; the element's content box and hit area stay its
    /// rectangle. This is the richer sibling of <see cref="VbShape"/> (the mesh painter behind <c>--vb-*</c>): a choice of which two
    /// corners are cut, lock-on brackets, a focus ring, glass and glow. The property names are <see cref="VbVectorVocabulary.Prefix"/> followed by
    /// (<c>--vb-vector-</c> by default; veilbreak's AR HUD reads the bare <c>--cut</c>, <c>--fill</c> and so on):
    ///   cut (chamfer size) flip (1: cut top-right/bottom-left instead of top-left/bottom-right)
    ///   fill [fill-to fill-dir (degrees, 0 = left to right, 90 = top to bottom)] stroke stroke-width glow
    ///   bracket [bracket-2 (the second corner) bracket-mask (1 TL, 2 TR, 4 BR, 8 BL; 0 = the two square corners)
    ///   bracket-len bracket-w bracket-out (px outside the outline; negative = flush inside)]
    ///   ring [ring-gap]: a focus frame, 2px in ring after a 2px gap in ring-gap.
    ///   Glass: sheen (top-lit: fades to clear at half height) scan [scan-pitch] (scanlines inside the outline)
    ///   halo (projector bleed: two soft strokes under stroke).
    /// A change of look (hover, selection, a class <see cref="VbRebuild"/> replays) blends over <see cref="VbVectorVocabulary.Blend"/> seconds.
    /// The stylesheet resets every property on the vocabulary's class, because custom properties inherit into nested shapes.
    /// </summary>
    public static class VbVectorShape
    {
        public const string ClassName = "vb-vector";
        public const string PropertyPrefix = "--vb-vector-";
        /// <summary>The gui vocabulary: <c>--vb-vector-*</c> properties and the <c>vb-vector</c> class.</summary>
        public static readonly VbVectorVocabulary Default = new VbVectorVocabulary(PropertyPrefix, ClassName);

        internal sealed class Properties
        {
            public readonly CustomStyleProperty<float> Cut;
            public readonly CustomStyleProperty<float> StrokeWidth;
            public readonly CustomStyleProperty<float> Flip;
            public readonly CustomStyleProperty<float> FillDir;
            public readonly CustomStyleProperty<float> BracketMask;
            public readonly CustomStyleProperty<float> BracketLength;
            public readonly CustomStyleProperty<float> BracketWidth;
            public readonly CustomStyleProperty<float> BracketOut;
            public readonly CustomStyleProperty<Color> Fill;
            public readonly CustomStyleProperty<Color> FillTo;
            public readonly CustomStyleProperty<Color> Stroke;
            public readonly CustomStyleProperty<Color> Glow;
            public readonly CustomStyleProperty<Color> Bracket;
            public readonly CustomStyleProperty<Color> Bracket2;
            public readonly CustomStyleProperty<Color> Ring;
            public readonly CustomStyleProperty<Color> RingGap;
            public readonly CustomStyleProperty<Color> Sheen;
            public readonly CustomStyleProperty<Color> Scan;
            public readonly CustomStyleProperty<Color> Halo;
            public readonly CustomStyleProperty<float> ScanPitch;
            public Properties(string prefix)
            {
                Cut = new CustomStyleProperty<float>(prefix + "cut");
                StrokeWidth = new CustomStyleProperty<float>(prefix + "stroke-width");
                Flip = new CustomStyleProperty<float>(prefix + "flip");
                FillDir = new CustomStyleProperty<float>(prefix + "fill-dir");
                BracketMask = new CustomStyleProperty<float>(prefix + "bracket-mask");
                BracketLength = new CustomStyleProperty<float>(prefix + "bracket-len");
                BracketWidth = new CustomStyleProperty<float>(prefix + "bracket-w");
                BracketOut = new CustomStyleProperty<float>(prefix + "bracket-out");
                Fill = new CustomStyleProperty<Color>(prefix + "fill");
                FillTo = new CustomStyleProperty<Color>(prefix + "fill-to");
                Stroke = new CustomStyleProperty<Color>(prefix + "stroke");
                Glow = new CustomStyleProperty<Color>(prefix + "glow");
                Bracket = new CustomStyleProperty<Color>(prefix + "bracket");
                Bracket2 = new CustomStyleProperty<Color>(prefix + "bracket-2");
                Ring = new CustomStyleProperty<Color>(prefix + "ring");
                RingGap = new CustomStyleProperty<Color>(prefix + "ring-gap");
                Sheen = new CustomStyleProperty<Color>(prefix + "sheen");
                Scan = new CustomStyleProperty<Color>(prefix + "scan");
                Halo = new CustomStyleProperty<Color>(prefix + "halo");
                ScanPitch = new CustomStyleProperty<float>(prefix + "scan-pitch");
            }
        }

        private static readonly Color Ground = new Color(.02f, .008f, .03f, 1);

        private sealed class Paint
        {
            public float cut = 10, strokeWidth = 1, dir, bracketLength = 8, bracketWidth = 2, bracketOut = 3, scanPitch = 3;
            public bool flip; public int mask;
            public Color fill, fillTo, stroke, glow, bracket, bracket2, ring, ringGap, sheen, scan, halo;

            // Between two resolved looks: colours and line weights blend, the rest is the target's.
            public static Paint Lerp(Paint a, Paint b, float t)
            {
                var p = (Paint)b.MemberwiseClone();
                p.strokeWidth = Mathf.Lerp(a.strokeWidth, b.strokeWidth, t); p.bracketWidth = Mathf.Lerp(a.bracketWidth, b.bracketWidth, t);
                p.fill = Color.Lerp(a.fill, b.fill, t); p.fillTo = Color.Lerp(a.fillTo, b.fillTo, t); p.stroke = Color.Lerp(a.stroke, b.stroke, t); p.glow = Color.Lerp(a.glow, b.glow, t);
                p.bracket = Color.Lerp(a.bracket, b.bracket, t); p.bracket2 = Color.Lerp(a.bracket2, b.bracket2, t); p.ring = Color.Lerp(a.ring, b.ring, t); p.ringGap = Color.Lerp(a.ringGap, b.ringGap, t);
                p.sheen = Color.Lerp(a.sheen, b.sheen, t); p.scan = Color.Lerp(a.scan, b.scan, t); p.halo = Color.Lerp(a.halo, b.halo, t);
                return p;
            }
        }

        // Each shape's look, as last painted, so overlays (a focus glow, say) can trace the same outline.
        private static readonly ConditionalWeakTable<VisualElement, Paint> Painted = new ConditionalWeakTable<VisualElement, Paint>();

        /// <summary>The chamfered outline grown by <paramref name="inset"/> px, in local coordinates. A plain element gives its rectangle.</summary>
        public static Vector2[] Outline(VisualElement element, float inset)
        {
            float w = element.layout.width, h = element.layout.height;
            if (!Painted.TryGetValue(element, out var paint) || w <= 0 || h <= 0)
                return new[] { new Vector2(-inset, -inset), new Vector2(w + inset, -inset), new Vector2(w + inset, h + inset), new Vector2(-inset, h + inset) };
            return Outline(paint.flip, w, h, Mathf.Min(paint.cut, w / 3, h / 3), inset);
        }

        /// <summary>The outline of a <paramref name="w"/> × <paramref name="h"/> shape with chamfer <paramref name="c"/>, grown by <paramref name="inset"/>.</summary>
        public static Vector2[] Outline(bool flip, float w, float h, float c, float inset) => !flip
            ? new[] { new Vector2(c - inset, -inset), new Vector2(w + inset, -inset), new Vector2(w + inset, h - c + inset), new Vector2(w - c + inset, h + inset), new Vector2(-inset, h + inset), new Vector2(-inset, c - inset) }
            : new[] { new Vector2(-inset, -inset), new Vector2(w - c + inset, -inset), new Vector2(w + inset, c - inset), new Vector2(w + inset, h + inset), new Vector2(c - inset, h + inset), new Vector2(-inset, h - c + inset) };

        /// <summary>A linear fill that keeps both colours' alpha (FillGradient.MakeLinearGradient(Color, Color, ...) drops it).</summary>
        public static FillGradient Linear(Color from, Color to, Vector2 start, Vector2 end)
        {
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(from, 0), new GradientColorKey(to, 1) }, new[] { new GradientAlphaKey(from.a, 0), new GradientAlphaKey(to.a, 1) });
            return FillGradient.MakeLinearGradient(gradient, start, end, AddressMode.Clamp);
        }

        /// <summary>Paints <paramref name="element"/> as a shape from its USS custom properties (those of <paramref name="vocabulary"/>, <see cref="Default"/> when omitted) and returns it.</summary>
        public static T Attach<T>(T element, VbVectorVocabulary vocabulary = null) where T : VisualElement
        {
            vocabulary ??= Default; var props = vocabulary.properties;
            Paint target = null, from = null; var since = 0f;
            var blending = element.schedule.Execute(() => element.MarkDirtyRepaint()).Every(0); blending.Pause();
            // The look on screen now: the target, or a blend toward it while a change is under way.
            Paint Shown()
            {
                var t = Mathf.Clamp01((Time.realtimeSinceStartup - since) / vocabulary.Blend);
                if (from == null || t >= 1) { from = null; blending.Pause(); return target; }
                return Paint.Lerp(from, target, 1 - (1 - t) * (1 - t));
            }
            element.AddToClassList(vocabulary.ClassName);
            element.RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                var style = element.customStyle; var next = new Paint();
                float Number(CustomStyleProperty<float> property, float fallback) => style.TryGetValue(property, out var value) ? value : fallback;
                Color Colour(CustomStyleProperty<Color> property) => style.TryGetValue(property, out var value) ? value : Color.clear;
                next.cut = Number(props.Cut, 10); next.strokeWidth = Number(props.StrokeWidth, 1); next.flip = Number(props.Flip, 0) > .5f; next.dir = Number(props.FillDir, 0);
                next.mask = Mathf.RoundToInt(Number(props.BracketMask, 0)); next.bracketLength = Number(props.BracketLength, 8); next.bracketWidth = Number(props.BracketWidth, 2); next.bracketOut = Number(props.BracketOut, 3);
                next.fill = Colour(props.Fill); next.fillTo = Colour(props.FillTo); next.stroke = Colour(props.Stroke); next.glow = Colour(props.Glow);
                next.bracket = Colour(props.Bracket); next.bracket2 = Colour(props.Bracket2); next.ring = Colour(props.Ring); next.ringGap = Colour(props.RingGap);
                next.sheen = Colour(props.Sheen); next.scan = Colour(props.Scan); next.halo = Colour(props.Halo); next.scanPitch = Mathf.Max(2, Number(props.ScanPitch, 3));
                // The first look snaps; later ones blend from whatever is on screen.
                if (target != null && element.panel != null) { from = Shown(); since = Time.realtimeSinceStartup; blending.Resume(); }
                target = next; Painted.AddOrUpdate(element, next);
                element.MarkDirtyRepaint();
            });
            element.RegisterCallback<GeometryChangedEvent>(_ => element.MarkDirtyRepaint());
            element.generateVisualContent += context =>
            {
                if (target == null) return;
                var paint = Shown();
                float w = element.layout.width, h = element.layout.height, c = Mathf.Min(paint.cut, w / 3, h / 3);
                if (w <= 0 || h <= 0) return;
                var p = context.painter2D;
                void Path(float inset)
                {
                    var points = Outline(paint.flip, w, h, c, inset);
                    p.BeginPath(); p.MoveTo(points[0]); for (var i = 1; i < points.Length; i++) p.LineTo(points[i]); p.ClosePath();
                }
                if (paint.glow.a > 0)
                    for (var i = 3; i >= 1; i--) { Path(i * 3); p.strokeColor = new Color(paint.glow.r, paint.glow.g, paint.glow.b, paint.glow.a / (i + 1)); p.lineWidth = 6; p.Stroke(); }
                if (paint.ring.a > 0)
                {
                    p.lineJoin = LineJoin.Miter; p.lineWidth = 2;
                    p.strokeColor = paint.ringGap.a > 0 ? paint.ringGap : Ground; Path(1); p.Stroke();
                    p.strokeColor = paint.ring; Path(3); p.Stroke();
                }
                if (paint.fill.a > 0 || paint.fillTo.a > 0)
                {
                    Path(0);
                    if (paint.fillTo.a > 0)
                    {
                        var angle = paint.dir * Mathf.Deg2Rad; var d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); var mid = new Vector2(w / 2, h / 2); var half = (Mathf.Abs(d.x) * w + Mathf.Abs(d.y) * h) / 2;
                        p.fillGradient = Linear(paint.fill, paint.fillTo, mid - d * half, mid + d * half);
                    }
                    else p.fillColor = paint.fill;
                    p.Fill(); p.fillColor = paint.fill;
                }
                // Top-lit glass, then scanlines clipped to the chamfers (Painter2D cannot clip, so each line spans the outline's width at its row).
                if (paint.sheen.a > 0) { Path(0); p.fillGradient = Linear(paint.sheen, new Color(paint.sheen.r, paint.sheen.g, paint.sheen.b, 0), Vector2.zero, new Vector2(0, h * .5f)); p.Fill(); p.fillColor = paint.fill; }
                if (paint.scan.a > 0)
                {
                    p.strokeColor = paint.scan; p.lineWidth = 1; p.lineCap = LineCap.Butt; p.BeginPath();
                    for (var y = paint.scanPitch * .5f; y < h - .5f; y += paint.scanPitch)
                    {
                        float x0 = 1, x1 = w - 1;
                        if (!paint.flip) { if (y < c) x0 = c - y + 1; if (y > h - c) x1 = w - (y - (h - c)) - 1; }
                        else { if (y < c) x1 = w - (c - y) - 1; if (y > h - c) x0 = y - (h - c) + 1; }
                        if (x1 > x0) { p.MoveTo(new Vector2(x0, y)); p.LineTo(new Vector2(x1, y)); }
                    }
                    p.Stroke();
                }
                if (paint.stroke.a > 0 && paint.strokeWidth > 0)
                {
                    p.lineJoin = LineJoin.Miter;
                    // Projector bleed: two soft strokes centred on the frame, under the crisp one.
                    if (paint.halo.a > 0)
                    {
                        Path(0); p.strokeColor = new Color(paint.halo.r, paint.halo.g, paint.halo.b, paint.halo.a * .5f); p.lineWidth = paint.strokeWidth + 7; p.Stroke();
                        p.strokeColor = paint.halo; p.lineWidth = paint.strokeWidth + 3; p.Stroke();
                    }
                    Path(0); p.strokeColor = paint.stroke; p.lineWidth = paint.strokeWidth; p.Stroke();
                }
                // Lock-on brackets just outside the two square corners, or at the corners the mask names.
                if (paint.bracket.a > 0)
                {
                    var o = paint.bracketOut; var arm = paint.bracketLength + o;
                    p.lineWidth = paint.bracketWidth; p.lineCap = LineCap.Butt; p.lineJoin = LineJoin.Miter;
                    void Mark(float x, float y, float sx, float sy, Color color)
                    {
                        p.strokeColor = color; p.BeginPath(); p.MoveTo(new Vector2(x + sx * arm, y)); p.LineTo(new Vector2(x, y)); p.LineTo(new Vector2(x, y + sy * arm)); p.Stroke();
                    }
                    var second = paint.bracket2.a > 0 ? paint.bracket2 : paint.bracket;
                    var mask = paint.mask > 0 ? paint.mask : paint.flip ? 5 : 10;
                    if ((mask & 1) != 0) Mark(-o, -o, 1, 1, paint.bracket);
                    if ((mask & 2) != 0) Mark(w + o, -o, -1, 1, paint.mask > 0 || !paint.flip ? paint.bracket : second);
                    if ((mask & 4) != 0) Mark(w + o, h + o, -1, -1, paint.mask > 0 ? paint.bracket : second);
                    if ((mask & 8) != 0) Mark(-o, h + o, 1, -1, paint.mask > 0 ? paint.bracket : second);
                }
            };
            return element;
        }
    }

    /// <summary>Which custom properties a shape reads, which class marks it and how long a change of look blends.</summary>
    public sealed class VbVectorVocabulary
    {
        public readonly string Prefix, ClassName;
        public readonly float Blend;
        internal readonly VbVectorShape.Properties properties;
        public VbVectorVocabulary(string prefix, string className, float blend = .2f)
        {
            Prefix = prefix; ClassName = className; Blend = blend; properties = new VbVectorShape.Properties(prefix);
        }
    }
}
