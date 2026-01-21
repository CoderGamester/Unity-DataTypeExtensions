# GameLovers GameData

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)](CHANGELOG.md)

> **Quick Links**: [Installation](#installation) | [Quick Start](#quick-start) | [Features](#features-documentation) | [Editor Tools](#editor-tools)| [Contributing](#contributing)

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

## Package Structure

```
Runtime/
├── ConfigServices/
│   ├── ConfigsProvider.cs        # Type-safe config storage
│   ├── ConfigsSerializer.cs      # JSON serialization for backend sync
│   ├── ConfigTypesBinder.cs      # Security whitelist for safe deserialization
│   ├── ConfigsScriptableObject.cs # ScriptableObject-based config containers
│   ├── Interfaces/               # IConfigsProvider, IConfigsAdder, etc.
│   └── Validation/               # Validation attributes (Editor validation lives in Editor/)
├── Observables/
│   ├── ObservableField.cs        # Reactive field wrapper
│   ├── ObservableList.cs         # Reactive list wrapper
│   ├── ObservableDictionary.cs   # Reactive dictionary wrapper
│   └── ComputedField.cs          # Auto-updating computed values
├── Math/
│   ├── floatP.cs                 # Deterministic floating-point type
│   └── MathfloatP.cs             # Math library for floatP
├── Serialization/
│   ├── UnitySerializedDictionary.cs # Dictionary serialization for Inspector
│   └── SerializableType.cs       # Type serialization for Inspector
└── Utilities/
    └── EnumSelector.cs           # Enum dropdown stored by name

Editor/
├── EnumSelectorPropertyDrawer.cs # Custom drawer for EnumSelector
├── ReadOnlyPropertyDrawer.cs     # ReadOnly attribute drawer
├── Inspectors/
│   └── ConfigsScriptableObjectInspector.cs # UI Toolkit inspector for ConfigsScriptableObject<,>
├── Windows/
│   ├── ConfigBrowserWindow.cs     # UI Toolkit window: browse/validate/migrate configs
│   └── ObservableDebugWindow.cs   # UI Toolkit window: inspect observables + dependencies
├── Elements/
│   ├── JsonViewerElement.cs       # Shared JSON viewer element
│   ├── ValidationErrorElement.cs  # Shared validation error row element
│   ├── MigrationPanelElement.cs   # Config Browser migrations panel
│   └── DependencyGraphElement.cs  # Observable Debugger dependency view
└── Validation/                   # EditorConfigValidator + ValidationResult

Samples~/
├── Reactive UI Demo/             # ObservableField + ComputedField with uGUI and UI Toolkit
├── Designer Workflow/            # ScriptableObject editing with ConfigsScriptableObject
└── Migration/                  # Editor-only schema migration demo using Config Browser
```

### Key Components

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

- **Menu**: `Window > GameLovers > Config Browser`
- **Purpose**: Unified window to browse configs from a provider, validate them, export JSON, and preview migrations.
- **Highlights**:
  - Browse tab: select a config entry to view JSON and run **Validate** for the selection.
  - Toolbar: **Validate All** (results displayed inside the window) and **Export JSON**.
  - Migrations tab: conditionally visible when migrations exist (uses `MigrationRunner`).

### Observable Debugger

<!-- Add observableDebugger.png -->

- **Menu**: `Window > GameLovers > Observable Debugger`
- **Purpose**: Inspect live observable instances (`ObservableField`, `ComputedField`, and observable collections).
- **Highlights**:
  - Filtering by name, kind, and “active only”.
  - Selecting a computed observable shows its current dependency list.

### ConfigsScriptableObject Inspector

<!-- Add inspector.png -->

- **Where**: Inspector UI for `ConfigsScriptableObject<,>` derived assets.
- **Purpose**: Inline entry status (duplicate keys / attribute-based validation) and a quick **Validate All** action.

## Quick Start

### 1. Define Your Configs

```csharp
using System;
using GameLovers.GameData;

[Serializable]
public struct EnemyConfig
{
    public int Id;
    public string Name;
    public float Health;
    public float Damage;
}
```

### 2. Initialize the Provider

```csharp
using GameLovers.GameData;
using System.Collections.Generic;

public class GameBootstrap
{
    private ConfigsProvider _configsProvider;
    
    public void Initialize()
    {
        _configsProvider = new ConfigsProvider();
        
        // Add configs with ID resolver
        var enemies = new List<EnemyConfig>
        {
            new EnemyConfig { Id = 1, Name = "Goblin", Health = 50, Damage = 10 },
            new EnemyConfig { Id = 2, Name = "Orc", Health = 100, Damage = 25 }
        };
        
        _configsProvider.AddConfigs(enemy => enemy.Id, enemies);
    }
}
```

### 3. Access Configs at Runtime

```csharp
// Get by ID (O(1) lookup)
var goblin = _configsProvider.GetConfig<EnemyConfig>(1);

// Get all as list
var allEnemies = _configsProvider.GetConfigsList<EnemyConfig>();

// Get as dictionary
var enemyDict = _configsProvider.GetConfigsDictionary<EnemyConfig>();

// Safe lookup
if (_configsProvider.TryGetConfig<EnemyConfig>(3, out var enemy))
{
    Debug.Log($"Found: {enemy.Name}");
}
```

### 4. Use Observable State

```csharp
using GameLovers.GameData;

public class PlayerController
{
    private readonly ObservableField<int> _health;
    private readonly ObservableList<string> _inventory;
    
    public PlayerController()
    {
        _health = new ObservableField<int>(100);
        _inventory = new ObservableList<string>(new List<string>());
        
        // Subscribe to changes (receives previous and current value)
        _health.Observe((prev, curr) => Debug.Log($"HP: {prev} → {curr}"));
        
        // Subscribe and invoke immediately
        _health.InvokeObserve((prev, curr) => UpdateHealthUI(curr));
    }
    
    public void TakeDamage(int damage)
    {
        _health.Value -= damage; // Triggers all observers
    }
    
    private void UpdateHealthUI(int health) { /* ... */ }
}
```

---

## Features Documentation

### ConfigsProvider

Type-safe, high-performance configuration storage with O(1) lookups.

**Key Points:**
- Stores configs in pre-built dictionaries for instant access
- Supports versioning for backend sync scenarios
- Singleton configs supported via `AddSingletonConfig<T>()`
- Zero-allocation enumeration for hot paths

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
- Uses `Newtonsoft.Json` for reliable serialization
- Supports `[IgnoreServerSerialization]` attribute to exclude configs
- Automatically handles enum serialization as strings
- Two security modes for different trust levels (see `SerializationSecurityMode`)
- Versioning is **numeric** (`ulong`) — `Deserialize` parses the serialized `Version` with `ulong.TryParse`

```csharp
var serializer = new ConfigsSerializer();

// Serialize for backend
string json = serializer.Serialize(configsProvider, "123"); // numeric version string

// Deserialize from backend
var newProvider = serializer.Deserialize<ConfigsProvider>(json);

// Or deserialize into existing provider
serializer.Deserialize(json, existingProvider);
```

**Exclude configs from serialization:**

```csharp
[IgnoreServerSerialization]
[Serializable]
public struct ClientOnlyConfig
{
    public string LocalSetting;
}
```

**Security Modes:**

The serializer supports two security modes with built-in protections:

```csharp
// TrustedOnly (default) - Polymorphism via TypeNameHandling.Auto with whitelist protection
// Uses ConfigTypesBinder to only allow explicitly registered config types
var trusted = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);

// Secure - Disables TypeNameHandling entirely (serialize-only)
// ⚠️ Cannot round-trip - use for sending configs TO untrusted targets
var secure = new ConfigsSerializer(SerializationSecurityMode.Secure);

// Optional: Set MaxDepth to prevent stack overflow from deeply nested JSON (default: 128)
var customDepth = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly, maxDepth: 64);
```

**Type Whitelisting (TrustedOnly mode):**

Types are auto-registered during `Serialize()`. For deserialization without prior serialization, pre-register types:

```csharp
var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);

// Pre-register specific types
serializer.RegisterAllowedTypes(new[] { typeof(EnemyConfig), typeof(ItemConfig) });

// Or register from an existing provider
serializer.RegisterAllowedTypesFromProvider(existingProvider);

// Now deserialize - only registered types are allowed
serializer.Deserialize(json, newProvider);
```

**Backend Sync Workflow:**

Typical workflow is to implement `IConfigBackendService` (UniTask-based) and keep versioning numeric (`ulong`):

```csharp
using Cysharp.Threading.Tasks;
using GameLovers.GameData;

public static class ConfigSync
{
    public static async UniTask SyncIfNeeded(IConfigBackendService backend, ConfigsProvider provider)
    {
        ulong remoteVersion = await backend.GetRemoteVersion();
        if (remoteVersion <= provider.Version)
        {
            return;
        }

        // Your backend service can fetch + deserialize using ConfigsSerializer internally.
        IConfigsProvider remoteProvider = await backend.FetchRemoteConfiguration(remoteVersion);

        // Apply atomically (updates configs + version).
        provider.UpdateTo(remoteVersion, remoteProvider.GetAllConfigs());
    }
}
```

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

// Changing any dependency automatically updates the computed field
baseHealth.Value = 120;  // Triggers: "Total HP: 125 → 145"
bonusHealth.Value = 50;  // Triggers: "Total HP: 145 → 170"

// Access current value (recomputes if dirty)
int currentTotal = totalHealth.Value;

// Cleanup when done
totalHealth.Dispose();
```

**Key Points:**
- Dependencies are tracked automatically during computation
- Lazy evaluation: only recomputes when accessed after a dependency change
- Supports chaining: computed fields can depend on other computed fields
- Call `Dispose()` to unsubscribe from all dependencies

##### Fluent API

For more ergonomic computed field creation, use the extension methods or static factory:

```csharp
using GameLovers.GameData;

var baseHealth = new ObservableField<int>(100);
var bonusHealth = new ObservableField<int>(25);
var multiplier = new ObservableField<float>(1.5f);

// Select: Transform a single field
var displayHealth = baseHealth.Select(h => $"HP: {h}");

// CombineWith: Combine two fields
var totalHealth = baseHealth.CombineWith(bonusHealth, (a, b) => a + b);

// CombineWith: Combine three fields  
var finalHealth = baseHealth.CombineWith(bonusHealth, multiplier, 
    (a, b, m) => (int)((a + b) * m));

// Static factory: Combine multiple fields explicitly
var combined = ObservableField.Combine(baseHealth, bonusHealth, (a, b) => a + b);

// All computed fields update automatically when any source changes
baseHealth.Value = 120;  // All computed fields recalculate
```

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
- IEEE 754 binary32 compatible representation
- Bit-exact results across all platforms (including WebGL)
- Essential for multiplayer synchronization and replays
- Full operator support (+, -, *, /, %, comparisons)

```csharp
using GameLovers.GameData;

// Implicit conversion from float
floatP a = 3.14f;
floatP b = 2.0f;

// Arithmetic operations
floatP sum = a + b;
floatP product = a * b;
floatP quotient = a / b;

// Comparisons
bool equal = a == b;
bool greater = a > b;

// Conversion back to float
float result = (float)sum;

// Conversion to int
int integer = (int)a;

// Raw value for serialization
uint raw = a.RawValue;
floatP restored = floatP.FromRaw(raw);
```

**Verifying Cross-Platform Determinism:**

To verify determinism across platforms, compare `RawValue` (the IEEE 754 bit representation) rather than float values:

```csharp
// On platform A: compute and log raw value
floatP result = MathfloatP.Sin(angle) * speed;
Debug.Log($"Result raw: {result.RawValue}");  // e.g., 1082130432

// On platform B: same computation should produce identical raw value
// Compare uint values, not float approximations
Assert.AreEqual(expectedRawValue, result.RawValue);

// For replays/networking: serialize RawValue, not float
public void SerializePosition(BinaryWriter writer, floatP x, floatP y)
{
    writer.Write(x.RawValue);
    writer.Write(y.RawValue);
}

public (floatP x, floatP y) DeserializePosition(BinaryReader reader)
{
    return (floatP.FromRaw(reader.ReadUInt32()), 
            floatP.FromRaw(reader.ReadUInt32()));
}
```

**MathfloatP Functions:**

```csharp
using GameLovers.GameData;

floatP x = 1.5f;

// Basic math
floatP abs = MathfloatP.Abs(x);
floatP max = MathfloatP.Max(x, 2.0f);
floatP min = MathfloatP.Min(x, 0.5f);

// Trigonometry
floatP sin = MathfloatP.Sin(x);
floatP cos = MathfloatP.Cos(x);
floatP tan = MathfloatP.Tan(x);
floatP atan2 = MathfloatP.Atan2(y, x);

// Power and roots
floatP sqrt = MathfloatP.Sqrt(x);
floatP pow = MathfloatP.Pow(x, 2);
floatP exp = MathfloatP.Exp(x);
floatP log = MathfloatP.Log(x);

// Rounding
floatP floor = MathfloatP.Floor(x);
floatP ceil = MathfloatP.Ceil(x);
floatP round = MathfloatP.Round(x);
floatP clamp = MathfloatP.Clamp(x, 0, 1);
```

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

We welcome contributions! Here's how you can help:

### Reporting Issues

- Use the [GitHub Issues](https://github.com/CoderGamester/com.gamelovers.gamedata/issues) page
- Include Unity version, package version, and reproduction steps
- Attach relevant code samples, error logs, or screenshots

### Development Setup

1. Fork the repository on GitHub
2. Clone your fork: `git clone https://github.com/yourusername/com.gamelovers.gamedata.git`
3. Create a feature branch: `git checkout -b feature/amazing-feature`
4. Make your changes with tests
5. Commit: `git commit -m 'Add amazing feature'`
6. Push: `git push origin feature/amazing-feature`
7. Create a Pull Request

### Code Guidelines

- Follow C# 9.0 syntax with explicit namespaces (no global usings)
- Add XML documentation to all public APIs
- Include unit tests for new features
- Runtime code must not reference `UnityEditor`
- Update CHANGELOG.md for notable changes

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
