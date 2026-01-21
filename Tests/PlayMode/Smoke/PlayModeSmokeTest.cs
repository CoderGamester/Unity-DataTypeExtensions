using System.Collections;
using GameLovers.GameData;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameLovers.GameData.Tests.PlayMode.Smoke
{
	[TestFixture]
	public class PlayModeSmokeTest
	{
		[UnityTest]
		public IEnumerator ObservableField_UpdatesDuringPlayMode()
		{
			var field = new ObservableField<int>(10);
			var val = 0;
			field.Observe((p, c) => val = c);
			
			yield return null; // Wait for one frame
			
			field.Value = 20;
			Assert.AreEqual(20, val);
		}

		[UnityTest]
		public IEnumerator ComputedField_UpdatesDuringPlayMode()
		{
			var field = new ObservableField<int>(10);
			var computed = field.Select(x => x * 2);
			
			yield return null;
			
			field.Value = 20;
			Assert.AreEqual(40, computed.Value);
		}
	}
}
