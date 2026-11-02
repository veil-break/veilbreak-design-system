using UnityEngine.UIElements;

namespace Veilbreak.ARHud
{
    /// <summary>
    /// The sample composition the Theme Workshop previews and a <see cref="HudHost"/> shows without a layout: three screens
    /// (menu, battle, loadout) on a <see cref="HudStage"/> (so they scale per board and carry the visor chrome), built from the kit
    /// (<see cref="HudKit"/>: panels, presses, a CTA, a switch, a segment, a bar) and the themed controls (<see cref="HudPanel"/>,
    /// <see cref="HudGauge"/>, <see cref="HudReticle"/>, <see cref="HudLoader"/>). The kit's look comes from Base.uss and Kit.uss
    /// on the view (and from the app's token and face variables when its sheets are there too); the themed controls and the
    /// chrome from the view's <see cref="HudTheme"/> and <see cref="HudEffects"/>.
    /// </summary>
    public static class HudGallery
    {
        public static void Build(HudView view, HudModel model)
        {
            view.Root.dataSource = model;
            var nav = view.Navigation;
            nav.Register("menu", content => Menu(view, content));
            nav.Register("battle", content => Battle(view, model, content));
            nav.Register("loadout", content => Loadout(view, content));
            nav.Show("menu");
        }

        private static void Menu(HudView view, VisualElement content)
        {
            var stage = Stage(content);
            var title = Anchor(stage, left: "40", top: "32");
            title.Add(HudKit.Text("VEIL / BREAK", "ar-g-title disp"));
            title.Add(HudKit.Text("AUGMENTED INTERFACE  /  SYSTEM 07", "label-caps"));
            var menu = Anchor(stage, left: "40", bottom: "32");
            var panel = HudKit.Panel("ar-g-menu");
            panel.Add(HudKit.Cta("DEPLOY", "ENTER HUD", () => view.Navigation.Show("battle")));
            panel.Add(Press("LOADOUT", () => view.Navigation.Show("loadout"), "autofocus"));
            panel.Add(Press("SYSTEM STATUS", () => Status(view)));
            menu.Add(panel);
            var side = Anchor(stage, right: "40", bottom: "32");
            var link = new HudPanel { Title = "NEURAL LINK" }; link.AddToClassList("ar-g-side");
            var ring = new HudGauge(); HudBinding.To(ring, nameof(HudGauge.Value), nameof(HudModel.Integrity));
            link.Add(ring); link.Add(HudKit.Text("INTEGRITY / STABLE", "ar-muted"));
            side.Add(link);
            view.Navigation.Back = null;
            view.RefreshTheme();
        }

        private static void Battle(HudView view, HudModel model, VisualElement content)
        {
            var stage = Stage(content);
            var top = Anchor(stage, left: "24", top: "20", right: "24");
            var row = HudKit.Box("row");
            var objective = new HudPanel { Title = "OBJECTIVE / 01" }; objective.style.flexGrow = 1;
            var objectiveText = new Label(); HudBinding.To(objectiveText, "text", nameof(HudModel.Objective));
            objective.Add(objectiveText); row.Add(objective);
            row.Add(HudKit.IconButton("close", () => view.Navigation.Show("menu"), "MENU"));
            top.Add(row);
            var centre = Anchor(stage, left: "0", top: "0", right: "0", bottom: "0");
            centre.AddToClassList("ar-open"); centre.Add(new HudReticle());
            var bottom = Anchor(stage, left: "24", right: "24", bottom: "20");
            var strip = HudKit.Box("row ar-g-strip");
            var vitals = new HudPanel { Title = "SUIT INTEGRITY" };
            var gauge = new HudGauge(); HudBinding.To(gauge, nameof(HudGauge.Value), nameof(HudModel.Integrity));
            vitals.Add(gauge); strip.Add(vitals);
            var skills = HudKit.Panel("ar-g-skills grow");
            skills.Add(HudKit.Text("TACTICAL SYSTEMS", "label-caps"));
            var abilities = HudKit.Box("row");
            abilities.Add(Press("01 / PULSE", () => { model.Integrity -= .05f; view.Navigation.Toast("PULSE EMITTED / DEMO"); }, "autofocus"));
            abilities.Add(Press("02 / REPAIR", () => { model.Integrity += .1f; view.Navigation.Toast("INTEGRITY RESTORED"); }));
            skills.Add(abilities);
            var bar = HudKit.Bar(model.Integrity, "ar-g-bar");
            skills.Add(bar); skills.schedule.Execute(() => HudKit.SetBar(bar, model.Integrity)).Every(100);
            skills.Add(HudKit.Text("DEMO CONTROLS / CLICK TO UPDATE BOUND TELEMETRY", "ar-muted"));
            strip.Add(skills);
            var ammo = HudKit.Panel("ar-g-ammo");
            ammo.Add(HudKit.Text("MAGAZINE", "label-caps"));
            var count = HudKit.Text("", "ar-number num"); HudBinding.To(count, "text", nameof(HudModel.Ammunition));
            ammo.Add(count); strip.Add(ammo);
            bottom.Add(strip);
            view.Navigation.Back = () => () => view.Navigation.Show("menu");
            view.RefreshTheme();
        }

