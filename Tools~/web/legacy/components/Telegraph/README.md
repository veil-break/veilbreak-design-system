# Telegraph

The seven skill shapes drawn on the ground in the simulation's real geometry — your aim preview, or the threat about to land on you.

**Provide** `shape` (`projectile`, `fan`, `cone`, `burst`, `zone`, `beam`, `dash`, `wall`), `team` (`ally` aim preview / `enemy` threat); optional `state` (`predicted` dashed / `confirmed` solid), `progress` (0–1 windup fill), `width` (px), `iso` (wrap in the tactical-camera tilt for 2D mockups).

- Ally previews: teal edge, 20% fill. Enemy threats: crimson edge at full strength, `danger-zone` hatch fill at `alpha-telegraph`.
- Predicted (client-side) marks are dashed at `alpha-predicted` until the server confirms; then they turn solid.
- The windup fill grows from the caster for exactly the kit's windup, tick-exact.
- Nothing covers a telegraph: not VFX, not labels, not the HUD after the camera settles.
