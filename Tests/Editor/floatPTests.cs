using GameLovers.GameData;
using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.DataExtensions.Tests
{
	[TestFixture]
	public class floatPTests
	{
		[Test]
		public void Representation()
		{
			Assert.AreEqual(floatP.Zero, (floatP)0f);
			Assert.AreEqual(-floatP.Zero, (floatP) (-0f));
			Assert.AreEqual(floatP.Zero, -floatP.Zero);
			Assert.AreEqual(floatP.NaN, (floatP) float.NaN);
			Assert.AreEqual(floatP.MinusOne, (floatP)  (- 1f));
			Assert.AreEqual(floatP.PositiveInfinity, (floatP)float.PositiveInfinity);
			Assert.AreEqual(floatP.NegativeInfinity, (floatP)float.NegativeInfinity);
			Assert.AreEqual(floatP.Epsilon, (floatP)float.Epsilon);
			Assert.AreEqual(floatP.MaxValue, (floatP)float.MaxValue);
			Assert.AreEqual(floatP.MinValue, (floatP)float.MinValue);
		}

		[Test]
		public void Equality()
		{
			Assert.IsTrue(floatP.NaN != floatP.NaN);
			Assert.IsTrue(floatP.NaN.Equals(floatP.NaN));
			Assert.IsTrue(floatP.Zero == -floatP.Zero);
			Assert.IsTrue(floatP.Zero.Equals(-floatP.Zero));
			Assert.IsTrue(!(floatP.NaN > floatP.Zero));
			Assert.IsTrue(!(floatP.NaN >= floatP.Zero));
			Assert.IsTrue(!(floatP.NaN < floatP.Zero));
			Assert.IsTrue(!(floatP.NaN <= floatP.Zero));
			Assert.IsTrue(floatP.NaN.CompareTo(floatP.Zero) == -1);
			Assert.IsTrue(floatP.NaN.CompareTo(floatP.NegativeInfinity) == -1);
			Assert.IsTrue(!(-floatP.Zero < floatP.Zero));
		}

		[Test]
		public void Addition()
		{
			Assert.AreEqual(floatP.One + floatP.One, (floatP)2f);
			Assert.AreEqual(floatP.One - floatP.One, (floatP)0f);
		}

		[Test]
		public void Multiplication()
		{
			Assert.AreEqual(floatP.PositiveInfinity * floatP.Zero, (floatP) (float.PositiveInfinity * 0f));
			Assert.AreEqual(floatP.PositiveInfinity * (-floatP.Zero), (floatP)(float.PositiveInfinity * (-0f)));
			Assert.AreEqual(floatP.PositiveInfinity * floatP.One, (floatP)(float.PositiveInfinity * 1f));
			Assert.AreEqual(floatP.PositiveInfinity * floatP.MinusOne, (floatP)(float.PositiveInfinity * -1f));

			Assert.AreEqual(floatP.NegativeInfinity * floatP.Zero, (floatP)(float.NegativeInfinity * 0f));
			Assert.AreEqual(floatP.NegativeInfinity * (-floatP.Zero), (floatP)(float.NegativeInfinity * (-0f)));
			Assert.AreEqual(floatP.NegativeInfinity * floatP.One, (floatP)(float.NegativeInfinity * 1f));
			Assert.AreEqual(floatP.NegativeInfinity * floatP.MinusOne, (floatP)(float.NegativeInfinity * -1f));

			Assert.AreEqual(floatP.One * floatP.One, (floatP)1f);
		}
	}
}
