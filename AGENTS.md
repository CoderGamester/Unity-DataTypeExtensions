# GameLovers.GameData - AI Agent Guide

## 1. Package Overview
- **Package**: `com.gamelovers.gamedata`
- **Unity**: 6000.0+
- **Assembly**: `Runtime/GameLovers.GameData.asmdef` (note: **allowUnsafeCode = true**)
- **Dependencies** (see `package.json`)
  - `com.unity.nuget.newtonsoft-json` (**3.2.1**)

This package provides core **game design data** utilities:
- **Config storage + versioning** (`ConfigsProvider`)
- **Backend sync serialization** (`ConfigsSerializer` via Newtonsoft.Json)
- **Observable data types** (fields, lists, dictionaries + "resolver" variants)
- **Deterministic math** (`floatP` + `MathfloatP`)
- **Unity-friendly serialization helpers** (e.g., `UnitySerializedDictionary`, `SerializableType<T>`)

For user-facing docs, treat `README.md` as the primary entry point. This file is for contributors/agents working on the package itself.

## 2. Runtime Architecture (high level)

### Namespace Organization
| Namespace | Purpose |
|-----------|---------|
| `GameLovers.Configs` | Config storage, versioning, serialization |
| `GameLovers.Observables` | Observable data types (field, list, dictionary) |
| `GameLovers.Math` | Deterministic math (`floatP`, `MathfloatP`) |
| `GameLovers.Serialization` | Unity serialization helpers, JSON converters |
| `GameLovers` | Extensions, attributes, utilities (flat for discoverability) |

### Configs pipeline (`GameLovers.Configs` namespace)
- **Provider API**: `Runtime/Configs/Interfaces/IConfigsProvider.cs`, `Runtime/Configs/Interfaces/IConfigsAdder.cs`
  - `IConfigsProvider` exposes read access + `ulong Version`.
  - `IConfigsAdder : IConfigsProvider` adds mutation APIs (`AddSingletonConfig`, `AddConfigs`, `UpdateTo`).
- **Default implementation**: `Runtime/Configs/ConfigsProvider.cs` (`ConfigsProvider : IConfigsAdder`)
  - Internally stores configs as `Dictionary<Type, IEnumerable>`.
  - Supports **singleton configs** and **id-keyed configs** (int ids).
- **Serialization**: `Runtime/Configs/ConfigsSerializer.cs`
  - `IgnoreServerSerialization` attribute marks config types to skip server payloads.
  - `ConfigsSerializer : IConfigsSerializer` uses Newtonsoft.Json and includes type metadata (`TypeNameHandling.Auto`).
- **Backend hook (optional)**: `Runtime/Configs/Interfaces/IConfigBackendService.cs`
  - Contract to fetch remote version and remote configuration.
- **ScriptableObject containers**: `Runtime/Configs/ConfigsScriptableObject.cs` + `Runtime/Configs/Interfaces/IConfigsContainer.cs`
  - Inspector-friendly key/value config containers using serialized pair lists.

### Observable data types (`GameLovers.Observables` namespace)
- **Field**: `Runtime/Observables/ObservableField.cs`
  - `ObservableField<T>` + `ObservableResolverField<T>` (rebind to new getter/setter).
- **List**: `Runtime/Observables/ObservableList.cs`
  - `ObservableList<T>` + `ObservableResolverList<T, TOrigin>` (keeps an origin list and maps values).
- **Dictionary**: `Runtime/Observables/ObservableDictionary.cs`
  - `ObservableDictionary<TKey, TValue>` + `ObservableResolverDictionary<TKey, TValue, TKeyOrigin, TValueOrigin>`.
  - Update routing controlled by `ObservableUpdateFlag` (`Runtime/Observables/ObservableUpdateType.cs`).

### Deterministic math (`GameLovers.Math` namespace)
- **Deterministic float**: `Runtime/Math/floatP.cs`
- **Math utilities**: `Runtime/Math/MathfloatP.cs`

### Serialization helpers (`GameLovers.Serialization` namespace)
- **Unity serialization helper**: `Runtime/Serialization/UnitySerializedDictionary.cs`
- **Serializable type reference**: `Runtime/Serialization/SerializableType.cs` (`SerializableType<T>`)
- **Newtonsoft converter for Unity Color**: `Runtime/Serialization/ColorJsonConverter.cs`
- **Value data structs**: `Runtime/Serialization/ValueData.cs` (`Pair<TKey, TValue>`, `StructPair<TKey, TValue>`, Vector serialization helpers)

