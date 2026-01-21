using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GameLovers.GameData;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.GameData
{
	/// <summary>
	/// This class contain custom drawer for ReadOnly attribute.
	/// Supports both UI Toolkit (CreatePropertyGUI) and IMGUI (OnGUI) for custom inspector compatibility.
	/// </summary>
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyPropertyDrawer : PropertyDrawer
	{
		/// <summary>
		/// UI Toolkit implementation for default inspector in Unity 6+.
		/// </summary>
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var field = new PropertyField(property);

			field.SetEnabled(false);

			return field;
		}

		/// <summary>
		/// IMGUI implementation for custom inspectors using OnInspectorGUI.
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Saving previous GUI enabled value
			var previousGUIState = GUI.enabled;
			// Disabling edit for property
			GUI.enabled = false;
			// Drawing Property
			EditorGUI.PropertyField(position, property, label, true);
			// Setting old GUI enabled value
			GUI.enabled = previousGUIState;
		}

		/// <summary>
		/// Returns the proper height for the property in IMGUI mode.
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
	}
}
