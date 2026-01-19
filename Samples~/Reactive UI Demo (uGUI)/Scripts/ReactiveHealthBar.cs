using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// uGUI view that binds to an <see cref="ObservableField{T}"/> health value.
	/// </summary>
	public sealed class ReactiveHealthBar : MonoBehaviour
	{
		[SerializeField] private Slider _slider;
		[SerializeField] private TMP_Text _label;
		[SerializeField] private int _maxHealth = 100;

		private IObservableFieldReader<int> _health;

		public void Bind(IObservableFieldReader<int> health, int maxHealth)
		{
			_health = health;
			_maxHealth = Mathf.Max(1, maxHealth);

			_health.InvokeObserve(OnHealthChanged);
		}

		private void OnDestroy()
		{
			_health?.StopObservingAll(this);
		}

		private void OnHealthChanged(int previous, int current)
		{
			if (_slider != null)
			{
				_slider.value = Mathf.Clamp01(current / (float)_maxHealth);
			}

			if (_label != null)
			{
				_label.text = $"Health: {current}/{_maxHealth}";
			}
		}
	}
}

