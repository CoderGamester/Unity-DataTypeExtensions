# GameLovers.GameData - AI Agent Guide

## 1. Package Overview
- **Package**: `com.gamelovers.gamedata`
- **Unity**: 6000.0+
- **Assembly**: `Runtime/GameLovers.GameData.asmdef` (note: **allowUnsafeCode = true**)
- **Dependencies** (see `package.json`)
  - `com.unity.nuget.newtonsoft-json` (**3.2.1**)
  - `com.cysharp.unitask` (**2.5.10**)

This package provides core **game design data** utilities:
- **Config storage + versioning** (`ConfigsProvider`)
- **Backend sync serialization** (`ConfigsSerializer` via Newtonsoft.Json)
- **Observable data types** (fields, lists, dictionaries + resolver variants)
- **Deterministic math** (`floatP` + `MathfloatP`)
- **Unity-friendly serialization helpers** (e.g., `UnitySerializedDictionary`, `SerializableType<T>`)

For user-facing docs, treat `README.md` as the primary entry point. This file is for contributors/agents working on the package itself.

## 2. Runtime Architecture (high level)
- **Configs**: provider + serializer (`Runtime/ConfigServices/*`)
- **Observables**: field/list/dictionary + resolver wrappers (`Runtime/Observables/*`)
- **Math**: deterministic `floatP` and helpers (`Runtime/Math/*`)
- **Serialization helpers**: Unity-friendly containers + converters (`Runtime/Serialization/*`)
- **Utilities + editor drawers**: `EnumSelector`, `ReadOnlyAttribute` + `Editor/Utilities/*`
- **Migrations**: Editor-only schema migrations (`Editor/Migration/*`)

## 3. Key Directories / Files
- `Runtime/ConfigServices/ConfigsProvider.cs` + `Runtime/ConfigServices/Interfaces/IConfigsProvider.cs`
- `Runtime/ConfigServices/ConfigsSerializer.cs` + `Runtime/ConfigServices/Interfaces/IConfigsSerializer.cs`
- `Runtime/ConfigServices/ConfigsScriptableObject.cs` + `Runtime/ConfigServices/Interfaces/IConfigsContainer.cs`
- `Runtime/Observables/ObservableField.cs`
- `Runtime/Observables/ObservableDictionary.cs` + `Runtime/Observables/ObservableUpdateType.cs`
- `Runtime/Math/floatP.cs` + `Runtime/Math/MathfloatP.cs`
- `Runtime/Serialization/UnitySerializedDictionary.cs` + `Runtime/Serialization/SerializableType.cs`
- `Runtime/Utilities/EnumSelector.cs`
- `Runtime/Attributes/` (Validation and ReadOnly attributes)
- `Editor/Utilities/EnumSelectorPropertyDrawer.cs` + `Editor/Utilities/ReadOnlyPropertyDrawer.cs`
- `Editor/Migration/MigrationRunner.cs` + `Editor/Migration/IConfigMigration.cs`
- `Tests/Editor/` (NUnit tests for core behaviors)

## 4. Important Behaviors / Gotchas
- **Singleton vs id-keyed**: `GetConfig<T>()` only for singleton; use `GetConfig<T>(int)` for id-keyed configs.
- **Missing configs throw**: `GetConfigsDictionary<T>()` throws if `T` was never added.
- **Duplicate adds throw**: `AddSingletonConfig<T>()` and `AddConfigs<T>()` throw on duplicate keys.
- **Validation is Editor-only**: use `GameLoversEditor.GameData.EditorConfigValidator` for development-time checks.
- **Migrations are Editor-only**: `MigrationRunner` and `IConfigMigration` live in `Editor/Migration/` since configs are baked into builds; schema migrations only need to run in editor when developers update schemas. Access the migration viewer via `Window > GameLovers > Config Migrations`.
- **Trusted JSON only**: `ConfigsSerializer` uses `TypeNameHandling.Auto`; do not deserialize untrusted payloads.
- **Serializable requirement**: config types must be `IsSerializable` unless `[IgnoreServerSerialization]`.
- **Null payload risk**: `Deserialize(...)` passes `configs?.Configs` into `UpdateTo`; guard if `Configs` can be null.
- **ConfigsScriptableObject keys must be unique**: duplicate keys throw during `OnAfterDeserialize()`.
- **ObservableDictionary update flags**: `KeyUpdateOnly` / `UpdateOnly` / `Both` change who is notified.
- **InvokeUpdate throws for missing keys**: `InvokeUpdate(key)` and `InvokeObserve(key, ...)` assume the key exists.
- **EnumSelector validity**: call `HasValidSelection()` before `GetSelection()` if enums might have changed; drawer expects `_selection`.

## 5. Coding Standards (Unity 6 / C# 9.0)
- **C#**: C# 9.0 syntax; explicit namespaces; no global usings.
- **Assemblies**: runtime must not reference `UnityEditor`; editor tooling under `Editor/`.
- **Determinism**: keep `floatP` operations consistent across platforms.

## 6. External Package Sources (for API lookups)
Prefer local Unity/UPM sources when needed:
- Unity Newtonsoft: `Library/PackageCache/com.unity.nuget.newtonsoft-json/`
- UniTask: `Library/PackageCache/com.cysharp.unitask/`

## 7. Dev Workflows (common changes)
- **Add a new config type**: mark `[Serializable]` (or `[IgnoreServerSerialization]`), decide singleton vs id-keyed, add tests.
- **Add observable behavior**: avoid allocations in hot paths; add tests for subscribe/unsubscribe and update ordering.
- **Change serialization**: update `ConfigsSerializer.cs` and verify trusted/untrusted boundaries in tests.
- **Add editor UX**: keep drawers under `Editor/` and provide a small sample when needed.

## 8. Update Policy
Update this file when:
- Configs API changes or serialization behavior changes.
- Observable semantics change (update flags, mutation safety).
- Deterministic math behavior changes (`floatP`, `MathfloatP`).
- Editor drawer contracts change or dependencies/asmdef settings change.
- Namespace or folder structure changes.

## 9. Unity 6 & AOT Compatibility (Maintenance)
- `SerializableType<T>` uses AOT-safe type resolution; keep reflection usage compatible with IL2CPP.
- `Runtime/link.xml` prevents stripping of core serialization logic; add new polymorphic types if needed.
- `ConfigsSerializer` includes Unity type converters (Color/Vector/Quaternion) for reliable JSON round-tripping.
- `EnumSelector` is name-based; safe for enum reordering, but handle invalid selections.
- `EnumSelectorPropertyDrawer` and `ReadOnlyPropertyDrawer` (in `Editor/Utilities/`) support both UI Toolkit (`CreatePropertyGUI`) and IMGUI (`OnGUI`) for compatibility with custom inspectors.

