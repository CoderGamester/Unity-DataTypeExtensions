using System;

namespace GameLoversEditor.GameData
{
	/// <summary>
	/// Marks a static method as a provider of preview data for migration previews.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This attribute is discovered via <see cref="UnityEditor.TypeCache"/> and used by the
	/// Config Browser's Migrations tab to provide realistic legacy-schema data for preview.
	/// </para>
	/// 
	/// <para><strong>Method Requirements:</strong></para>
	/// <list type="bullet">
	///   <item><description>Must be <c>static</c> (instance methods are not supported)</description></item>
	///   <item><description>Must return <see cref="Newtonsoft.Json.Linq.JObject"/></description></item>
	///   <item><description>Must take no parameters</description></item>
	/// </list>
	/// 
	/// <para><strong>Example usage:</strong></para>
	/// <code>
	/// #if UNITY_EDITOR
	/// [MigrationPreviewData(typeof(EnemyConfig))]
	/// #endif
	/// private static JObject GetEnemyPreviewData()
	/// {
	///     return new JObject
	///     {
	///         ["Id"] = 1,
	///         ["Name"] = "Goblin",
	///         ["Health"] = 50
	///     };
	/// }
	/// </code>
	/// 
	/// <para>
	/// The method itself can be reused elsewhere in your code (e.g., for in-scene migration demos),
	/// while the attribute enables automatic discovery by the Config Browser.
	/// </para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class MigrationPreviewDataAttribute : Attribute
	{
		/// <summary>
		/// The config type that this preview data represents.
		/// </summary>
		public Type ConfigType { get; }

		/// <summary>
		/// Creates a new MigrationPreviewDataAttribute for the specified config type.
		/// </summary>
		/// <param name="configType">The config type that the preview data represents.</param>
		public MigrationPreviewDataAttribute(Type configType)
		{
			ConfigType = configType ?? throw new ArgumentNullException(nameof(configType));
		}
	}
}
