using System;
using GameLovers.GameData;
using NUnit.Framework;
using UnityEngine;

namespace GameLovers.GameData.Tests
{
	[TestFixture]
	public class SerializableTypeTest
	{
		[Test]
		public void Constructor_WithType_StoresCorrectly()
		{
			var st = new SerializableType<int>();
			Assert.AreEqual(typeof(int), st.Value);
		}

		[Test]
		public void Value_Property_ResolvesCorrectly()
		{
			var st = new SerializableType<object>();
			// Set the private fields to simulate deserialization
			var type = typeof(SerializableType<object>);
			var classNameField = type.GetField("_className", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var assemblyNameField = type.GetField("_assemblyName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			
			object boxed = st;
			classNameField.SetValue(boxed, typeof(string).FullName);
			assemblyNameField.SetValue(boxed, typeof(string).Assembly.FullName);
			
			// Trigger OnAfterDeserialize on the same boxed instance (struct boxing semantics!)
			((ISerializationCallbackReceiver)boxed).OnAfterDeserialize();
			
			// Now unbox the modified struct
			st = (SerializableType<object>)boxed;
			
			Assert.AreEqual(typeof(string), st.Value);
		}

		[Test]
		public void Equals_SameType_ReturnsTrue()
		{
			var st1 = new SerializableType<int>();
			var st2 = new SerializableType<int>();
			Assert.IsTrue(st1.Equals(st2));
		}

		[Test]
		public void Equals_DifferentType_ReturnsFalse()
		{
			var st1 = new SerializableType<int>();
			var st2 = new SerializableType<string>();
			Assert.IsFalse(st1.Equals(st2));
		}

		[Test]
		public void GetHashCode_SameType_SameHash()
		{
			var st1 = new SerializableType<int>();
			var st2 = new SerializableType<int>();
			Assert.AreEqual(st1.GetHashCode(), st2.GetHashCode());
		}

		[Test]
		public void ImplicitConversion_ToType_Works()
		{
			var st = new SerializableType<int>();
			Type t = st;
			Assert.AreEqual(typeof(int), t);
		}
	}
}
