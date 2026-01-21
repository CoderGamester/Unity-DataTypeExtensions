using System;
using GameLovers.GameData;
using NUnit.Framework;

namespace GameLovers.GameData.Tests.Boundary
{
	[TestFixture]
	public class floatPBoundaryTest
	{
		[Test]
		public void MaxValue_Operations()
		{
			var max = floatP.MaxValue;
			// Adding 1 to MaxValue doesn't overflow to infinity due to float precision limits
			// (the mantissa can't represent the difference). Multiplying by 2 does cause overflow.
			Assert.IsTrue((max * (floatP)2f).IsInfinity());
			Assert.AreEqual(max, max * floatP.One);
		}

		[Test]
		public void MinValue_Operations()
		{
			var min = floatP.MinValue;
			// Subtracting 1 from MinValue doesn't overflow to infinity due to float precision limits.
			// Multiplying by 2 does cause overflow.
			Assert.IsTrue((min * (floatP)2f).IsInfinity());
			Assert.AreEqual(min, min * floatP.One);
		}

		[Test]
		public void NaN_Propagation_AllOperations()
		{
			var nan = floatP.NaN;
			Assert.IsTrue((nan + floatP.One).IsNaN());
			Assert.IsTrue((nan * floatP.Zero).IsNaN());
			Assert.IsTrue(MathfloatP.Sin(nan).IsNaN());
		}

		[Test]
		public void Zero_NegativeZero_Equality()
		{
			var zero = floatP.Zero;
			var negZero = -floatP.Zero;
			Assert.IsTrue(zero == negZero);
			Assert.IsFalse(zero.RawValue == negZero.RawValue); // They differ in sign bit
		}
	}
}
