# Consumer integration

Create FeedbackPool with an explicitly supplied additive material. Submit FeedbackRequest payloads and Advance(simulationDelta). CueCatalog maps presentation keys to size, duration and pulse. ProjectedReadouts receives projected actor positions and health; it never queries a game singleton. Register CinematicPulseFeature in the URP renderer, assign its shader, and drive the CinematicPulse VolumeComponent from the consumer. The URP and UI adapters have separate assembly boundaries.

Supply all dependencies explicitly. Runtime packages must compile without Assets/_Arcade, an ArcadeApp singleton, or access to a sibling checkout. The arcade simulation package intentionally exposes its game-specific rules.
