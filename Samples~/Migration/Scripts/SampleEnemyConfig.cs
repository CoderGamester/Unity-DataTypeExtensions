using System;

namespace GameLovers.GameData.Samples.Migration
{
	/// <summary>
	/// Config used to demonstrate complex schema migrations with <see cref="GameLoversEditor.GameData.MigrationRunner"/>.
	/// 
	/// Schema Evolution:
	/// - v1: Id, Name, Health, Damage (original schema)
	/// - v2: Renamed Damage → AttackDamage, added ArmorType with conditional default
	/// - v3: Split Health → BaseHealth + BonusHealth, added nested Stats object with derived values,
	///       added Abilities array
	/// </summary>
	[Serializable]
	public struct SampleEnemyConfig
	{
		// ═══════════════════════════════════════════════════════════════════
		// Fields present since v1
		// ═══════════════════════════════════════════════════════════════════
		
		public int Id;
		public string Name;

		// ═══════════════════════════════════════════════════════════════════
		// Fields renamed in v2 (Damage → AttackDamage)
		// ═══════════════════════════════════════════════════════════════════
		
		/// <summary>
		/// Base attack damage. Renamed from "Damage" in v2 migration.
		/// </summary>
		public int AttackDamage;

		// ═══════════════════════════════════════════════════════════════════
		// Fields added in v2
		// ═══════════════════════════════════════════════════════════════════
		
		/// <summary>
		/// Armor type classification. Added in v2 with conditional default based on Health:
		/// - Health >= 100: "Heavy"
		/// - Health >= 50: "Medium"  
		/// - Otherwise: "Light"
		/// </summary>
		public string ArmorType;

		// ═══════════════════════════════════════════════════════════════════
		// Fields added/modified in v3
		// ═══════════════════════════════════════════════════════════════════
		
		/// <summary>
		/// Base health pool (80% of original Health value during migration).
		/// Split from "Health" in v3.
		/// </summary>
		public int BaseHealth;

		/// <summary>
		/// Bonus health from equipment/buffs (20% of original Health value during migration).
		/// Split from "Health" in v3.
		/// </summary>
		public int BonusHealth;

		/// <summary>
		/// Computed stats derived from other fields during v3 migration.
		/// </summary>
		public EnemyStats Stats;

		/// <summary>
		/// List of ability identifiers. Initialized as empty array in v3.
		/// </summary>
		public string[] Abilities;
	}
}
