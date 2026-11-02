using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Veilbreak.ARHud.Tests
{
    public class HudTests
    {
        [TestCase(-1f, 0f)] [TestCase(2f, 1f)] [TestCase(.4f, .4f)]
        public void GaugeClampsInput(float input, float expected)
        {
            var gauge = new HudGauge { Value = input }; Assert.AreEqual(expected, gauge.Value);
        }
        [Test] public void GaugeRejectsNaN()
        {
            var gauge = new HudGauge { Value = float.NaN }; Assert.AreEqual(0, gauge.Value);
        }
        [Test] public void ModelOnlyNotifiesChangedValues()
        {
            var model = new HudModel(); int calls = 0;
            model.propertyChanged += (_, __) => calls++;
            model.Integrity = .5f; model.Integrity = .5f;
            Assert.AreEqual(1, calls);
        }
        [Test] public void PanelRoutesChildrenIntoBody()
        {
            var panel = new HudPanel { Title = "Test" }; var child = new Label("Body"); panel.Add(child);
            // Unity 7000 reports the logical parent (the panel) through `parent`; the element itself sits in the body.
            Assert.AreSame(panel.contentContainer, child.hierarchy.parent);
            Assert.AreSame(panel, child.parent);
            Assert.AreEqual("Test", panel.Title);
        }
        [Test] public void NavigationRejectsDuplicateIds()
        {
            var root = new VisualElement(); var content = new VisualElement(); root.Add(content);
            using (var nav = new HudNavigation(root, content))
            {
                nav.Register("one", new VisualElement());
                Assert.Throws<ArgumentException>(() => nav.Register("one", new VisualElement()));
                Assert.Throws<ArgumentException>(() => nav.Show("missing"));
            }
        }
        [Test] public void ModalsLayerAboveTheContentWithItsSheets()
        {
            var root = new VisualElement(); var content = new VisualElement(); root.Add(content); content.SetEnabled(false);
            var sheet = ScriptableObject.CreateInstance<StyleSheet>(); content.styleSheets.Add(sheet);
            try
            {
                using (var nav = new HudNavigation(root, content))
                {
                    var body = new Button(); nav.OpenModal(body, HudModal.Dim); var layer = body.hierarchy.parent;
                    Assert.AreSame(root, layer.hierarchy.parent); Assert.Greater(root.IndexOf(layer), root.IndexOf(content));
                    Assert.IsTrue(layer.ClassListContains(HudNavigation.ModalClass) && layer.ClassListContains(HudNavigation.Modal), "a dimmed layer is a scrim");
                    Assert.IsTrue(layer.styleSheets.Contains(sheet), "the body keeps the screens' look");
                    Assert.AreEqual(PickingMode.Ignore, layer.pickingMode, "only Block takes the pointer over the whole panel");
                    nav.CloseModal(); Assert.IsNull(layer.hierarchy.parent); Assert.IsFalse(nav.HasModal);
                    Assert.IsFalse(content.enabledSelf, "the content's own state is left alone");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(sheet); }
        }
        [Test] public void ShowingAScreenClosesOnlyDismissibleModals()
        {
            var root = new VisualElement(); var content = new VisualElement(); root.Add(content);
            using (var nav = new HudNavigation(root, content))
            {
                nav.Register("a", c => c.Add(new Label("a")));
                nav.OpenModal(new VisualElement(), HudModal.OwnInput); nav.OpenModal(new Button(), HudModal.Dialog);
                nav.Show("a");
                Assert.IsTrue(nav.HasModal, "an intro or loading cover stays up"); nav.CloseModal(); Assert.IsFalse(nav.HasModal);
            }
        }
        [Test] public void BuiltScreensRebuildIntoTheContent()
        {
            var root = new VisualElement(); var content = new VisualElement(); root.Add(content);
            using (var nav = new HudNavigation(root, content))
            {
                var kept = new VisualElement(); nav.Register("kept", kept); var builds = 0; string shown = null;
                nav.Register("a", c => { builds++; c.Add(new Label("a")); }); nav.Register("b", c => c.Add(new Label("b")));
                nav.ScreenChanged += id => shown = id;
                nav.Show("a"); nav.Show("a"); Assert.AreEqual(2, builds, "showing a screen again rebuilds it");
                nav.Show("b");
                Assert.AreEqual("b", nav.Current); Assert.AreEqual("b", shown);
                Assert.AreEqual(2, content.childCount, "the persistent screen and the new build");
                Assert.AreEqual(DisplayStyle.None, kept.style.display.value); Assert.AreEqual("b", ((Label)content[1]).text);
            }
        }
        [Test] public void ViewsAreFoundFromTheirElements()
        {
            var view = new HudView(); var label = new Label(); view.Content.Add(label);
            try
            {
                Assert.AreSame(view, HudView.Of(label)); Assert.AreSame(view.Content, view.Root[0]);
                Assert.IsTrue(view.Root.ClassListContains("ar-root") && view.Content.ClassListContains("ar-content"));
                Assert.IsNull(HudView.Of(new Label()));
            }
            finally { view.Dispose(); }
        }
        private sealed class Probe : HudViewModel { private int value; public int Value { get => value; set => Set(ref this.value, value); } }
        [Test] public void ViewModelsNotifyChangesByPropertyName()
        {
            var model = new Probe(); var names = new System.Collections.Generic.List<string>();
            model.propertyChanged += (_, e) => names.Add(e.propertyName.ToString());
            model.Value = 3; model.Value = 3; model.Value = 4;
            CollectionAssert.AreEqual(new[] { "Value", "Value" }, names);
        }
        [Test] public void ShowingScreenClosesModalAndSelectsOnlyOneScreen()
        {
            var root = new VisualElement(); var content = new VisualElement(); root.Add(content);
            using (var nav = new HudNavigation(root, content))
            {
                var a = new VisualElement(); var b = new VisualElement(); nav.Register("a", a); nav.Register("b", b);
                nav.Show("a"); nav.OpenModal(new Button()); nav.Show("b");
                Assert.IsFalse(nav.HasModal); Assert.IsTrue(content.enabledSelf);
                Assert.AreEqual(DisplayStyle.None, a.style.display.value); Assert.AreEqual(DisplayStyle.Flex, b.style.display.value);
            }
        }
        [Test] public void LoaderTracesEveryEdgeOnce()
        {
            Assert.AreEqual(30, HudLoader.Trace.Count);
            var edges = new System.Collections.Generic.HashSet<(int, int)>(); var degree = new int[12];
            foreach (var (a, b) in HudLoader.Trace)
            {
                Assert.IsTrue(edges.Add(a < b ? (a, b) : (b, a)), "edge traced twice"); degree[a]++; degree[b]++;
            }
            foreach (var d in degree) Assert.AreEqual(5, d);
        }
        [Test] public void LoaderTurnsSixtyDegreesAfterEachLitEdge()
        {
            var loader = new HudLoader { EdgeSeconds = .2f, TurnSeconds = .4f, TurnDegrees = 60 };
            loader.Restart(); var start = Time.realtimeSinceStartup;
            loader.Advance(start + .1f); Assert.AreEqual(0, loader.Lit); Assert.AreEqual(0, loader.Angle, 1e-3);
            loader.Advance(start + .21f); Assert.AreEqual(1, loader.Lit);
            loader.Advance(start + .61f); Assert.AreEqual(60, loader.Angle, 1e-3);
            loader.Advance(start + .81f); Assert.AreEqual(2, loader.Lit);
            loader.Advance(start + 1.21f); Assert.AreEqual(120, loader.Angle, 1e-3);
        }
        [Test] public void LoaderKeepsStillWithReducedMotion()
        {
            var loader = new HudLoader { EdgeSeconds = .2f, ReducedMotion = true }; loader.Restart(); var start = Time.realtimeSinceStartup;
            loader.Advance(start + .21f); loader.Advance(start + .41f); loader.Advance(start + .61f);
            Assert.AreEqual(3, loader.Lit); Assert.AreEqual(0, loader.Angle, 1e-3);
        }
        [Test] public void MotionMatchesElementsByName()
        {
            var before = new VisualElement(); before.Add(new Label("1,000") { name = "rating" }); before.Add(new VisualElement { name = "row" }); before.Add(new VisualElement { name = "row" });
            var snapshot = HudMotion.Capture(before);
            Assert.IsTrue(snapshot.Contains("/rating")); Assert.IsTrue(snapshot.Contains("/row")); Assert.IsTrue(snapshot.Contains("/row@2"));
            var after = new VisualElement(); var label = new Label("1,482") { name = "rating" }; label.AddToClassList("on"); after.Add(label);
            HudMotion.Play(after, snapshot, reduced: true);
            Assert.IsFalse(label.ClassListContains("on"), "the old classes show for one frame"); Assert.AreEqual("1,482", label.text);
        }
        [Test] public void ChangedTextFadesUnlessItDecodes()
        {
            var before = new VisualElement(); before.Add(new Label("ARIA") { name = "name" }); var was = new Label("RANGED") { name = "role" }; was.AddToClassList(HudMotion.Decode); before.Add(was);
            var snapshot = HudMotion.Capture(before);
            var after = new VisualElement(); var name = new Label("NYRA") { name = "name" }; var role = new Label("HEALER") { name = "role" }; role.AddToClassList(HudMotion.Decode);
            after.Add(name); after.Add(role);
            HudMotion.Play(after, snapshot, reduced: false);
            Assert.AreEqual("NYRA", name.text, "a plain label shows its new text at once"); Assert.Less(name.style.opacity.value, 1, "and fades it in");
            Assert.AreNotEqual("HEALER", role.text, "a decode label starts scrambled");
        }
        [Test] public void ShapesAreThemableFramesAndButtons()
        {
            var frame = new HudFrame(); var button = new HudButton();
            Assert.IsTrue(frame.ClassListContains(HudShape.ClassName) && frame.ClassListContains(HudFrame.ClassName));
            Assert.IsTrue(button.ClassListContains(HudShape.ClassName) && button.ClassListContains(HudButton.ClassName));
            Assert.AreEqual(4, HudShape.Outline(frame, 0).Length, "an unpainted shape traces its rectangle");
        }
        [Test] public void OutlineCutsTwoOppositeCorners()
        {
            var cut = HudShape.Outline(false, 100, 40, 10, 0);
            CollectionAssert.AreEqual(new[] { new Vector2(10, 0), new Vector2(100, 0), new Vector2(100, 30), new Vector2(90, 40), new Vector2(0, 40), new Vector2(0, 10) }, cut, "top-left and bottom-right");
            var flipped = HudShape.Outline(true, 100, 40, 10, 2);
            CollectionAssert.AreEqual(new[] { new Vector2(-2, -2), new Vector2(92, -2), new Vector2(102, 8), new Vector2(102, 42), new Vector2(8, 42), new Vector2(-2, 32) }, flipped, "top-right and bottom-left, grown by the inset");
        }
        [Test] public void HighContrastIsOpaqueAndReversible()
        {
            var theme = ScriptableObject.CreateInstance<HudTheme>();
            try
            {
                theme.ApplyPreset(HudPalette.HighContrast); Assert.IsTrue(theme.highContrast); Assert.AreEqual(1, theme.surface.a);
                theme.ApplyPreset(HudPalette.Veilbreak); Assert.IsFalse(theme.highContrast); Assert.AreEqual(.86f, theme.surface.a);
            }
            finally { UnityEngine.Object.DestroyImmediate(theme); }
        }
    }
}
