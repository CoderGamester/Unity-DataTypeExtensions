using System;
using System.Collections.Generic;
using GameLovers.GameData;
using NUnit.Framework;

namespace GameLovers.GameData.Tests.Smoke
{
	[TestFixture]
	public class EditModeSmokeTest
	{
		[Test]
		public void Configs_SmokeTest()
		{
			var provider = new ConfigsProvider();
			provider.AddSingletonConfig(new int[] { 1, 2, 3 });
			Assert.AreEqual(3, provider.GetConfig<int[]>().Length);
		}

		[Test]
		public void Observables_SmokeTest()
		{
			var field = new ObservableField<int>(10);
			var notified = false;
			field.Observe((p, c) => notified = true);
			field.Value = 20;
			Assert.IsTrue(notified);
		}

		[Test]
		public void Math_SmokeTest()
		{
			var a = (floatP)1.5f;
			var b = (floatP)2.5f;
			Assert.AreEqual((floatP)4.0f, a + b);
			Assert.AreEqual((floatP)1.0f, MathfloatP.Abs((floatP)(-1.0f)));
		}
	}
}
