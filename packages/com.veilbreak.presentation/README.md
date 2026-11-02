# veilbreak presentation

Entry package for the shared presentation stack. It holds no assets and no assembly; it only declares the tested set of
design-system packages so a game can root the whole stack with one dependency:

| package | version |
| --- | --- |
| `com.veilbreak.gui` | 0.5.1 |
| `com.veilbreak.feedback` | 1.5.0 |
| `com.veilbreak.branding` | 1.2.0 |
| `com.veilbreak.environment` | 0.3.0 |

Unity does not resolve git dependencies declared inside a package, and these semantic versions are not published to a
registry. Consumers therefore still list every design-system package as a pinned git source in their own manifest, all at
the same design-system commit. The [workspace tool](https://github.com/veil-break/.github-private/blob/main/docs/development/workspace.md)
expands that closure: a game spec roots `com.veilbreak.presentation` and `vb packages remote` writes all five git pins.

Rules this package keeps:

- no assets of its own, so there is never a competing copy of actor, effect or environment content;
- no assembly, in particular none named `Veilbreak.Presentation` (main's integration assembly);
- character assets stay in `com.veilbreak.character-assets` (veilbreak-characters) and are not part of this stack.

Game-specific catalogs stay in the games: arcade's is `com.veilbreak.arcade.presentation`.
