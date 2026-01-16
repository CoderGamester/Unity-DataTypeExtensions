using GameLoversEditor.GameData;
using Newtonsoft.Json.Linq;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Example migration from version 1 to version 2 for <see cref="SampleEnemyConfig"/>.
	/// </summary>
	[ConfigMigration(typeof(SampleEnemyConfig))]
	public sealed class SampleEnemyConfigMigration_v1_v2 : IConfigMigration
	{
		public ulong FromVersion => 1;
		public ulong ToVersion => 2;

		public void Migrate(JObject configJson)
		{
			if (configJson["ArmorType"] == null)
			{
				configJson["ArmorType"] = "None";
			}
		}
	}
}