        private static void Loadout(HudView view, VisualElement content)
        {
            var stage = Stage(content);
            var anchor = Anchor(stage, left: "40", top: "32", bottom: "32");
            var panel = HudKit.Panel("ar-g-loadout");
            panel.Add(HudKit.Text("LOADOUT / SELECT A MODULE", "label-caps"));
            panel.Add(HudKit.Segment(new[] { ("CORE", ""), ("PLATING", ""), ("SIGNAL", "") }, 0, _ => { }));
            var list = new ScrollView(); list.AddToClassList("grow");
            foreach (var name in new[] { "PULSE / AREA SCAN", "BARRIER / DEFENSE", "REPAIR / RECOVERY", "LANCE / PRECISION" })
            {
                var captured = name;
                list.Add(Press(name, () => view.Navigation.Toast("EQUIPPED: " + captured), list.childCount == 0 ? "autofocus" : ""));
            }
            panel.Add(list);
            panel.Add(HudKit.Switch("AUTO-EQUIP", true, _ => { }));
            panel.Add(Press("BACK", () => view.Navigation.Show("menu")));
            anchor.Add(panel);
            var side = Anchor(stage, right: "40", top: "32");
            var loading = new HudPanel { Title = "SYNCING" }; loading.AddToClassList("ar-g-side"); loading.Add(new HudLoader());
            side.Add(loading);
            view.Navigation.Back = () => () => view.Navigation.Show("menu");
            view.RefreshTheme();
        }

        private static void Status(HudView view)
        {
            var holder = HudKit.Box(HudView.ThemedClass + " ar-g-dialog-holder");
            var dialog = new HudPanel { Title = "SYSTEM STATUS" }; dialog.AddToClassList("ar-dialog");
            dialog.Add(HudKit.Text("VISOR ONLINE\nTelemetry connected. Ready for deployment.", "ar-copy"));
            dialog.Add(Press("CLOSE", view.Navigation.CloseModal, "autofocus"));
            holder.Add(dialog);
            view.Navigation.OpenModal(holder, HudModal.Dialog);
            view.RefreshTheme();
        }

        // A gallery screen: a themed stage filling the content.
        private static HudStage Stage(VisualElement content)
        {
            var screen = HudKit.Box("ar-screen " + HudView.ThemedClass); screen.pickingMode = PickingMode.Ignore;
            var stage = new HudStage(); screen.Add(stage); content.Add(screen);
            return stage;
        }

        private static HudAnchor Anchor(HudStage stage, string left = "", string top = "", string right = "", string bottom = "")
        {
            var anchor = new HudAnchor { Left = left, Top = top, Right = right, Bottom = bottom };
            stage.Add(anchor); return anchor;
        }

        private static Button Press(string title, System.Action action, string css = "")
        {
            var button = HudKit.Press(action, "small-cta ar-g-press " + css, HudKit.Text(title, "ui"));
            return button;
        }
    }
}
