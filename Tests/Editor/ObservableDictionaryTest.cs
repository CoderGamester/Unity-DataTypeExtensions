using System;
using System.Collections.Generic;
using GameLovers;
using GameLovers.Observables;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.DataExtensions.Tests
{
	[TestFixture]
	public class ObservableDictionaryTest
	{
		private const int _key = 0;

		/// <summary>
		/// Mocking interface to check method calls received
		/// </summary>
		public interface IMockCaller<in TKey, in TValue>
		{
			void Call(TKey key, TValue previousValue, TValue newValue, ObservableUpdateType updateType);
		}

		private ObservableDictionary<int, int> _dictionary;
		private IDictionary<int, int> _mockDictionary;
		private IMockCaller<int, int> _caller;

		[SetUp]
		public void Init()
		{
			_caller = Substitute.For<IMockCaller<int, int>>();
			_mockDictionary = new Dictionary<int, int>();
			_dictionary = new ObservableDictionary<int, int>(_mockDictionary);
		}

		[Test]
		public void TryGetValue_ReturnsFalse_WhenKeyDoesNotExist()
		{
			bool result = _dictionary.TryGetValue(1, out int value);

			Assert.IsFalse(result);
		}

		[Test]
		public void TryGetValue_ReturnsTrue_WhenKeyExists()
		{
			_dictionary.Add(1, 100);

			bool result = _dictionary.TryGetValue(1, out int value);

			Assert.IsTrue(result);
			Assert.AreEqual(100, value);
		}

		[Test]
		public void ContainsKey_ReturnsFalse_WhenKeyDoesNotExist()
		{
			Assert.IsFalse(_dictionary.ContainsKey(1));
		}

		[Test]
		public void ContainsKey_ReturnsTrue_WhenKeyExists()
		{
			_dictionary.Add(1, 100);

			Assert.IsTrue(_dictionary.ContainsKey(1));
		}

		[Test]
		public void Indexer_ReturnsValue_WhenKeyExists()
		{
			_dictionary.Add(1, 100);

			Assert.AreEqual(100, _dictionary[1]);
		}

		[Test]
		public void Indexer_SetsValue_WhenKeyExists()
		{
			_dictionary.Add(1, 100);
			_dictionary[1] = 200;

			Assert.AreEqual(200, _dictionary[1]);
		}

		[Test]
		public void Add_AddsKeyValuePair_WhenKeyDoesNotExist()
		{
			_dictionary.Add(1, 100);

			Assert.AreEqual(100, _dictionary[1]);
		}

		[Test]
		public void Add_ThrowsException_WhenKeyAlreadyExists()
		{
			_dictionary.Add(1, 100);

			Assert.Throws<ArgumentException>(() => _dictionary.Add(1, 200));
		}

		[Test]
		public void Remove_RemovesKeyValuePair_WhenKeyExists()
		{
			_dictionary.Add(1, 100);

			Assert.IsTrue(_dictionary.Remove(1));
			Assert.AreEqual(0, _dictionary.Count);
		}

		[Test]
		public void Remove_ReturnsFalse_WhenKeyDoesNotExist()
		{
			Assert.IsFalse(_dictionary.Remove(1));
		}

		[Test]
		public void Clear_RemovesAllKeyValuePairs()
		{
			_dictionary.Add(1, 100);
			_dictionary.Add(2, 200);
			_dictionary.Clear();

			Assert.AreEqual(0, _dictionary.Count);
		}

		[Test]
		public void ValueSetCheck()
		{
			const int valueCheck1 = 5;
			const int valueCheck2 = 6;

			_mockDictionary.Add(_key, valueCheck1);
			_dictionary[_key] = valueCheck2;

			Assert.AreNotEqual(valueCheck1, _mockDictionary[_key]);
			Assert.AreEqual(valueCheck2, _dictionary[_key]);
		}

		[Test]
		public void ObserveCheck()
		{
			var startValue = 0;
			var newValue = 1;

			_dictionary.Observe(_key, _caller.Call);
			_dictionary.Observe(_caller.Call);

			_dictionary.Add(_key, startValue);

			_dictionary[_key] = newValue;

			_dictionary.Remove(_key);

			_caller.Received().Call(_key, 0, startValue, ObservableUpdateType.Added);
			_caller.Received().Call(_key, startValue, newValue, ObservableUpdateType.Updated);
			_caller.Received().Call(_key, newValue, 0, ObservableUpdateType.Removed);
		}

		[Test]
		public void InvokeObserveCheck()
		{
			_dictionary.Add(_key, 0);
			_dictionary.InvokeObserve(_key, _caller.Call);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), ObservableUpdateType.Added);
			_caller.Received().Call(_key, 0, 0, ObservableUpdateType.Updated);
			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), ObservableUpdateType.Removed);
		}

		[Test]
		public void InvokeUpdate_MissingKey_ThrowsException()
		{
			Assert.Throws<KeyNotFoundException>(() => _dictionary.InvokeUpdate(_key));
		}

		[Test]
		public void InvokeObserve_MissingKey_ThrowsException()
		{
			Assert.Throws<KeyNotFoundException>(() => _dictionary.InvokeObserve(_key, _caller.Call));
		}

		[Test]
		public void InvokeUpdateCheck()
		{
			_dictionary.Add(_key, 0);
			_dictionary.Observe(_key, _caller.Call);
			_dictionary.Observe(_caller.Call);

			_dictionary.InvokeUpdate(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), ObservableUpdateType.Added);
			_caller.Received().Call(_key, 0, 0, ObservableUpdateType.Updated);
			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), ObservableUpdateType.Removed);
		}

		[Test]
		public void InvokeUpdate_NotObserving_DoesNothing()
		{
			_dictionary.Add(_key, 0);
			_dictionary.InvokeUpdate(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObserveCheck()
		{
			_dictionary.Observe(_caller.Call);
			_dictionary.StopObserving(_caller.Call);

			_dictionary.Add(_key, 0);
			_dictionary[_key] = 0;
			_dictionary.Remove(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObserve_KeyCheck()
		{
			_dictionary.Observe(_key, _caller.Call);
			_dictionary.StopObserving(_key);

			_dictionary.Add(_key, 0);
			_dictionary[_key] = 0;
			_dictionary.Remove(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAllCheck()
		{
			_dictionary.Observe(_caller.Call);
			_dictionary.StopObservingAll(_caller);

			_dictionary.Add(_key, 0);
			_dictionary[_key] = 0;
			_dictionary.Remove(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAll_MultipleCalls_Check()
		{
			_dictionary.Observe(_caller.Call);
			_dictionary.Observe(_caller.Call);
			_dictionary.StopObservingAll(_caller);

			_dictionary.Add(_key, 0);
			_dictionary[_key] = 0;
			_dictionary.Remove(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAll_Everything_Check()
		{
			_dictionary.Observe(_caller.Call);
			_dictionary.StopObservingAll();

			_dictionary.Add(_key, 0);
			_dictionary[_key] = 0;
			_dictionary.Remove(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAll_NotObserving_DoesNothing()
		{
			_dictionary.StopObservingAll(_caller);

			_dictionary.Add(_key, 0);
			_dictionary[_key] = 0;
			_dictionary.Remove(_key);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void RebindCheck_BaseClass()
		{
			// Add initial data
			_dictionary.Add(1, 100);
			_dictionary.Add(2, 200);

			// Setup key-specific observer (this works with default KeyUpdateOnly flag)
			_dictionary.Observe(40, _caller.Call);

			// Create new dictionary and rebind
			var newDictionary = new Dictionary<int, int> { { 10, 1000 }, { 20, 2000 }, { 30, 3000 } };
			_dictionary.Rebind(newDictionary);

			// Verify new dictionary is being used
			Assert.AreEqual(3, _dictionary.Count);
			Assert.IsTrue(_dictionary.ContainsKey(10));
			Assert.IsTrue(_dictionary.ContainsKey(20));
			Assert.IsTrue(_dictionary.ContainsKey(30));
			Assert.AreEqual(1000, _dictionary[10]);
			Assert.AreEqual(2000, _dictionary[20]);
			Assert.AreEqual(3000, _dictionary[30]);

			// Verify old keys are no longer present
			Assert.IsFalse(_dictionary.ContainsKey(1));
			Assert.IsFalse(_dictionary.ContainsKey(2));

			// Verify observer still works after rebind
			_dictionary.Add(40, 4000);
			_caller.Received(1).Call(40, 0, 4000, ObservableUpdateType.Added);
		}
	}
}