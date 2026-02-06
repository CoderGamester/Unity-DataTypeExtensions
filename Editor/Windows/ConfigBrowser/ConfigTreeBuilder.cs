using System;
using System.Collections.Generic;
using System.Linq;
using GameLovers.GameData;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Builds the hierarchical <see cref="TreeViewItemData{T}"/> structure used by the
	/// Config Browser tree view. Groups configs into "Singletons" and "Collections" headers
	/// and supports text-based search filtering by type name or entry id.
	/// </summary>
	internal static class ConfigTreeBuilder
	{
		private const int SingleConfigId = 0;

		/// <summary>
		/// Builds a list of root <see cref="TreeViewItemData{T}"/> items from the given
		/// <paramref name="provider"/>. When <paramref name="search"/> is non-empty, only entries
		/// whose type name or id matches the search term are included.
		/// </summary>
		public static IList<TreeViewItemData<ConfigNode>> BuildTreeItems(IConfigsProvider provider, string search)
		{
			var id = 1;
			var childrenSingletons = new List<TreeViewItemData<ConfigNode>>();
			var childrenCollections = new List<TreeViewItemData<ConfigNode>>();

			var hasSearch = !string.IsNullOrWhiteSpace(search);
			var searchLower = hasSearch ? search.Trim().ToLowerInvariant() : string.Empty;

			if (provider == null)
			{
				return new List<TreeViewItemData<ConfigNode>>
				{
					new TreeViewItemData<ConfigNode>(id++, ConfigNode.Header("No providers available.\nEnter Play Mode to create a ConfigsProvider."))
				};
			}

			var allConfigs = provider.GetAllConfigs();
			foreach (var kv in allConfigs.OrderBy(k => k.Key.Name))
			{
				var type = kv.Key;
				var container = kv.Value;

				if (!ConfigsEditorUtil.TryReadConfigs(container, out var entries))
				{
					continue;
				}

				var isSingleton = entries.Count == 1 && entries[0].Id == SingleConfigId;
				var typeMatches = !hasSearch || type.Name.ToLowerInvariant().Contains(searchLower);

				var entryNodes = new List<TreeViewItemData<ConfigNode>>();
				for (int i = 0; i < entries.Count; i++)
				{
					var entry = entries[i];
					var idStr = isSingleton ? "singleton" : entry.Id.ToString();
					var label = $"{idStr}: {type.Name}";

					if (!typeMatches && hasSearch)
					{
						// Allow searching by id.
						if (!idStr.Contains(searchLower))
						{
							continue;
						}
					}

					entryNodes.Add(new TreeViewItemData<ConfigNode>(id++, ConfigNode.Entry(type, entry.Id, entry.Value, label)));
				}

				if (entryNodes.Count == 0)
				{
					continue;
				}

				var typeNode = new TreeViewItemData<ConfigNode>(id++, ConfigNode.Type(type, $"{type.Name} ({entryNodes.Count})"), entryNodes);
				if (isSingleton)
				{
					childrenSingletons.Add(typeNode);
				}
				else
				{
					childrenCollections.Add(typeNode);
				}
			}

			var roots = new List<TreeViewItemData<ConfigNode>>();
			roots.Add(new TreeViewItemData<ConfigNode>(id++, ConfigNode.Header("Singletons"), childrenSingletons));
			roots.Add(new TreeViewItemData<ConfigNode>(id++, ConfigNode.Header("Collections"), childrenCollections));
			return roots;
		}

		/// <summary>
		/// Searches the given <paramref name="roots"/> tree for an entry node matching the specified
		/// config <paramref name="type"/> and <paramref name="id"/>. Returns the tree item id
		/// if found, or null if no matching entry exists.
		/// </summary>
		public static int? FindTreeItemIdForEntry(IList<TreeViewItemData<ConfigNode>> roots, Type type, int id)
		{
			foreach (var root in roots)
			{
				if (TryFind(root, out var found))
				{
					return found;
				}
			}
			return null;

			bool TryFind(TreeViewItemData<ConfigNode> node, out int foundId)
			{
				if (node.data.Kind == ConfigNodeKind.Entry && node.data.ConfigType == type && node.data.ConfigId == id)
				{
					foundId = node.id;
					return true;
				}

				if (node.hasChildren)
				{
					foreach (var child in node.children)
					{
						if (TryFind(child, out foundId))
						{
							return true;
						}
					}
				}

				foundId = 0;
				return false;
			}
		}
	}
}
