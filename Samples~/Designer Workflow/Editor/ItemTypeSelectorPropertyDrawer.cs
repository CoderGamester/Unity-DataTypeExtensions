using GameLoversEditor.GameData;
using UnityEditor;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// PropertyDrawer for <see cref="ItemTypeSelector"/> using the shared EnumSelector drawer.
	/// </summary>
	[CustomPropertyDrawer(typeof(ItemTypeSelector))]
	public sealed class ItemTypeSelectorPropertyDrawer : EnumSelectorPropertyDrawer<ItemType>
	{
	}
}

