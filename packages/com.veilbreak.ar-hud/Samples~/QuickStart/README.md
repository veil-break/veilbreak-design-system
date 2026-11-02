# Quick Start

1. Run **Veilbreak > AR HUD > Create Demo in Scene**. It adds a GameObject with a `PanelRenderer` (panel settings on the package's `RuntimeTheme.tss`, which imports `Hud.uss`) and a `HudHost` showing the sample gallery (menu, battle and loadout screens plus a status dialog), with a new theme and effects profile to edit.
2. For a minimal custom screen, assign this sample's `Telemetry.uxml` to the host's **Layout** field: it replaces the gallery. Its root carries `ar-themed`, so the theme colours its text and themed controls.
3. Add `TelemetryDriver` to a GameObject and assign the `HudHost` to its **Host** field. In Play mode adjust Integrity, Objective and Ammunition in the Inspector: the values reach the screen through native runtime data binding on `HudHost.Model` (a `HudViewModel`).

Screens of your own register with the view's navigation once the host is up:

```csharp
host.Ready += view =>
{
    view.Navigation.Register("status", content => statusUxml.CloneTree(content));
    view.Navigation.Show("status");
};
```

The driver is illustrative and is not combat simulation.
