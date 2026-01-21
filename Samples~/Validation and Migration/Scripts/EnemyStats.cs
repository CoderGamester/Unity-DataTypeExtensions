using System;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Nested stats structure used by <see cref="SampleEnemyConfig"/> to demonstrate
	/// complex schema migrations that introduce nested objects.
	/// Added in v3 via migration.
	/// </summary>
	[Serializable]
	public struct EnemyStats
	{
		/// <summary>
		/// Damage reduction percentage (0-100). Derived from ArmorType during migration.
		/// </summary>
		public int DamageReduction;

		/// <summary>
		/// Critical hit chance percentage (0-100).
		/// </summary>
		public int CritChance;

		/// <summary>
		/// Movement speed multiplier (1.0 = normal speed).
		/// </summary>
		public float MoveSpeedMultiplier;
	}
}
