# GameLovers GameData

Core data utilities and configuration management for Unity games. This package provides a set of high-performance, type-safe tools for managing game design data, observable state, and deterministic math.

## Features

- 🎮 **Configuration Management**: Type-safe, high-performance storage for design data (enemy stats, level configs, item databases).
- 🔄 **Backend Sync**: Built-in JSON serialization/deserialization for atomic updates and client/server config synchronization.
- 📡 **Observable Data Types**: `ObservableField`, `ObservableList`, and `ObservableDictionary` for clean reactive programming.
- 🎯 **Deterministic Math**: Includes `floatP`, a deterministic floating-point type for reproducible cross-platform calculations.
- 📦 **ScriptableObject Containers**: Designer-friendly workflows for editing configs directly in the Unity Inspector.
- 🚀 **O(1) Lookups**: Pre-built in-memory dictionaries for maximum runtime performance.

## Installation

### Via Unity Package Manager (Git URL)

1. Open Unity Package Manager (`Window` → `Package Manager`).
2. Click the **+** button → `Add package from git URL...`.
3. Enter: `https://github.com/CoderGamester/com.gamelovers.gamedata.git`

## Quick Start

### 1. Define Your Configs

```csharp
[Serializable]
public class EnemyConfig : IConfig
{
    public int Id;
    public int ConfigId => Id;
    public string Name;
    public float Health;
}
```

### 2. Initialize the Provider

```csharp
var provider = new ConfigsProvider();
var configs = new List<EnemyConfig> { /* ... */ };

provider.AddConfigs(enemy => enemy.Id, configs);
```

### 3. Use Observable State

```csharp
var playerHp = new ObservableField<float>(100f);
playerHp.Subscribe(hp => Debug.Log($"New HP: {hp}"));
playerHp.Value = 90f; // Triggers callback
```

## Requirements

- Unity 6000.0+
- `Newtonsoft.Json` (com.unity.nuget.newtonsoft-json)

## License

MIT
