using System.Text;
using TMPro;
using UnityEngine;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// Simple runtime UI that displays loaded configs.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ConfigDisplayUI : MonoBehaviour
	{
 #pragma warning disable CS0649 // Unity assigns via Inspector
		[SerializeField] private TMP_Text _text;
 #pragma warning restore CS0649

		public void Render(LoadedConfigs data)
		{
			if (_text == null)
			{
				return;
			}

			var sb = new StringBuilder(1024);

			sb.AppendLine("Designer Workflow");
			sb.AppendLine();

			sb.AppendLine("Assets (Resources)");
			sb.AppendLine($"- GameSettingsAsset: {(data.SettingsAsset != null ? "Loaded" : "Missing")}");
			sb.AppendLine($"- EnemyConfigsAsset: {(data.EnemiesAsset != null ? "Loaded" : "Missing")}");
			sb.AppendLine($"- LootTableAsset: {(data.LootTableAsset != null ? "Loaded" : "Missing")}");
			sb.AppendLine();

			sb.AppendLine("GameSettings (singleton)");
			sb.AppendLine($"- Difficulty: {data.Provider.GetConfig<GameSettingsConfig>().Difficulty}");
			sb.AppendLine($"- MasterVolume: {data.Provider.GetConfig<GameSettingsConfig>().MasterVolume:0.00}");
			sb.AppendLine();

			sb.AppendLine("Enemies (id-keyed)");
			for (var i = 0; i < data.Enemies.Count; i++)
			{
				var e = data.Enemies[i];
				sb.AppendLine($"- [{e.Id}] {e.Name} HP:{e.Health} DMG:{e.Damage}");
			}
			sb.AppendLine();

			sb.AppendLine("LootTable (UnitySerializedDictionary)");
			if (data.LootTable.Count == 0)
			{
				sb.AppendLine("- (empty)");
			}
			else
			{
				foreach (var pair in data.LootTable)
				{
					var key = pair.Key != null ? pair.Key.GetSelectionString() : "<null>";
					sb.AppendLine($"- {key}: {pair.Value:0.00}");
				}
			}

			_text.text = sb.ToString();
		}
	}
}

