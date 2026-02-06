using System;
using System.Collections;
using System.Collections.Generic;
using GameLovers.GameData;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Lightweight pair holding a config id and its boxed value.
	/// Used by editor utilities that iterate provider containers via reflection.
	/// </summary>
	internal readonly struct ConfigEntry
	{
		public readonly int Id;
		public readonly object Value;

		public ConfigEntry(int id, object value)
		{
			Id = id;
			Value = value;
		}
	}

	/// <summary>
	/// Editor-only utility methods for reading and summarizing config data from
	/// <see cref="IConfigsProvider"/> containers without compile-time knowledge of the config types.
	/// </summary>
	internal static class ConfigsEditorUtil
	{
		/// <summary>
		/// Attempts to read all entries from a <c>Dictionary&lt;int, T&gt;</c> <paramref name="container"/>
		/// using reflection. Returns true if the container was a valid int-keyed dictionary,
		/// populating <paramref name="entries"/> sorted by id.
		/// </summary>
		public static bool TryReadConfigs(IEnumerable container, out List<ConfigEntry> entries)
		{
			entries = new List<ConfigEntry>();
			if (container == null) return false;

			// ConfigsProvider stores Dictionary<int, T> for both singleton and collections.
			// Use reflection to iterate entries regardless of T.
			var type = container.GetType();
			if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
			{
				return false;
			}

			var keyType = type.GetGenericArguments()[0];
			if (keyType != typeof(int))
			{
				return false;
			}

			foreach (var item in container)
			{
				var itemType = item.GetType();
				var keyProp = itemType.GetProperty("Key");
				var valueProp = itemType.GetProperty("Value");
				if (keyProp == null || valueProp == null) continue;

				var id = (int)keyProp.GetValue(item);
				var value = valueProp.GetValue(item);
				entries.Add(new ConfigEntry(id, value));
			}

			entries.Sort((a, b) => a.Id.CompareTo(b.Id));
			return true;
		}

		/// <summary>
		/// Computes summary counts for the given <paramref name="provider"/>: the number of registered
		/// config types and the total number of individual config entries across all types.
		/// </summary>
		public static (int typeCount, int totalCount) ComputeConfigCounts(IConfigsProvider provider)
		{
			var allConfigs = provider.GetAllConfigs();
			var typeCount = allConfigs.Count;
			var totalCount = 0;

			foreach (var kv in allConfigs)
			{
				if (TryReadConfigs(kv.Value, out var entries))
				{
					totalCount += entries.Count;
				}
			}

			return (typeCount, totalCount);
		}
	}
}
