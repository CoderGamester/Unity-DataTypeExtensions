using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// UI Toolkit inspector for any concrete type deriving from <see cref="ConfigsScriptableObject{TId,TAsset}"/>.
	/// Shows per-entry status (duplicate keys / validation errors) and provides a "Validate All" action.
	/// </summary>
	[CustomEditor(typeof(ConfigsScriptableObject<,>), true)]
	public sealed class ConfigsScriptableObjectInspector : UnityEditor.Editor
	{
		private const string ConfigsFieldName = "_configs";

		private readonly Dictionary<int, EntryStatus> _statusByIndex = new Dictionary<int, EntryStatus>();
		private readonly HashSet<object> _seenKeys = new HashSet<object>();

		private ListView _listView;
		private Label _summaryLabel;

		/// <inheritdoc />
		public override VisualElement CreateInspectorGUI()
		{
			var root = new VisualElement();

			var configsProp = serializedObject.FindProperty(ConfigsFieldName);
			if (configsProp == null || !configsProp.isArray)
			{
				root.Add(new HelpBox($"Expected serialized array field '{ConfigsFieldName}' on {target.GetType().Name}.", HelpBoxMessageType.Error));
				InspectorElement.FillDefaultInspector(root, serializedObject, this);
				return root;
			}

			root.Add(BuildHeader(configsProp));
			root.Add(BuildListView(configsProp));

			// Keep status badges up to date as the user edits values.
			root.TrackSerializedObjectValue(serializedObject, _ => Revalidate(configsProp));
			Revalidate(configsProp);

			return root;
		}

		private VisualElement BuildHeader(SerializedProperty configsProp)
		{
			var header = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.SpaceBetween,
					marginBottom = 6,
				}
			};

			_summaryLabel = new Label();
			_summaryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

			var validateAllBtn = new Button(() => Revalidate(configsProp))
			{
				text = "Validate All"
			};

			header.Add(_summaryLabel);
			header.Add(validateAllBtn);
			return header;
		}

		private VisualElement BuildListView(SerializedProperty configsProp)
		{
			_listView = new ListView
			{
				selectionType = SelectionType.Single,
				showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
				reorderable = true,
				showBorder = true,
				style =
				{
					flexGrow = 1,
					minHeight = 200
				}
			};

			_listView.itemsSource = CreateIndexSource(configsProp.arraySize);
			_listView.makeItem = MakeRow;
			_listView.bindItem = (e, i) => BindRow(e, configsProp, i);

			_listView.itemsChosen += _ => Revalidate(configsProp);

			return _listView;
		}

		private static List<int> CreateIndexSource(int size)
		{
			var list = new List<int>(size);
			for (int i = 0; i < size; i++)
			{
				list.Add(i);
			}
			return list;
		}

		private static VisualElement MakeRow()
		{
			var row = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					paddingLeft = 4,
					paddingRight = 4,
					paddingTop = 2,
					paddingBottom = 2,
				}
			};

			var keyField = new PropertyField { name = "KeyField" };
			keyField.style.flexGrow = 1;

			var valueField = new PropertyField { name = "ValueField" };
			valueField.style.flexGrow = 2;

			var statusLabel = new Label { name = "StatusLabel" };
			statusLabel.style.minWidth = 120;
			statusLabel.style.unityTextAlign = TextAnchor.MiddleRight;

			row.Add(keyField);
			row.Add(valueField);
			row.Add(statusLabel);
			return row;
		}

		private void BindRow(VisualElement row, SerializedProperty configsProp, int index)
		{
			if (index >= configsProp.arraySize)
			{
				return;
			}

			var elementProp = configsProp.GetArrayElementAtIndex(index);
			var keyProp = elementProp.FindPropertyRelative("Key");
			var valueProp = elementProp.FindPropertyRelative("Value");

			var keyField = row.Q<PropertyField>("KeyField");
			var valueField = row.Q<PropertyField>("ValueField");
			var statusLabel = row.Q<Label>("StatusLabel");

			keyField.label = $"[{index}] Key";
			valueField.label = "Value";

			if (keyProp != null) keyField.BindProperty(keyProp);
			if (valueProp != null) valueField.BindProperty(valueProp);

			statusLabel.text = GetStatusText(index, out var level);
			ApplyStatusStyle(statusLabel, level);
		}

		private void Revalidate(SerializedProperty configsProp)
		{
			serializedObject.Update();

			_statusByIndex.Clear();
			_seenKeys.Clear();

			var list = TryGetConfigsList(target);
			var errorCount = 0;
			var duplicateCount = 0;

			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					var pair = list[i];
					if (pair == null)
					{
						_statusByIndex[i] = EntryStatus.Error("Null entry");
						errorCount++;
						continue;
					}

					var key = pair.GetType().GetField("Key")?.GetValue(pair);
					var value = pair.GetType().GetField("Value")?.GetValue(pair);

					var isDuplicate = key != null && !_seenKeys.Add(key);
					var messages = ValidateObject(value);

					if (isDuplicate)
					{
						duplicateCount++;
					}

					if (messages.Count > 0)
					{
						errorCount += messages.Count;
					}

					if (isDuplicate)
					{
						_statusByIndex[i] = EntryStatus.DuplicateKey(messages.Count);
					}
					else if (messages.Count > 0)
					{
						_statusByIndex[i] = EntryStatus.Errors(messages.Count);
					}
					else
					{
						_statusByIndex[i] = EntryStatus.Ok();
					}
				}
			}

			_summaryLabel.text = $"Entries: {configsProp.arraySize}  Errors: {errorCount}  Duplicates: {duplicateCount}";

			// Keep list indices in sync if size changed.
			_listView.itemsSource = CreateIndexSource(configsProp.arraySize);
			_listView.RefreshItems();
		}

		private static IList TryGetConfigsList(UnityEngine.Object targetObject)
		{
			if (targetObject == null) return null;
			var field = targetObject.GetType().GetField(ConfigsFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			return field?.GetValue(targetObject) as IList;
		}

		private static List<string> ValidateObject(object obj)
		{
			var messages = new List<string>();
			if (obj == null)
			{
				return messages;
			}

			var type = obj.GetType();

			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				AddValidationMessages(obj, field.FieldType, field.GetCustomAttributes(typeof(ValidationAttribute), inherit: true), field.GetValue(obj), field.Name, messages);
			}

			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (!prop.CanRead) continue;
				AddValidationMessages(obj, prop.PropertyType, prop.GetCustomAttributes(typeof(ValidationAttribute), inherit: true), prop.GetValue(obj), prop.Name, messages);
			}

			return messages;
		}

		private static void AddValidationMessages(
			object owner,
			Type memberType,
			object[] attributes,
			object value,
			string memberName,
			List<string> messages)
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				if (attributes[i] is ValidationAttribute validationAttribute)
				{
					if (!validationAttribute.IsValid(value, out var message))
					{
						messages.Add($"{memberName}: {message}");
					}
				}
			}
		}

		private string GetStatusText(int index, out StatusLevel level)
		{
			if (!_statusByIndex.TryGetValue(index, out var status))
			{
				level = StatusLevel.Info;
				return "…";
			}

			level = status.Level;
			return status.Text;
		}

		private static void ApplyStatusStyle(Label label, StatusLevel level)
		{
			switch (level)
			{
				case StatusLevel.Ok:
					label.style.color = new StyleColor(new Color(0.25f, 0.6f, 0.3f));
					break;
				case StatusLevel.Warning:
					label.style.color = new StyleColor(new Color(0.7f, 0.55f, 0.15f));
					break;
				case StatusLevel.Error:
					label.style.color = new StyleColor(new Color(0.8f, 0.25f, 0.25f));
					break;
				default:
					label.style.color = StyleKeyword.Null;
					break;
			}
		}

		private readonly struct EntryStatus
		{
			public readonly string Text;
			public readonly StatusLevel Level;

			private EntryStatus(string text, StatusLevel level)
			{
				Text = text;
				Level = level;
			}

			// Creates a status indicating validation passed
			public static EntryStatus Ok() => new EntryStatus("OK", StatusLevel.Ok);

			// Creates a status indicating validation errors were found
			public static EntryStatus Errors(int count) => new EntryStatus($"Errors: {count}", StatusLevel.Error);

			// Creates a status indicating a duplicate key was detected
			public static EntryStatus DuplicateKey(int errorCount) =>
				errorCount > 0
					? new EntryStatus($"DUPLICATE (+{errorCount})", StatusLevel.Warning)
					: new EntryStatus("DUPLICATE", StatusLevel.Warning);

			// Creates a status with a custom error message
			public static EntryStatus Error(string message) => new EntryStatus(message, StatusLevel.Error);
		}

		private enum StatusLevel
		{
			Info, // Informational status (neutral color)
			Ok, // Validation passed (positive color)
			Warning, // Non-critical issue such as duplicate keys (warning color)
			Error // Validation failure (error color)
		}
	}
}

