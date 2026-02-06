using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameLovers.GameData;
using GameLoversEditor.GameData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Controller that drives the migration panel UI.
	/// Discovers available migrations from <see cref="MigrationRunner"/>, populates the
	/// <see cref="MigrationPanelView"/> with rows, and handles preview / apply actions.
	/// </summary>
	internal sealed class MigrationPanelController
	{
		private readonly MigrationPanelView _view;
		private readonly List<MigrationRow> _rows = new List<MigrationRow>();
		private IConfigsProvider _provider;

		public MigrationPanelController(MigrationPanelView view)
		{
			_view = view;
			_view.PreviewRequested += OnPreviewRequested;
			_view.ApplyRequested += OnApplyRequested;
		}

		/// <summary>
		/// Assigns the <paramref name="provider"/> to inspect for migrations and rebuilds the panel.
		/// </summary>
		public void SetProvider(IConfigsProvider provider)
		{
			_provider = provider;
			Rebuild();
		}

		/// <summary>
		/// Rebuilds the migration row list and refreshes the view.
		/// </summary>
		public void Rebuild()
		{
			_rows.Clear();
			_view.SetInputJson(string.Empty);
			_view.SetOutputJson(string.Empty);
			_view.SetLog(string.Empty);

			if (_provider == null)
			{
				_view.SetHeader("Migrations (no provider)");
				_view.SetEmptyStateVisible(false);
				_view.SetRows(_rows);
				var hasChoices = _rows.Count > 0;
				_view.SetButtonsEnabled(hasChoices, hasChoices && _provider != null);
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
				_view.SetHeader("Migrations");
				_view.SetEmptyStateVisible(true);
				_view.SetRows(_rows);
				_view.SetButtonsEnabled(false, false);
				return;
			}

			_view.SetEmptyStateVisible(false);

			var currentVersion = _provider.Version;
			var latestVersion = migratableTypes.Max(t => MigrationRunner.GetLatestVersion(t));
			_view.SetHeader($"Current Config Version: {currentVersion}    Latest Available: {latestVersion}");

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

			_view.SetRows(_rows);
			var hasRows = _rows.Count > 0;
			_view.SetButtonsEnabled(hasRows, hasRows && _provider != null);
		}

		private void OnPreviewRequested()
		{
			var selectedIndex = _view.SelectedIndex;
			if (selectedIndex < 0 || selectedIndex >= _rows.Count)
			{
				return;
			}

			RunPreview(_rows[selectedIndex]);
		}

		private void OnApplyRequested()
		{
			var selectedIndex = _view.SelectedIndex;
			if (selectedIndex < 0 || selectedIndex >= _rows.Count || _provider == null)
			{
				return;
			}

			var row = _rows[selectedIndex];
			ApplyMigration(row);
		}

		private void ApplyMigration(MigrationRow row)
		{
			// UpdateTo is only available on ConfigsProvider, not the interface
			if (!(_provider is ConfigsProvider concreteProvider))
			{
				_view.SetLog("Apply Failed: Provider does not support UpdateTo (must be ConfigsProvider).");
				return;
			}

			var currentVersion = _provider.Version;

			// Get all configs of this type from the provider
			var allConfigs = _provider.GetAllConfigs();
			if (!allConfigs.TryGetValue(row.ConfigType, out var container))
			{
				_view.SetLog($"Apply Failed: No configs of type {row.ConfigType.Name} found in provider.");
				return;
			}

			// Read configs into a list of (id, value) pairs
			if (!ConfigsEditorUtil.TryReadConfigs(container, out var entries) || entries.Count == 0)
			{
				_view.SetLog($"Apply Failed: Could not read configs of type {row.ConfigType.Name}.");
				return;
			}

			try
			{
				// Migrate each config and collect results
				var migratedEntries = new List<(int Id, object Value)>();
				int totalApplied = 0;

				foreach (var entry in entries)
				{
					var inputJson = JObject.FromObject(entry.Value);
					var outputJson = (JObject)inputJson.DeepClone();

					var applied = MigrationRunner.Migrate(row.ConfigType, outputJson, currentVersion, row.ToVersion);
					totalApplied += applied;

					// Deserialize back to the config type
					var migratedValue = outputJson.ToObject(row.ConfigType);
					migratedEntries.Add((entry.Id, migratedValue));
				}

				// Create a new dictionary with migrated values using reflection
				var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(int), row.ConfigType);
				var newDictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

				foreach (var (id, value) in migratedEntries)
				{
					newDictionary.Add(id, value);
				}

				// Update the provider
				var updateDict = new Dictionary<Type, IEnumerable> { { row.ConfigType, newDictionary } };
				concreteProvider.UpdateTo(row.ToVersion, updateDict);

				_view.SetLog($"Apply Success: Migrated {entries.Count} config(s), {totalApplied} migration step(s) applied. Provider now at v{row.ToVersion}.");

				// Rebuild to reflect new state
				Rebuild();
			}
			catch (Exception ex)
			{
				_view.SetLog($"Apply Failed: {ex.Message}");
			}
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
			var customJson = _view.CustomJson;
			if (!string.IsNullOrEmpty(customJson))
			{
				try
				{
					inputJson = JObject.Parse(customJson);
					instanceLabel = $"{row.ConfigType.Name} (custom input)";
				}
				catch (Exception ex)
				{
					_view.SetInputJson($"// Invalid JSON: {ex.Message}");
					_view.SetOutputJson(string.Empty);
					_view.SetLog(string.Empty);
					return;
				}
			}
			// Priority 2: Provider data (current schema)
			else if (TryGetFirstInstance(row.ConfigType, out var id, out var instance))
			{
				inputJson = JObject.FromObject(instance);
				var idStr = id == 0 ? "singleton" : id.ToString();
				instanceLabel = $"{row.ConfigType.Name} ({idStr})";
			}
			else
			{
				_view.SetInputJson("// No instance found for this config type in the provider.");
				_view.SetOutputJson(string.Empty);
				_view.SetLog(string.Empty);
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
				_view.SetInputJson(inputJson.ToString(Formatting.Indented));
				_view.SetOutputJson($"// Migration failed: {ex.Message}");
				_view.SetLog($"Migration Log: {row.MigrationType.Name} - FAILED");
				return;
			}

			_view.SetInputJson(inputJson.ToString(Formatting.Indented));
			_view.SetOutputJson(outputJson.ToString(Formatting.Indented));
			_view.SetLog($"Migration Log: {row.MigrationType.Name} - SUCCESS (Applied: {applied})  Preview Instance: {instanceLabel}");
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

			if (!ConfigsEditorUtil.TryReadConfigs(container, out var entries) || entries.Count == 0)
			{
				return false;
			}

			id = entries[0].Id;
			instance = entries[0].Value;
			return instance != null;
		}

		private static MigrationState GetState(ulong currentVersion, ulong from, ulong to)
		{
			if (currentVersion >= to) return MigrationState.Applied;
			if (currentVersion == from) return MigrationState.Current;
			return MigrationState.Pending;
		}
	}
}
