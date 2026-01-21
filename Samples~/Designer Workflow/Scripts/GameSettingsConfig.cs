using System;

namespace GameLovers.GameData.Samples.DesignerWorkflow
{
	/// <summary>
	/// Sample singleton-style settings config (loaded via <see cref="ConfigsProvider.AddSingletonConfig{T}"/>).
	/// </summary>
	[Serializable]
	public struct GameSettingsConfig
	{
		public int Difficulty;
		public float MasterVolume;
	}
}

