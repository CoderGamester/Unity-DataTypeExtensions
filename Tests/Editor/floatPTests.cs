using NUnit.Framework;
using GameLovers;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.DataExtensions.Tests
{
	[TestFixture]
	public class floatPTests
	{
		[Test]
		public void Representation()
		{
			Assert.AreEqual(floatP.Zero, 0f);
			Assert.AreNotEqual(-floatP.Zero, -0f);
			Assert.AreEqual(floatP.Zero, -floatP.Zero);
			Assert.AreEqual(floatP.NaN, float.NaN);
			Assert.AreEqual(floatP.One, 1f);
			Assert.AreEqual(floatP.MinusOne, -1f);
			Assert.AreEqual(floatP.PositiveInfinity, float.PositiveInfinity);
			Assert.AreEqual(floatP.NegativeInfinity, float.NegativeInfinity);
			Assert.AreEqual(floatP.Epsilon, float.Epsilon);
			Assert.AreEqual(floatP.MaxValue, float.MaxValue);
			Assert.AreEqual(floatP.MinValue, float.MinValue);
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
			Assert.AreEqual(floatP.One + floatP.One, 2f);
			Assert.AreEqual(floatP.One - floatP.One, 0f);
		}

		[Test]
		public void Multiplication()
		{
			Assert.AreEqual(floatP.PositiveInfinity * floatP.Zero, float.PositiveInfinity * 0f);
			Assert.AreEqual(floatP.PositiveInfinity * (-floatP.Zero), float.PositiveInfinity * (-0f));
			Assert.AreEqual(floatP.PositiveInfinity * floatP.One, float.PositiveInfinity * 1f);
			Assert.AreEqual(floatP.PositiveInfinity * floatP.MinusOne, float.PositiveInfinity * -1f);

			Assert.AreEqual(floatP.NegativeInfinity * floatP.Zero, float.NegativeInfinity * 0f);
			Assert.AreEqual(floatP.NegativeInfinity * (-floatP.Zero), float.NegativeInfinity * (-0f));
			Assert.AreEqual(floatP.NegativeInfinity * floatP.One, float.NegativeInfinity * 1f);
			Assert.AreEqual(floatP.NegativeInfinity * floatP.MinusOne, float.NegativeInfinity * -1f);

			Assert.AreEqual(floatP.One * floatP.One, 1f);
		}
	}
}
