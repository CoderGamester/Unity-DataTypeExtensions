# GameLovers GameData

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)](CHANGELOG.md)

> **Quick Links**: [Installation](#installation) | [Quick Start](#quick-start) | [Features](#features-documentation) | [Contributing](#contributing)

## Why Use This Package?

Managing game data in Unity often leads to fragmented solutions: scattered config files, tight coupling between data and logic, and cross-platform inconsistencies. This **GameData** package addresses these challenges:

| Problem | Solution |
|---------|----------|
| **Scattered config management** | Type-safe `ConfigsProvider` with O(1) lookups and versioning |
| **Tight coupling to data changes** | Observable types (`ObservableField`, `ObservableList`, `ObservableDictionary`) for reactive programming |
| **Cross-platform float inconsistencies** | Deterministic `floatP` type for reproducible calculations across all platforms |
| **Backend sync complexity** | Built-in JSON serialization with `ConfigsSerializer` for client/server sync |
| **Dictionary Inspector editing** | `UnitySerializedDictionary` for seamless Inspector support |
| **Fragile enum serialization** | `EnumSelector` stores enum names (not values) to survive enum changes |

**Built for production:** Minimal dependencies. Zero per-frame allocations in observable types. Used in real games.

### Key Features

- **📊 Configuration Management** - Type-safe, high-performance config storage with versioning
- **🔄 Observable Data Types** - `ObservableField`, `ObservableList`, `ObservableDictionary` for reactive updates
- **🎯 Deterministic Math** - `floatP` type for cross-platform reproducible floating-point calculations
- **📡 Backend Sync** - JSON serialization/deserialization for atomic config updates
- **📦 ScriptableObject Containers** - Designer-friendly workflows for editing configs in Inspector
- **🔢 Unity Serialization** - `UnitySerializedDictionary` and `EnumSelector` for Inspector support
- **⚡ O(1) Lookups** - Pre-built in-memory dictionaries for maximum runtime performance

---

## System Requirements

- **[Unity](https://unity.com/download)** 6000.0+ (Unity 6)
- **[Newtonsoft.Json](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html)** (com.unity.nuget.newtonsoft-json v3.2.1) - Automatically resolved

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
   https://github.com/CoderGamester/com.gamelovers.gamedata.git
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
├── ConfigProvider.cs             # Type-safe config storage
├── ConfigSerializer.cs           # JSON serialization for backend sync
├── ConfigsScriptableObject.cs    # ScriptableObject-based config containers
├── IConfigsProvider.cs           # Config provider interface
├── IConfigsAdder.cs              # Config adder interface
├── IConfigSerializer.cs          # Serializer interface
├── ObservableField.cs            # Reactive field wrapper
├── ObservableList.cs             # Reactive list wrapper
├── ObservableDictionary.cs       # Reactive dictionary wrapper
├── floatP.cs                     # Deterministic floating-point type
├── MathfloatP.cs                 # Math library for floatP
├── EnumSelector.cs               # Enum dropdown stored by name
├── UnitySerializedDictionary.cs  # Dictionary serialization for Inspector
├── SerializableType.cs           # Type serialization for Inspector
└── ValueData.cs                  # Vector extensions (Pair, StructPair)

Editor/
├── EnumSelectorPropertyDrawer.cs # Custom drawer for EnumSelector
└── ReadOnlyPropertyDrawer.cs     # ReadOnly attribute drawer

Samples~/
└── Enum Selector Example/        # EnumSelector usage sample
```

### Key Components

| Component | Responsibility |
|-----------|----------------|
| **ConfigsProvider** | Type-safe config storage with O(1) lookups and versioning |
| **ConfigsSerializer** | JSON serialization for client/server config synchronization |
| **ObservableField** | Reactive wrapper for single values with change callbacks |
| **ObservableList** | Reactive wrapper for lists with add/remove/update callbacks |
| **ObservableDictionary** | Reactive wrapper for dictionaries with key-based callbacks |
| **floatP** | Deterministic floating-point type for cross-platform math |
| **MathfloatP** | Math functions (Sin, Cos, Sqrt, etc.) for floatP type |
| **EnumSelector** | Enum dropdown that survives enum value changes |
| **UnitySerializedDictionary** | Dictionary type visible in Unity Inspector |

---

## Quick Start

### 1. Define Your Configs

```csharp
using System;
using GameLovers.ConfigsProvider;

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
using GameLovers.ConfigsProvider;

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
        _configsProvider.SetVersion(1);
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
using GameLovers;

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

```csharp
var provider = new ConfigsProvider();

// Add collection with ID resolver
provider.AddConfigs(item => item.Id, itemConfigs);

// Add singleton config
provider.AddSingletonConfig(new GameSettings { Difficulty = 2 });

// Access configs
var item = provider.GetConfig<ItemConfig>(42);              // By ID
var settings = provider.GetConfig<GameSettings>();          // Singleton
var allItems = provider.GetConfigsList<ItemConfig>();       // As list
var itemDict = provider.GetConfigsDictionary<ItemConfig>(); // As dictionary

// Version management
provider.SetVersion(12345);
ulong version = provider.Version;
```

---

### ConfigsSerializer

JSON serialization for client/server config synchronization.

**Key Points:**
- Uses `Newtonsoft.Json` for reliable serialization
- Supports `[IgnoreServerSerialization]` attribute to exclude configs
- Automatically handles enum serialization as strings

```csharp
var serializer = new ConfigsSerializer();

// Serialize for backend
string json = serializer.Serialize(configsProvider, "1.0.0");

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

---

### Deterministic floatP

Deterministic floating-point type for cross-platform reproducible calculations.

**Key Points:**
- IEEE 754 binary32 compatible representation
- Bit-exact results across all platforms (including WebGL)
- Essential for multiplayer synchronization and replays
- Full operator support (+, -, *, /, %, comparisons)

```csharp
using GameLovers;

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

**MathfloatP Functions:**

```csharp
using GameLovers;

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
using GameLovers;

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
using GameLovers;

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

> **Note:** See `Samples~/Enum Selector Example/` for complete PropertyDrawer implementation.

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

### Enum Selector Example

Import via Package Manager → GameLovers GameData → Samples → Enum Selector Example

Demonstrates:
- Creating concrete `EnumSelector<T>` types
- Custom `PropertyDrawer` for Inspector dropdown
- Runtime enum access and validation

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
