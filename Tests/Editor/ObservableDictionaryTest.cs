using System.Collections.Generic;
using GameLovers;
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
			void AddCall(TKey key, TValue value);
			void UpdateCall(TKey key, TValue value);
			void RemoveCall(TKey key, TValue value);
		}
		
		private ObservableDictionary<int, int> _observableDictionary;
		private ObservableResolverDictionary<int, int> _observableResolverDictionary;
		private IDictionary<int,int> _mockDictionary;
		private IMockCaller<int, int> _caller;
		
		[SetUp]
		public void Init()
		{
			_caller = Substitute.For<IMockCaller<int, int>>();
			_mockDictionary = Substitute.For<IDictionary<int, int>>();
			_observableDictionary = new ObservableDictionary<int, int>(_mockDictionary);
			_observableResolverDictionary = new ObservableResolverDictionary<int, int>(() => _mockDictionary);

			_mockDictionary.TryGetValue(_key, out _).Returns(callInfo =>
			{
				callInfo[1] = _mockDictionary[_key];
				return true;
			});
		}

		[Test]
		public void ValueCheck()
		{
			const int valueCheck = 5;
			
			_mockDictionary[_key].Returns(valueCheck);
			
			Assert.AreEqual(valueCheck, _observableDictionary[_key]);
			Assert.AreEqual(valueCheck, _observableResolverDictionary[_key]);
		}

		[Test]
		public void ValueSetCheck()
		{
			const int valueCheck1 = 5;
			const int valueCheck2 = 6;
			const int valueCheck3 = 7;
			
			_mockDictionary[_key] = valueCheck1;
			
			Assert.AreEqual(valueCheck1, _observableDictionary[_key]);
			Assert.AreEqual(valueCheck1, _observableResolverDictionary[_key]);

			_observableDictionary[_key] = valueCheck2;
			
			Assert.AreEqual(valueCheck2, _observableDictionary[_key]);
			Assert.AreEqual(valueCheck2, _observableResolverDictionary[_key]);

			_observableResolverDictionary[_key] = valueCheck3;
			
			Assert.AreEqual(valueCheck3, _observableDictionary[_key]);
			Assert.AreEqual(valueCheck3, _observableResolverDictionary[_key]);
		}

		[Test]
		public void ObserveCheck()
		{
			const int valueCheck = 5;
			
			_observableDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableDictionary.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			
			_caller.DidNotReceive().AddCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().UpdateCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().RemoveCall(Arg.Any<int>(), Arg.Any<int>());
			
			_observableDictionary.Add(_key, valueCheck);
			_observableResolverDictionary.Add(_key, valueCheck);
			_observableDictionary[_key] = valueCheck;
			_observableResolverDictionary[_key] = valueCheck;
			_observableDictionary.Remove(_key);
			_observableResolverDictionary.Remove(_key);
			
			_caller.Received(4).AddCall(_key, valueCheck);
			_caller.Received(4).UpdateCall(_key, valueCheck);
			_caller.Received(4).RemoveCall(_key, valueCheck);
		}

		[Test]
		public void InvokeObserveCheck()
		{
			_observableDictionary.InvokeObserve(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.InvokeObserve(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.InvokeObserve(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.InvokeObserve(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.InvokeObserve(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.InvokeObserve(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			
			_caller.Received(2).AddCall(_key, 0);
			_caller.Received(2).UpdateCall(_key, 0);
			_caller.Received(2).RemoveCall(_key, 0);
		}

		[Test]
		public void InvokeCheck()
		{
			_observableDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableDictionary.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			
			_observableDictionary.InvokeUpdate(_key);
			_observableResolverDictionary.InvokeUpdate(_key);
			
			_caller.DidNotReceive().AddCall(_key, 0);
			_caller.Received(4).UpdateCall(_key, 0);
			_caller.DidNotReceive().RemoveCall(_key, 0);
		}

		[Test]
		public void InvokeCheck_NotObserving_DoesNothing()
		{
			_observableDictionary.InvokeUpdate(_key);
			_observableResolverDictionary.InvokeUpdate(_key);
			
			_caller.DidNotReceive().AddCall(_key, 0);
			_caller.DidNotReceive().UpdateCall(_key, 0);
			_caller.DidNotReceive().RemoveCall(_key, 0);
		}

		[Test]
		public void StopObserveCheck()
		{
			const int valueCheck = 5;
			
			_observableDictionary.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableDictionary.StopObserving(ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.StopObserving(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.StopObserving(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.StopObserving(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.StopObserving(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.StopObserving(ObservableUpdateType.Removed, _caller.RemoveCall);

			_observableDictionary.Add(_key, valueCheck);
			_observableResolverDictionary.Add(_key, valueCheck);
			_observableDictionary[_key] = valueCheck;
			_observableResolverDictionary[_key] = valueCheck;
			_observableDictionary.Remove(_key);
			_observableResolverDictionary.Remove(_key);
			
			_caller.DidNotReceive().AddCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().UpdateCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().RemoveCall(Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void StopObserve_KeyCheck()
		{
			const int valueCheck = 5;
			
			_observableDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableDictionary.StopObserving(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.StopObserving(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.StopObserving(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.StopObserving(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.StopObserving(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.StopObserving(_key, ObservableUpdateType.Removed, _caller.RemoveCall);

			_observableDictionary.Add(_key, valueCheck);
			_observableResolverDictionary.Add(_key, valueCheck);
			_observableDictionary[_key] = valueCheck;
			_observableResolverDictionary[_key] = valueCheck;
			_observableDictionary.Remove(_key);
			_observableResolverDictionary.Remove(_key);
			
			_caller.DidNotReceive().AddCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().UpdateCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().RemoveCall(Arg.Any<int>(), Arg.Any<int>());
		}

		[Test]
		public void StopObserve_OnlyKeyCheck()
		{
			const int valueCheck = 5;
			
			_observableDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverDictionary.Observe(_key, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableDictionary.StopObserving(_key);
			_observableResolverDictionary.StopObserving(_key);

			_observableDictionary.Add(_key, valueCheck);
			_observableResolverDictionary.Add(_key, valueCheck);
			_observableDictionary[_key] = valueCheck;
			_observableResolverDictionary[_key] = valueCheck;
			_observableDictionary.Remove(_key);
			_observableResolverDictionary.Remove(_key);
			
			_caller.DidNotReceive().AddCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().UpdateCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().RemoveCall(Arg.Any<int>(), Arg.Any<int>());
		}
	}
}