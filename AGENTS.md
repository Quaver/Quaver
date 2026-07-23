# Repository Guidance

## Wobble Component Reuse

- Always use an existing component from Wobble when it provides the required UI behavior. Inspect Wobble's available components before creating a custom implementation.
- Prefer `Wobble.Graphics.FlexContainer` for flex-based layout instead of implementing equivalent positioning or layout logic manually.
- Prefer the Wobble tooltip system in `Wobble.Graphics.UI.Tooltips`, including `AddTooltip`, `TooltipOptions`, and `TooltipManager`, instead of building screen-specific tooltip behavior.
- Always use Wobble's `RoundedButton` when creating a button. Extend or compose `RoundedButton` when specialized button behavior is required instead of starting from a lower-level drawable or implementing button interaction manually.
- Reuse Wobble's existing buttons, form controls, navigation, dialogs, and other UI primitives whenever they satisfy the requirement.
- Create a custom component only when Wobble has no suitable component or the existing component cannot support the required behavior. Keep custom code focused on the missing behavior and build on Wobble primitives where possible.

## New V2 Screens and Skinning

These rules apply whenever a new screen is created in the V2 screen architecture. They do not require retroactive changes to existing screens or skin configurations.

- Implement every new V2 screen cleanly from scratch. Do not reuse, inherit from, copy, or rewrite an old screen implementation. The new screen must remain independent from its legacy counterpart while still reusing shared non-screen infrastructure and Wobble components.
- Every new V2 screen must have an adjacent, screen-owned Skinning V2 configuration file. Follow the structure and naming pattern established by `Quaver.Shared/Screens/V2/Main/MainMenuSkinConfig.cs`.
- The screen's root configuration object must be initialized and exposed from `SkinV2ScreensConfig` in `Quaver.Shared/Skinning/V2/SkinV2Config.cs`.
- Visual properties and configurable layout measurements must be declared in the screen's skin configuration and read from that configuration by the screen implementation. Do not duplicate those values as hardcoded constants or literals in the screen.
- Acquire Skinning V2 through `SkinManager.AcquireV2()` and always dispose the resulting `SkinStoreV2Lease` in `ScreenView.Destroy()`.
- Load configurable textures through `SkinStoreV2Lease.LoadTexture(path, fallback)`, parse configured colors with `SkinV2Color.Parse`, and resolve configured fonts through `FontManager.GetWobbleFont`.
- Make layouts responsive to `WindowManager` size changes. Update the root and affected child sizes and refresh affected `FlexContainer` layouts when the window dimensions change.
- When a screen uses the shared navigation, implement `IPersistentScreen`, include `ScreenNavigation.ElementKey` in `PersistentElementKeys`, and call `ScreenNavigation.EnsureAttached(View.Container)` from `OnActivated()`.
- Unsubscribe event handlers and dispose tooltip registrations, owned textures, controllers, the Skinning V2 lease, and any other owned resources in `Destroy()`.
- Use `LocalizationManager.Get(...)` for all user-facing labels and tooltip text. Reuse an existing localization key when it represents the same text; do not create a duplicate key specifically for a new screen.
- Defaults must reference an existing shared preset from `SkinV2Config.cs` whenever that preset is an exact, applicable match. Check:
  - `SkinV2FontSizesConfig`
  - `SkinV2FontWeightsConfig`
  - `SkinV2MarginsConfig`
  - `SkinV2Spacing`
  - `SkinV2BorderRadiusConfig`
- Do not use a merely similar preset if doing so changes the intended value or meaning. When no existing preset is an exact match, a screen-specific literal default is allowed.
- Add the validation and skin metadata attributes appropriate to each property, including `[Range]`, `[Required]`, `[SkinColor]`, `[SkinFont]`, and `[SkinAssetPath]`.

Prefer:

```csharp
[Range(0, 4096)]
public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;
```

over:

```csharp
[Range(0, 4096)]
public float CornerRadius { get; set; } = 6;
```

Before completing a new V2 screen, verify that its config file exists, is registered in `SkinV2ScreensConfig`, supplies the screen's configurable visual and layout values, reuses all exact matching shared presets, and applies the appropriate validation and skin metadata attributes.
