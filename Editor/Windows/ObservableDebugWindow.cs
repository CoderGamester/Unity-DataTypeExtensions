using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Editor window that displays all active observable instances tracked by <see cref="ObservableDebugRegistry"/>.
	/// Access via <c>Window/GameLovers/Observable Debugger</c>.
	/// </summary>
	/// <remarks>
	/// <para>Observables automatically register themselves when constructed in the editor via the self-registration pattern.</para>
	/// <para>Features filtering by name, type (Field/Computed/List/etc.), and subscriber activity.</para>
	/// <para>Selecting a Computed observable shows its dependency graph in the bottom panel.</para>
	/// </remarks>
	public sealed class ObservableDebugWindow : EditorWindow
	{
		private ToolbarSearchField _filterField;
		private PopupField<string> _kindField;
		private Toggle _activeOnlyToggle;
		private Button _clearHistoryButton;

		private ListView _listView;
		private readonly List<ObservableDebugRegistry.EntrySnapshot> _rows = new List<ObservableDebugRegistry.EntrySnapshot>();

		private Label _headerLabel;
		private DependencyGraphElement _dependencyGraph;

		private static readonly List<string> _kinds = new List<string>
		{
			"All",
			"Field",
			"Computed",
			"List",
			"Dictionary",
			"HashSet"
		};

		/// <summary>
		/// Opens the Observable Debugger window.
		/// </summary>
		[MenuItem("Window/GameLovers/Observable Debugger")]
		public static void ShowWindow()
		{
			var window = GetWindow<ObservableDebugWindow>("Observable Debugger");
			window.minSize = new Vector2(720, 420);
			window.Show();
		}

		/// <summary>
		/// Creates the UI Toolkit GUI when the window is opened or reloaded.
		/// </summary>
		public void CreateGUI()
		{
			rootVisualElement.Clear();
			rootVisualElement.style.flexGrow = 1;

			rootVisualElement.Add(BuildToolbar());

			var split = new TwoPaneSplitView(1, 180, TwoPaneSplitViewOrientation.Vertical);
			split.style.flexGrow = 1;
			split.Add(BuildList());

			_dependencyGraph = new DependencyGraphElement();
			split.Add(_dependencyGraph);

			rootVisualElement.Add(split);

			// Poll periodically while window is open (editor-only registry, no playmode requirement).
			rootVisualElement.schedule.Execute(RefreshData).Every(250);
			RefreshData();
		}

		private VisualElement BuildToolbar()
		{
			var toolbar = new Toolbar();

			_headerLabel = new Label("Observables");
			_headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			_headerLabel.style.marginRight = 8;

			_filterField = new ToolbarSearchField();
			_filterField.style.flexGrow = 1;
			_filterField.RegisterValueChangedCallback(_ => RefreshData());

			_kindField = new PopupField<string>("Type", _kinds, 0);
			_kindField.RegisterValueChangedCallback(_ => RefreshData());

			_activeOnlyToggle = new Toggle("Active only");
			_activeOnlyToggle.RegisterValueChangedCallback(_ => RefreshData());

			_clearHistoryButton = new Button(() =>
			{
				// No persistent history yet (planned via DependencyGraphElement todo).
				// Keep button for future expansion; for now it just refreshes.
				RefreshData();
			})
			{
				text = "Clear History"
			};

			toolbar.Add(_headerLabel);
			toolbar.Add(_filterField);
			toolbar.Add(_kindField);
			toolbar.Add(_activeOnlyToggle);
			toolbar.Add(_clearHistoryButton);
			return toolbar;
		}

		private VisualElement BuildList()
		{
			_listView = new ListView
			{
				selectionType = SelectionType.Single,
				showBorder = true,
				showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
				style = { flexGrow = 1 }
			};

			_listView.itemsSource = _rows;
			_listView.makeItem = MakeRow;
			_listView.bindItem = BindRow;
			_listView.selectionChanged += OnSelectionChanged;

			return _listView;
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

			row.Add(new Label { name = "Name", style = { flexGrow = 2, minWidth = 260 } });
			row.Add(new Label { name = "Value", style = { flexGrow = 3, minWidth = 260 } });
			row.Add(new Label { name = "Subs", style = { minWidth = 70, unityTextAlign = TextAnchor.MiddleRight } });
			row.Add(new Label { name = "Kind", style = { minWidth = 100, unityTextAlign = TextAnchor.MiddleRight } });

			return row;
		}

		private void BindRow(VisualElement element, int index)
		{
			if (index < 0 || index >= _rows.Count) return;
			var s = _rows[index];

			element.Q<Label>("Name").text = $"{s.Info.Name}#{s.Info.Id}";
			element.Q<Label>("Value").text = s.Value;
			element.Q<Label>("Subs").text = $"Subs: {s.Subscribers}";
			element.Q<Label>("Kind").text = s.Info.Kind;
		}

		private void RefreshData()
		{
			_rows.Clear();

			var filter = _filterField?.value?.Trim();
			var hasFilter = !string.IsNullOrEmpty(filter);
			var filterLower = hasFilter ? filter.ToLowerInvariant() : string.Empty;

			var kind = _kindField?.value ?? "All";
			var activeOnly = _activeOnlyToggle != null && _activeOnlyToggle.value;

			foreach (var s in ObservableDebugRegistry.EnumerateSnapshots())
			{
				if (activeOnly && s.Subscribers <= 0)
				{
					continue;
				}

				if (kind != "All" && !string.Equals(s.Info.Kind, kind, StringComparison.Ordinal))
				{
					continue;
				}

				if (hasFilter)
				{
					var name = s.Info.Name ?? string.Empty;
					if (!name.ToLowerInvariant().Contains(filterLower))
					{
						continue;
					}
				}

				_rows.Add(s);
			}

			_rows.Sort((a, b) =>
			{
				var c = string.CompareOrdinal(a.Info.Kind, b.Info.Kind);
				return c != 0 ? c : string.CompareOrdinal(a.Info.Name, b.Info.Name);
			});

			_headerLabel.text = $"Observables: {_rows.Count}";
			_listView.RefreshItems();

			if (_rows.Count == 0)
			{
				_dependencyGraph.SetTarget(default);
			}
		}

		private void OnSelectionChanged(IEnumerable<object> selected)
		{
			var first = selected.FirstOrDefault();
			if (first is ObservableDebugRegistry.EntrySnapshot snapshot)
			{
				_dependencyGraph.SetTarget(snapshot);
			}
			else
			{
				_dependencyGraph.SetTarget(default);
			}
		}
	}
}

