using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// Entry point MonoBehaviour for the Reactive UI Demo (uGUI) sample.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ReactiveUGuiDemoController : MonoBehaviour
	{
		[Header("Reactive Views")]
		[SerializeField] private ReactiveHealthBar _healthBar;
		[SerializeField] private ReactiveInventoryList _inventoryList;
		[SerializeField] private ReactiveUGuiStatsPanel _statsPanel;

		[Header("Buttons")]
		[SerializeField] private Button _damageButton;
		[SerializeField] private Button _healButton;
		[SerializeField] private Button _weaponBonusButton;
		[SerializeField] private Button _baseDamageButton;
		[SerializeField] private Button _addItemButton;
		[SerializeField] private Button _removeItemButton;
		[SerializeField] private Button _batchButton;

		private ReactiveUGuiPlayerData _data;

		private void Awake()
		{
			_data = new ReactiveUGuiPlayerData();
			SeedInventory(_data);
		}

		private void Start()
		{
			BindViews();
			WireButtons();
		}

		private void OnDestroy()
		{
			_data?.Dispose();
		}

		private void BindViews()
		{
			if (_data == null)
			{
				return;
			}

			_healthBar?.Bind(_data.Health, 100);
			_inventoryList?.Bind(_data.Inventory);
			_statsPanel?.Bind(_data.BaseDamage, _data.WeaponBonus, _data.TotalDamage);
		}

		private void WireButtons()
		{
			if (_data == null)
			{
				return;
			}

			if (_damageButton != null)
			{
				_damageButton.onClick.AddListener(() => _data.Health.Value = Mathf.Max(0, _data.Health.Value - 10));
			}

			if (_healButton != null)
			{
				_healButton.onClick.AddListener(() => _data.Health.Value = Mathf.Min(100, _data.Health.Value + 10));
			}

			if (_weaponBonusButton != null)
			{
				_weaponBonusButton.onClick.AddListener(() => _data.WeaponBonus.Value += 1);
			}

			if (_baseDamageButton != null)
			{
				_baseDamageButton.onClick.AddListener(() => _data.BaseDamage.Value += 1);
			}

			if (_addItemButton != null)
			{
				_addItemButton.onClick.AddListener(() => _data.Inventory.Add($"Item_{_data.Inventory.Count + 1}"));
			}

			if (_removeItemButton != null)
			{
				_removeItemButton.onClick.AddListener(RemoveLastInventoryItem);
			}

			if (_batchButton != null)
			{
				_batchButton.onClick.AddListener(ApplyBatchUpdate);
			}
		}

		private static void SeedInventory(ReactiveUGuiPlayerData data)
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
