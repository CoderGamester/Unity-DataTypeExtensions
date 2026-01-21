using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// UI Toolkit view that binds to an <see cref="ObservableField{T}"/> health value.
	/// </summary>
	public sealed class ReactiveToolkitHealthBar : IDisposable
	{
		private readonly ProgressBar _healthBar;
		private readonly Label _healthLabel;
		private readonly int _maxHealth;

		private IObservableFieldReader<int> _health;

		public ReactiveToolkitHealthBar(ProgressBar healthBar, Label healthLabel, int maxHealth)
		{
			_healthBar = healthBar;
			_healthLabel = healthLabel;
			_maxHealth = Mathf.Max(1, maxHealth);
		}

		public void Bind(IObservableFieldReader<int> health)
		{
			_health?.StopObservingAll(this);
			_health = health;
			_health.InvokeObserve(OnHealthChanged);
		}

		public void Dispose()
		{
			_health?.StopObservingAll(this);
			_health = null;
		}

		private void OnHealthChanged(int previous, int current)
		{
			if (_healthBar != null)
			{
				_healthBar.lowValue = 0;
				_healthBar.highValue = _maxHealth;
				_healthBar.value = Mathf.Clamp(current, 0, _maxHealth);
				_healthBar.title = string.Empty;
			}

			if (_healthLabel != null)
			{
				_healthLabel.text = $"Health: {current}/{_maxHealth}";
			}
		}
	}
}
