using System;

namespace GameLovers.GameData.Samples.ValidationAndMigration
{
	/// <summary>
	/// Config used to demonstrate editor-time validation via <see cref="GameLoversEditor.GameData.EditorConfigValidator"/>.
	/// </summary>
	[Serializable]
	public struct SamplePlayerConfig
	{
		public int Id;

		[Required]
		public string Name;

		[Range(1, 1000)]
		public int MaxHealth;

		[MinLength(3)]
		public string Description;
	}
}
