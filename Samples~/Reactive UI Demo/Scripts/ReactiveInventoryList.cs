using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// Simple uGUI inventory view bound to an <see cref="ObservableList{T}"/>.
	/// </summary>
	public sealed class ReactiveInventoryList : MonoBehaviour
	{
		[SerializeField] private RectTransform _itemsRoot;
		[SerializeField] private Text _itemPrefab;

		private ObservableList<string> _inventory;
		private readonly List<Text> _spawned = new List<Text>();

		public void Setup(RectTransform itemsRoot, Text itemPrefab)
		{
			_itemsRoot = itemsRoot;
			_itemPrefab = itemPrefab;
		}

		public void Bind(ObservableList<string> inventory)
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
			// For clarity (and to avoid edge cases with batched updates), rebuild everything.
			Rebuild();
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
					Destroy(_spawned[i].gameObject);
				}
			}
			_spawned.Clear();

			for (var i = 0; i < _inventory.Count; i++)
			{
				var text = Instantiate(_itemPrefab, _itemsRoot);
				text.gameObject.SetActive(true);
				text.text = $"- {_inventory[i]}";
				_spawned.Add(text);
			}
		}
	}
}

