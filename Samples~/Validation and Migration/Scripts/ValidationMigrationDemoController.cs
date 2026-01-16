using System;
using System.Collections.Generic;
using System.Text;
using GameLoversEditor.GameData;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Editor-only demo (play mode) for validation and migration.
	/// </summary>
	public sealed class ValidationMigrationDemoController : MonoBehaviour
	{
		private readonly List<GameObject> _createdObjects = new List<GameObject>();

		private Text _output;

		private void Awake()
		{
			EnsureEventSystem();
			var canvas = EnsureCanvas();

			var panel = CreatePanel(canvas.transform, "Validation & Migration", new Vector2(12, -12), new Vector2(560, 560));
			CreateButton(panel.transform, "ValidateBtn", "Validate All (EditorConfigValidator)", RunValidation);
			CreateButton(panel.transform, "MigrateBtn", "Run Migration Demo (v1 -> v2)", RunMigration);
			CreateSpacer(panel.transform, 8);
			_output = CreateMultilineLabel(panel.transform, "Output", "(click a button)");
		}

		private void OnDestroy()
		{
			for (var i = 0; i < _createdObjects.Count; i++)
			{
				if (_createdObjects[i] != null)
				{
					Destroy(_createdObjects[i]);
				}
			}
			_createdObjects.Clear();
		}

		private void RunValidation()
		{
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

			var result = EditorConfigValidator.ValidateAll(provider);

			var sb = new StringBuilder(1024);
			sb.AppendLine("Validation Result");
			sb.AppendLine($"IsValid: {result.IsValid}");
			sb.AppendLine();

			if (result.IsValid)
			{
				sb.AppendLine("(no errors)");
			}
			else
			{
				for (var i = 0; i < result.Errors.Count; i++)
				{
					sb.AppendLine($"- {result.Errors[i]}");
				}
			}

			SetOutput(sb.ToString());
		}

		private void RunMigration()
		{
			MigrationRunner.Initialize(force: true);

			var before = new JObject
			{
				["Id"] = 1,
				["Name"] = "Goblin",
				["Health"] = 50,
				["Damage"] = 10
			};

			var after = (JObject)before.DeepClone();

			var configType = typeof(SampleEnemyConfig);
			var applied = MigrationRunner.Migrate(configType, after, currentVersion: 1, targetVersion: 2);

			var sb = new StringBuilder(2048);
			sb.AppendLine("Migration Demo");
			sb.AppendLine();

			sb.AppendLine("Available migrations:");
			var migrations = MigrationRunner.GetAvailableMigrations(configType);
			if (migrations.Count == 0)
			{
				sb.AppendLine("- (none discovered)");
			}
			else
			{
				for (var i = 0; i < migrations.Count; i++)
				{
					sb.AppendLine($"- {migrations[i].MigrationType.Name} ({migrations[i].FromVersion} -> {migrations[i].ToVersion})");
				}
			}
			sb.AppendLine();

			sb.AppendLine($"Applied migrations: {applied}");
			sb.AppendLine();

			sb.AppendLine("Before (v1 JSON):");
			sb.AppendLine(before.ToString());
			sb.AppendLine();

			sb.AppendLine("After (v2 JSON):");
			sb.AppendLine(after.ToString());

			SetOutput(sb.ToString());
		}

		private void SetOutput(string text)
		{
			if (_output != null)
			{
				_output.text = text;
			}
		}

		private void EnsureEventSystem()
		{
			if (FindObjectOfType<EventSystem>() != null)
			{
				return;
			}

			var go = new GameObject("EventSystem");
			_createdObjects.Add(go);
			go.AddComponent<EventSystem>();
			go.AddComponent<StandaloneInputModule>();
		}

		private Canvas EnsureCanvas()
		{
			var existing = FindObjectOfType<Canvas>();
			if (existing != null)
			{
				return existing;
			}

			var go = new GameObject("Validation & Migration Canvas");
			_createdObjects.Add(go);

			var canvas = go.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			go.AddComponent<CanvasScaler>();
			go.AddComponent<GraphicRaycaster>();

			return canvas;
		}

		private static GameObject CreatePanel(Transform parent, string title, Vector2 anchoredPos, Vector2 size)
		{
			var root = new GameObject("Panel");
			root.transform.SetParent(parent, false);

			var rt = root.AddComponent<RectTransform>();
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(0, 1);
			rt.pivot = new Vector2(0, 1);
			rt.anchoredPosition = anchoredPos;
			rt.sizeDelta = size;

			var img = root.AddComponent<Image>();
			img.color = new Color(0f, 0f, 0f, 0.55f);

			var v = root.AddComponent<VerticalLayoutGroup>();
			v.padding = new RectOffset(10, 10, 10, 10);
			v.spacing = 6;
			v.childControlHeight = true;
			v.childControlWidth = true;
			v.childForceExpandHeight = false;
			v.childForceExpandWidth = true;

			var header = CreateLabel(root.transform, "Title", title);
			header.fontStyle = FontStyle.Bold;

			return root;
		}

		private static void CreateSpacer(Transform parent, float height)
		{
			var spacer = new GameObject("Spacer");
			spacer.transform.SetParent(parent, false);
			var rt = spacer.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, height);
		}

		private static Text CreateLabel(Transform parent, string name, string text)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);
			var rt = go.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, 22);

			var label = go.AddComponent<Text>();
			label.text = text;
			label.color = Color.white;
			label.fontSize = 14;
			label.alignment = TextAnchor.MiddleLeft;
			label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			return label;
		}

		private static Text CreateMultilineLabel(Transform parent, string name, string text)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);
			var rt = go.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, 420);

			var label = go.AddComponent<Text>();
			label.text = text;
			label.color = Color.white;
			label.fontSize = 12;
			label.alignment = TextAnchor.UpperLeft;
			label.horizontalOverflow = HorizontalWrapMode.Wrap;
			label.verticalOverflow = VerticalWrapMode.Truncate;
			label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			return label;
		}

		private static Button CreateButton(Transform parent, string name, string label, Action onClick)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);

			var rt = go.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, 28);

			var img = go.AddComponent<Image>();
			img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

			var button = go.AddComponent<Button>();
			button.targetGraphic = img;
			button.onClick.AddListener(() => onClick?.Invoke());

			var textGo = new GameObject("Label");
			textGo.transform.SetParent(go.transform, false);
			var textRt = textGo.AddComponent<RectTransform>();
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;

			var textLabel = textGo.AddComponent<Text>();
			textLabel.text = label;
			textLabel.color = Color.white;
			textLabel.fontSize = 14;
			textLabel.alignment = TextAnchor.MiddleCenter;
			textLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

			return button;
		}
	}
}
