using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Minimal JSON viewer element (read-only) for editor tooling.
	/// This intentionally starts simple (TextField) and can be upgraded later to a tree/diff view.
	/// </summary>
	public sealed class JsonViewerElement : VisualElement
	{
		private readonly TextField _text;

		public JsonViewerElement()
		{
			style.flexGrow = 1;

			_text = new TextField
			{
				multiline = true,
				isReadOnly = true
			};
			_text.style.flexGrow = 1;
			_text.style.unityFontStyleAndWeight = FontStyle.Normal;

			Add(_text);
		}

		/// <summary>
		/// Sets the JSON string to display in the viewer.
		/// </summary>
		public void SetJson(string json)
		{
			_text.value = json ?? string.Empty;
		}
	}
}

