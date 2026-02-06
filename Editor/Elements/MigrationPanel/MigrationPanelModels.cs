using System;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Immutable data row representing a single config migration step displayed in the migration panel.
	/// Each row maps a <see cref="ConfigType"/> from <see cref="FromVersion"/> to <see cref="ToVersion"/>
	/// using the migration logic defined by <see cref="MigrationType"/>.
	/// </summary>
	internal readonly struct MigrationRow
	{
		public readonly Type ConfigType;
		public readonly ulong FromVersion;
		public readonly ulong ToVersion;
		public readonly Type MigrationType;
		public readonly MigrationState State;

		public MigrationRow(Type configType, ulong fromVersion, ulong toVersion, Type migrationType, MigrationState state)
		{
			ConfigType = configType;
			FromVersion = fromVersion;
			ToVersion = toVersion;
			MigrationType = migrationType;
			State = state;
		}
	}

	/// <summary>
	/// Describes the lifecycle state of a migration relative to the current provider version.
	/// </summary>
	internal enum MigrationState
	{
		Applied, // The migration has already been applied (current version is greater than or equal to ToVersion)
		Current, // The migration is the next one to apply (current version equals FromVersion)
		Pending // The migration is pending (current version is less than FromVersion)
	}
}
