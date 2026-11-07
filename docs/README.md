# Design system docs

Guides for the packages in this repository. Each package keeps its own README and CHANGELOG next to its code; these guides
cover the parts that cross a package or need a worked procedure.

| guide | package | covers |
| --- | --- | --- |
| [tokens.md](tokens.md) | `com.veilbreak.gui` | Editing `Tokens~/tokens.json`, running the generators, what the outputs contain, the `--vb-*` shape vocabulary |
| [feedback.md](feedback.md) | `com.veilbreak.feedback` | Cue catalog, `CueStage`, particle routines, telegraphs, ground marks, decals, screen impacts, shake and pulse hooks |
| [shaders.md](shaders.md) | `com.veilbreak.rendering` | Render features, volume overrides and shaders on the URP 17.3 baseline |
| [environment.md](environment.md) | `com.veilbreak.environment` | `ISegmentPlan`, `SegmentStreamer`, street kits, atmosphere and reflections |

## Where the rest of the documentation lives

The standard documentation layout (specification, validation, manuals, archive) maps onto existing material, so nothing is
duplicated here:

| standard part | location |
| --- | --- |
| Specification | the package READMEs, listed in the [repository README](../README.md) |
| Manuals | [`com.veilbreak.gui/Documentation~`](../packages/com.veilbreak.gui/Documentation~/index.md) (brand book, integration guide, component and token reference, guidelines) and the GUI Handbook published with GitHub Pages |
| Validation | [`com.veilbreak.gui/Documentation~/validation.md`](../packages/com.veilbreak.gui/Documentation~/validation.md) and each package's `CHANGELOG.md` |
| Archive | none; superseded text is removed, history stays in git and the changelogs |

The guides describe the packages as they are in this checkout. When a guide and the code disagree, the code wins: fix the guide.
Agent rules for this repository are in [CLAUDE.md](../CLAUDE.md).
