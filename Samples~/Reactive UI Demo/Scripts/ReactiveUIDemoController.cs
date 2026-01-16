using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.ReactiveUiDemo
{
	/// <summary>
	/// Entry point MonoBehaviour for the Reactive UI Demo sample.
	/// </summary>
	public sealed class ReactiveUIDemoController : MonoBehaviour
	{
		private PlayerData _data;
		private readonly List<GameObject> _createdObjects = new List<GameObject>();

		private void Awake()
		{
			_data = new PlayerData();

			EnsureEventSystem();
			var canvas = CreateCanvas();

			BuildUgUi(canvas.transform);
			BuildUiToolkit();
		}

		private void OnDestroy()
		{
			_data?.Dispose();

			// We created everything at runtime to keep the scene YAML small and robust.
			// Clean up only what we created.
			for (var i = 0; i < _createdObjects.Count; i++)
			{
				if (_createdObjects[i] != null)
				{
					Destroy(_createdObjects[i]);
				}
			}
			_createdObjects.Clear();
		}

		private void EnsureEventSystem()
		{
			if (FindObjectOfType<EventSystem>() != null)
			{
				return;
			}

			var go = new GameObject("EventSystem");
			_createdObjects.Add(go);
			go.AddComponent<EventSystem>();
			go.AddComponent<StandaloneInputModule>();
		}

		private Canvas CreateCanvas()
		{
			var existing = FindObjectOfType<Canvas>();
			if (existing != null)
			{
				return existing;
			}

			var go = new GameObject("uGUI Canvas");
			_createdObjects.Add(go);

			var canvas = go.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			go.AddComponent<CanvasScaler>();
			go.AddComponent<GraphicRaycaster>();

			return canvas;
		}

		private void BuildUgUi(Transform parent)
		{
			var root = CreatePanel(parent, "uGUI Controls", new Vector2(12, -12), new Vector2(320, 520));

			// Health
			var healthView = CreateChild(root, "HealthView");
			var healthSlider = CreateSlider(healthView.transform, "HealthSlider");
			var healthLabel = CreateLabel(healthView.transform, "HealthLabel", "Health: --/--");
			var healthBar = healthView.AddComponent<ReactiveHealthBar>();
			healthBar.Setup(healthSlider, healthLabel);
			healthBar.Bind(_data.Health, 100);
			healthBar.GetType(); // keep reference for serialization-stripping warnings

			// Inventory
			CreateSpacer(root.transform, 8);
			CreateLabel(root.transform, "InventoryTitle", "Inventory (ObservableList)");

			var inventoryRoot = CreateChild(root, "InventoryRoot");
			var inventoryRt = inventoryRoot.AddComponent<RectTransform>();
			inventoryRt.sizeDelta = new Vector2(0, 140);
			var vlg = inventoryRoot.AddComponent<VerticalLayoutGroup>();
			vlg.childControlHeight = true;
			vlg.childForceExpandHeight = false;
			vlg.childForceExpandWidth = true;
			vlg.spacing = 2;

			var itemPrefab = CreateLabel(inventoryRoot.transform, "ItemPrefab", "- Item");
			itemPrefab.gameObject.SetActive(false);

			var inventoryView = inventoryRoot.AddComponent<ReactiveInventoryList>();
			inventoryView.Setup(inventoryRoot.GetComponent<RectTransform>(), itemPrefab);
			inventoryView.Bind(_data.Inventory);

			// Buttons
			CreateSpacer(root.transform, 10);
			CreateLabel(root.transform, "ActionsTitle", "Actions");

			CreateButton(root.transform, "DamageBtn", "Take 10 Damage", () => _data.Health.Value = Mathf.Max(0, _data.Health.Value - 10));
			CreateButton(root.transform, "HealBtn", "Heal 10", () => _data.Health.Value = Mathf.Min(100, _data.Health.Value + 10));

			CreateButton(root.transform, "WeaponBonusBtn", "Weapon Bonus +1", () => _data.WeaponBonus.Value += 1);
			CreateButton(root.transform, "BaseDamageBtn", "Base Damage +1", () => _data.BaseDamage.Value += 1);

			CreateSpacer(root.transform, 6);
			CreateButton(root.transform, "AddItemBtn", "Add Item", () => _data.Inventory.Add($"Item_{_data.Inventory.Count + 1}"));
			CreateButton(root.transform, "RemoveItemBtn", "Remove Last Item", () =>
			{
				if (_data.Inventory.Count > 0)
				{
					_data.Inventory.RemoveAt(_data.Inventory.Count - 1);
				}
			});

			CreateSpacer(root.transform, 6);
			CreateButton(root.transform, "BatchBtn", "Batch Update (Atomic)", ApplyBatchUpdate);

			// Seed inventory
			_data.Inventory.Add("Sword");
			_data.Inventory.Add("Potion");
		}

		private void BuildUiToolkit()
		{
			var go = new GameObject("UI Toolkit Stats");
			_createdObjects.Add(go);
			var panel = go.AddComponent<ReactiveStatsPanel>();
			panel.Bind(_data.BaseDamage, _data.WeaponBonus, _data.TotalDamage);
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

		private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
		{
			var root = new GameObject(name);
			root.transform.SetParent(parent, false);

			var rt = root.AddComponent<RectTransform>();
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(0, 1);
			rt.pivot = new Vector2(0, 1);
			rt.anchoredPosition = anchoredPos;
			rt.sizeDelta = size;

			var img = root.AddComponent<Image>();
			img.color = new Color(0f, 0f, 0f, 0.55f);

			var v = root.AddComponent<VerticalLayoutGroup>();
			v.padding = new RectOffset(10, 10, 10, 10);
			v.spacing = 4;
			v.childControlHeight = true;
			v.childControlWidth = true;
			v.childForceExpandHeight = false;
			v.childForceExpandWidth = true;

			return root;
		}

		private static void CreateSpacer(Transform parent, float height)
		{
			var spacer = new GameObject("Spacer");
			spacer.transform.SetParent(parent, false);
			var rt = spacer.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, height);
		}

		private static GameObject CreateChild(GameObject parent, string name)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent.transform, false);
			go.AddComponent<RectTransform>();
			return go;
		}

		private static Text CreateLabel(Transform parent, string name, string text)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);
			var rt = go.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, 22);

			var label = go.AddComponent<Text>();
			label.text = text;
			label.color = Color.white;
			label.fontSize = 14;
			label.alignment = TextAnchor.MiddleLeft;

			// Use built-in Arial font (always available).
			label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			return label;
		}

		private static Slider CreateSlider(Transform parent, string name)
		{
			// Minimal uGUI slider that renders without relying on Unity's built-in UI prefabs.
			var root = new GameObject(name);
			root.transform.SetParent(parent, false);

			var rt = root.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, 18);

			var background = root.AddComponent<Image>();
			background.color = new Color(1f, 1f, 1f, 0.18f);

			var fillArea = new GameObject("Fill Area");
			fillArea.transform.SetParent(root.transform, false);
			var fillAreaRt = fillArea.AddComponent<RectTransform>();
			fillAreaRt.anchorMin = new Vector2(0, 0);
			fillAreaRt.anchorMax = new Vector2(1, 1);
			fillAreaRt.offsetMin = new Vector2(6, 4);
			fillAreaRt.offsetMax = new Vector2(-6, -4);

			var fill = new GameObject("Fill");
			fill.transform.SetParent(fillArea.transform, false);
			var fillRt = fill.AddComponent<RectTransform>();
			fillRt.anchorMin = new Vector2(0, 0);
			fillRt.anchorMax = new Vector2(1, 1);
			fillRt.offsetMin = Vector2.zero;
			fillRt.offsetMax = Vector2.zero;

			var fillImg = fill.AddComponent<Image>();
			fillImg.color = new Color(0.25f, 0.8f, 0.35f, 0.95f);

			var handleSlideArea = new GameObject("Handle Slide Area");
			handleSlideArea.transform.SetParent(root.transform, false);
			var handleAreaRt = handleSlideArea.AddComponent<RectTransform>();
			handleAreaRt.anchorMin = new Vector2(0, 0);
			handleAreaRt.anchorMax = new Vector2(1, 1);
			handleAreaRt.offsetMin = new Vector2(6, 4);
			handleAreaRt.offsetMax = new Vector2(-6, -4);

			var handle = new GameObject("Handle");
			handle.transform.SetParent(handleSlideArea.transform, false);
			var handleRt = handle.AddComponent<RectTransform>();
			handleRt.sizeDelta = new Vector2(12, 12);

			var handleImg = handle.AddComponent<Image>();
			handleImg.color = new Color(1f, 1f, 1f, 0.9f);

			var slider = root.AddComponent<Slider>();
			slider.minValue = 0f;
			slider.maxValue = 1f;
			slider.value = 1f;
			slider.direction = Slider.Direction.LeftToRight;
			slider.targetGraphic = handleImg;
			slider.fillRect = fillRt;
			slider.handleRect = handleRt;

			return slider;
		}

		private static Button CreateButton(Transform parent, string name, string label, Action onClick)
		{
			var go = new GameObject(name);
			go.transform.SetParent(parent, false);

			var rt = go.AddComponent<RectTransform>();
			rt.sizeDelta = new Vector2(0, 28);

			var img = go.AddComponent<Image>();
			img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

			var button = go.AddComponent<Button>();
			button.targetGraphic = img;
			button.onClick.AddListener(() => onClick?.Invoke());

			var text = CreateLabel(go.transform, "Label", label);
			text.alignment = TextAnchor.MiddleCenter;
			text.rectTransform.anchorMin = Vector2.zero;
			text.rectTransform.anchorMax = Vector2.one;
			text.rectTransform.offsetMin = Vector2.zero;
			text.rectTransform.offsetMax = Vector2.zero;

			return button;
		}

		// No reflection needed: views expose Setup(...) APIs for runtime-built UI.
	}
}

