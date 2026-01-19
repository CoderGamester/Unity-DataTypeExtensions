using UnityEngine;
using UnityEngine.UI;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// Entry point MonoBehaviour for the Designer Workflow sample.
	/// Loads ScriptableObject configs and displays them at runtime, with a reload button.
	/// </summary>
	public sealed class DesignerWorkflowDemoController : MonoBehaviour
	{
 #pragma warning disable CS0649 // Unity assigns via Inspector
		[SerializeField] private ConfigDisplayUI _display;
		[SerializeField] private Button _reloadButton;
 #pragma warning restore CS0649

		private ConfigLoader _loader;

		private void Awake()
		{
			_loader = new ConfigLoader();
		}

		private void Start()
		{
			if (_reloadButton != null)
			{
				_reloadButton.onClick.AddListener(ReloadAndRender);
			}

			ReloadAndRender();
		}

		private void ReloadAndRender()
		{
			var data = _loader.Load();
			_display?.Render(data);
		}
	}
}

