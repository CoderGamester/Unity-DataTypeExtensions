using System.Collections.Generic;
using GameLovers.GameData;
using Newtonsoft.Json;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Stateless service that serializes config data from a <see cref="IConfigsProvider"/> to JSON.
	/// Used by the Config Browser for full-provider and per-entry export actions.
	/// </summary>
	internal static class ConfigExportService
	{
		/// <summary>
		/// Serializes all configs in the given <paramref name="provider"/> into a single indented JSON string.
		/// Each config type is keyed by its full name, containing a dictionary of id-to-value pairs.
		/// </summary>
		public static string ExportProviderToJson(IConfigsProvider provider)
		{
			var result = new Dictionary<string, object>();
			foreach (var kv in provider.GetAllConfigs())
			{
				if (ConfigsEditorUtil.TryReadConfigs(kv.Value, out var entries))
				{
					var dict = new Dictionary<int, object>();
					for (int i = 0; i < entries.Count; i++)
					{
						dict[entries[i].Id] = entries[i].Value;
					}
					result[kv.Key.FullName ?? kv.Key.Name] = dict;
				}
			}

			return JsonConvert.SerializeObject(result, Formatting.Indented);
		}

		/// <summary>
		/// Serializes a single object to an indented JSON string.
		/// Returns an empty string for null, or a comment with the error message on failure.
		/// </summary>
		public static string ToJson(object obj)
		{
			if (obj == null) return string.Empty;
			try
			{
				return JsonConvert.SerializeObject(obj, Formatting.Indented);
			}
			catch (System.Exception ex)
			{
				return $"// Failed to serialize {obj.GetType().Name}: {ex.Message}";
			}
		}
	}
}
