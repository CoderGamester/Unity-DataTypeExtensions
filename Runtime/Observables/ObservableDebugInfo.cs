using System;

namespace GameLovers.GameData
{
	// ═══════════════════════════════════════════════════════════════════════════
	// EDITOR-ONLY: Observable Debug Window Support
	// ═══════════════════════════════════════════════════════════════════════════
	// Lightweight metadata attached to observable instances for the Observable
	// Debug Window. This is intentionally editor-only and compiled out in builds.
	// ═══════════════════════════════════════════════════════════════════════════
	/// <summary>
	/// Lightweight metadata attached to observable instances for the Observable Debug Window.
	/// This struct is editor-only and compiled out in player builds.
	/// </summary>
	public readonly struct ObservableDebugInfo
	{
#if UNITY_EDITOR
		public readonly int Id;
		public readonly string Name;
		public readonly string Kind;
		public readonly DateTime CreatedAt;

		public ObservableDebugInfo(int id, string name, string kind, DateTime createdAt)
		{
			Id = id;
			Name = name;
			Kind = kind;
			CreatedAt = createdAt;
		}
#endif
	}
}

