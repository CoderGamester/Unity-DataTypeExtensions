using System;
using System.Collections.Generic;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// Simple data model used by the Reactive UI Demo (UI Toolkit) sample.
	/// </summary>
	[Serializable]
	public sealed class ReactiveToolkitPlayerData : IDisposable
	{
		public ObservableField<int> Health { get; } = new ObservableField<int>(100);
		public ObservableField<int> BaseDamage { get; } = new ObservableField<int>(10);
		public ObservableField<int> WeaponBonus { get; } = new ObservableField<int>(5);
		public ComputedField<int> TotalDamage { get; }

		public ObservableList<string> Inventory { get; } = new ObservableList<string>(new List<string>());

		public ReactiveToolkitPlayerData()
		{
			TotalDamage = new ComputedField<int>(() => BaseDamage.Value + WeaponBonus.Value);
		}

		public void Dispose()
		{
			TotalDamage.Dispose();
		}
	}
}

