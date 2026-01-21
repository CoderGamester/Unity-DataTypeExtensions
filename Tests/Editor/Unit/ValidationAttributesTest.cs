using System;
using System.Collections;
using System.Collections.Generic;
using GameLovers.GameData;
using NUnit.Framework;

namespace GameLovers.GameData.Tests
{
	[TestFixture]
	public class ValidationAttributesTest
	{
		#region RequiredAttribute Tests

		[TestCase(null, false, Description = "Null value fails")]
		[TestCase("", false, Description = "Empty string fails")]
		[TestCase("Hello", true, Description = "Non-empty string passes")]
		[TestCase(42, true, Description = "Integer passes")]
		[TestCase(0, true, Description = "Zero passes")]
		public void RequiredAttribute_Validates(object value, bool expectedResult)
		{
			var attr = new RequiredAttribute();
			Assert.AreEqual(expectedResult, attr.IsValid(value, out _));
		}

		[Test]
		public void RequiredAttribute_NonNullObject_PassesValidation()
		{
			var attr = new RequiredAttribute();
			Assert.IsTrue(attr.IsValid(new object(), out _));
		}

		[Test]
		public void RequiredAttribute_FailedValidation_ReturnsMessage()
		{
			var attr = new RequiredAttribute();
			attr.IsValid(null, out var message);
			Assert.IsNotNull(message);
			Assert.IsNotEmpty(message);
		}

		#endregion

		#region RangeAttribute Tests

		[TestCase(0, 10, 5, true, Description = "Middle value passes")]
		[TestCase(0, 10, 0, true, Description = "Min boundary passes")]
		[TestCase(0, 10, 10, true, Description = "Max boundary passes")]
		[TestCase(0, 10, -1, false, Description = "Below min fails")]
		[TestCase(0, 10, 11, false, Description = "Above max fails")]
		[TestCase(-100, 100, 0, true, Description = "Zero in negative range passes")]
		[TestCase(-100, -50, -75, true, Description = "Negative range works")]
		public void RangeAttribute_IntValues_Validates(int min, int max, int value, bool expectedResult)
		{
			var attr = new RangeAttribute(min, max);
			Assert.AreEqual(expectedResult, attr.IsValid(value, out _));
		}

		[TestCase(0.0, 1.0, 0.5, true, Description = "Float middle value passes")]
		[TestCase(0.0, 1.0, 0.0, true, Description = "Float min boundary passes")]
		[TestCase(0.0, 1.0, 1.0, true, Description = "Float max boundary passes")]
		[TestCase(0.0, 1.0, -0.1, false, Description = "Float below min fails")]
		[TestCase(0.0, 1.0, 1.1, false, Description = "Float above max fails")]
		public void RangeAttribute_FloatValues_Validates(double min, double max, double value, bool expectedResult)
		{
			var attr = new RangeAttribute(min, max);
			Assert.AreEqual(expectedResult, attr.IsValid(value, out _));
		}

		[Test]
		public void RangeAttribute_FailedValidation_ReturnsMessage()
		{
			var attr = new RangeAttribute(0, 10);
			attr.IsValid(100, out var message);
			Assert.IsNotNull(message);
			Assert.IsNotEmpty(message);
		}

		#endregion

		#region MinLengthAttribute Tests

		[TestCase(3, "abc", true, Description = "Exact length passes")]
		[TestCase(3, "abcd", true, Description = "Longer string passes")]
		[TestCase(3, "ab", false, Description = "Too short fails")]
		[TestCase(0, "", true, Description = "Zero min with empty passes")]
		[TestCase(1, "", false, Description = "Empty with min 1 fails")]
		public void MinLengthAttribute_Strings_Validates(int minLength, string value, bool expectedResult)
		{
			var attr = new MinLengthAttribute(minLength);
			Assert.AreEqual(expectedResult, attr.IsValid(value, out _));
		}

		[Test]
		public void MinLengthAttribute_NullString_FailsValidation()
		{
			var attr = new MinLengthAttribute(1);
			Assert.IsFalse(attr.IsValid(null, out _));
		}

		[Test]
		public void MinLengthAttribute_CollectionMeetsLength_PassesValidation()
		{
			var attr = new MinLengthAttribute(2);
			Assert.IsTrue(attr.IsValid(new List<int> { 1, 2 }, out _));
			Assert.IsTrue(attr.IsValid(new List<int> { 1, 2, 3 }, out _));
		}

		[Test]
		public void MinLengthAttribute_CollectionTooShort_FailsValidation()
		{
			var attr = new MinLengthAttribute(2);
			Assert.IsFalse(attr.IsValid(new List<int> { 1 }, out _));
			Assert.IsFalse(attr.IsValid(new List<int>(), out _));
		}

		[Test]
		public void MinLengthAttribute_Array_Validates()
		{
			var attr = new MinLengthAttribute(2);
			Assert.IsTrue(attr.IsValid(new int[] { 1, 2 }, out _));
			Assert.IsFalse(attr.IsValid(new int[] { 1 }, out _));
		}

		[Test]
		public void MinLengthAttribute_FailedValidation_ReturnsMessage()
		{
			var attr = new MinLengthAttribute(5);
			attr.IsValid("ab", out var message);
			Assert.IsNotNull(message);
			Assert.IsNotEmpty(message);
		}

		#endregion

		#region ValidationAttribute Base Tests

		[Test]
		public void ValidationAttribute_IsValid_ReturnsCorrectMessage()
		{
			var required = new RequiredAttribute();
			required.IsValid(null, out var msg1);
			Assert.That(msg1, Does.Contain("required").IgnoreCase);

			var range = new RangeAttribute(0, 10);
			range.IsValid(100, out var msg2);
			Assert.That(msg2, Does.Contain("range").IgnoreCase.Or.Contain("between").IgnoreCase.Or.Contain("0").And.Contain("10"));

			var minLen = new MinLengthAttribute(5);
			minLen.IsValid("ab", out var msg3);
			Assert.That(msg3, Does.Contain("length").IgnoreCase.Or.Contain("5"));
		}

		#endregion
	}
}
