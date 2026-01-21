using System;
using System.Collections.Generic;
using GameLovers.GameData;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace GameLovers.GameData.Tests.Performance
{
	[TestFixture]
	public class ObservablePerformanceTest
	{
		[Test, Performance]
		public void ObservableField_10kUpdates_Performance()
		{
			var field = new ObservableField<int>(0);
			field.Observe((p, c) => { });

			Measure.Method(() =>
			{
				for (int i = 0; i < 10000; i++)
				{
					field.Value = i;
				}
			}).Run();
		}

		[Test, Performance]
		public void ComputedField_DeepDependencyChain_Performance()
		{
			var root = new ObservableField<int>(0);
			var current = root.Select(x => x + 1);
			for (int i = 0; i < 50; i++)
			{
				current = current.Select(x => x + 1);
			}

			Measure.Method(() =>
			{
				for (int i = 0; i < 100; i++)
				{
					root.Value = i;
					var val = current.Value;
				}
			}).Run();
		}
	}
}