### Extensions (`GameLovers` namespace - flat for discoverability)
- **Object extensions**: `Runtime/Extensions/ObjectExtensions.cs`
- **Reflection extensions**: `Runtime/Extensions/ReflectionExtensions.cs`
- **Sorted list extensions**: `Runtime/Extensions/SortedListExtensions.cs`
- **Unity objects extensions**: `Runtime/Extensions/UnityObjectsExtensions.cs`

### Utilities (`GameLovers` namespace - flat for discoverability)
- **Enum selection helper**: `Runtime/Utilities/EnumSelector.cs` (stores enum names as strings for stability when enums change)
- **ReadOnly attribute**: `Runtime/Utilities/ReadOnlyAttribute.cs`

## 3. Key Directories / Files
```
Runtime/
├── Configs/
│   ├── ConfigsProvider.cs
│   ├── ConfigsSerializer.cs
│   ├── ConfigsScriptableObject.cs
│   └── Interfaces/
│       ├── IConfigsProvider.cs
│       ├── IConfigsAdder.cs
│       ├── IConfigsSerializer.cs
│       ├── IConfigsContainer.cs
│       └── IConfigBackendService.cs
├── Observables/
│   ├── ObservableField.cs
│   ├── ObservableList.cs
│   ├── ObservableDictionary.cs
│   └── ObservableUpdateType.cs
├── Math/
│   ├── floatP.cs
│   └── MathfloatP.cs
├── Serialization/
│   ├── ColorJsonConverter.cs
│   ├── SerializableType.cs
│   ├── UnitySerializedDictionary.cs
│   └── ValueData.cs
├── Extensions/
│   ├── ObjectExtensions.cs
│   ├── ReflectionExtensions.cs
│   ├── SortedListExtensions.cs
│   └── UnityObjectsExtensions.cs
├── Utilities/
│   ├── EnumSelector.cs
│   └── ReadOnlyAttribute.cs
└── GameLovers.GameData.asmdef

Editor/
├── EnumSelectorPropertyDrawer.cs
└── ReadOnlyPropertyDrawer.cs

Samples~/
└── Enum Selector Example/

Tests/Editor/
└── (NUnit tests)
```

## 4. Important Behaviors / Gotchas
### ConfigsProvider behavior
- **Singleton vs id-keyed**:
  - `GetConfig<T>()` is **only** for singleton configs; it throws if `T` isn't stored as a singleton.
  - Use `GetConfig<T>(int id)` / `TryGetConfig<T>(int id, ...)` for id-keyed configs.
- **Missing configs throw**:
  - `GetConfigsDictionary<T>()` indexes `_configs[typeof(T)]`; if `T` was never added, this throws `KeyNotFoundException`.
- **Duplicate adds throw**:
  - `AddSingletonConfig<T>()` uses `Dictionary.Add` → adding the same `T` twice throws.
  - `AddConfigs<T>()` throws if `referenceIdResolver` produces duplicate ids.
- **Allocations**:
  - `GetConfigsList<T>()` allocates a new `List<T>` every call.

### ConfigsSerializer / Newtonsoft details
- **Type metadata included**:
  - Uses `TypeNameHandling.Auto`, which embeds `$type` metadata. Treat deserialization as **trusted-data only** (do not deserialize untrusted remote payloads without hardening).
- **Serializable requirement**:
  - Types are required to be `type.IsSerializable` unless marked `[IgnoreServerSerialization]`.
- **Null payload risk**:
  - `Deserialize(...)` currently calls `cfg.UpdateTo(versionNumber, configs?.Configs)`.
  - If `configs?.Configs` is `null`, `ConfigsProvider.UpdateTo` → `AddAllConfigs` will throw. Callers should ensure the serialized payload always includes `Configs`, or guard before calling `UpdateTo`.

### ConfigsScriptableObject containers
- **Key uniqueness required**:
  - `OnAfterDeserialize()` builds a dictionary via `Dictionary.Add` and will throw if duplicate keys exist in the serialized list.

### Observable types (mutation & performance)
- **Subscriber removal by owner**:
  - `StopObservingAll(object subscriber)` uses `delegate.Target == subscriber`.
  - Static method subscribers have `Target == null`, so they can't be removed by passing an owner; use `StopObservingAll()` to clear everything.
- **ObservableUpdateFlag semantics (Dictionary)**:
  - Default is `KeyUpdateOnly` (key-specific observers only).
  - `UpdateOnly` notifies only global observers (`Observe(Action<...>)`).
  - `Both` notifies both key-specific and global observers (highest cost).
