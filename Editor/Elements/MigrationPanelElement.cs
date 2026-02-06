using GameLovers.GameData;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// UI Toolkit panel that lists migrations discovered by the editor migration system and provides
	/// an in-memory migration preview ("Migration Preview") for a selected row.
	/// </summary>
	public sealed class MigrationPanelElement : VisualElement
	{
		private readonly MigrationPanelView _view;
		private readonly MigrationPanelController _controller;

		public MigrationPanelElement()
		{
			style.flexGrow = 1;
			_view = new MigrationPanelView();
			_controller = new MigrationPanelController(_view);
			Add(_view);
		}

		/// <summary>
		/// Sets the config provider to inspect for migrations.
		/// </summary>
		public void SetProvider(IConfigsProvider provider)
		{
			_controller.SetProvider(provider);
		}

		/// <summary>
		/// Rebuilds the migration list and preview panels based on the current provider.
		/// </summary>
		public void Rebuild()
		{
			_controller.Rebuild();
		}
	}
}
