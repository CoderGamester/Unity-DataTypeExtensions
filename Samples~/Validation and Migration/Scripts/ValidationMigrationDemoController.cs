using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Editor-only demo (play mode) for validation and migration.
	/// Demonstrates complex multi-version migrations with intermediate state visualization.
	/// </summary>
	public sealed class ValidationMigrationDemoController : MonoBehaviour
	{
		[SerializeField] private Button _validateButton;
		[SerializeField] private Button _migrateButton;
		[SerializeField] private TMP_Text _output;
    
		private void Awake()
		{
			if (_validateButton != null)
			{
				_validateButton.onClick.AddListener(RunValidation);
			}

			if (_migrateButton != null)
			{
				_migrateButton.onClick.AddListener(RunMigration);
			}

			SetOutput(BuildInitialOutput());
		}

		private static string BuildInitialOutput()
		{
			var sb = new StringBuilder(2048);
			sb.AppendLine("═══════════════════════════════════════════");
			sb.AppendLine("Click a button on the left to execute an operation.");
			sb.AppendLine("═══════════════════════════════════════════");
			sb.AppendLine();

			sb.AppendLine("CONFIG SCHEMAS:");
			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine();

			sb.AppendLine("SamplePlayerConfig (for Validation):");
			sb.AppendLine("  • Id: int");
			sb.AppendLine("  • Name: string [Required]");
			sb.AppendLine("  • MaxHealth: int [Range(1, 1000)]");
			sb.AppendLine("  • Description: string [MinLength(3)]");
			sb.AppendLine();

			sb.AppendLine("SampleEnemyConfig (for Migration v1→v2→v3):");
			sb.AppendLine("  v1: Id, Name, Health, Damage");
			sb.AppendLine("  v2: +ArmorType, Damage→AttackDamage");
			sb.AppendLine("  v3: Health→BaseHealth+BonusHealth, +Stats, +Abilities[]");
			sb.AppendLine();

			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine("AVAILABLE OPERATIONS:");
			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine();
			sb.AppendLine("[Validate] - Validates SamplePlayerConfig instances");
			sb.AppendLine("             using validation attributes (Required,");
			sb.AppendLine("             Range, MinLength). Shows valid/invalid configs.");
			sb.AppendLine();
			sb.AppendLine("[Migrate]  - Demonstrates multi-version schema migration");
			sb.AppendLine("             (v1→v2→v3) with intermediate states showing");
			sb.AppendLine("             field renaming, splitting, nested objects,");
			sb.AppendLine("             and computed values.");

			return sb.ToString();
		}

		private void RunValidation()
		{
#if !UNITY_EDITOR
			SetOutput("This sample is Editor-only.");
			return;
#else
			var provider = new ConfigsProvider();

			var configs = new List<SamplePlayerConfig>
			{
				new SamplePlayerConfig
				{
					Id = 1,
					Name = "",
					MaxHealth = 0,
					Description = "a"
				},
				new SamplePlayerConfig
				{
					Id = 2,
					Name = "Hero",
					MaxHealth = 100,
					Description = "Valid"
				}
			};

			provider.AddConfigs(c => c.Id, configs);

			var result = GameLoversEditor.GameData.EditorConfigValidator.ValidateAll(provider);

			var sb = new StringBuilder(1024);
			sb.AppendLine("Validation Result");
			sb.AppendLine($"IsValid: {result.IsValid}");
			sb.AppendLine();

			sb.AppendLine($"Valid Configs ({result.ValidConfigs.Count}):");
			if (result.ValidConfigs.Count == 0)
			{
				sb.AppendLine("- (none)");
			}
			else
			{
				for (var i = 0; i < result.ValidConfigs.Count; i++)
				{
					sb.AppendLine($"- {result.ValidConfigs[i]}");
				}
			}
			sb.AppendLine();

			sb.AppendLine($"Errors ({result.Errors.Count}):");
			if (result.Errors.Count == 0)
			{
				sb.AppendLine("- (none)");
			}
			else
			{
				for (var i = 0; i < result.Errors.Count; i++)
				{
					sb.AppendLine($"- {result.Errors[i]}");
				}
			}

			SetOutput(sb.ToString());
#endif
		}

		private void RunMigration()
		{
#if !UNITY_EDITOR
			SetOutput("This sample is Editor-only.");
			return;
#else
			GameLoversEditor.GameData.MigrationRunner.Initialize(force: true);

			// ═══════════════════════════════════════════════════════════════
			// Create v1 data (original schema)
			// ═══════════════════════════════════════════════════════════════
			var v1Data = new JObject
			{
				["Id"] = 1,
				["Name"] = "Orc Warlord",
				["Health"] = 150,
				["Damage"] = 25
			};

			var sb = new StringBuilder(4096);
			sb.AppendLine("═══════════════════════════════════════════");
			sb.AppendLine("MIGRATION DEMO: SampleEnemyConfig (v1 → v2 → v3)");
			sb.AppendLine("═══════════════════════════════════════════");
			sb.AppendLine();

			// ═══════════════════════════════════════════════════════════════
			// Show available migrations
			// ═══════════════════════════════════════════════════════════════
			var configType = typeof(SampleEnemyConfig);
			var migrations = GameLoversEditor.GameData.MigrationRunner.GetAvailableMigrations(configType);
			
			sb.AppendLine("Available Migrations:");
			if (migrations.Count == 0)
			{
				sb.AppendLine("  (none discovered)");
			}
			else
			{
				for (var i = 0; i < migrations.Count; i++)
				{
					sb.AppendLine($"  • {migrations[i].MigrationType.Name}");
					sb.AppendLine($"    Version: {migrations[i].FromVersion} → {migrations[i].ToVersion}");
				}
			}
			sb.AppendLine();

			// ═══════════════════════════════════════════════════════════════
			// STEP 1: Show v1 (Original)
			// ═══════════════════════════════════════════════════════════════
			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine("STEP 1: Original Data (v1)");
			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine("Schema: Id, Name, Health, Damage");
			sb.AppendLine();
			sb.AppendLine(v1Data.ToString(Newtonsoft.Json.Formatting.Indented));
			sb.AppendLine();

			// ═══════════════════════════════════════════════════════════════
			// STEP 2: Migrate v1 → v2
			// ═══════════════════════════════════════════════════════════════
			var v2Data = (JObject)v1Data.DeepClone();
			var v1ToV2Count = GameLoversEditor.GameData.MigrationRunner.Migrate(configType, v2Data, currentVersion: 1, targetVersion: 2);

			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine($"STEP 2: After v1 → v2 Migration ({v1ToV2Count} migration applied)");
			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine("Changes:");
			sb.AppendLine("  • Renamed: Damage → AttackDamage");
			sb.AppendLine("  • Added: ArmorType (derived from Health ≥ 100 → 'Heavy')");
			sb.AppendLine();
			sb.AppendLine(v2Data.ToString(Newtonsoft.Json.Formatting.Indented));
			sb.AppendLine();

			// ═══════════════════════════════════════════════════════════════
			// STEP 3: Migrate v2 → v3
			// ═══════════════════════════════════════════════════════════════
			var v3Data = (JObject)v2Data.DeepClone();
			var v2ToV3Count = GameLoversEditor.GameData.MigrationRunner.Migrate(configType, v3Data, currentVersion: 2, targetVersion: 3);

			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine($"STEP 3: After v2 → v3 Migration ({v2ToV3Count} migration applied)");
			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine("Changes:");
			sb.AppendLine("  • Split: Health → BaseHealth (80%) + BonusHealth (20%)");
			sb.AppendLine("  • Added: Stats (nested object with derived values)");
			sb.AppendLine("    - DamageReduction: derived from ArmorType");
			sb.AppendLine("    - CritChance: derived from AttackDamage");
			sb.AppendLine("    - MoveSpeedMultiplier: derived from ArmorType");
			sb.AppendLine("  • Added: Abilities (empty array)");
			sb.AppendLine();
			sb.AppendLine(v3Data.ToString(Newtonsoft.Json.Formatting.Indented));
			sb.AppendLine();

			// ═══════════════════════════════════════════════════════════════
			// BONUS: Show full chain v1 → v3 in one call
			// ═══════════════════════════════════════════════════════════════
			var fullChainData = (JObject)v1Data.DeepClone();
			var fullChainCount = GameLoversEditor.GameData.MigrationRunner.Migrate(configType, fullChainData, currentVersion: 1, targetVersion: 3);

			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine($"BONUS: Full Chain (v1 → v3) in One Call");
			sb.AppendLine("───────────────────────────────────────────");
			sb.AppendLine($"Total migrations applied: {fullChainCount}");
			sb.AppendLine($"Matches step-by-step result: {JToken.DeepEquals(v3Data, fullChainData)}");
			sb.AppendLine();

			// ═══════════════════════════════════════════════════════════════
			// Summary
			// ═══════════════════════════════════════════════════════════════
			sb.AppendLine("═══════════════════════════════════════════");
			sb.AppendLine("MIGRATION PATTERNS DEMONSTRATED:");
			sb.AppendLine("═══════════════════════════════════════════");
			sb.AppendLine("  1. Field Renaming (Damage → AttackDamage)");
			sb.AppendLine("  2. Conditional Defaults (ArmorType from Health)");
			sb.AppendLine("  3. Field Splitting (Health → Base + Bonus)");
			sb.AppendLine("  4. Nested Object Creation (Stats)");
			sb.AppendLine("  5. Computed/Derived Values (DamageReduction, CritChance)");
			sb.AppendLine("  6. Array Initialization (Abilities)");
			sb.AppendLine("  7. Migration Chaining (v1 → v2 → v3)");

			SetOutput(sb.ToString());
#endif
		}

		private void SetOutput(string text)
		{
			if (_output != null)
			{
				_output.text = text;
			}
		}
	}
}