- **InvokeUpdate throws when missing key (Dictionary)**:
  - `InvokeUpdate(key)` and `InvokeObserve(key, ...)` use `Dictionary[key]` and will throw if the key is missing (see tests).
- **Allocations**:
  - `ObservableList<T>.ReadOnlyList` allocates a new `List<T>` on each call.
  - Resolver wrappers (`OriginList`, `OriginDictionary`) also allocate copies/wrappers.

### EnumSelector + drawer contract
- **Stores string, not ordinal**:
  - `EnumSelector<T>` persists enum names to avoid "wrong enum value" when items are inserted/removed.
- **Invalid selection can crash if unchecked**:
  - If the stored string no longer matches any enum constant, `GetSelectedIndex()` returns `-1`.
  - Calling `GetSelection()` with an invalid selection will attempt `EnumValues[-1]` and can throw. Prefer `HasValidSelection()` checks, and/or use the editor drawer which presents an "Invalid: …" option.
- **Drawer expects `_selection` field**:
  - `EnumSelectorPropertyDrawer<T>` looks for a serialized string field named `_selection`.

## 5. Coding Standards (Unity 6 / C# 9.0)
- **C#**: C# 9.0 syntax; explicit namespaces; no global usings.
- **Assemblies**:
  - Runtime must not reference `UnityEditor`.
  - Editor tooling must live under `Editor/` (or be guarded by `#if UNITY_EDITOR` if absolutely necessary).
- **Determinism**:
  - `floatP` is compiled with `allowUnsafeCode`; keep deterministic operations consistent across platforms.

## 6. External Package Sources (for API lookups)
Prefer local Unity/UPM sources when needed:
- Unity Newtonsoft: `Library/PackageCache/com.unity.nuget.newtonsoft-json/`

## 7. Dev Workflows (common changes)
- **Add a new config type**
  - Ensure the type is `[Serializable]` (unless `[IgnoreServerSerialization]`).
  - Decide singleton vs id-keyed and add via `IConfigsAdder.AddSingletonConfig` or `AddConfigs`.
  - Add/extend tests under `Tests/Editor/` for retrieval and versioning expectations.
- **Add a new observable type / behavior**
  - Keep allocations out of hot paths; document any copying (e.g., "safe publish" style).
  - Add tests validating subscribe/unsubscribe safety and update ordering.
- **Change serialization format**
  - Update `ConfigsSerializer.cs` and validate compatibility in tests.
  - Be explicit about trust boundaries (remote payloads vs local).
- **Add editor UX**
  - Put drawers/tools under `Editor/` and keep runtime assemblies clean.
  - Provide a small sample under `Samples~/` where appropriate.

## 8. Update Policy
Update this file when:
- Configs API changes (`IConfigsProvider`, `IConfigsAdder`, `ConfigsProvider`, `ConfigsSerializer`)
- Observable semantics change (update flags, mutation safety, allocation behavior)
- Deterministic math behavior changes (`floatP`, `MathfloatP`)
- Editor drawer contracts change (`EnumSelectorPropertyDrawer`, `ReadOnlyPropertyDrawer`)
- Dependencies / asmdef settings change (`package.json`, `GameLovers.GameData.asmdef`)
- **Namespace or folder structure changes**

## 9. Unity 6 & AOT Compatibility (Maintenance)

### UI Toolkit Inspector Support
- **PropertyDrawers**: Both `ReadOnlyPropertyDrawer` and `EnumSelectorPropertyDrawer` support UI Toolkit via `CreatePropertyGUI`.
- **Recommendation**: Prefer UI Toolkit for new editor tools; maintain `OnGUI` for legacy compatibility if needed.

### AOT / IL2CPP Safety
- **Type Resolution**: `SerializableType<T>` uses an AOT-safe resolution pattern that searches loaded assemblies.
- **Code Stripping**: The package includes a `Runtime/link.xml` to prevent stripping of core serialization logic. Ensure any new types used in polymorphic serialization are added to `link.xml` if stripping issues occur.

### Serialization Robustness
- **EnumSelector**: Correctly handles enums with explicit values (non-contiguous) using name-based serialization.
- **ConfigsScriptableObject**: Includes duplicate key detection during deserialization to prevent silent data loss or runtime exceptions.
- **JSON Converters**: Default `ConfigsSerializer` includes converters for `Color`, `Vector2/3/4`, and `Quaternion` to ensure clean round-tripping with Newtonsoft.Json.

