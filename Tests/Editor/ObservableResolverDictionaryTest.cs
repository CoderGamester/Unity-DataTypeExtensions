using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using GameLovers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.DataExtensions.Tests
{
	[TestFixture]
	public class ObservableResolverDictionaryTest
	{
		private int _key = 0;
		private string _value = "1";
		private ObservableResolverDictionary<int, int, int, string> _dictionary;
		private IDictionary<int, string> _mockDictionary;

		[SetUp]
		public void Init()
		{
			_mockDictionary = Substitute.For<IDictionary<int, string>>();
			_dictionary = new ObservableResolverDictionary<int, int, int, string>(
				_mockDictionary,
				origin => new KeyValuePair<int, int>(origin.Key, int.Parse(origin.Value)),
				(key, value) => new KeyValuePair<int, string>(key, key.ToString()));

			_mockDictionary[_key].Returns(_value);
			_mockDictionary.TryGetValue(_key, out _).Returns(callInfo =>
			{
				callInfo[1] = _mockDictionary[_key];
				return true;
			});
		}

		[Test]
		public void TryGetOriginValue_KeyExists_ReturnsTrueAndOutValue()
		{
			Assert.IsTrue(_dictionary.TryGetOriginValue(_key, out var value));
		}

		[Test]
		public void TryGetOriginValue_KeyDoesNotExist_ReturnsFalseAndOutDefault()
		{
			var result = _dictionary.TryGetOriginValue(999, out var value);

			Assert.IsFalse(result);
			Assert.IsNull(value);
		}

		[Test]
		public void AddOrigin_AddsValueToOriginDictionary()
		{
			_dictionary.AddOrigin(_key, _value);

			_mockDictionary.Received().Add(_key, _value);
		}

		[Test]
		public void UpdateOrigin_UpdatesValueInOriginDictionary()
		{
			_dictionary.AddOrigin(_key, _value);
			_dictionary.UpdateOrigin(_key, _value);

			_mockDictionary.Received()[_key] = _value;
		}

		[Test]
		public void RemoveOrigin_RemovesValueFromOriginDictionary()
		{
			_dictionary.AddOrigin(_key, _value);

			Assert.IsTrue(_dictionary.RemoveOrigin(_key));
			_mockDictionary.Received().Remove(_key);
		}

		[Test]
		public void ClearOrigin_ClearsOriginDictionary()
		{
			_dictionary.ClearOrigin();

			_mockDictionary.Received().Clear();
		}

		[Test]
		public void Rebind_ChangesOriginDictionary()
		{
			// Setup: Add items to original dictionary
			_dictionary.AddOrigin(_key, _value);

			// Create new dictionary and rebind
			var newDictionary = new Dictionary<int, string> { { 100, "100" }, { 200, "200" } };
			_dictionary.Rebind(
				newDictionary,
				origin => new KeyValuePair<int, int>(origin.Key, int.Parse(origin.Value)),
				(key, value) => new KeyValuePair<int, string>(key, value.ToString()));

			// Verify new dictionary is being used
			Assert.AreEqual(2, _dictionary.Count);
			Assert.IsTrue(_dictionary.ContainsKey(100));
			Assert.IsTrue(_dictionary.ContainsKey(200));
			Assert.AreEqual(100, _dictionary[100]);
			Assert.AreEqual(200, _dictionary[200]);

			// Verify old dictionary is no longer used
			Assert.IsFalse(_dictionary.ContainsKey(_key));
		}

		[Test]
		public void Rebind_KeepsObservers()
		{
			// Setup observer
			var observerCalls = 0;
			_dictionary.ObservableUpdateFlag = ObservableUpdateFlag.UpdateOnly;
			_dictionary.Observe((key, prev, curr, type) => observerCalls++);

			// Create new dictionary and rebind
			var newDictionary = new Dictionary<int, string> { { 100, "100" } };
			_dictionary.Rebind(
				newDictionary,
				origin => new KeyValuePair<int, int>(origin.Key, int.Parse(origin.Value)),
				(key, value) => new KeyValuePair<int, string>(key, value.ToString()));

			// Trigger update and verify observer is still active
			_dictionary.Add(300, 300);
			Assert.AreEqual(1, observerCalls);
		}
	}
}