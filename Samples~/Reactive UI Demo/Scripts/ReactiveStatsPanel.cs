using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// UI Toolkit view that binds to computed stats.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ReactiveStatsPanel : MonoBehaviour
	{
		private const string RootName = "gamedata-reactiveui-root";
		private const string DamageLabelName = "damageLabel";
		private const string BaseDamageLabelName = "baseDamageLabel";
		private const string WeaponBonusLabelName = "weaponBonusLabel";

		private UIDocument _document;

		private ObservableField<int> _baseDamage;
		private ObservableField<int> _weaponBonus;
		private ComputedField<int> _totalDamage;

		private Label _damageLabel;
		private Label _baseDamageLabel;
		private Label _weaponBonusLabel;

		private Action<int, int> _onBaseChanged;
		private Action<int, int> _onBonusChanged;
		private Action<int, int> _onTotalChanged;

		public void Bind(ObservableField<int> baseDamage, ObservableField<int> weaponBonus, ComputedField<int> totalDamage)
		{
			_baseDamage = baseDamage;
			_weaponBonus = weaponBonus;
			_totalDamage = totalDamage;

			_document = _document ?? GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
			EnsurePanelSettings(_document);

			BuildUiIfNeeded(_document);
			CacheElements(_document);

			_onBaseChanged = (_, __) => UpdateLabels();
			_onBonusChanged = (_, __) => UpdateLabels();
			_onTotalChanged = (_, __) => UpdateLabels();

			_baseDamage.InvokeObserve(_onBaseChanged);
			_weaponBonus.InvokeObserve(_onBonusChanged);
			_totalDamage.InvokeObserve(_onTotalChanged);
		}

		private void OnDestroy()
		{
			if (_onBaseChanged != null)
			{
				_baseDamage?.StopObserving(_onBaseChanged);
			}

			if (_onBonusChanged != null)
			{
				_weaponBonus?.StopObserving(_onBonusChanged);
			}

			if (_onTotalChanged != null)
			{
				_totalDamage?.StopObserving(_onTotalChanged);
			}
		}

		private static void EnsurePanelSettings(UIDocument document)
		{
			// A UIDocument needs PanelSettings. If none is set (common in a hand-authored scene),
			// create a runtime PanelSettings instance.
			if (document.panelSettings != null)
			{
				return;
			}

			var settings = ScriptableObject.CreateInstance<PanelSettings>();
			settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
			settings.referenceResolution = new Vector2Int(1920, 1080);
			document.panelSettings = settings;
		}

		private static void BuildUiIfNeeded(UIDocument document)
		{
			var root = document.rootVisualElement;
			if (root == null)
			{
				return;
			}

			if (root.Q<VisualElement>(RootName) != null)
			{
				return;
			}

			var container = new VisualElement { name = RootName };
			container.style.position = Position.Absolute;
			container.style.left = 16;
			container.style.top = 16;
			container.style.paddingLeft = 10;
			container.style.paddingRight = 10;
			container.style.paddingTop = 10;
			container.style.paddingBottom = 10;
			container.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
			container.style.borderBottomLeftRadius = 6;
			container.style.borderBottomRightRadius = 6;
			container.style.borderTopLeftRadius = 6;
			container.style.borderTopRightRadius = 6;

			container.Add(new Label("UI Toolkit Stats") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
			container.Add(new Label { name = BaseDamageLabelName });
			container.Add(new Label { name = WeaponBonusLabelName });
			container.Add(new Label { name = DamageLabelName });

			root.Add(container);
		}

		private void CacheElements(UIDocument document)
		{
			var root = document.rootVisualElement;
			var container = root?.Q<VisualElement>(RootName);
			if (container == null)
			{
				return;
			}

			_baseDamageLabel = container.Q<Label>(BaseDamageLabelName);
			_weaponBonusLabel = container.Q<Label>(WeaponBonusLabelName);
			_damageLabel = container.Q<Label>(DamageLabelName);

			UpdateLabels();
		}

		private void UpdateLabels()
		{
			if (_baseDamageLabel != null)
			{
				_baseDamageLabel.text = $"BaseDamage: {_baseDamage.Value}";
			}

			if (_weaponBonusLabel != null)
			{
				_weaponBonusLabel.text = $"WeaponBonus: {_weaponBonus.Value}";
			}

			if (_damageLabel != null)
			{
				_damageLabel.text = $"TotalDamage (computed): {_totalDamage.Value}";
			}
		}
	}
}

