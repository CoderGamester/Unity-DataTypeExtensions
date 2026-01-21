using System;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// Simple UI Toolkit inventory view bound to an <see cref="ObservableList{T}"/>.
	/// </summary>
	public sealed class ReactiveToolkitInventoryList : IDisposable
	{
		private readonly ScrollView _itemsRoot;

		private IObservableListReader<string> _inventory;

		public ReactiveToolkitInventoryList(ScrollView itemsRoot)
		{
			_itemsRoot = itemsRoot;
		}

		public void Bind(IObservableListReader<string> inventory)
		{
			_inventory?.StopObservingAll(this);
			_inventory = inventory;
			_inventory.Observe(OnInventoryChanged);
			Rebuild();
		}

		public void Dispose()
		{
			_inventory?.StopObservingAll(this);
			_inventory = null;
		}

		private void OnInventoryChanged(int index, string prev, string curr, ObservableUpdateType type)
		{
			switch (type)
			{
				case ObservableUpdateType.Added:
					OnItemAdded(index, curr);
					break;
				case ObservableUpdateType.Updated:
					OnItemUpdated(index, curr);
					break;
				case ObservableUpdateType.Removed:
					OnItemRemoved(index);
					break;
			}
		}

		private void OnItemAdded(int index, string value)
		{
			_itemsRoot.Insert(index, new Label($"- {value}"));
		}

		private void OnItemUpdated(int index, string value)
		{
			// Handle batch notifications for newly added items (batch fires Updated for all indices)
			if (index >= _itemsRoot.childCount)
			{
				OnItemAdded(index, value);
				return;
			}

			((Label)_itemsRoot[index]).text = $"- {value}";
		}

		private void OnItemRemoved(int index)
		{
			_itemsRoot.RemoveAt(index);
		}

		private void Rebuild()
		{
			if (_itemsRoot == null || _inventory == null)
			{
				return;
			}

			_itemsRoot.Clear();

			for (var i = 0; i < _inventory.Count; i++)
			{
				OnItemAdded(i, _inventory[i]);
			}
		}
	}
}
