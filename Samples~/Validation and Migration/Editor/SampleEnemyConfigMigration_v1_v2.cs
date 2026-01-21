using GameLoversEditor.GameData;
using Newtonsoft.Json.Linq;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Migration from version 1 to version 2 for <see cref="SampleEnemyConfig"/>.
	/// 
	/// Demonstrates:
	/// - Field renaming (Damage → AttackDamage)
	/// - Conditional default values based on existing data (ArmorType derived from Health)
	/// </summary>
	[ConfigMigration(typeof(SampleEnemyConfig))]
	public sealed class SampleEnemyConfigMigration_v1_v2 : IConfigMigration
	{
		public ulong FromVersion => 1;
		public ulong ToVersion => 2;

		public void Migrate(JObject configJson)
		{
			// ─────────────────────────────────────────────────────────────────
			// Pattern 1: Field Renaming (Damage → AttackDamage)
			// ─────────────────────────────────────────────────────────────────
			if (configJson["Damage"] != null)
			{
				configJson["AttackDamage"] = configJson["Damage"];
				configJson.Remove("Damage");
			}

			// ─────────────────────────────────────────────────────────────────
			// Pattern 2: Conditional Default (ArmorType based on Health)
			// ─────────────────────────────────────────────────────────────────
			if (configJson["ArmorType"] == null)
			{
				var health = configJson["Health"]?.Value<int>() ?? 0;

				// Derive armor type from health value
				string armorType;
				if (health >= 100)
				{
					armorType = "Heavy";
				}
				else if (health >= 50)
				{
					armorType = "Medium";
				}
				else
				{
					armorType = "Light";
				}

				configJson["ArmorType"] = armorType;
			}
		}
	}
}
