using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameLoversEditor.GameData
{
	/// <summary>
	/// Editor window for viewing and managing config migrations.
	/// Access via Window > GameLovers > Config Migrations.
	/// </summary>
	public class MigrationEditorWindow : EditorWindow
	{
		private Vector2 _scrollPosition;
		private Dictionary<Type, bool> _foldouts = new Dictionary<Type, bool>();

		[MenuItem("Window/GameLovers/Config Migrations")]
		public static void ShowWindow()
		{
			var window = GetWindow<MigrationEditorWindow>("Config Migrations");
			window.minSize = new Vector2(400, 300);
			window.Show();
		}

		private void OnEnable()
		{
			MigrationRunner.Initialize(force: true);
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(10);
			
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Config Migrations", EditorStyles.boldLabel);
				
				if (GUILayout.Button("Refresh", GUILayout.Width(80)))
				{
					MigrationRunner.Initialize(force: true);
					Repaint();
				}
			}
			
			EditorGUILayout.Space(5);
			EditorGUILayout.HelpBox(
				"Migrations are discovered automatically from classes that implement IConfigMigration " +
				"and have the [ConfigMigration] attribute.", 
				MessageType.Info);
			
			EditorGUILayout.Space(10);

			var configTypes = MigrationRunner.GetConfigTypesWithMigrations();

			if (configTypes.Count == 0)
			{
				EditorGUILayout.HelpBox(
					"No migrations found. Create migration classes with [ConfigMigration(typeof(YourConfig))] attribute.",
					MessageType.None);
				return;
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			foreach (var configType in configTypes.OrderBy(t => t.Name))
			{
				DrawConfigMigrations(configType);
			}

			EditorGUILayout.EndScrollView();
		}

		private void DrawConfigMigrations(Type configType)
		{
			if (!_foldouts.TryGetValue(configType, out var isExpanded))
			{
				isExpanded = true;
				_foldouts[configType] = isExpanded;
			}

			var migrations = MigrationRunner.GetAvailableMigrations(configType);
			var latestVersion = MigrationRunner.GetLatestVersion(configType);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					_foldouts[configType] = EditorGUILayout.Foldout(isExpanded, configType.Name, true, EditorStyles.foldoutHeader);
					
					GUILayout.FlexibleSpace();
					
					EditorGUILayout.LabelField($"Latest: v{latestVersion}", EditorStyles.miniLabel, GUILayout.Width(80));
					EditorGUILayout.LabelField($"{migrations.Count} migration(s)", EditorStyles.miniLabel, GUILayout.Width(100));
				}

				if (_foldouts[configType])
				{
					EditorGUI.indentLevel++;
					
					foreach (var migration in migrations)
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.LabelField($"v{migration.FromVersion} → v{migration.ToVersion}", GUILayout.Width(100));
							EditorGUILayout.LabelField(migration.MigrationType.Name, EditorStyles.miniLabel);
						}
					}
					
					EditorGUI.indentLevel--;
				}
			}
		}
	}
}
