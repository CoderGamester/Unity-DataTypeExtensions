using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using GameLoversEditor.GameData;
#endif
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Editor-only demo (play mode) for validation and migration.
	/// </summary>
	public sealed class ValidationMigrationDemoController : MonoBehaviour
	{
		private void Awake()
		{
			if (_validateButton != null)
			{
				_validateButton.onClick.AddListener(RunValidation);
			}

			if (_migrateButton != null)
			{
				_migrateButton.onClick.AddListener(RunMigration);
			}

			SetOutput("(click a button)");
		}

		private void RunValidation()
		{
#if !UNITY_EDITOR
			SetOutput("This sample is Editor-only.");
			return;
#else
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
#endif
		}

		private void RunMigration()
		{
#if !UNITY_EDITOR
			SetOutput("This sample is Editor-only.");
			return;
#else
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
#endif
		}

		private void SetOutput(string text)
		{
			if (_output != null)
			{
				_output.text = text;
			}
		}

 #pragma warning disable CS0649 // Unity assigns via Inspector
		[SerializeField] private Button _validateButton;
		[SerializeField] private Button _migrateButton;
		[SerializeField] private TMP_Text _output;
 #pragma warning restore CS0649
	}
}
