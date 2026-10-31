# NavBar

The desktop top bar: the VEIL/BREAK wordmark, the main destinations, and the player's rating.

**Provide** `active` (the current item); optional `items` (default PLAY, TEAM, AUTOMATION, STUDIO, TRAINING, REPLAYS, SETTINGS), `rating`, `onNavigate(item)`.

- Desktop only; on mobile the same destinations live in the left `RailButton` column.
- The active item gets a 2px `ally` underline and full `text`; the rest are `text-2`.
- In battle the bar is hidden; Settings opens as an overlay and the match keeps running.
