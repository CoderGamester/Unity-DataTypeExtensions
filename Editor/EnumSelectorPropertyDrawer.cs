using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor
{
	/// <summary>
	/// Implement this property drawer with your own custom EnumSelectorPropertyDrawer implementation for the given
	/// enum of type <typeparamref name="T"/>
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
		private static readonly Dictionary<Type, string[]> _sortedEnums = new Dictionary<Type, string[]>();
	 
		private bool _errorFound;
	 
		/// <inheritdoc />
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var enumType = typeof(T);
			var enumValues = GetSortedEnumConstants(enumType);
			var selectionWidth = Mathf.Clamp(EditorGUIUtility.labelWidth, EditorGUIUtility.labelWidth, position.width * 0.33f);
			var selectionRect = new Rect(position.width - selectionWidth, position.y, selectionWidth, position.height);
			var selectionProperty = property.FindPropertyRelative("_selection");
			var currentString = selectionProperty.stringValue;
			var currentIndex = Array.IndexOf(enumValues, currentString);
	 
			EditorGUI.LabelField(position, label);
	 
			if (currentIndex != -1)
			{
				selectionProperty.stringValue = enumValues[EditorGUI.Popup(selectionRect, currentIndex, enumValues)];
	 
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
				var finalArray = new[] { "Invalid: " + currentString }.Concat(enumValues).ToArray();
	 
				GUI.contentColor = Color.red;
				var newSelection = EditorGUI.Popup(selectionRect, 0, finalArray);
				GUI.contentColor = color;
				
				if (newSelection > 0)
				{
					selectionProperty.stringValue = finalArray[newSelection];
				}
			}
	 
			EditorGUI.EndProperty();
		}
	 
		private string[] GetSortedEnumConstants(Type enumType)
		{
			if (!_sortedEnums.TryGetValue(enumType, out var values))
			{
				values = Enum.GetNames(enumType);
				Array.Sort(values);
				_sortedEnums.Add(enumType, values);
			}
			return values;
		}
	}
}