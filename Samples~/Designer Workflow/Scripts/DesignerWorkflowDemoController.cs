using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// Entry point MonoBehaviour for the Designer Workflow sample.
	/// Loads ScriptableObject configs and displays them at runtime, with a reload button.
	/// </summary>
	public sealed class DesignerWorkflowDemoController : MonoBehaviour
	{
		private readonly List<GameObject> _createdObjects = new List<GameObject>();

		private ConfigLoader _loader;
		private ConfigDisplayUI _display;

		private void Awake()
		{
			_loader = new ConfigLoader();

			EnsureEventSystem();
			var canvas = EnsureCanvas();

			var displayGo = new GameObject("ConfigDisplayUI");
			_createdObjects.Add(displayGo);
			_display = displayGo.AddComponent<ConfigDisplayUI>();

			ReloadAndRender();

			BuildActionsUi(canvas.transform);
		}

		private void OnDestroy()
		{
			for (var i = 0; i < _createdObjects.Count; i++)
			{
				if (_createdObjects[i] != null)
				{
					Destroy(_createdObjects[i]);
				}
			}
			_createdObjects.Clear();
		}

		private void ReloadAndRender()
		{
			var data = _loader.Load();
			_display.Render(data);
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

		private Canvas EnsureCanvas()
		{
			var existing = FindObjectOfType<Canvas>();
			if (existing != null)
			{
				return existing;
			}

			var canvasGo = new GameObject("Designer Workflow Canvas");
			_createdObjects.Add(canvasGo);

			var canvas = canvasGo.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvasGo.AddComponent<CanvasScaler>();
			canvasGo.AddComponent<GraphicRaycaster>();

			return canvas;
		}

		private void BuildActionsUi(Transform parent)
		{
			var panel = CreatePanel(parent, "ActionsPanel", new Vector2(-12, -12), new Vector2(240, 80));
			CreateLabel(panel.transform, "Title", "Actions");
			CreateButton(panel.transform, "ReloadBtn", "Reload Configs", ReloadAndRender);
		}

		private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
		{
			var root = new GameObject(name);
			root.transform.SetParent(parent, false);

			var rt = root.AddComponent<RectTransform>();
			rt.anchorMin = new Vector2(1, 1);
			rt.anchorMax = new Vector2(1, 1);
			rt.pivot = new Vector2(1, 1);
			rt.anchoredPosition = anchoredPos;
			rt.sizeDelta = size;

			var img = root.AddComponent<Image>();
			img.color = new Color(0f, 0f, 0f, 0.55f);

			var v = root.AddComponent<VerticalLayoutGroup>();
			v.padding = new RectOffset(10, 10, 10, 10);
			v.spacing = 6;
			v.childControlHeight = true;
			v.childControlWidth = true;
			v.childForceExpandHeight = false;
			v.childForceExpandWidth = true;

			return root;
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
			label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			return label;
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
	}
}

