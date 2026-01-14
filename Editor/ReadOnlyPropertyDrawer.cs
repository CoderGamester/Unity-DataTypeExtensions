using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GameLovers;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor
{
	/// <summary>
	/// This class contain custom drawer for ReadOnly attribute.
	/// Uses UI Toolkit for rendering.
	/// </summary>
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyPropertyDrawer : PropertyDrawer
	{
		/// <inheritdoc />
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var field = new PropertyField(property);

			field.SetEnabled(false);

			return field;
		}
	}
}
