using System;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// UI Toolkit view that displays computed stats (BaseDamage, WeaponBonus, TotalDamage).
	/// </summary>
	public sealed class ReactiveToolkitStatsPanel : IDisposable
	{
		private readonly Label _baseDamageLabel;
		private readonly Label _weaponBonusLabel;
		private readonly Label _totalDamageLabel;

		private IObservableFieldReader<int> _baseDamage;
		private IObservableFieldReader<int> _weaponBonus;
		private IObservableFieldReader<int> _totalDamage;

		public ReactiveToolkitStatsPanel(Label baseDamageLabel, Label weaponBonusLabel, Label totalDamageLabel)
		{
			_baseDamageLabel = baseDamageLabel;
			_weaponBonusLabel = weaponBonusLabel;
			_totalDamageLabel = totalDamageLabel;
		}

		public void Bind(IObservableFieldReader<int> baseDamage, IObservableFieldReader<int> weaponBonus, IObservableFieldReader<int> totalDamage)
		{
			Unbind();

			_baseDamage = baseDamage;
			_weaponBonus = weaponBonus;
			_totalDamage = totalDamage;

			_baseDamage?.InvokeObserve(OnBaseDamageChanged);
			_weaponBonus?.InvokeObserve(OnWeaponBonusChanged);
			_totalDamage?.InvokeObserve(OnTotalDamageChanged);
		}

		public void Dispose()
		{
			Unbind();
		}

		private void Unbind()
		{
			_baseDamage?.StopObservingAll(this);
			_weaponBonus?.StopObservingAll(this);
			_totalDamage?.StopObservingAll(this);

			_baseDamage = null;
			_weaponBonus = null;
			_totalDamage = null;
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
