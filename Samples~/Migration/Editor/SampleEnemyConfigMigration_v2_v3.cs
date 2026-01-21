using GameLoversEditor.GameData;
using Newtonsoft.Json.Linq;

namespace GameLovers.GameData.Samples.Migration
{
	/// <summary>
	/// Migration from version 2 to version 3 for <see cref="SampleEnemyConfig"/>.
	/// 
	/// Demonstrates:
	/// - Field splitting (Health → BaseHealth + BonusHealth)
	/// - Nested object creation (Stats with derived DamageReduction)
	/// - Array initialization (Abilities)
	/// - Data derivation (computing values from existing fields)
	/// </summary>
	[ConfigMigration(typeof(SampleEnemyConfig))]
	public sealed class SampleEnemyConfigMigration_v2_v3 : IConfigMigration
	{
		public ulong FromVersion => 2;
		public ulong ToVersion => 3;

		public void Migrate(JObject configJson)
		{
			// ─────────────────────────────────────────────────────────────────
			// Pattern 1: Field Splitting (Health → BaseHealth + BonusHealth)
			// Split original health: 80% base, 20% bonus
			// ─────────────────────────────────────────────────────────────────
			if (configJson["Health"] != null)
			{
				var totalHealth = configJson["Health"].Value<int>();
				
				// Calculate split (80% base, 20% bonus, ensuring no rounding loss)
				var baseHealth = (int)(totalHealth * 0.8f);
				var bonusHealth = totalHealth - baseHealth;

				configJson["BaseHealth"] = baseHealth;
				configJson["BonusHealth"] = bonusHealth;
				
				// Remove the old field
				configJson.Remove("Health");
			}
			else
			{
				// Fallback if Health is somehow missing
				if (configJson["BaseHealth"] == null) configJson["BaseHealth"] = 0;
				if (configJson["BonusHealth"] == null) configJson["BonusHealth"] = 0;
			}

			// ─────────────────────────────────────────────────────────────────
			// Pattern 2: Nested Object Creation with Derived Values
			// Create Stats object with DamageReduction derived from ArmorType
			// ─────────────────────────────────────────────────────────────────
			if (configJson["Stats"] == null)
			{
				var armorType = configJson["ArmorType"]?.Value<string>() ?? "Light";
				var attackDamage = configJson["AttackDamage"]?.Value<int>() ?? 0;

				// Derive DamageReduction from ArmorType
				int damageReduction;
				switch (armorType)
				{
					case "Heavy":
						damageReduction = 40;
						break;
					case "Medium":
						damageReduction = 20;
						break;
					case "Light":
					default:
						damageReduction = 5;
						break;
				}

				// Derive CritChance from AttackDamage (higher damage = lower crit, cap at 25%)
				var critChance = attackDamage > 0 
					? System.Math.Min(25, 500 / attackDamage) 
					: 10;

				// Derive MoveSpeedMultiplier from ArmorType
				float moveSpeed;
				switch (armorType)
				{
					case "Heavy":
						moveSpeed = 0.7f;
						break;
					case "Medium":
						moveSpeed = 1.0f;
						break;
					case "Light":
					default:
						moveSpeed = 1.3f;
						break;
				}

				configJson["Stats"] = new JObject
				{
					["DamageReduction"] = damageReduction,
					["CritChance"] = critChance,
					["MoveSpeedMultiplier"] = moveSpeed
				};
			}

			// ─────────────────────────────────────────────────────────────────
			// Pattern 3: Array Initialization
			// Initialize empty Abilities array for future use
			// ─────────────────────────────────────────────────────────────────
			if (configJson["Abilities"] == null)
			{
				configJson["Abilities"] = new JArray();
			}
		}
	}
}
