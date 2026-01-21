using System;
using System.Collections.Generic;
using GameLovers.GameData;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace GameLovers.GameData.Tests.Performance
{
	[TestFixture]
	public class ConfigsProviderPerformanceTest
	{
		[Serializable]
		public struct PerformanceConfig
		{
			public int Id;
			public string Data;
		}

		private ConfigsProvider _provider;

		[SetUp]
		public void Setup()
		{
			_provider = new ConfigsProvider();
			var list = new List<PerformanceConfig>();
			for (int i = 0; i < 1000; i++)
			{
				list.Add(new PerformanceConfig { Id = i, Data = "Value" + i });
			}
			_provider.AddConfigs(c => c.Id, list);
		}

		[Test, Performance]
		public void GetConfig_100kCalls_Performance()
		{
			Measure.Method(() =>
			{
				for (int i = 0; i < 100000; i++)
				{
					var cfg = _provider.GetConfig<PerformanceConfig>(i % 1000);
				}
			}).Run();
		}

		[Test, Performance]
		public void AddConfigs_10kItems_Performance()
		{
			var list = new List<PerformanceConfig>();
			for (int i = 0; i < 10000; i++)
			{
				list.Add(new PerformanceConfig { Id = i, Data = "Value" + i });
			}

			Measure.Method(() =>
			{
				var localProvider = new ConfigsProvider();
				localProvider.AddConfigs(c => c.Id, list);
			}).Run();
		}
	}
}
