# Schema Migration Sample

This sample demonstrates the real developer workflow for migrating configuration schemas using the **Config Browser** and the `MigrationRunner` API.

## Design Philosophy

Config schemas evolve over time as new features are added. This package provides a robust way to transform legacy data to match current class definitions without losing information or manually editing files.

The sample teaches by letting you explore the real tooling:
1. **Explore**: See how migrations are registered and discovered.
2. **Preview**: Visualize transformations before they are applied.
3. **Apply**: Execute migrations individually or in a chain.

## Schema Evolution (SampleEnemyConfig)

The sample follows the evolution of a combat unit config across three versions:

### Version 1 (Original)
- `Id`: int
- `Name`: string
- `Health`: int
- `Damage`: int

### Version 2 (Refactoring)
- **Rename**: `Damage` → `AttackDamage` (more descriptive)
- **New Field**: `ArmorType` (string)
- **Derived Logic**: `ArmorType` is automatically set to "Heavy" if `Health` ≥ 100, otherwise "Medium" or "Light".

### Version 3 (Complexity)
- **Split**: `Health` is split into `BaseHealth` (80%) and `BonusHealth` (20%).
- **New Object**: `Stats` (nested object) containing:
  - `DamageReduction`: derived from `ArmorType`.
  - `CritChance`: derived from `AttackDamage`.
  - `MoveSpeedMultiplier`: derived from `ArmorType`.
- **New Array**: `Abilities` (initialized as empty).

## How to Use

1. **Import the sample** and open the `Migration.unity` scene.
2. **Enter Play Mode**. This initializes a `ConfigsProvider` with v1-schema data and sets its internal version to 1.
3. **Open Config Browser** via the button in the scene or `Window > GameLovers > Config Browser`.
4. Select the active provider in the browser.
5. Navigate to the **Migrations** tab.
6. You will see two pending migrations:
   - `SampleEnemyConfigMigration_v1_v2`
   - `SampleEnemyConfigMigration_v2_v3`
7. Click **Preview** on either to see the JSON transformation.
8. Click **Apply** to execute the migration.

## Implementation Details

### Migration Classes
Migrations are implemented by classes inheriting from `IConfigMigration` and marked with the `[ConfigMigration]` attribute. See the `Editor/` folder for examples:
- `SampleEnemyConfigMigration_v1_v2.cs`: Demonstrates renaming and conditional defaults.
- `SampleEnemyConfigMigration_v2_v3.cs`: Demonstrates splitting fields, nested objects, and arrays.

### API Reference
- `MigrationRunner.GetAvailableMigrations<T>()`: Discovers registered migrations for a type.
- `MigrationRunner.Migrate()`: Applies transformations to a `JObject`.
- `MigrationRunner.MigrateScriptableObject()`: High-level helper for `ScriptableObject` assets.
