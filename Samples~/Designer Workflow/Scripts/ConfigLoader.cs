using System.Collections.Generic;
using UnityEngine;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// Loads designer-authored ScriptableObject assets into a runtime <see cref="ConfigsProvider"/>.
	/// This sample uses Resources to avoid requiring scene references.
	/// </summary>
	public sealed class ConfigLoader
	{
		public const string GameSettingsResourcePath = "SampleGameSettings";
		public const string EnemyConfigsResourcePath = "SampleEnemyConfigs";
		public const string LootTableResourcePath = "SampleLootTable";

		public LoadedConfigs Load()
		{
			var settingsAsset = Resources.Load<GameSettingsAsset>(GameSettingsResourcePath);
			var enemiesAsset = Resources.Load<EnemyConfigsAsset>(EnemyConfigsResourcePath);
			var lootAsset = Resources.Load<LootTableAsset>(LootTableResourcePath);

			var provider = new ConfigsProvider();

			// Settings: stored as a single entry (key 0)
			var settings = default(GameSettingsConfig);
			if (settingsAsset != null && settingsAsset.ConfigsDictionary != null &&
			    settingsAsset.ConfigsDictionary.TryGetValue(GameSettingsAsset.SingletonKey, out var loadedSettings))
			{
				settings = loadedSettings;
			}
			provider.AddSingletonConfig(settings);

			// Enemies: pair list -> list -> id-keyed provider
			var enemies = new List<EnemyConfig>();
			if (enemiesAsset != null && enemiesAsset.Configs != null)
			{
				for (var i = 0; i < enemiesAsset.Configs.Count; i++)
				{
					enemies.Add(enemiesAsset.Configs[i].Value);
				}
			}
			provider.AddConfigs(e => e.Id, enemies);

			// Loot table: stored in a UnitySerializedDictionary-derived concrete type
			var lootTable = lootAsset != null ? lootAsset.DropRates : new LootTable();

			return new LoadedConfigs(provider, settingsAsset, enemiesAsset, lootAsset, enemies, lootTable);
		}
	}

	public readonly struct LoadedConfigs
	{
		public ConfigsProvider Provider { get; }
		public GameSettingsAsset SettingsAsset { get; }
		public EnemyConfigsAsset EnemiesAsset { get; }
		public LootTableAsset LootTableAsset { get; }
		public IReadOnlyList<EnemyConfig> Enemies { get; }
		public LootTable LootTable { get; }

		public LoadedConfigs(
			ConfigsProvider provider,
			GameSettingsAsset settingsAsset,
			EnemyConfigsAsset enemiesAsset,
			LootTableAsset lootTableAsset,
			IReadOnlyList<EnemyConfig> enemies,
			LootTable lootTable)
		{
			Provider = provider;
			SettingsAsset = settingsAsset;
			EnemiesAsset = enemiesAsset;
			LootTableAsset = lootTableAsset;
			Enemies = enemies;
			LootTable = lootTable;
		}
	}
}

