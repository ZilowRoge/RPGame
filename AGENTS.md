# RPGame Agent Guide

## Project

* Unity `6000.4.3f1`.
* URP `17.4.0`.
* Unity Input System `1.19.0`.

## Scope And Efficiency

* Start with files explicitly mentioned in the task or directly related to the requested feature.
* Prefer targeted searches over repository-wide searches.
* Do not scan the entire repository unless necessary.
* Do not inspect unrelated modules merely to understand the architecture.
* Make the smallest change that fully satisfies the task.
* Do not perform unrelated refactoring, cleanup, formatting, renaming, or architectural changes.
* Do not add speculative APIs, abstractions, fields, configuration, hooks, or extension points.

## Unity Files

* Do not inspect or modify `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, generated `.csproj`, or solution files unless explicitly requested.
* Do not read or modify `.meta` files unless adding, moving, renaming, or deleting the corresponding asset.
* Preserve existing `.meta` files and GUIDs when moving or renaming assets.
* Do not inspect `.unity`, `.prefab`, `.asset`, `.mat`, `.controller`, or `.inputactions` unless the task directly depends on their serialized state.
* Prefer inspecting C# code before opening large serialized Unity files.

## Implementation

* Prefer small, focused changes.
* Reuse or extend existing project APIs before introducing new ones.
* Do not implement unused methods, fields, properties, events, or configuration.
* Do not introduce abstractions only for hypothetical future use.
* Avoid changing public APIs unless required.
* Preserve existing behavior outside the requested scope.

## Unity Components

* Prefer explicit dependencies and serialized references.
* Use `GetComponent` when a dependency is guaranteed to exist on the same GameObject and cache it when reused.
* Avoid repeated runtime scene-wide lookups such as `GameObject.Find`, `FindFirstObjectByType`, and similar APIs.
* Do not use `MonoBehaviour` as a generic serialized field type for a required contract.
* Prefer a concrete component when one implementation is expected.
* When multiple interchangeable component implementations are required, prefer a dedicated abstract `MonoBehaviour` base class.
* Do not create a new interface or base class for a single implementation unless it solves an existing dependency problem.

## Architecture

* Organize gameplay by feature/module.
* Use `.asmdef` files for meaningful modules, not individual classes or micro-folders.
* Shared contracts, small value types, and cross-feature abstractions belong in `Assets/Core/Scripts/`.
* `Core` must not contain feature-specific implementation.
* Feature modules may depend on `Core`.
* Avoid feature-to-feature dependencies unless intentional.
* Prefer existing `Core` contracts before adding a new feature dependency.
* Do not move feature-specific code into `Core` merely to avoid an assembly reference.

## Code Style

* Follow existing project C# style.
* Use `PascalCase` for types, methods, properties, and events.
* Use `camelCase` for locals, parameters, and private fields.
* Serialized fields should use:

```csharp
[SerializeField] private Type fieldName;
```

* Avoid magic numbers in gameplay code.
* Add comments only for non-obvious intent or constraints.
* Do not reformat unrelated code.

## Input

* `Assets/PlayerControls.cs` is generated from `Assets/InputSystem_Actions.inputactions`.
* Do not manually edit `PlayerControls.cs`.
* Modify the `.inputactions` asset only when the task concerns input configuration.

## Testing

* Do not launch Unity, run tests, or run builds unless explicitly requested.
* The user is responsible for runtime validation.
* Write or update tests only when explicitly requested or when directly updating existing tests for changed behavior.
* Do not add broad or speculative test coverage.

## Git

* Use `git status` when needed.
* Prefer targeted Git inspection limited to files relevant to the task.
* Avoid repository-wide `git diff` unless explicitly requested or necessary.
* Do not inspect extensive Git history unless required.
* Do not switch branches, merge, rebase, commit, push, reset, or rewrite history unless explicitly requested.
* Never discard unrelated user changes.
* For clean merge or squash merge operations, do not inspect the full resulting diff; verify with `git status`.
* If merge conflicts occur, report them instead of resolving them unless explicitly asked.
* When asked for change notes, keep them concise and derive them from the task, branch history, commits, or targeted inspection rather than a full repository diff.

## Decision Rules

When multiple solutions are valid:

1. Prefer the smallest implementation that satisfies the requirement.
2. Prefer existing project patterns over new patterns.
3. Prefer explicit dependencies over global lookups.
4. Prefer extending an existing contract over creating a parallel abstraction.
5. Keep feature-specific behavior inside its feature.
6. Do not solve hypothetical future requirements.
7. Avoid touching unrelated files.
