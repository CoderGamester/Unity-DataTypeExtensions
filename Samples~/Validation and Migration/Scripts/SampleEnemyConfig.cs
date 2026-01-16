using System;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Config used to demonstrate schema migrations with <see cref="GameLoversEditor.GameData.MigrationRunner"/>.
	/// </summary>
	[Serializable]
	public struct SampleEnemyConfig
	{
		public int Id;
		public string Name;
		public int Health;
		public int Damage;

		// Added in v2 via migration
		public string ArmorType;
	}
}
