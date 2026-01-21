using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Minimal JSON viewer element (read-only) for editor tooling.
	/// This intentionally starts simple (Label in ScrollView) and can be upgraded later to a tree/diff view.
	/// </summary>
	public sealed class JsonViewerElement : VisualElement
	{
		private readonly ScrollView _scrollView;
		private readonly Label _label;

		public JsonViewerElement()
		{
			style.flexGrow = 1;

			_scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
			{
				horizontalScrollerVisibility = ScrollerVisibility.Auto,
				verticalScrollerVisibility = ScrollerVisibility.Auto
			};
			_scrollView.style.flexGrow = 1;
			_scrollView.style.backgroundColor = new StyleColor(new Color(0.16f, 0.16f, 0.16f, 1f));
			_scrollView.style.borderTopWidth = 1;
			_scrollView.style.borderBottomWidth = 1;
			_scrollView.style.borderLeftWidth = 1;
			_scrollView.style.borderRightWidth = 1;
			_scrollView.style.borderTopColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 1f));
			_scrollView.style.borderBottomColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 1f));
			_scrollView.style.borderLeftColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 1f));
			_scrollView.style.borderRightColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 1f));
			_scrollView.style.borderTopLeftRadius = 3;
			_scrollView.style.borderTopRightRadius = 3;
			_scrollView.style.borderBottomLeftRadius = 3;
			_scrollView.style.borderBottomRightRadius = 3;

			_label = new Label();
			_label.style.unityFontStyleAndWeight = FontStyle.Normal;
			_label.style.whiteSpace = WhiteSpace.NoWrap;
			_label.style.paddingLeft = 4;
			_label.style.paddingRight = 4;
			_label.style.paddingTop = 2;
			_label.style.paddingBottom = 2;
			_label.enableRichText = false;
			_label.selection.isSelectable = true;

			_scrollView.Add(_label);
			Add(_scrollView);
		}

		/// <summary>
		/// Sets the JSON string to display in the viewer.
		/// </summary>
		public void SetJson(string json)
		{
			_label.text = json ?? string.Empty;
		}
	}
}

