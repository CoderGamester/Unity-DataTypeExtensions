using System.Collections.Generic;

using TMPro;
using UnityEngine;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// Simple uGUI inventory view bound to an <see cref="ObservableList{T}"/>.
	/// </summary>
	public sealed class ReactiveInventoryList : MonoBehaviour
	{
		[SerializeField] private RectTransform _itemsRoot;
		[SerializeField] private GameObject _itemPrefab;

		private IObservableListReader<string> _inventory;
		private readonly List<GameObject> _spawned = new List<GameObject>();

		private void Awake()
		{
			_itemPrefab.SetActive(false);
		}

		public void Bind(IObservableListReader<string> inventory)
		{
			_inventory = inventory;
			_inventory.Observe(OnInventoryChanged);
			Rebuild();
		}

		private void OnDestroy()
		{
			_inventory?.StopObservingAll(this);
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
			var item = Instantiate(_itemPrefab, _itemsRoot);
			item.SetActive(true);
			item.transform.SetSiblingIndex(index);

			var label = item.GetComponent<TMP_Text>();
			if (label != null)
			{
				label.text = $"- {value}";
			}

			_spawned.Insert(index, item);
		}

		private void OnItemUpdated(int index, string value)
		{
			// Handle batch notifications for newly added items (batch fires Updated for all indices)
			if (index >= _spawned.Count)
			{
				OnItemAdded(index, value);
				return;
			}

			var label = _spawned[index].GetComponent<TMP_Text>();
			if (label != null)
			{
				label.text = $"- {value}";
			}
		}

		private void OnItemRemoved(int index)
		{
			Destroy(_spawned[index]);
			_spawned.RemoveAt(index);
		}

		private void Rebuild()
		{
			if (_itemsRoot == null || _itemPrefab == null || _inventory == null)
			{
				return;
			}

			for (var i = 0; i < _spawned.Count; i++)
			{
				if (_spawned[i] != null)
				{
					Destroy(_spawned[i]);
				}
			}
			_spawned.Clear();

			for (var i = 0; i < _inventory.Count; i++)
			{
				OnItemAdded(i, _inventory[i]);
			}
		}
	}
}

