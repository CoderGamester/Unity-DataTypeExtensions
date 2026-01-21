using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Editor window that provides a unified interface for browsing, validating, and migrating config data.
	/// Access via <c>Window/GameLovers/Config Browser</c>.
	/// </summary>
	/// <remarks>
	/// <para>The Browse tab displays a tree view of all configs in the assigned provider with JSON preview and validation.</para>
	/// <para>The Migrations tab (visible only when migrations exist) shows migration status and provides in-memory preview.</para>
	/// </remarks>
	public sealed class ConfigBrowserWindow : EditorWindow
	{
		private const int SingleConfigId = 0;

		private IConfigsProvider _provider;
		private int _selectedProviderId = -1;
		private List<ConfigsProviderDebugRegistry.ProviderSnapshot> _snapshots = new();

		private Toolbar _toolbar;
		private ToolbarMenu _providerMenu;
		private ToolbarSearchField _searchField;
		private Button _validateAllButton;
		private Button _exportJsonButton;
		private Button _refreshButton;

		private ToolbarToggle _browseTab;
		private ToolbarToggle _migrationsTab;
		private VisualElement _browseRoot;
		private VisualElement _migrationsRoot;
		private MigrationPanelElement _migrationPanel;

		private TreeView _treeView;
		private JsonViewerElement _jsonViewer;
		private Label _detailsHeader;
		private Button _validateSelectedButton;

		private VisualElement _validationPanel;
		private Label _validationHeaderLabel;
		private Button _clearValidationFilterButton;
		private ScrollView _validationList;

		private ValidationFilter _validationFilter = ValidationFilter.All();
		private ConfigSelection _selection;

		/// <summary>
		/// Opens the Config Browser window.
		/// </summary>
		[MenuItem("Tools/Game Data/Config Browser")]
		public static void ShowWindow()
		{
			var window = GetWindow<ConfigBrowserWindow>("Config Browser");
			window.minSize = new Vector2(720, 420);
			window.Show();
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			// Clear selection when entering or exiting play mode to avoid stale references
			if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
			{
				_provider = null;
				_selectedProviderId = -1;
				_snapshots.Clear();
			}
		}

		/// <summary>
		/// Creates the UI Toolkit GUI when the window is opened or reloaded.
		/// </summary>
		public void CreateGUI()
		{
			rootVisualElement.Clear();
			rootVisualElement.style.flexGrow = 1;

			rootVisualElement.Add(BuildToolbar());
			rootVisualElement.Add(BuildTabs());

			_browseRoot = BuildBrowseRoot();
			_migrationsRoot = BuildMigrationsRoot();

			var content = new VisualElement { style = { flexGrow = 1 } };
			content.Add(_browseRoot);
			content.Add(_migrationsRoot);
			rootVisualElement.Add(content);

			rootVisualElement.schedule.Execute(RefreshProviderList).Every(250);
			RefreshProviderList();
			RefreshAll();
		}

		private VisualElement BuildToolbar()
		{
			_toolbar = new Toolbar();

			_providerMenu = new ToolbarMenu { text = "No providers" };
			_providerMenu.style.minWidth = 280;

			_searchField = new ToolbarSearchField();
			_searchField.style.flexGrow = 1;
			_searchField.RegisterValueChangedCallback(_ => RefreshTree());

			_validateAllButton = new Button(() =>
			{
				_validationFilter = ValidationFilter.All();
				PopulateValidationResults(ValidateAllConfigs());
			})
			{
				text = "Validate All"
			};

			_exportJsonButton = new Button(ExportJson)
			{
				text = "Export JSON"
			};

			_refreshButton = new Button(RefreshAll)
			{
				text = "Refresh"
			};

			_toolbar.Add(_providerMenu);
			_toolbar.Add(_searchField);
			_toolbar.Add(_validateAllButton);
			_toolbar.Add(_exportJsonButton);
			_toolbar.Add(_refreshButton);

			return _toolbar;
		}

		private void RefreshProviderList()
		{
			_snapshots.Clear();
			_snapshots.AddRange(ConfigsProviderDebugRegistry.EnumerateSnapshots().OrderBy(s => s.Id));

			var currentProviderGone = _provider != null && !_snapshots.Any(s => s.Id == _selectedProviderId);
			if (currentProviderGone || (_provider == null && _snapshots.Count > 0))
			{
				var first = _snapshots.FirstOrDefault();
				if (first.ProviderRef != null && first.ProviderRef.TryGetTarget(out var target))
				{
					_provider = target;
					_selectedProviderId = first.Id;
				}
				else
				{
					_provider = null;
					_selectedProviderId = -1;
				}
				RefreshAll();
			}

			UpdateProviderMenu();
		}

		private string _lastMenuText;
		private int _lastSnapshotCount = -1;

		private void UpdateProviderMenu()
		{
			if (_providerMenu == null || _toolbar == null) return;

			var currentSnapshot = _snapshots.FirstOrDefault(s => s.Id == _selectedProviderId);
			var menuText = _snapshots.Count == 0 ? "No providers" : 
				currentSnapshot.Id != 0 ? $"{currentSnapshot.Name} ({currentSnapshot.ConfigTypeCount} types)" : "Select Provider";

			if (_lastMenuText == menuText && _lastSnapshotCount == _snapshots.Count)
			{
				return;
			}

			_lastMenuText = menuText;
			_lastSnapshotCount = _snapshots.Count;

			var newMenu = new ToolbarMenu { text = menuText };
			newMenu.style.minWidth = 280;

			foreach (var snap in _snapshots)
			{
				var s = snap;
				newMenu.menu.AppendAction($"{s.Name} ({s.ConfigTypeCount} types) ##{s.Id}", a =>
				{
					if (s.ProviderRef.TryGetTarget(out var target))
					{
						_provider = target;
						_selectedProviderId = s.Id;
						RefreshAll();
					}
				}, a => s.Id == _selectedProviderId ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
			}

			_toolbar.Insert(_toolbar.IndexOf(_providerMenu), newMenu);
			_toolbar.Remove(_providerMenu);
			_providerMenu = newMenu;
		}

		private VisualElement BuildTabs()
		{
			var tabs = new Toolbar();

			_browseTab = new ToolbarToggle { text = "Browse", value = true };
			_migrationsTab = new ToolbarToggle { text = "Migrations", value = false };

			_browseTab.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue)
				{
					_migrationsTab.SetValueWithoutNotify(false);
					SetActiveTab(isBrowse: true);
				}
			});

			_migrationsTab.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue)
				{
					_browseTab.SetValueWithoutNotify(false);
					SetActiveTab(isBrowse: false);
				}
			});

			tabs.Add(_browseTab);
			tabs.Add(_migrationsTab);
			return tabs;
		}

		private VisualElement BuildBrowseRoot()
		{
			var root = new VisualElement { style = { flexGrow = 1 } };

			// Horizontal split: tree view (left) + details panel (right)
			var horizontalSplit = new TwoPaneSplitView(0, 260, TwoPaneSplitViewOrientation.Horizontal);
			horizontalSplit.viewDataKey = "ConfigBrowser_HorizontalSplit";
			horizontalSplit.style.flexGrow = 1;

			_treeView = new TreeView
			{
				selectionType = SelectionType.Single,
				showBorder = true,
				style = { flexGrow = 1 },
				makeItem = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 4 } },
				bindItem = (element, index) =>
				{
					var node = _treeView.GetItemDataForIndex<ConfigNode>(index);
					var label = (Label)element;
					label.text = node.DisplayName;

					// Style based on node kind
					label.style.unityFontStyleAndWeight = node.Kind == ConfigNodeKind.Header
						? FontStyle.Bold
						: FontStyle.Normal;
				}
			};
			_treeView.selectionChanged += OnTreeSelectionChanged;

			horizontalSplit.Add(_treeView);
			horizontalSplit.Add(BuildDetailsPanel());

			// Vertical split: content (top) + validation panel (bottom)
			var verticalSplit = new TwoPaneSplitView(1, 180, TwoPaneSplitViewOrientation.Vertical);
			verticalSplit.viewDataKey = "ConfigBrowser_BrowseVerticalSplit";
			verticalSplit.style.flexGrow = 1;

			verticalSplit.Add(horizontalSplit);
			verticalSplit.Add(BuildValidationPanel());

			root.Add(verticalSplit);
			return root;
		}

		private VisualElement BuildMigrationsRoot()
		{
			var root = new VisualElement { style = { flexGrow = 1 } };
			_migrationPanel = new MigrationPanelElement();
			root.Add(_migrationPanel);
			return root;
		}

		private void SetActiveTab(bool isBrowse)
		{
			_browseRoot.style.display = isBrowse ? DisplayStyle.Flex : DisplayStyle.None;
			_migrationsRoot.style.display = isBrowse ? DisplayStyle.None : DisplayStyle.Flex;

			// Keep migration panel updated when switching.
			if (!isBrowse)
			{
				_migrationPanel.SetProvider(_provider);
			}
		}

		private VisualElement BuildDetailsPanel()
		{
			var panel = new VisualElement
			{
				style =
				{
					flexGrow = 1,
					paddingLeft = 8,
					paddingRight = 8,
					paddingTop = 6,
					paddingBottom = 6
				}
			};

			var header = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.SpaceBetween,
					marginBottom = 6
				}
			};

			_detailsHeader = new Label("No selection");
			_detailsHeader.style.unityFontStyleAndWeight = FontStyle.Bold;

			_validateSelectedButton = new Button(() =>
			{
				if (!_selection.IsValid || _provider == null) return;
				_validationFilter = ValidationFilter.Single(_selection.ConfigType, _selection.ConfigId);
				PopulateValidationResults(ValidateSingleConfig(_selection));
			})
			{
				text = "Validate"
			};
			_validateSelectedButton.SetEnabled(false);

			header.Add(_detailsHeader);
			header.Add(_validateSelectedButton);

			_jsonViewer = new JsonViewerElement();

			panel.Add(header);
			panel.Add(_jsonViewer);

			return panel;
		}

		private VisualElement BuildValidationPanel()
		{
			_validationPanel = new VisualElement
			{
				style =
				{
					flexGrow = 1,
					minHeight = 100,
					borderTopWidth = 1,
					borderTopColor = new StyleColor(new Color(0f, 0f, 0f, 0.2f))
				}
			};

			var header = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.SpaceBetween,
					paddingLeft = 8,
					paddingRight = 8,
					paddingTop = 4,
					paddingBottom = 4
				}
			};

			_validationHeaderLabel = new Label("Validation Results");
			_validationHeaderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

			_clearValidationFilterButton = new Button(() =>
			{
				_validationFilter = ValidationFilter.All();
				PopulateValidationResults(ValidateAllConfigs());
			})
			{
				text = "Clear"
			};

			header.Add(_validationHeaderLabel);
			header.Add(_clearValidationFilterButton);

			_validationList = new ScrollView(ScrollViewMode.Vertical);
			_validationList.style.flexGrow = 1;

			_validationPanel.Add(header);
			_validationPanel.Add(_validationList);

			return _validationPanel;
		}

		private void RefreshAll()
		{
			_selection = ConfigSelection.None();
			_detailsHeader.text = "No selection";
			_validateSelectedButton.SetEnabled(false);
			_jsonViewer.SetJson(string.Empty);

			RefreshTree();
			PopulateValidationResults(new List<ValidationErrorInfo>());
			RefreshMigrationsVisibility();
			SetActiveTab(isBrowse: true);
		}

		private void RefreshMigrationsVisibility()
		{
			// Migrations tab is always visible; empty state is handled by MigrationPanelElement.
			_migrationPanel.SetProvider(_provider);
		}

		private void RefreshTree()
		{
			var items = BuildTreeItems(_provider, _searchField?.value);
			_treeView.SetRootItems(items);
			_treeView.Rebuild();
		}

		private static IList<TreeViewItemData<ConfigNode>> BuildTreeItems(IConfigsProvider provider, string search)
		{
			var id = 1;
			var childrenSingletons = new List<TreeViewItemData<ConfigNode>>();
			var childrenCollections = new List<TreeViewItemData<ConfigNode>>();

			var hasSearch = !string.IsNullOrWhiteSpace(search);
			var searchLower = hasSearch ? search.Trim().ToLowerInvariant() : string.Empty;

			if (provider == null)
			{
				return new List<TreeViewItemData<ConfigNode>>
				{
					new TreeViewItemData<ConfigNode>(id++, ConfigNode.Header("No providers available. Enter Play Mode to create a ConfigsProvider."))
				};
			}

			var allConfigs = provider.GetAllConfigs();
			foreach (var kv in allConfigs.OrderBy(k => k.Key.Name))
			{
				var type = kv.Key;
				var container = kv.Value;

				if (!TryReadConfigs(container, out var entries))
				{
					continue;
				}

				var isSingleton = entries.Count == 1 && entries[0].Id == SingleConfigId;
				var typeMatches = !hasSearch || type.Name.ToLowerInvariant().Contains(searchLower);

				var entryNodes = new List<TreeViewItemData<ConfigNode>>();
				for (int i = 0; i < entries.Count; i++)
				{
					var entry = entries[i];
					var idStr = isSingleton ? "singleton" : entry.Id.ToString();
					var label = $"{idStr}: {type.Name}";

					if (!typeMatches && hasSearch)
					{
						// Allow searching by id.
						if (!idStr.Contains(searchLower))
						{
							continue;
						}
					}

					entryNodes.Add(new TreeViewItemData<ConfigNode>(id++, ConfigNode.Entry(type, entry.Id, entry.Value, label)));
				}

				if (entryNodes.Count == 0)
				{
					continue;
				}

				var typeNode = new TreeViewItemData<ConfigNode>(id++, ConfigNode.Type(type, $"{type.Name} ({entryNodes.Count})"), entryNodes);
				if (isSingleton)
				{
					childrenSingletons.Add(typeNode);
				}
				else
				{
					childrenCollections.Add(typeNode);
				}
			}

			var roots = new List<TreeViewItemData<ConfigNode>>();
			roots.Add(new TreeViewItemData<ConfigNode>(id++, ConfigNode.Header("Singletons"), childrenSingletons));
			roots.Add(new TreeViewItemData<ConfigNode>(id++, ConfigNode.Header("Collections"), childrenCollections));
			return roots;
		}

		private void OnTreeSelectionChanged(IEnumerable<object> selected)
		{
			var first = selected.FirstOrDefault();
			if (first is not ConfigNode node || node.Kind != ConfigNodeKind.Entry)
			{
				_selection = ConfigSelection.None();
				_detailsHeader.text = "No selection";
				_validateSelectedButton.SetEnabled(false);
				_jsonViewer.SetJson(string.Empty);
				return;
			}

			_selection = new ConfigSelection(node.ConfigType, node.ConfigId, node.Value);
			_detailsHeader.text = node.DisplayName;
			_validateSelectedButton.SetEnabled(true);
			_jsonViewer.SetJson(ToJson(node.Value));
		}

		private void PopulateValidationResults(List<ValidationErrorInfo> errors)
		{
			_validationList.Clear();

			var showing = _validationFilter.IsAll ? "All" : $"{_validationFilter.ConfigType?.Name} (ID:{_validationFilter.ConfigId})";
			_validationHeaderLabel.text = $"Validation Results  Showing: {showing}  Errors: {errors.Count}";
			_clearValidationFilterButton.SetEnabled(!_validationFilter.IsAll);

			if (_provider == null)
			{
				_validationList.Add(new HelpBox("No provider selected. Enter Play Mode and create a ConfigsProvider.", HelpBoxMessageType.Info));
				return;
			}

			if (errors.Count == 0)
			{
				_validationList.Add(new HelpBox("No validation errors.", HelpBoxMessageType.Info));
				return;
			}

			for (int i = 0; i < errors.Count; i++)
			{
				var e = errors[i];
				var row = new ValidationErrorElement();
				row.Bind(e.ConfigTypeName, e.ConfigId, e.FieldName, e.Message);
				row.Clicked += OnValidationRowClicked;
				_validationList.Add(row);
			}
		}

		private void OnValidationRowClicked(string configTypeName, int? configId)
		{
			// Best-effort: select matching node in tree by scanning visible items.
			var provider = _provider;
			if (provider == null) return;
			var targetType = provider.GetAllConfigs().Keys.FirstOrDefault(t => t.Name == configTypeName);
			if (targetType == null) return;

			// Rebuild tree item data so we can search it deterministically.
			var roots = BuildTreeItems(provider, _searchField?.value);
			_treeView.SetRootItems(roots);
			_treeView.Rebuild();

			var itemId = FindTreeItemIdForEntry(roots, targetType, configId ?? SingleConfigId);
			if (itemId.HasValue)
			{
				_treeView.SetSelection(new List<int> { itemId.Value });
				_treeView.ScrollToItem(itemId.Value);
			}
		}

		private static int? FindTreeItemIdForEntry(IList<TreeViewItemData<ConfigNode>> roots, Type type, int id)
		{
			foreach (var root in roots)
			{
				if (TryFind(root, out var found))
				{
					return found;
				}
			}
			return null;

			bool TryFind(TreeViewItemData<ConfigNode> node, out int foundId)
			{
				if (node.data.Kind == ConfigNodeKind.Entry && node.data.ConfigType == type && node.data.ConfigId == id)
				{
					foundId = node.id;
					return true;
				}

				if (node.hasChildren)
				{
					foreach (var child in node.children)
					{
						if (TryFind(child, out foundId))
						{
							return true;
						}
					}
				}

				foundId = 0;
				return false;
			}
		}

		private void ExportJson()
		{
			var provider = _provider;
			if (provider == null)
			{
				EditorUtility.DisplayDialog("Export JSON", "No provider selected. Enter Play Mode and create a ConfigsProvider.", "OK");
				return;
			}

			var json = ExportProviderToJson(provider);
			var path = EditorUtility.SaveFilePanel("Export Configs JSON", Application.dataPath, "configs.json", "json");
			if (string.IsNullOrWhiteSpace(path))
			{
				return;
			}

			System.IO.File.WriteAllText(path, json);
			EditorUtility.RevealInFinder(path);
		}

		private static string ExportProviderToJson(IConfigsProvider provider)
		{
			var result = new Dictionary<string, object>();
			foreach (var kv in provider.GetAllConfigs())
			{
				if (TryReadConfigs(kv.Value, out var entries))
				{
					var dict = new Dictionary<int, object>();
					for (int i = 0; i < entries.Count; i++)
					{
						dict[entries[i].Id] = entries[i].Value;
					}
					result[kv.Key.FullName ?? kv.Key.Name] = dict;
				}
			}

			return JsonConvert.SerializeObject(result, Formatting.Indented);
		}

		private static string ToJson(object obj)
		{
			if (obj == null) return string.Empty;
			try
			{
				return JsonConvert.SerializeObject(obj, Formatting.Indented);
			}
			catch (Exception ex)
			{
				return $"// Failed to serialize {obj.GetType().Name}: {ex.Message}";
			}
		}

		private List<ValidationErrorInfo> ValidateAllConfigs()
		{
			var provider = _provider;
			if (provider == null) return new List<ValidationErrorInfo>();

			var errors = new List<ValidationErrorInfo>();
			foreach (var kv in provider.GetAllConfigs())
			{
				if (!TryReadConfigs(kv.Value, out var entries))
				{
					continue;
				}

				for (int i = 0; i < entries.Count; i++)
				{
					ValidateObject(kv.Key, entries[i].Id, entries[i].Value, errors);
				}
			}
			return errors;
		}

		private List<ValidationErrorInfo> ValidateSingleConfig(ConfigSelection selection)
		{
			var errors = new List<ValidationErrorInfo>();
			if (!selection.IsValid) return errors;
			ValidateObject(selection.ConfigType, selection.ConfigId, selection.Value, errors);
			return errors;
		}

		private static void ValidateObject(Type configType, int configId, object instance, List<ValidationErrorInfo> errors)
		{
			if (instance == null) return;

			foreach (var field in configType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				var attrs = field.GetCustomAttributes(typeof(ValidationAttribute), inherit: true);
				AddValidationErrors(configType, configId, field.Name, attrs, field.GetValue(instance), errors);
			}

			foreach (var prop in configType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (!prop.CanRead) continue;
				var attrs = prop.GetCustomAttributes(typeof(ValidationAttribute), inherit: true);
				AddValidationErrors(configType, configId, prop.Name, attrs, prop.GetValue(instance), errors);
			}
		}

		private static void AddValidationErrors(Type configType, int configId, string memberName, object[] attrs, object value, List<ValidationErrorInfo> errors)
		{
			for (int i = 0; i < attrs.Length; i++)
			{
				if (attrs[i] is ValidationAttribute validationAttribute)
				{
					if (!validationAttribute.IsValid(value, out var message))
					{
						errors.Add(new ValidationErrorInfo(configType.Name, configId == SingleConfigId ? null : configId, memberName, message));
					}
				}
			}
		}

		private static bool TryReadConfigs(IEnumerable container, out List<ConfigEntry> entries)
		{
			entries = new List<ConfigEntry>();
			if (container == null) return false;

			// ConfigsProvider stores Dictionary<int, T> for both singleton and collections.
			// Use reflection to iterate entries regardless of T.
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

				var id = (int)keyProp.GetValue(item);
				var value = valueProp.GetValue(item);
				entries.Add(new ConfigEntry(id, value));
			}

			entries.Sort((a, b) => a.Id.CompareTo(b.Id));
			return true;
		}

		private readonly struct ConfigEntry
		{
			public readonly int Id;
			public readonly object Value;

			public ConfigEntry(int id, object value)
			{
				Id = id;
				Value = value;
			}
		}

		private readonly struct ConfigSelection
		{
			public readonly Type ConfigType;
			public readonly int ConfigId;
			public readonly object Value;

			public bool IsValid => ConfigType != null;

			public ConfigSelection(Type configType, int configId, object value)
			{
				ConfigType = configType;
				ConfigId = configId;
				Value = value;
			}

			/// <summary>Creates an empty selection.</summary>
			public static ConfigSelection None() => new ConfigSelection(null, 0, null);
		}

		private readonly struct ValidationFilter
		{
			public readonly bool IsAll;
			public readonly Type ConfigType;
			public readonly int ConfigId;

			private ValidationFilter(bool isAll, Type configType, int configId)
			{
				IsAll = isAll;
				ConfigType = configType;
				ConfigId = configId;
			}

			/// <summary>Creates a filter that shows all validation errors.</summary>
			public static ValidationFilter All() => new ValidationFilter(true, null, 0);

			/// <summary>Creates a filter for a single config entry.</summary>
			public static ValidationFilter Single(Type type, int id) => new ValidationFilter(false, type, id);
		}

		private readonly struct ValidationErrorInfo
		{
			public readonly string ConfigTypeName;
			public readonly int? ConfigId;
			public readonly string FieldName;
			public readonly string Message;

			public ValidationErrorInfo(string configTypeName, int? configId, string fieldName, string message)
			{
				ConfigTypeName = configTypeName;
				ConfigId = configId;
				FieldName = fieldName;
				Message = message;
			}
		}

		private enum ConfigNodeKind
		{
			Header, // A header node grouping other nodes.
			Type, // A config type node containing entry children.
			Entry // A selectable config entry node.
		}

		private readonly struct ConfigNode
		{
			public readonly ConfigNodeKind Kind;
			public readonly string DisplayName;
			public readonly Type ConfigType;
			public readonly int ConfigId;
			public readonly object Value;

			private ConfigNode(ConfigNodeKind kind, string displayName, Type configType, int configId, object value)
			{
				Kind = kind;
				DisplayName = displayName;
				ConfigType = configType;
				ConfigId = configId;
				Value = value;
			}

			/// <summary>Creates a header node.</summary>
			public static ConfigNode Header(string name) => new ConfigNode(ConfigNodeKind.Header, name, null, 0, null);

			/// <summary>Creates a type node.</summary>
			public static ConfigNode Type(Type type, string name) => new ConfigNode(ConfigNodeKind.Type, name, type, 0, null);

			/// <summary>Creates an entry node.</summary>
			public static ConfigNode Entry(Type type, int id, object value, string name) => new ConfigNode(ConfigNodeKind.Entry, name, type, id, value);
		}
	}
}

