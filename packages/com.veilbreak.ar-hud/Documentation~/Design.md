# Design and source notes

The supplied helmet, windshield and futuristic instrumentation references inform the curved rim, open central sightline, cyan linework, sparse labels and segmented rings. They are reference-only and are not redistributed. All package visuals are procedural; no stock images, downloaded fonts or asset-store content are included.

The supplied Veilbreak specification v2 provides the Veilbreak palette: #0e0914 panel at 86%, #faeef4 foreground, #1dd9a0 ally/accent, #f0509a danger, #c084fc power and #ffc35a warning. It also informs the top-left/bottom-right cuts, readable targets, strong focus and effect-free high contrast. This generic package is not a claim of complete conformance with every Veilbreak screen, localization rule or game mechanic. The engineering manual informs UI Toolkit-first controls, UXML source generation, presentation-model binding and separate runtime/editor assemblies. The screen archive informs the menu/battle/loadout compositions.

Default optical effects are subtle and localized. Warning/error messages should always include words or symbols; color alone is insufficient. Decorative frame glow is not a status indicator. Scanlines render behind controls. No text flicker, full-screen flash, camera motion or audio is generated.

## Official references consulted (21 October 2026)

- UI systems / Unity 7000: https://docs.unity.com/en-us/engine/7000.0/manual/uitoolkits
- UI Toolkit: https://docs.unity.com/en-us/engine/7000.0/manual/uitoolkits/uielements
- Built-in filters: https://docs.unity.com/en-us/engine/7000.0/manual/uitoolkits/uielements/uie-uss/uss-filter/built-in-filters
- Backdrop filters: https://docs.unity.com/en-us/engine/7000.0/manual/uitoolkits/uielements/uie-uss/uss-filter/backdrop-filter
- Panel settings: https://docs.unity.com/en-us/engine/7000.0/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-runtime-panel-settings
- Native binding: https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-runtime-binding-types.html
- Painter2D: https://docs.unity3d.com/ScriptReference/UIElements.Painter2D.html

The package authors source code independently around those public APIs. No Unity sample project assets are bundled.
