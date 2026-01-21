using System;
using System.Collections.Generic;
using GameLovers.GameData;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameLovers.GameData.Tests.PlayMode.Integration
{
	[TestFixture]
	public class ConfigsScriptableObjectIntegrationTest
	{
		[Serializable]
		public class MockHeroConfigSO : ConfigsScriptableObject<int, string> { }

		[Test]
		public void OnAfterDeserialize_BuildsDictionary()
		{
			var so = ScriptableObject.CreateInstance<MockHeroConfigSO>();
			so.Configs = new List<Pair<int, string>>
			{
				new Pair<int, string>(1, "Hero1"),
				new Pair<int, string>(2, "Hero2")
			};

			((ISerializationCallbackReceiver)so).OnAfterDeserialize();

			Assert.IsNotNull(so.ConfigsDictionary);
			Assert.AreEqual(2, so.ConfigsDictionary.Count);
			Assert.AreEqual("Hero1", so.ConfigsDictionary[1]);
			Assert.AreEqual("Hero2", so.ConfigsDictionary[2]);
		}

		[Test]
		public void OnAfterDeserialize_DuplicateKeys_LogsError()
		{
			var so = ScriptableObject.CreateInstance<MockHeroConfigSO>();
			so.Configs = new List<Pair<int, string>>
			{
				new Pair<int, string>(1, "First"),
				new Pair<int, string>(1, "Second")
			};

			LogAssert.Expect(LogType.Error, "Duplicate key '1' found in MockHeroConfigSO. Skipping.");
			((ISerializationCallbackReceiver)so).OnAfterDeserialize();

			Assert.AreEqual(1, so.ConfigsDictionary.Count);
			Assert.AreEqual("First", so.ConfigsDictionary[1]);
		}
	}
}
