using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameLoversEditor.GameData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// UI Toolkit panel that lists migrations discovered by the editor migration system and provides
	/// an in-memory migration preview ("Migration Preview") for a selected row.
	/// </summary>
	public sealed class MigrationPanelElement : VisualElement
	{
		private static Dictionary<Type, Func<JObject>> _previewDataProviders;

		private Label _header;
		private HelpBox _emptyState;
		private VisualElement _contentContainer;
		private ListView _listView;
		private readonly List<MigrationRow> _rows = new List<MigrationRow>();

		private TextField _customJsonInput;
		private JsonViewerElement _inputJson;
		private JsonViewerElement _outputJson;
		private Label _logLabel;

		private IConfigsProvider _provider;

		public MigrationPanelElement()
		{
			style.flexGrow = 1;
			style.paddingLeft = 8;
			style.paddingRight = 8;
			style.paddingTop = 6;
			style.paddingBottom = 6;

			_header = new Label("Migrations");
			_header.style.unityFontStyleAndWeight = FontStyle.Bold;
			_header.style.marginBottom = 6;

			_emptyState = new HelpBox("No migrations available in this project.", HelpBoxMessageType.Info);
			_emptyState.style.display = DisplayStyle.None;

			// Vertical split: migrations list (top) + preview section (bottom)
			var verticalSplit = new TwoPaneSplitView(1, 220, TwoPaneSplitViewOrientation.Vertical);
			verticalSplit.viewDataKey = "ConfigBrowser_MigrationVerticalSplit";
			verticalSplit.style.flexGrow = 1;

			verticalSplit.Add(BuildMigrationsList());
			verticalSplit.Add(BuildPreview());

			_contentContainer = new VisualElement { style = { flexGrow = 1 } };
			_contentContainer.Add(verticalSplit);

			Add(_header);
			Add(_emptyState);
			Add(_contentContainer);
		}

		/// <summary>
		/// Sets the config provider to inspect for migrations.
		/// </summary>
		public void SetProvider(IConfigsProvider provider)
		{
			_provider = provider;
			Rebuild();
		}

		/// <summary>
		/// Rebuilds the migration list and preview panels based on the current provider.
		/// </summary>
		public void Rebuild()
		{
			_rows.Clear();
			_inputJson.SetJson(string.Empty);
			_outputJson.SetJson(string.Empty);
			_logLabel.text = string.Empty;

			if (_provider == null)
			{
				_header.text = "Migrations (no provider)";
				_emptyState.style.display = DisplayStyle.None;
				_contentContainer.style.display = DisplayStyle.Flex;
				_listView.itemsSource = _rows;
				_listView.RefreshItems();
				return;
			}

			var providerTypes = _provider.GetAllConfigs().Keys.ToHashSet();
			var migratableTypes = MigrationRunner.GetConfigTypesWithMigrations()
				.Where(providerTypes.Contains)
				.OrderBy(t => t.Name)
				.ToList();

			// Show empty state when no migrations are available.
			if (migratableTypes.Count == 0)
			{
				_header.text = "Migrations";
				_emptyState.style.display = DisplayStyle.Flex;
				_contentContainer.style.display = DisplayStyle.None;
				_listView.itemsSource = _rows;
				_listView.RefreshItems();
				return;
			}

			_emptyState.style.display = DisplayStyle.None;
			_contentContainer.style.display = DisplayStyle.Flex;

			var currentVersion = _provider.Version;
			var latestVersion = migratableTypes.Max(t => MigrationRunner.GetLatestVersion(t));
			_header.text = $"Current Config Version: {currentVersion}    Latest Available: {latestVersion}";

			foreach (var type in migratableTypes)
			{
				var migrations = MigrationRunner.GetAvailableMigrations(type);
				for (int i = 0; i < migrations.Count; i++)
				{
					var m = migrations[i];
					var state = GetState(currentVersion, m.FromVersion, m.ToVersion);
					_rows.Add(new MigrationRow(type, m.FromVersion, m.ToVersion, m.MigrationType, state));
				}
			}

			_listView.itemsSource = _rows;
			_listView.RefreshItems();
		}

		private VisualElement BuildMigrationsList()
		{
			var container = new VisualElement { style = { flexGrow = 1, minHeight = 100 } };

			_listView = new ListView
			{
				selectionType = SelectionType.Single,
				showBorder = true,
				showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
				style = { flexGrow = 1 }
			};

			_listView.makeItem = MakeRow;
			_listView.bindItem = (e, i) => BindRow(e, i);

			container.Add(_listView);
			return container;
		}

		private VisualElement BuildPreview()
		{
			var container = new VisualElement
			{
				style =
				{
					flexGrow = 1,
					minHeight = 140,
					borderTopWidth = 1,
					borderTopColor = new StyleColor(new Color(0f, 0f, 0f, 0.2f)),
					paddingTop = 4
				}
			};

			var title = new Label("Migration Preview");
			title.style.unityFontStyleAndWeight = FontStyle.Bold;
			title.style.marginBottom = 4;

			// Custom JSON input section
			var customInputSection = new VisualElement { style = { marginBottom = 6 } };
			var customInputLabel = new Label("Custom Input JSON (optional - paste legacy-schema JSON here):");
			customInputLabel.style.marginBottom = 2;
			_customJsonInput = new TextField
			{
				multiline = true,
				style =
				{
					minHeight = 60,
					maxHeight = 100,
					whiteSpace = WhiteSpace.Normal
				}
			};
			_customJsonInput.AddToClassList("unity-text-field__input");
			customInputSection.Add(customInputLabel);
			customInputSection.Add(_customJsonInput);

			var split = new TwoPaneSplitView(0, 360, TwoPaneSplitViewOrientation.Horizontal);
			split.style.flexGrow = 1;
			split.style.minHeight = 140;

			var left = new VisualElement { style = { flexGrow = 1 } };
			left.Add(new Label("Input"));
			_inputJson = new JsonViewerElement();
			left.Add(_inputJson);

			var right = new VisualElement { style = { flexGrow = 1 } };
			right.Add(new Label("Output"));
			_outputJson = new JsonViewerElement();
			right.Add(_outputJson);

			split.Add(left);
			split.Add(right);

			_logLabel = new Label();
			_logLabel.style.marginTop = 4;
			_logLabel.style.whiteSpace = WhiteSpace.Normal;

			container.Add(title);
			container.Add(customInputSection);
			container.Add(split);
			container.Add(_logLabel);
			return container;
		}

		private static VisualElement MakeRow()
		{
			var row = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					paddingLeft = 6,
					paddingRight = 6,
					paddingTop = 2,
					paddingBottom = 2
				}
			};

			row.Add(new Label { name = "Type", style = { minWidth = 160 } });
			row.Add(new Label { name = "Migration", style = { minWidth = 100 } });
			row.Add(new Label { name = "State", style = { minWidth = 90 } });

			var button = new Button { name = "Action", text = "Migrate" };
			button.style.marginLeft = StyleKeyword.Auto;
			row.Add(button);

			return row;
		}

		private void BindRow(VisualElement row, int index)
		{
			if (index < 0 || index >= _rows.Count) return;
			var data = _rows[index];

			row.Q<Label>("Type").text = data.ConfigType.Name;
			row.Q<Label>("Migration").text = $"v{data.FromVersion} -> v{data.ToVersion}";
			row.Q<Label>("State").text = data.State.ToString();

			var button = row.Q<Button>("Action");
			button.SetEnabled(_provider != null && data.State != MigrationState.Applied);
			if (button.userData is Action previous)
			{
				button.clicked -= previous;
			}

			Action current = () => RunPreview(data);
			button.userData = current;
			button.clicked += current;
		}

		private void RunPreview(MigrationRow row)
		{
			if (_provider == null)
			{
				return;
			}

			var currentVersion = _provider.Version;
			JObject inputJson;
			string instanceLabel;

			// Priority 1: Custom JSON input from the text field
			var customJson = _customJsonInput?.value?.Trim();
			if (!string.IsNullOrEmpty(customJson))
			{
				try
				{
					inputJson = JObject.Parse(customJson);
					instanceLabel = $"{row.ConfigType.Name} (custom input)";
				}
				catch (Exception ex)
				{
					_inputJson.SetJson($"// Invalid JSON: {ex.Message}");
					_outputJson.SetJson(string.Empty);
					_logLabel.text = string.Empty;
					return;
				}
			}
			// Priority 2: Preview data discovered via [MigrationPreviewData] attribute
			else if (TryGetPreviewData(row.ConfigType, out var factory))
			{
				try
				{
					inputJson = factory();
					instanceLabel = $"{row.ConfigType.Name} (preview data)";
				}
				catch (Exception ex)
				{
					_inputJson.SetJson($"// Failed to create preview data: {ex.Message}");
					_outputJson.SetJson(string.Empty);
					_logLabel.text = string.Empty;
					return;
				}
			}
			// Priority 3: Provider data (current schema)
			else if (TryGetFirstInstance(row.ConfigType, out var id, out var instance))
			{
				inputJson = JObject.FromObject(instance);
				var idStr = id == 0 ? "singleton" : id.ToString();
				instanceLabel = $"{row.ConfigType.Name} ({idStr})";
			}
			else
			{
				_inputJson.SetJson("// No instance found for this config type in the provider.");
				_outputJson.SetJson(string.Empty);
				_logLabel.text = string.Empty;
				return;
			}

			var outputJson = (JObject)inputJson.DeepClone();

			int applied;
			try
			{
				applied = MigrationRunner.Migrate(row.ConfigType, outputJson, currentVersion, row.ToVersion);
			}
			catch (Exception ex)
			{
				_inputJson.SetJson(inputJson.ToString(Formatting.Indented));
				_outputJson.SetJson($"// Migration failed: {ex.Message}");
				_logLabel.text = $"Migration Log: {row.MigrationType.Name} - FAILED";
				return;
			}

			_inputJson.SetJson(inputJson.ToString(Formatting.Indented));
			_outputJson.SetJson(outputJson.ToString(Formatting.Indented));
			_logLabel.text = $"Migration Log: {row.MigrationType.Name} - SUCCESS (Applied: {applied})  Preview Instance: {instanceLabel}";
		}

		/// <summary>
		/// Attempts to get preview data for the specified config type using [MigrationPreviewData] attributes.
		/// </summary>
		private static bool TryGetPreviewData(Type configType, out Func<JObject> factory)
		{
			factory = null;
			EnsurePreviewDataProvidersInitialized();

			return _previewDataProviders.TryGetValue(configType, out factory);
		}

		/// <summary>
		/// Discovers all methods marked with [MigrationPreviewData] attribute via TypeCache.
		/// </summary>
		private static void EnsurePreviewDataProvidersInitialized()
		{
			if (_previewDataProviders != null)
			{
				return;
			}

			_previewDataProviders = new Dictionary<Type, Func<JObject>>();

			var methods = TypeCache.GetMethodsWithAttribute<MigrationPreviewDataAttribute>();
			foreach (var method in methods)
			{
				if (!method.IsStatic)
				{
					Debug.LogWarning($"[MigrationPreviewData] Method {method.DeclaringType?.Name}.{method.Name} must be static. Skipping.");
					continue;
				}

				if (method.GetParameters().Length != 0)
				{
					Debug.LogWarning($"[MigrationPreviewData] Method {method.DeclaringType?.Name}.{method.Name} must have no parameters. Skipping.");
					continue;
				}

				if (method.ReturnType != typeof(JObject))
				{
					Debug.LogWarning($"[MigrationPreviewData] Method {method.DeclaringType?.Name}.{method.Name} must return JObject. Skipping.");
					continue;
				}

				var attr = method.GetCustomAttribute<MigrationPreviewDataAttribute>();
				if (attr?.ConfigType == null)
				{
					continue;
				}

				var capturedMethod = method;
				_previewDataProviders[attr.ConfigType] = () => (JObject)capturedMethod.Invoke(null, null);
			}
		}

		private bool TryGetFirstInstance(Type configType, out int id, out object instance)
		{
			id = 0;
			instance = null;

			var all = _provider.GetAllConfigs();
			if (!all.TryGetValue(configType, out var container))
			{
				return false;
			}

			if (!TryReadConfigs(container, out var entries) || entries.Count == 0)
			{
				return false;
			}

			id = entries[0].Id;
			instance = entries[0].Value;
			return instance != null;
		}

		private static bool TryReadConfigs(IEnumerable container, out List<ConfigEntry> entries)
		{
			entries = new List<ConfigEntry>();
			if (container == null) return false;

			var type = container.GetType();
			if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
			{
				return false;
			}

			var keyType = type.GetGenericArguments()[0];
			if (keyType != typeof(int))
			{
				return false;
			}

			foreach (var item in container)
			{
				var itemType = item.GetType();
				var keyProp = itemType.GetProperty("Key");
				var valueProp = itemType.GetProperty("Value");
				if (keyProp == null || valueProp == null) continue;

				var entryId = (int)keyProp.GetValue(item);
				var value = valueProp.GetValue(item);
				entries.Add(new ConfigEntry(entryId, value));
			}

			entries.Sort((a, b) => a.Id.CompareTo(b.Id));
			return true;
		}

		private static MigrationState GetState(ulong currentVersion, ulong from, ulong to)
		{
			if (currentVersion >= to) return MigrationState.Applied;
			if (currentVersion == from) return MigrationState.Current;
			return MigrationState.Pending;
		}

		/// <summary>
		/// Represents a single config entry (ID and value) read from the provider.
		/// </summary>
		private readonly struct ConfigEntry
		{
			/// <summary>The config ID (0 for singletons).</summary>
			public readonly int Id;

			/// <summary>The config value instance.</summary>
			public readonly object Value;

			/// <summary>Creates a new config entry.</summary>
			public ConfigEntry(int id, object value)
			{
				Id = id;
				Value = value;
			}
		}

		private readonly struct MigrationRow
		{
			public readonly Type ConfigType;
			public readonly ulong FromVersion;
			public readonly ulong ToVersion;
			public readonly Type MigrationType;
			public readonly MigrationState State;

			public MigrationRow(Type configType, ulong fromVersion, ulong toVersion, Type migrationType, MigrationState state)
			{
				ConfigType = configType;
				FromVersion = fromVersion;
				ToVersion = toVersion;
				MigrationType = migrationType;
				State = state;
			}
		}

		/// <summary>
		/// Represents the application state of a migration relative to the current provider version.
		/// </summary>
		private enum MigrationState
		{
			Applied, // The migration has already been applied (current version is greater than or equal to ToVersion)
			Current, // The migration is the next one to apply (current version equals FromVersion)
			Pending // The migration is pending (current version is less than FromVersion)
		}
	}
}

