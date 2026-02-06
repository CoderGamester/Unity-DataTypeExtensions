using System.Collections.Generic;
using GameLovers.GameData;
using GameLovers.GameData.Editor;
using NUnit.Framework;

namespace GameLovers.GameData.Tests
{
	/// <summary>
	/// Unit tests for <see cref="ConfigsEditorUtil"/>, verifying that reflection-based
	/// config reading from <c>Dictionary&lt;int, T&gt;</c> containers works correctly
	/// and rejects unsupported container types.
	/// </summary>
	[TestFixture]
	public class ConfigsEditorUtilTests
	{
		[Test]
		public void TryReadConfigs_WithDictionaryIntKey_ReturnsSortedEntries()
		{
			var dict = new Dictionary<int, string>
			{
				{ 2, "B" },
				{ 1, "A" }
			};

			var success = ConfigsEditorUtil.TryReadConfigs(dict, out var entries);

			Assert.IsTrue(success);
			Assert.AreEqual(2, entries.Count);
			Assert.AreEqual(1, entries[0].Id);
			Assert.AreEqual("A", entries[0].Value);
			Assert.AreEqual(2, entries[1].Id);
			Assert.AreEqual("B", entries[1].Value);
		}

		[Test]
		public void TryReadConfigs_WithNonDictionary_ReturnsFalse()
		{
			var list = new List<int> { 1, 2, 3 };

			var success = ConfigsEditorUtil.TryReadConfigs(list, out var entries);

			Assert.IsFalse(success);
			Assert.AreEqual(0, entries.Count);
		}

		[Test]
		public void TryReadConfigs_WithNonIntKey_ReturnsFalse()
		{
			var dict = new Dictionary<string, int>
			{
				{ "one", 1 }
			};

			var success = ConfigsEditorUtil.TryReadConfigs(dict, out var entries);

			Assert.IsFalse(success);
			Assert.AreEqual(0, entries.Count);
		}
	}
}
