using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor
{
	/// <summary>
	/// Implement this property drawer with your own custom EnumSelectorPropertyDrawer implementation for the given
	/// enum of type <typeparamref name="T"/>.
	/// Uses UI Toolkit for rendering.
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
		/// <inheritdoc />
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
	}
}
