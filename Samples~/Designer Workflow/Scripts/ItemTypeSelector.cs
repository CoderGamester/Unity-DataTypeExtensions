using System;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// Concrete <see cref="EnumSelector{T}"/> for <see cref="ItemType"/>.
	/// Uses name-based persistence for stability when enum values are reordered.
	/// </summary>
	[Serializable]
	public sealed class ItemTypeSelector : EnumSelector<ItemType>, IEquatable<ItemTypeSelector>
	{
		public ItemTypeSelector() : base(ItemType.Weapon)
		{
		}

		public ItemTypeSelector(ItemType value) : base(value)
		{
		}

		public bool Equals(ItemTypeSelector other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(GetSelectionString(), other.GetSelectionString(), StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is ItemTypeSelector other && Equals(other);
		}

		public override int GetHashCode()
		{
			var selection = GetSelectionString();
			return selection == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(selection);
		}

		public static implicit operator ItemTypeSelector(ItemType value)
		{
			return new ItemTypeSelector(value);
		}
	}
}

