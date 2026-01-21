using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.GameData
{
	/// <summary>
	/// Implement this property drawer with your own custom EnumSelectorPropertyDrawer implementation for the given
	/// enum of type <typeparamref name="T"/>.
	/// Supports both UI Toolkit (CreatePropertyGUI) and IMGUI (OnGUI) for custom inspector compatibility.
	/// 
	/// Ex:
	/// [CustomPropertyDrawer(typeof(EnumSelectorExample))]
	/// public class EnumSelectorExamplePropertyDrawer : EnumSelectorPropertyDrawer{EnumExample}
	/// {
	/// }
	/// </summary>
	public abstract class EnumSelectorPropertyDrawer<T> : PropertyDrawer
		where T : Enum
	{
		private static readonly Dictionary<Type, GUIContent[]> _sortedEnums = new Dictionary<Type, GUIContent[]>();

		private bool _errorFound;

		/// <summary>
		/// UI Toolkit implementation for default inspector in Unity 6+.
		/// </summary>
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var container = new VisualElement();
			var enumType = typeof(T);
			var enumNames = Enum.GetNames(enumType).OrderBy(n => n).ToList();
			var selectionProperty = property.FindPropertyRelative("_selection");
			var currentString = selectionProperty.stringValue;
			var currentIndex = enumNames.IndexOf(currentString);

			if (currentIndex == -1 && !string.IsNullOrWhiteSpace(currentString))
			{
				enumNames.Insert(0, $"Invalid: {currentString}");
				currentIndex = 0;
			}
			else if (currentIndex == -1)
			{
				currentIndex = 0;
			}

			var dropdown = new DropdownField(property.displayName, enumNames, currentIndex);

			dropdown.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue.StartsWith("Invalid: "))
				{
					return;
				}

				selectionProperty.stringValue = evt.newValue;
				selectionProperty.serializedObject.ApplyModifiedProperties();
			});

			container.Add(dropdown);

			return container;
		}

		/// <summary>
		/// IMGUI implementation for custom inspectors using OnInspectorGUI.
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var enumType = typeof(T);
			var enumValues = GetSortedEnumConstants(enumType);
			var selectionProperty = property.FindPropertyRelative("_selection");
			var currentString = selectionProperty.stringValue;
			var currentIndex = string.IsNullOrWhiteSpace(currentString) ? 0 : Array.FindIndex(enumValues, s => s.text == currentString);

			if (currentIndex != -1)
			{
				selectionProperty.stringValue = enumValues[EditorGUI.Popup(position, label, currentIndex, enumValues)].text;

				_errorFound = false;
			}
			else
			{
				// The string is not a valid enum constant, because it was renamed or removed
				if (!_errorFound)
				{
					var targetObject = selectionProperty.serializedObject.targetObject;

					Debug.LogError($"Invalid enum constant: {enumType.Name}.{currentString} in object {targetObject.name} of type: {targetObject.GetType().Name}");

					_errorFound = true;
				}

				var color = GUI.contentColor;
				var finalArray = new[] { new GUIContent("Invalid: " + currentString) }.Concat(enumValues).ToArray();

				GUI.contentColor = Color.red;
				var newSelection = EditorGUI.Popup(position, label, 0, finalArray);
				GUI.contentColor = color;

				if (newSelection > 0)
				{
					selectionProperty.stringValue = finalArray[newSelection].text;
				}
			}

			EditorGUI.EndProperty();
		}

		private GUIContent[] GetSortedEnumConstants(Type enumType)
		{
			if (!_sortedEnums.TryGetValue(enumType, out var content))
			{
				var values = Enum.GetNames(enumType);

				content = new GUIContent[values.Length];

				Array.Sort(values);

				for (var i = 0; i < values.Length; i++)
				{
					content[i] = new GUIContent(values[i]);
				}

				_sortedEnums.Add(enumType, content);
			}
			return content;
		}
	}
}
