using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// Simple runtime UI that displays loaded configs.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ConfigDisplayUI : MonoBehaviour
	{
		private Text _text;

		public void EnsureBuilt()
		{
			if (_text != null)
			{
				return;
			}

			var canvas = FindObjectOfType<Canvas>();
			if (canvas == null)
			{
				var canvasGo = new GameObject("Designer Workflow Canvas");
				canvas = canvasGo.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvasGo.AddComponent<CanvasScaler>();
				canvasGo.AddComponent<GraphicRaycaster>();
			}

			var panelGo = new GameObject("ConfigDisplayPanel");
			panelGo.transform.SetParent(canvas.transform, false);

			var rt = panelGo.AddComponent<RectTransform>();
			rt.anchorMin = new Vector2(1, 1);
			rt.anchorMax = new Vector2(1, 1);
			rt.pivot = new Vector2(1, 1);
			rt.anchoredPosition = new Vector2(-12, -12);
			rt.sizeDelta = new Vector2(520, 520);

			var img = panelGo.AddComponent<Image>();
			img.color = new Color(0f, 0f, 0f, 0.55f);

			_text = CreateLabel(panelGo.transform, "ConfigsText", "--");
			_text.rectTransform.anchorMin = new Vector2(0, 0);
			_text.rectTransform.anchorMax = new Vector2(1, 1);
			_text.rectTransform.offsetMin = new Vector2(10, 10);
			_text.rectTransform.offsetMax = new Vector2(-10, -10);
			_text.alignment = TextAnchor.UpperLeft;
		}

		public void Render(LoadedConfigs data)
		{
			EnsureBuilt();

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

		private static Text CreateLabel(Transform parent, string name, string text)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);
			var rt = go.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, 0);

			var label = go.AddComponent<Text>();
			label.text = text;
			label.color = Color.white;
			label.fontSize = 14;
			label.alignment = TextAnchor.UpperLeft;
			label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			return label;
		}
	}
}

