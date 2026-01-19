using TMPro;
using UnityEngine;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// uGUI view that binds to computed stats.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ReactiveUGuiStatsPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text _baseDamageLabel;
		[SerializeField] private TMP_Text _weaponBonusLabel;
		[SerializeField] private TMP_Text _totalDamageLabel;

		private IObservableFieldReader<int> _baseDamage;
		private IObservableFieldReader<int> _weaponBonus;
		private IObservableFieldReader<int> _totalDamage;

		public void Bind(IObservableFieldReader<int> baseDamage, IObservableFieldReader<int> weaponBonus, IObservableFieldReader<int> totalDamage)
		{
			_baseDamage = baseDamage;
			_weaponBonus = weaponBonus;
			_totalDamage = totalDamage;

			_baseDamage.InvokeObserve(OnBaseDamageChanged);
			_weaponBonus.InvokeObserve(OnWeaponBonusChanged);
			_totalDamage.InvokeObserve(OnTotalDamageChanged);
		}

		private void OnDestroy()
		{
			_baseDamage?.StopObservingAll(this);
			_weaponBonus?.StopObservingAll(this);
			_totalDamage?.StopObservingAll(this);
		}

		private void OnBaseDamageChanged(int previous, int current)
		{
			if (_baseDamageLabel != null)
			{
				_baseDamageLabel.text = $"BaseDamage: {current}";
			}
		}

		private void OnWeaponBonusChanged(int previous, int current)
		{
			if (_weaponBonusLabel != null)
			{
				_weaponBonusLabel.text = $"WeaponBonus: {current}";
			}
		}

		private void OnTotalDamageChanged(int previous, int current)
		{
			if (_totalDamageLabel != null)
			{
				_totalDamageLabel.text = $"TotalDamage (computed): {current}";
			}
		}
	}
}
