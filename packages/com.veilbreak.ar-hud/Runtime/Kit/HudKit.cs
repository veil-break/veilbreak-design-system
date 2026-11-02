using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    // The shared kit: builders for the kit's controls and binding helpers for widgets declared in UXML. Every panel and button is
    // a HudShape (cut corners painted from USS custom properties; HudFrame / HudButton in UXML); the `pnl` / `press` classes
    // give them the kit look and the other classes named here are styled in Base.uss (cta, rail, icon-button, switch, segment,
    // bar, tag) and Kit.uss (the k-* desktop components). Colours come from the design tokens (--vb-color-*), faces from the
    // --vb-font-* variables the app defines.
    public static class HudKit
    {
        public static VisualElement Box(string css = "", params VisualElement[] children)
        {
            var box = new VisualElement(); Classes(box, css);
            foreach (var child in children) box.Add(child); return box;
        }
        public static Label Text(string value, string css = "") { var node = new Label(value); Classes(node, css); return node; }
        public static void Classes(VisualElement node, string css) { if (!string.IsNullOrEmpty(css)) foreach (var name in css.Split(' ', StringSplitOptions.RemoveEmptyEntries)) node.AddToClassList(name); }
        public static DropdownField Choice(string label, IEnumerable<string> options, string value, Action<string> changed)
        {
            var field = new DropdownField(label, options.ToList(), value); field.RegisterValueChangedCallback(e => changed(e.newValue)); return field;
        }

        // ---------- binding helpers for widgets declared in UXML ----------
        public static void Show(VisualElement element, bool visible) { if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None; }
        public static void Click(VisualElement root, string name, Action action) { var b = root.Q<Button>(name); if (b != null) b.clicked += action; }
        public static void SetText(VisualElement root, string name, string text) { var l = root.Q<Label>(name); if (l != null) l.text = text; }
        // A switch reads as on/off by knob position and fill, not colour alone.
        public static void BindSwitch(Button button, bool value, Action<bool> changed)
        {
            button.EnableInClassList("on", value);
            button.clicked += () => { value = !value; button.EnableInClassList("on", value); changed(value); };
        }
        // A segment is a HudFrame of buttons with the "seg" class; exactly one is "on".
        public static void BindSegment(VisualElement group, int selected, Action<int> changed)
        {
            var buttons = group.Query<Button>(className: "seg").ToList();
            for (var i = 0; i < buttons.Count; i++)
            {
                var index = i; buttons[i].EnableInClassList("on", i == selected);
                buttons[i].clicked += () => { for (var j = 0; j < buttons.Count; j++) buttons[j].EnableInClassList("on", j == index); changed(index); };
            }
        }
        public static void SetBar(VisualElement bar, float fraction) { var fill = bar?.Q(className: "bar-fill"); if (fill != null) fill.style.width = Length.Percent(Mathf.Clamp01(fraction) * 100); }
        public static Toggle BindToggle(VisualElement root, string name, bool value, Action<bool> changed) { var t = root.Q<Toggle>(name); t.SetValueWithoutNotify(value); t.RegisterValueChangedCallback(e => changed(e.newValue)); return t; }
        public static Slider BindSlider(VisualElement root, string name, float value, Action<float> changed) { var s = root.Q<Slider>(name); s.SetValueWithoutNotify(value); s.RegisterValueChangedCallback(e => changed(e.newValue)); return s; }
        public static TextField BindInput(VisualElement root, string name, string value, Action<string> changed) { var f = root.Q<TextField>(name); f.SetValueWithoutNotify(value); f.RegisterValueChangedCallback(e => changed(e.newValue)); return f; }
        public static IntegerField BindInteger(VisualElement root, string name, int value, int min, int max, Action<int> changed) { var f = root.Q<IntegerField>(name); f.SetValueWithoutNotify(value); f.RegisterValueChangedCallback(e => changed(Math.Max(min, Math.Min(max, e.newValue)))); return f; }
        public static DropdownField BindChoice(VisualElement root, string name, string value, Action<string> changed) { var f = root.Q<DropdownField>(name); f.SetValueWithoutNotify(value); f.RegisterValueChangedCallback(e => changed(e.newValue)); return f; }
        // Clears design-time sample children from a list container before real items are added.
        public static VisualElement Clear(VisualElement root, string name) { var list = root.Q(name); list?.Clear(); return list; }

        // ---------- kit controls ----------
        public static VisualElement Panel(string css = "", params VisualElement[] children) => HudShape.Attach(Box("pnl " + css, children));

        public static Button Press(Action action, string css = "", params VisualElement[] children)
        {
            var button = new Button(action); Classes(button, "press " + css);
            foreach (var child in children) button.Add(child);
            return HudShape.Attach(button);
        }

        public static Button Cta(string title, string sub, Action action)
        {
            var button = Press(action, "cta", Text(title, "cta-title disp"));
            if (!string.IsNullOrEmpty(sub)) button.Add(Text(sub, "cta-sub num"));
            HudFx.AddSheen(button);
            return button;
        }

        public static Button Rail(string icon, string label, Action action, bool badge = false)
        {
            var button = Press(action, "rail", new HudIcon(icon), Text(label, "rail-label"));
            if (badge) { var dot = new HudIcon("diamond", 9); dot.AddToClassList("badge-new"); button.Add(dot); }
            return button;
        }

        public static Button IconButton(string icon, Action action, string label, string css = "")
        {
            var button = Press(action, "icon-button " + css, new HudIcon(icon, 18)); button.tooltip = label; return button;
        }

        // A switch reads as on/off by knob position and fill, not colour alone.
        public static Button Switch(string label, bool value, Action<bool> changed, string css = "")
        {
            var knob = Box("knob", Box("knob-dot"));
            Button button = null;
            button = Press(() => { value = !value; button.EnableInClassList("on", value); changed(value); }, "switch " + css, knob, Text(label, "switch-label"));
            button.EnableInClassList("on", value);
            return button;
        }

        public static VisualElement Segment(IList<(string title, string sub)> options, int selected, Action<int> changed, string css = "")
        {
            var group = Panel("segment " + css);
            var buttons = new List<Button>();
            for (var i = 0; i < options.Count; i++)
            {
                var index = i;
                var button = new Button(() => { for (var j = 0; j < buttons.Count; j++) buttons[j].EnableInClassList("on", j == index); changed(index); });
                button.AddToClassList("seg");
                button.Add(Text(options[i].title, "seg-title"));
                if (!string.IsNullOrEmpty(options[i].sub)) button.Add(Text(options[i].sub, "seg-sub num"));
                button.EnableInClassList("on", i == selected);
                buttons.Add(button); group.Add(button);
            }
            return group;
        }

        public static VisualElement Bar(float fraction, string css = "")
        {
            var fill = Box("bar-fill"); fill.style.width = Length.Percent(Mathf.Clamp01(fraction) * 100);
            return Box("bar " + css, fill);
        }

        public static Label Tag(string text, string css = "") => Text(text, "tag " + css);
    }
}
