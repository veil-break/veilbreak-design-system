# Integration guide

How to put the kit on screen in a game, from an empty scene to a running battle board. The `BattleHud` sample is this
guide in code. The arcade (`unity/Assets/_Arcade/Runtime/ArcadeUI.cs`, `WorldReadouts.cs`) is the full client.

## 1. Install

Add the package from Git in `Packages/manifest.json` (append `#<tag or commit>` to pin a version):

```json
"com.veilbreak.gui": "https://github.com/veil-break/veilbreak-design-system.git"
```

or reference a checkout as a local package (`"file:../../packages/com.veilbreak.gui"`).

It needs Unity 6000.3 or newer and the built-in UI Elements and Vector Graphics modules. It has no other package
dependencies, uses no `Resources.Load` or scene objects, and never reads game types.

## 2. Panel settings and theme

Use `Runtime/Settings/VeilbreakPortrait.asset` as the `UIDocument`'s Panel Settings: Scale With Screen Size,
390 × 844, match width, theme `VeilbreakGui.tss`. To keep your own asset (and its GUID), copy the template's values
into it from an editor script, then set your theme:

```csharp
var template = AssetDatabase.LoadAssetAtPath<PanelSettings>("Packages/com.veilbreak.gui/Runtime/Settings/VeilbreakPortrait.asset");
EditorUtility.CopySerialized(template, panel); panel.name = "Panel"; panel.themeStyleSheet = myTheme;
```

Your theme imports the kit first, then your sheets:

```css
@import url("/Packages/com.veilbreak.gui/Runtime/Styles/VeilbreakGui.tss");
@import url("MyGame.uss");
```

- `VeilbreakGui.tss` imports the Unity default theme, then `Tokens.uss`, `Gui.uss`, `Layout.uss`, `Views.uss`,
  `World.uss` and `Icons.uss`.
- `VeilbreakGuiContrast.tss` is the same with the contrast colours on `:root`, for an always-high-contrast build. For a
  runtime toggle, use the `vb-contrast` root class instead.
- Keep the kit in the **theme**. Popovers are placed on the panel root, where sheets attached to a UXML element never
  reach.
- Sheets on a UXML `<Style>` apply after the theme, so on equal specificity your game rules win over the kit's.

## 3. The tree

A portrait board is a column. Put the world layer first, so it draws under every panel, and the screen layer last:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="root" class="vb-root">
    <ui:VisualElement name="hud"/>
    <ui:VisualElement name="battlefield" class="vb-grow"/>
    <ui:VisualElement name="cards" class="vb-row vb-row--between"/>
    <ui:VisualElement name="screen" class="vb-screen" style="display: none;"/>
  </ui:VisualElement>
</ui:UXML>
```

## 4. Safe area, navigation and Back

```csharp
var safe = new SafeAreaRoot(root, document.panelSettings, screen);   // pads root, insets the screen layer
var nav = new Navigator(screen) {
    Blocked  = () => introPlaying,                                  // Back does nothing meanwhile
    Fallback = () => { if (inRun) Pause(); else ConfirmExit(); },   // nothing else handled Back
};
nav.Opening += _ => { CancelDrags(); Cover(true); };                // a page covers the board
nav.Closed  += () => Cover(false);

void Update() {
    safe.Refresh();                                                 // cheap; re-applies only on change
    safe.Flags(settings.largerText, settings.reducedMotion, settings.highContrast, settings.reducedFlashes);
    if (escapePressed) nav.Back();                                  // Android back, Escape, gamepad B
}
```

Use `vb-covered` on the HUD, cards and world layer for `Cover`. A page:

```csharp
VisualElement Settings() {
    var page = nav.Open("settings", back: MainMenu);                // vb-page--settings on vb-screen--settings
    new ScreenHeader("SETTINGS", "SAVED").Into(page).Back += () => nav.Back();
    page.AddToClassList("vb-page--headed");
    var body = Ui.Scroll(page, "vb-page__scroll").contentContainer;
    var audio = Ui.Section(body, "AUDIO");
    new VbSlider("Music", settings.music).Into(audio).Changed(v => settings.music = v);
    new VbSwitch("REDUCED MOTION", "No camera shake", settings.reducedMotion).Into(audio).Changed(v => settings.reducedMotion = v);
    return page;
}
```

A confirmation:

```csharp
var sheet = Dialog.Push(nav, "LEAVE GAME?", "Your profile is saved.");
new VbButton("EXIT GAME", ButtonVariant.Danger, null, "back", Application.Quit).Into(sheet);
new VbButton("CANCEL", ButtonVariant.Secondary, null, "close", () => nav.Back()).Into(sheet);
```

Behind every page a `Backdrop` can carry the menu art: `root.Insert(0, new Backdrop(menuArt))`.

## 5. Binding

Map your state to the view models (`HudView`, `OperativeView`, `SkillView`, `QueueView`, `PerkView`, `BossView`,
`ScoreView`, `ContributionView`, `RuleView`) and call `Bind` every frame. Components reuse their children. The kit
never reads your types.

```csharp
hud.Bind(hudView);
for (int i = 0; i < 4; i++) cards[i].Bind(operativeViews[i]);
strip.Set(power, 100, "+12/s", hint, previewFrom, previewTo).Reserve(25).Queue(queue, q => game.CancelQueued(q.actor, q.slot));
```

Formatting helpers keep numbers consistent: `Ui.Number`, `Ui.Compact` and `Ui.Seconds`. Colours for rich text come
from `VbTokens` (`VbTokens.Hex(VbTokens.Text3)`).

## 6. Input: aim drags and camera gestures

The kit draws; the game decides. Attach a `TargetDrag` to each skill button or to the battlefield, then a
`GestureSurface` after it on the same element:

```csharp
card.Skill(0).AddManipulator(new TargetDrag(
    begin: (p, id) => game.CanAim(op, 0),  move: p => Aim(p),  end: p => Release(p),  cancel: CancelAim));
