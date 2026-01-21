using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// Entry point MonoBehaviour for the Reactive UI Demo (UI Toolkit) sample.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ReactiveToolkitDemoController : MonoBehaviour
	{
		[SerializeField] private UIDocument _document;

		// Cached UI elements
		private ProgressBar _healthBar;
		private Label _healthLabel;
		private ScrollView _inventoryList;

		private Button _damageButton;
		private Button _healButton;
		private Button _weaponBonusButton;
		private Button _baseDamageButton;
		private Button _addItemButton;
		private Button _removeItemButton;
		private Button _batchButton;

		private Label _baseDamageLabel;
		private Label _weaponBonusLabel;
		private Label _totalDamageLabel;

		// View helpers
		private ReactiveToolkitHealthBar _healthBarView;
		private ReactiveToolkitInventoryList _inventoryListView;
		private ReactiveToolkitStatsPanel _statsPanel;

		private ReactiveToolkitPlayerData _data;

		private void Awake()
		{
			_document ??= GetComponent<UIDocument>();

			_data = new ReactiveToolkitPlayerData();
			SeedInventory(_data);
		}

		private void Start()
		{
			CacheElements();
			CreateViewHelpers();
			BindViews();
			WireButtons();
		}

		private void OnDestroy()
		{
			_healthBarView?.Dispose();
			_inventoryListView?.Dispose();
			_statsPanel?.Dispose();
			_data?.Dispose();
		}

		private void CacheElements()
		{
			var root = _document != null ? _document.rootVisualElement : null;
			if (root == null)
			{
				return;
			}

			_healthBar = root.Q<ProgressBar>("healthBar");
			_healthLabel = root.Q<Label>("healthLabel");
			_inventoryList = root.Q<ScrollView>("inventoryList");

			_damageButton = root.Q<Button>("damageButton");
			_healButton = root.Q<Button>("healButton");
			_weaponBonusButton = root.Q<Button>("weaponBonusButton");
			_baseDamageButton = root.Q<Button>("baseDamageButton");
			_addItemButton = root.Q<Button>("addItemButton");
			_removeItemButton = root.Q<Button>("removeItemButton");
			_batchButton = root.Q<Button>("batchButton");

			_baseDamageLabel = root.Q<Label>("baseDamageLabel");
			_weaponBonusLabel = root.Q<Label>("weaponBonusLabel");
			_totalDamageLabel = root.Q<Label>("damageLabel");
		}

		private void CreateViewHelpers()
		{
			_healthBarView = new ReactiveToolkitHealthBar(_healthBar, _healthLabel, 100);
			_inventoryListView = new ReactiveToolkitInventoryList(_inventoryList);
			_statsPanel = new ReactiveToolkitStatsPanel(_baseDamageLabel, _weaponBonusLabel, _totalDamageLabel);
		}

		private void BindViews()
		{
			if (_data == null)
			{
				return;
			}

			_healthBarView?.Bind(_data.Health);
			_inventoryListView?.Bind(_data.Inventory);
			_statsPanel?.Bind(_data.BaseDamage, _data.WeaponBonus, _data.TotalDamage);
		}

		private void WireButtons()
		{
			if (_data == null)
			{
				return;
			}

			WireButton(_damageButton, () => _data.Health.Value = Mathf.Max(0, _data.Health.Value - 10));
			WireButton(_healButton, () => _data.Health.Value = Mathf.Min(100, _data.Health.Value + 10));

			WireButton(_weaponBonusButton, () => _data.WeaponBonus.Value += 1);
			WireButton(_baseDamageButton, () => _data.BaseDamage.Value += 1);

			WireButton(_addItemButton, () => _data.Inventory.Add($"Item_{_data.Inventory.Count + 1}"));
			WireButton(_removeItemButton, RemoveLastInventoryItem);

			WireButton(_batchButton, ApplyBatchUpdate);
		}

		private static void WireButton(Button button, Action onClick)
		{
			if (button == null)
			{
				return;
			}

			button.clicked += () => onClick?.Invoke();
		}

		private static void SeedInventory(ReactiveToolkitPlayerData data)
		{
			if (data == null)
			{
				return;
			}

			data.Inventory.Add("Sword");
			data.Inventory.Add("Potion");
		}

		private void ApplyBatchUpdate()
		{
			// Batch multiple changes so observers get a consolidated update.
			using var batch = new ObservableBatch();
			batch.Add(_data.Health);
			batch.Add(_data.BaseDamage);
			batch.Add(_data.WeaponBonus);
			batch.Add(_data.Inventory);

			_data.Health.Value = Mathf.Clamp(_data.Health.Value - 5, 0, 100);
			_data.BaseDamage.Value += 2;
			_data.WeaponBonus.Value += 2;
			_data.Inventory.Add($"BatchItem_{DateTime.Now:HHmmss}");
		}

		private void RemoveLastInventoryItem()
		{
			if (_data.Inventory.Count > 0)
			{
				_data.Inventory.RemoveAt(_data.Inventory.Count - 1);
			}
		}
	}
}
