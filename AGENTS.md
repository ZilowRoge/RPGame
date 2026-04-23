# RPGame Agent Guide

## Project Snapshot
- Unity project using Unity `6000.4.3f1`.
- Render pipeline: Universal Render Pipeline `17.4.0`.
- Input: Unity Input System `1.19.0`.
- Main project content lives under `Assets/`.
- Package definitions live under `Packages/`.
- Project-wide Unity settings live under `ProjectSettings/`.

## Working Rules
- Do not edit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, generated `.csproj` files, or the solution files unless the user explicitly asks for it.
- Keep Unity `.meta` files with their assets. When adding, moving, or deleting assets, preserve GUID stability by doing it through Unity when practical.
- Prefer small, focused changes. Avoid broad scene, prefab, or project setting rewrites unless the task requires them.
- Do not introduce new packages without checking whether an existing Unity/package API already covers the need.
- Treat serialized Unity files (`.unity`, `.prefab`, `.asset`, `.mat`, `.controller`, `.inputactions`) carefully; avoid manual edits unless the format and intent are clear.
- Do not use MonoBehavior as field type in classes instad make abstract class that inherits from MonoBehaviour

## Code Style
- Use C# with Unity conventions: `PascalCase` for types/properties/methods, `camelCase` for locals/parameters, and serialized private fields as `[SerializeField] private Type fieldName;`.
- Prefer composition via `MonoBehaviour` components and serialized references over global lookups.
- Cache component references when they are used repeatedly.
- Keep `Update()` lightweight. Move expensive or event-driven work to events, coroutines, timers, or dedicated systems.
- Use `Time.deltaTime`/`Time.fixedDeltaTime` appropriately for frame-rate-independent behavior.
- Avoid magic numbers in gameplay code; expose tunable values through serialized fields or constants with clear names.
- Add comments only where they explain intent that is not obvious from the code.

## Input System
- `Assets/PlayerControls.cs` is generated from `Assets/InputSystem_Actions.inputactions`.
- Do not manually edit `PlayerControls.cs`; change the `.inputactions` asset and regenerate the wrapper from Unity.
- Keep gameplay input handling separate from UI input handling where practical.

## Scenes And Assets
- Current scene content starts from `Assets/Scenes/SampleScene.unity`.
- Rendering assets are under `Assets/Settings/`.
- Organize gameplay by feature/module, not by asset type. Preferred shape:

```text
Assets/
  Core/
    Scripts/
  Player/
    Scripts/
    Prefabs/
    Data/
  Combat/
    Scripts/
    Prefabs/
    Data/
  UI/
    Scripts/
    Prefabs/
    Data/
```

- Each feature folder should be independently compilable where practical, using an `.asmdef` with explicit dependencies.
- Avoid micro-folders with their own `.asmdef` files. Assemblies should represent meaningful modules or layers, not individual classes, tiny utilities, or every nested folder.
- Put shared interfaces, base classes, abstractions, small value types, and cross-feature contracts in `Assets/Core/Scripts/`.
- Feature modules may depend on `Core`; avoid feature-to-feature dependencies unless the relationship is intentional and documented in assembly references.
- Prefer communication through `Core` contracts over direct references to concrete components from another feature.
- Keep editor-only code under an `Editor/` folder so it is excluded from player builds.
- Use prefabs for reusable scene objects instead of duplicating configured GameObjects across scenes.

## Testing And Validation
- Prefer Unity Test Framework tests for gameplay logic where feasible.
- Suggested command-line test shape on Windows:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.3f1\Editor\Unity.exe" -batchmode -quit -projectPath . -runTests -testPlatform EditMode -testResults TestResults\EditMode.xml
```

- Run PlayMode tests for changes involving scene behavior, physics, animation, input, or lifecycle timing.
- When command-line Unity is unavailable, validate through the Unity Editor and report what was checked.

## Git Hygiene
- Before editing, check for existing user changes and do not revert unrelated work.
- Keep generated/editor cache files out of source control.
- Commit asset changes together with their `.meta` files.
- If a merge conflict touches serialized Unity assets, prefer resolving through Unity/YAML awareness instead of blind text edits.
