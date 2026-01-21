using System;
using GameLovers.GameData;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace GameLovers.GameData.Tests.Performance
{
	[TestFixture]
	public class floatPPerformanceTest
	{
		[Test, Performance]
		public void Arithmetic_1MillionOps_Performance()
		{
			var a = (floatP)1.23f;
			var b = (floatP)4.56f;

			Measure.Method(() =>
			{
				for (int i = 0; i < 1000000; i++)
				{
					var res = a * b + a / b - a % b;
				}
			}).Run();
		}

		[Test, Performance]
		public void MathfloatP_SinCos_10kCalls_Performance()
		{
			var a = (floatP)1.23f;

			Measure.Method(() =>
			{
				for (int i = 0; i < 10000; i++)
				{
					var res = MathfloatP.Sin(a) + MathfloatP.Cos(a);
				}
			}).Run();
		}
	}
}
