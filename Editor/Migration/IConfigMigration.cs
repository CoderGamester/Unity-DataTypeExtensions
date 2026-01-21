using Newtonsoft.Json.Linq;

namespace GameLoversEditor.GameData
{
	/// <summary>
	/// Interface for configuration migrations.
	/// Migrations are run in the Editor when config schemas change between versions.
	/// </summary>
	public interface IConfigMigration
	{
		/// <summary>
		/// The version this migration moves from.
		/// </summary>
		ulong FromVersion { get; }
		
		/// <summary>
		/// The version this migration moves to.
		/// </summary>
		ulong ToVersion { get; }

		/// <summary>
		/// Migrates the given JSON object.
		/// </summary>
		void Migrate(JObject configJson);
	}
}