var gestures = new GestureSurface(busy: () => aiming);
battlefield.AddManipulator(gestures);                               // after the targeting drag
gestures.Panned  += (a, b) => rig.PanBy(ToMetres(b - a));
gestures.Pinched += f => rig.ZoomBy(f);
gestures.Twisted += d => rig.RotateBy(d, 0);
gestures.DoubleTapped += _ => rig.ResetView();
gestures.Hovered += p => rig.PointAt(p.HasValue ? ToViewport(p.Value) : null);
```

A drag always wins. A pointer the targeting drag captured never becomes a gesture, and while `busy` is true the
surface ignores everything. Call `gestures.Cancel()` when a page opens.

## 7. The world layer

```csharp
layer = new WorldLayer { Project = Project };
root.Insert(0, layer);                                              // under every panel
Vector2? Project(Vector3 p) {
    var v = camera.WorldToViewportPoint(p); if (v.z <= 0 || layer.panel == null) return null;
    var size = layer.panel.visualTree.worldBound.size;
    return layer.WorldToLocal(new Vector2(v.x * size.x, (1 - v.y) * size.y));
}
void LateUpdate() {
    layer.style.top = battlefield.worldBound.yMin - root.worldBound.yMin;     // fit the battlefield band
    layer.style.bottom = root.worldBound.yMax - battlefield.worldBound.yMax;
    layer.ReducedMotion = settings.reducedMotion;
    layer.BeginFrame();
    foreach (var a in actors) if (a.alive) layer.Bar(a.id, a.head, a.hp / a.maxHp, a.barrier / a.maxHp, a.team, a.rank);
    layer.EndFrame(simulationDelta);                                // simulation time, not Time.deltaTime
}
void OnConfirmedHit(Hit h) => layer.Text(h.target, h.point, h.amount, h.toAlly ? CombatTextKind.Taken : CombatTextKind.Damage);
```

Call `Text` only from confirmed simulation events. Presentation never applies damage, and the layer does not decide
what happened.

## 8. Accessibility checklist

- Pass every setting to `SafeAreaRoot.Flags`, and `ReducedMotion` to the world layer.
- Give every control a word: `Blocked` reasons, status words, tooltips on icon-only buttons.
- Keep targets at least 44 units on both axes. Run the PlayMode `FitTests` after layout changes.
- One `Primary` button per screen. At most two glows on screen.
- Back must always do something, even if that is only `Fallback`.

## 9. Your own components

- Build with `Ui.Box`, `Ui.Text` and `VbShape.Attach`, and name classes BEM-style with your own block prefix.
- Read colours and sizes from tokens: `var(--vb-color-…)`, `var(--vb-cut-md)`, `var(--vb-font-body-medium)`.
- Put states in modifier classes with `Ui.State`, so USS owns the look.
- Uppercase display text in code.

## 10. Verify

| Check | How |
| --- | --- |
| Tokens and generated files | `python3 Tools~/tokens.py --check`, `python3 Tools~/samples.py --check` |
| EditMode | `Tests/Editor`: navigation, world layer, ports, tokens, icons |
| PlayMode | `Tests/Runtime/FitTests` renders the kit at widths 320–430 |
| Clean consumer | The repository's `tools/verify-packages.py` copies the package and both samples into an empty project and compiles them |

## Screen views

The menus and run screens ship as views with plain view models: open a page, bind, handle the events.

```csharp
var menu = new MainMenuView().Into(nav.Open("menu"));
menu.Bind(new MainMenuViewModel { best = 18240, bestWave = 12, checkpoint = cp, squad = names });
menu.Play += Squad; menu.Settings += () => new SettingsView().Into(nav.Open("settings", MainMenu)).Bind(profile.settings);

var battle = new BattleHudView().Into(root);          // HUD band, boss bar, announcer, world, cards
var world = new WorldLayer { Project = Project }.Into(battle.World);
battle.Pause += () => new PauseOverlayView().Into(nav.Open("pause", Resume)).Bind(pauseView);
ConfirmDialogView.Push(nav, "END THIS RUN?", "Your run is recorded.", ("ABANDON", "remove", Abandon), ("KEEP PLAYING", "chevron"));
```

The same views exist for web apps in `Web~` (`@veilbreak/gui`), with the same names and view models.

