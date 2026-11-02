# ConnectionPill

Latency and sync state, top-left in battle and in matchmaking.

**Provide** `ms`; optional `region` ("EU North"), `state="reconnecting"`, `showState` (default true).

- Thresholds: under 120 ms teal and SYNCED; 120–199 `warn` and UNSTABLE; 200+ crimson. Matchmaking advises against ranked above 200 ms.
- Desktop puts diagnostics behind it (tap to expand). It never covers the clock.
- During a resync, pair it with `ConnectionBanner`.
