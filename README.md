# GameLovers GameData

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/version-1.0.1-green.svg)](CHANGELOG.md)

> **Quick Links**: [Installation](#installation) | [Features](#features-documentation) | [Editor Tools](#editor-tools) | [Contributing](#contributing)

## Why Use This Package?

Managing game data in Unity often leads to fragmented solutions: scattered config files, tight coupling between data and logic, and cross-platform inconsistencies. This **GameData** package addresses these challenges:

| Problem | Solution |
|---------|----------|
| **Scattered config management** | Type-safe `ConfigsProvider` with O(1) lookups and versioning |
| **Tight coupling to data changes** | Observable types (`ObservableField`, `ObservableList`, `ObservableDictionary`) for reactive programming |
| **Manual derived state updates** | `ComputedField` for auto-updating calculated values with dependency tracking |
| **Cross-platform float inconsistencies** | Deterministic `floatP` type for reproducible calculations across all platforms |
| **Backend sync complexity** | Built-in JSON serialization with `ConfigsSerializer` for client/server sync |
| **Dictionary Inspector editing** | `UnitySerializedDictionary` for seamless Inspector support |
| **Fragile enum serialization** | `EnumSelector` stores enum names (not values) to survive enum changes |

**Built for production:** Minimal dependencies. Zero per-frame allocations in observable types. Used in real games.

### Key Features

- **📊 Configuration Management** - Type-safe, high-performance config storage with versioning
- **🔄 Observable Data Types** - `ObservableField`, `ObservableList`, `ObservableDictionary` for reactive updates
- **🧮 Computed Values** - `ComputedField` for auto-updating derived state with dependency tracking
- **🎯 Deterministic Math** - `floatP` type for cross-platform reproducible floating-point calculations
- **📡 Backend Sync** - JSON serialization/deserialization for atomic config updates
- **📦 ScriptableObject Containers** - Designer-friendly workflows for editing configs in Inspector
- **🔢 Unity Serialization** - `UnitySerializedDictionary` and `EnumSelector` for Inspector support
- **⚡ O(1) Lookups** - Pre-built in-memory dictionaries for maximum runtime performance

---

## System Requirements

- **[Unity](https://unity.com/download)** 6000.0+ (Unity 6)
- **[Newtonsoft.Json](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html)** (com.unity.nuget.newtonsoft-json v3.2.1) - Automatically resolved
- **[UniTask](https://github.com/Cysharp/UniTask)** (com.cysharp.unitask v2.5.10) - Used by the async backend interface `IConfigBackendService`
- **[TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)** (com.unity.textmeshpro v3.0.6) - Used by the **Samples~** UI scripts (`TMPro`). If you don't import samples, you may not use TMP at runtime, but the package dependency will still be installed.

### Compatibility Matrix

| Unity Version | Status | Notes |
|---------------|--------|-------|
| 6000.0+ (Unity 6) | ✅ Fully Tested | Primary development target |
| 2022.3 LTS | ⚠️ Untested | May require minor adaptations |

| Platform | Status | Notes |
|----------|--------|-------|
| Standalone (Windows/Mac/Linux) | ✅ Supported | Full feature support |
| WebGL | ✅ Supported | Full feature support |
| Mobile (iOS/Android) | ✅ Supported | Full feature support |
| Console | ⚠️ Untested | Should work without modifications |

## Installation

### Via Unity Package Manager (Recommended)

1. Open Unity Package Manager (`Window` → `Package Manager`)
2. Click the `+` button and select `Add package from git URL`
3. Enter the following URL:
   ```
   https://github.com/CoderGamester/com.gamelovers.gamedata.git#1.0.0
   ```

### Via manifest.json

Add the following line to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.gamelovers.gamedata": "https://github.com/CoderGamester/com.gamelovers.gamedata.git"
  }
}
```

---

## Key Components

| Component | Responsibility |
|-----------|----------------|
| **ConfigsProvider** | Type-safe config storage with O(1) lookups and versioning |
| **ConfigsSerializer** | JSON serialization for client/server config synchronization |
| **ConfigTypesBinder** | Whitelist-based type binder for secure deserialization |
| **ObservableField** | Reactive wrapper for single values with change callbacks |
| **ObservableList** | Reactive wrapper for lists with add/remove/update callbacks |
| **ObservableDictionary** | Reactive wrapper for dictionaries with key-based callbacks |
| **ComputedField** | Auto-updating derived values that track dependencies |
| **floatP** | Deterministic floating-point type for cross-platform math |
| **MathfloatP** | Math functions (Sin, Cos, Sqrt, etc.) for floatP type |
| **EnumSelector** | Enum dropdown that survives enum value changes |
| **UnitySerializedDictionary** | Dictionary type visible in Unity Inspector |

---

## Editor Tools

### Config Browser

<!-- Add configBrowser.gif -->

- **Menu**: `Tools > Game Data > Config Browser`
- **Purpose**: Unified window to browse configs from a provider, validate them, export JSON, and preview migrations.
- **Highlights**:
  - Browse tab: select a config entry to view JSON and run **Validate** for the selection.
  - Toolbar: **Validate All** (results displayed inside the window) and **Export JSON**.
  - Migrations tab: conditionally visible when migrations exist (uses `MigrationRunner`).

### Observable Debugger

<!-- Add observableDebugger.png -->

- **Menu**: `Tools > Game Data > Observable Debugger`
- **Purpose**: Inspect live observable instances (`ObservableField`, `ComputedField`, and observable collections).
- **Highlights**:
  - Filtering by name, kind, and “active only”.
  - Selecting a computed observable shows its current dependency list.

### ConfigsScriptableObject Inspector

<!-- Add inspector.png -->

- **Where**: Inspector UI for `ConfigsScriptableObject<,>` derived assets.
- **Purpose**: Inline entry status (duplicate keys / attribute-based validation) and a quick **Validate All** action.

## Features Documentation

### ConfigsProvider

Type-safe, high-performance configuration storage with O(1) lookups.

**Key Points:**
- Pre-built dictionaries for instant access; versioning for backend sync
- Singleton configs via `AddSingletonConfig<T>()`
- Zero-allocation enumeration via `EnumerateConfigs<T>()`

```csharp
var provider = new ConfigsProvider();

// Add collection with ID resolver
provider.AddConfigs(item => item.Id, itemConfigs);

// Add singleton config
provider.AddSingletonConfig(new GameSettings { Difficulty = 2 });

// Access configs
var item = provider.GetConfig<ItemConfig>(42);              // By ID
var settings = provider.GetConfig<GameSettings>();          // Singleton
var allItems = provider.GetConfigsList<ItemConfig>();       // As list (allocates)
var itemDict = provider.GetConfigsDictionary<ItemConfig>(); // As dictionary

// Version is read-only at runtime (set via UpdateTo during deserialization)
ulong version = provider.Version;
```

**Zero-Allocation Enumeration:**

For performance-critical code paths, use `EnumerateConfigs<T>()` to avoid list allocations:

```csharp
// ❌ Allocates a new list every call
foreach (var enemy in provider.GetConfigsList<EnemyConfig>())
{
    ProcessEnemy(enemy);
}

// ✅ Zero allocations - enumerate directly over internal storage
foreach (var enemy in provider.EnumerateConfigs<EnemyConfig>())
{
    ProcessEnemy(enemy);
}

// Works with LINQ (but LINQ itself may allocate)
var bosses = provider.EnumerateConfigs<EnemyConfig>()
    .Where(e => e.IsBoss);
```

---

### ConfigsSerializer

JSON serialization for client/server config synchronization.

**Key Points:**
- Uses `Newtonsoft.Json` with enum-as-string handling
- `[IgnoreServerSerialization]` attribute to exclude client-only configs
- Versioning is **numeric** (`ulong`) — parsed with `ulong.TryParse`
- Two security modes: **TrustedOnly** (default, whitelist via `ConfigTypesBinder`) and **Secure** (no `TypeNameHandling`, serialize-only)
- Optional `MaxDepth` (default: 128) prevents stack overflow from deeply nested JSON

```csharp
var serializer = new ConfigsSerializer(); // TrustedOnly by default

// Serialize / Deserialize
string json = serializer.Serialize(configsProvider, "123"); // numeric version string
var newProvider = serializer.Deserialize<ConfigsProvider>(json);

// Pre-register types for deserialization without prior serialization
serializer.RegisterAllowedTypes(new[] { typeof(EnemyConfig), typeof(ItemConfig) });
```

**Backend Sync:** Implement `IConfigBackendService` (UniTask-based) to fetch remote configs and apply atomically via `provider.UpdateTo(remoteVersion, remoteProvider.GetAllConfigs())`.

---

### Observable Data Types

Reactive wrappers for fields, lists, and dictionaries with change callbacks.

#### ObservableField

```csharp
var score = new ObservableField<int>(0);

// Subscribe to changes
score.Observe((prev, curr) => Debug.Log($"Score: {prev} → {curr}"));

// Subscribe and invoke immediately
score.InvokeObserve((prev, curr) => UpdateScoreUI(curr));

// Modify (triggers observers)
score.Value = 100;

// Implicit conversion
int currentScore = score;

// Rebind to new value without losing observers
score.Rebind(0);

// Cleanup
score.StopObservingAll(this);   // Stop this subscriber
score.StopObservingAll();       // Stop all subscribers
```

#### ObservableList

```csharp
var inventory = new ObservableList<string>(new List<string>());

// Subscribe to all changes
inventory.Observe((index, prev, curr, updateType) =>
{
    switch (updateType)
    {
        case ObservableUpdateType.Added:
            Debug.Log($"Added at {index}: {curr}");
            break;
        case ObservableUpdateType.Removed:
            Debug.Log($"Removed from {index}: {prev}");
            break;
        case ObservableUpdateType.Updated:
            Debug.Log($"Updated at {index}: {prev} → {curr}");
            break;
    }
});

// Modify (triggers observers)
inventory.Add("Sword");
inventory[0] = "Golden Sword";
inventory.RemoveAt(0);
inventory.Clear();
```

#### ObservableDictionary

```csharp
var stats = new ObservableDictionary<string, int>(new Dictionary<string, int>());

// Subscribe to specific key
stats.Observe("health", (key, prev, curr, type) =>
{
    Debug.Log($"{key}: {prev} → {curr}");
});

// Subscribe to all changes
stats.Observe((key, prev, curr, type) => { /* ... */ });

// Modify
stats.Add("health", 100);
stats["health"] = 90;
stats.Remove("health");
```

#### ComputedField

Automatically computed values that update when their dependencies change.

```csharp
using GameLovers.GameData;

// Source observable fields
var baseHealth = new ObservableField<int>(100);
var bonusHealth = new ObservableField<int>(25);

// Computed field automatically tracks dependencies
var totalHealth = new ComputedField<int>(() => baseHealth.Value + bonusHealth.Value);

// Subscribe to computed value changes
totalHealth.Observe((prev, curr) => Debug.Log($"Total HP: {prev} → {curr}"));

// Fluent API: Select, CombineWith, Combine
var displayHealth = baseHealth.Select(h => $"HP: {h}");
var combined1 = baseHealth.CombineWith(bonusHealth, (a, b) => a + b);
var combined2 = ObservableField.Combine(baseHealth, bonusHealth, (a, b) => a + b);

// Changing any dependency automatically updates the computed field
baseHealth.Value = 120;  // Triggers: "Total HP: 125 → 145"
bonusHealth.Value = 50;  // Triggers: "Total HP: 145 → 170"

// Access current value (recomputes if dirty)
int currentTotal = totalHealth.Value;

// Cleanup when done
totalHealth.Dispose();
```

**Key Points:**
- Dependencies tracked automatically; lazy evaluation (recomputes only when accessed after change)
- Supports chaining: computed fields can depend on other computed fields
- Fluent API: `Select()` (transform), `CombineWith()` (merge 2–3 fields), `ObservableField.Combine()` (static factory)
- Call `Dispose()` to unsubscribe from all dependencies

#### Batched Updates

Use `BeginBatch()` to make multiple changes without triggering intermediate callbacks:

```csharp
var health = new ObservableField<int>(100);
var mana = new ObservableField<int>(50);
var stamina = new ObservableField<int>(75);

// Without batching: each change triggers observers immediately
health.Value = 80;   // Observer called
mana.Value = 40;     // Observer called
stamina.Value = 60;  // Observer called

// With batching: observers called once after all changes
using (health.BeginBatch())
{
    health.Value = 80;
    mana.Value = 40;
    stamina.Value = 60;
}
// All observers called here, after the batch completes

// Useful for atomic state transitions
public void ApplyDamage(int damage, int manaCost)
{
    using (_health.BeginBatch())
    {
        _health.Value -= damage;
        _mana.Value -= manaCost;
        _lastHitTime.Value = Time.time;
    }
    // UI updates once with final state, not intermediate values
}
```

---

### Deterministic floatP

Deterministic floating-point type for cross-platform reproducible calculations.

**Key Points:**
- IEEE 754 binary32 compatible — bit-exact results across all platforms (including WebGL)
- Essential for multiplayer synchronization and replays
- Full operator support (+, -, *, /, %, comparisons)

```csharp
floatP a = 3.14f;              // Implicit conversion from float
floatP sum = a + 2.0f;         // Arithmetic operations
float result = (float)sum;     // Convert back to float
int integer = (int)a;          // Convert to int

// Raw value for deterministic serialization (compare RawValue, not float)
uint raw = a.RawValue;
floatP restored = floatP.FromRaw(raw);
```

**MathfloatP** provides deterministic equivalents: `Abs`, `Min`, `Max`, `Sin`, `Cos`, `Tan`, `Atan2`, `Sqrt`, `Pow`, `Exp`, `Log`, `Floor`, `Ceil`, `Round`, `Clamp`.

---

### UnitySerializedDictionary

Dictionary type that can be edited in the Unity Inspector.

**Key Points:**
- Inherit from `UnitySerializedDictionary<TKey, TValue>` to create concrete types
- Unity requires concrete types for serialization (no generics in Inspector)

```csharp
using System;
using GameLovers.GameData;

// Create concrete dictionary type
[Serializable]
public class StringIntDictionary : UnitySerializedDictionary<string, int> { }

// Use in MonoBehaviour
public class GameManager : MonoBehaviour
{
    [SerializeField] 
    private StringIntDictionary _levelScores = new();
    
    void Start()
    {
        // Use as regular dictionary
        _levelScores["Level1"] = 1000;
        
        if (_levelScores.TryGetValue("Level1", out int score))
        {
            Debug.Log($"Score: {score}");
        }
    }
}
```

---

### EnumSelector

Enum dropdown that stores the name instead of the value, surviving enum changes.

**Key Points:**
- Stores enum as string internally
- Survives when enum values are added/removed/reordered
- Requires custom `PropertyDrawer` for Inspector dropdown

```csharp
using System;
using GameLovers.GameData;

public enum ItemType { Weapon, Armor, Consumable }

// Create concrete selector type
[Serializable]
public class ItemTypeSelector : EnumSelector<ItemType>
{
    public ItemTypeSelector() : base(ItemType.Weapon) { }
    public ItemTypeSelector(ItemType type) : base(type) { }
}

// Use in ScriptableObject or MonoBehaviour
public class ItemData : ScriptableObject
{
    public ItemTypeSelector Type;
    
    void Start()
    {
        // Get enum value
        ItemType type = Type.GetSelection();
        
        // Or use implicit conversion
        ItemType type2 = Type;
        
        // Check validity (if enum was renamed/removed)
        if (Type.HasValidSelection())
        {
            Debug.Log($"Type: {Type.GetSelectionString()}");
        }
    }
}
```

> **Note:** See `Samples~/Designer Workflow/` for a complete `EnumSelector` PropertyDrawer implementation with `ItemTypeSelector`.

---

### Observable Resolver Types

Transform collections while maintaining observability.

```csharp
// Original data
var rawScores = new Dictionary<int, float>
{
    { 1, 95.5f },
    { 2, 87.2f }
};

// Create resolver that transforms to display format
var displayScores = new ObservableResolverDictionary<string, string, int, float>(
    rawScores,
    pair => new KeyValuePair<string, string>($"Player{pair.Key}", $"{pair.Value:F1}%"),
    (key, value) => new KeyValuePair<int, float>(int.Parse(key.Replace("Player", "")), float.Parse(value.Replace("%", "")))
);

// Access transformed data
string score = displayScores["Player1"]; // "95.5%"

// Observe changes on transformed data
displayScores.Observe((key, prev, curr, type) => UpdateUI(key, curr));
```

---

## Samples

Import samples via **Package Manager** → **GameLovers GameData** → **Samples**

### Reactive UI Demo

Demonstrates:
- `ObservableField<T>` for reactive value binding
- `ObservableList<T>` for dynamic list UI
- `ComputedField<T>` for auto-updating derived values
- `ObservableBatch` for atomic multi-field updates
- Both uGUI and UI Toolkit bindings

### Designer Workflow

Demonstrates:
- `ConfigsScriptableObject<TId, TAsset>` for designer-editable configs
- `UnitySerializedDictionary<TKey, TValue>` for Inspector dictionary editing
- `EnumSelector<T>` with custom `PropertyDrawer`
- Loading ScriptableObject configs into `ConfigsProvider` at runtime

### Migration

Demonstrates (Editor-only):
- Schema evolution (v1 → v2 → v3)
- `IConfigMigration` for defining transformations
- `MigrationRunner` for applying migrations
- Config Browser workflow for previewing and executing migrations

---

## Contributing

We welcome contributions! Report bugs or request features via [GitHub Issues](https://github.com/CoderGamester/com.gamelovers.gamedata/issues). Include Unity version, package version, and reproduction steps.

**Code Guidelines:** C# 9.0 with explicit namespaces, XML docs on public APIs, unit tests for new features, runtime code must not reference `UnityEditor`. Update [CHANGELOG.md](CHANGELOG.md) for notable changes.

---

## Support

- **Issues**: [Report bugs or request features](https://github.com/CoderGamester/com.gamelovers.gamedata/issues)
- **Discussions**: [Ask questions and share ideas](https://github.com/CoderGamester/com.gamelovers.gamedata/discussions)
- **Changelog**: See [CHANGELOG.md](CHANGELOG.md) for version history

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

---

**Made with ❤️ for the Unity community**

*If this package helps your project, please consider giving it a ⭐ on GitHub!*
