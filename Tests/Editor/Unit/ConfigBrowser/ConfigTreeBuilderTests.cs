using System.Collections.Generic;
using System.Linq;
using GameLovers.GameData;
using GameLovers.GameData.Editor;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace GameLovers.GameData.Tests
{
	/// <summary>
	/// Unit tests for <see cref="ConfigTreeBuilder"/>, verifying that the tree view hierarchy
	/// is correctly constructed from provider data and that search filtering works as expected.
	/// </summary>
	[TestFixture]
	public class ConfigTreeBuilderTests
	{
		[Test]
		public void BuildTreeItems_WithSingletonAndCollection_BuildsRootsAndEntries()
		{
			var provider = BuildProvider();

			var roots = ConfigTreeBuilder.BuildTreeItems(provider, null);

			Assert.AreEqual(2, roots.Count);
			Assert.AreEqual(ConfigNodeKind.Header, roots[0].data.Kind);
			Assert.AreEqual("Singletons", roots[0].data.DisplayName);
			Assert.AreEqual(ConfigNodeKind.Header, roots[1].data.Kind);
			Assert.AreEqual("Collections", roots[1].data.DisplayName);

			Assert.AreEqual(1, roots[0].children.Count());
			Assert.AreEqual(1, roots[1].children.Count());

			var collectionTypeNode = roots[1].children.First();
			Assert.AreEqual(ConfigNodeKind.Type, collectionTypeNode.data.Kind);
			Assert.AreEqual(2, collectionTypeNode.children.Count());
		}

		[Test]
		public void BuildTreeItems_SearchByTypeName_FiltersOtherTypes()
		{
			var provider = BuildProvider();

			var roots = ConfigTreeBuilder.BuildTreeItems(provider, nameof(MockSingletonConfig));
			var entries = FlattenEntries(roots);

			Assert.IsTrue(entries.Any(e => e.ConfigType == typeof(MockSingletonConfig)));
			Assert.IsFalse(entries.Any(e => e.ConfigType == typeof(MockCollectionConfig)));
		}

		[Test]
		public void BuildTreeItems_SearchById_FiltersToMatchingEntry()
		{
			var provider = BuildProvider();

			var roots = ConfigTreeBuilder.BuildTreeItems(provider, "20");
			var entries = FlattenEntries(roots);

			Assert.AreEqual(1, entries.Count);
			Assert.AreEqual(typeof(MockCollectionConfig), entries[0].ConfigType);
			Assert.AreEqual(20, entries[0].ConfigId);
		}

		private static ConfigsProvider BuildProvider()
		{
			var provider = new ConfigsProvider();
			provider.AddSingletonConfig(new MockSingletonConfig { Value = 1 });
			provider.AddConfigs(c => c.Id, new List<MockCollectionConfig>
			{
				new MockCollectionConfig { Id = 10, Name = "Alpha" },
				new MockCollectionConfig { Id = 20, Name = "Beta" }
			});
			return provider;
		}

		private static List<ConfigNode> FlattenEntries(IList<TreeViewItemData<ConfigNode>> roots)
		{
			var results = new List<ConfigNode>();
			foreach (var root in roots)
			{
				Collect(root, results);
			}
			return results;

			static void Collect(TreeViewItemData<ConfigNode> node, List<ConfigNode> results)
			{
				if (node.data.Kind == ConfigNodeKind.Entry)
				{
					results.Add(node.data);
				}

				if (node.hasChildren)
				{
					foreach (var child in node.children)
					{
						Collect(child, results);
					}
				}
			}
		}
	}
}
