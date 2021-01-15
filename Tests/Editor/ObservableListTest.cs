using System.Collections.Generic;
using GameLovers;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.DataExtensions.Tests
{
	[TestFixture]
	public class ObservableListTest
	{
		/// <summary>
		/// Mocking interface to check method calls received
		/// </summary>
		public interface IMockCaller<in T>
		{
			void AddCall(int index, T value);
			void UpdateCall(int index, T value);
			void RemoveCall(int index, T value);
		}
		
		private const int _index = 0;

		private ObservableList<int> _observableList;
		private ObservableResolverList<int> _observableResolverList;
		private List<int> _list;
		private IMockCaller<int> _caller;
		
		[SetUp]
		public void Init()
		{
			_caller = Substitute.For<IMockCaller<int>>();
			_list = Substitute.For<List<int>>();
			_observableList = new ObservableList<int>(_list);
			_observableResolverList = new ObservableResolverList<int>(() => _list);
		}

		[Test]
		public void ValueCheck()
		{
			const int valueCheck = 5;
			
			_list.Add(valueCheck);
			
			Assert.AreEqual(valueCheck, _observableList[_index]);
			Assert.AreEqual(valueCheck, _observableResolverList[_index]);
		}

		[Test]
		public void ValueSetCheck()
		{
			const int valueCheck1 = 5;
			const int valueCheck2 = 6;
			const int valueCheck3 = 7;
			
			_list.Add(valueCheck1);
			
			Assert.AreEqual(valueCheck1, _observableList[_index]);
			Assert.AreEqual(valueCheck1, _observableResolverList[_index]);

			_observableList[_index] = valueCheck2;
			
			Assert.AreEqual(valueCheck2, _observableList[_index]);
			Assert.AreEqual(valueCheck2, _observableResolverList[_index]);

			_observableResolverList[_index] = valueCheck3;
			
			Assert.AreEqual(valueCheck3, _observableList[_index]);
			Assert.AreEqual(valueCheck3, _observableResolverList[_index]);
		}

		[Test]
		public void ObserveCheck()
		{
			const int valueCheck = 5;
			
			_observableList.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableList.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableList.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverList.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverList.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverList.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			
			_caller.DidNotReceive().AddCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().UpdateCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().RemoveCall(Arg.Any<int>(), Arg.Any<int>());
			
			_observableList.Add(valueCheck);
			_observableResolverList.Add(valueCheck);
			_observableList[_index] = valueCheck;
			_observableResolverList[_index] = valueCheck;
			_observableList.RemoveAt(_index);
			_observableResolverList.RemoveAt(_index);
			
			_caller.Received(2).AddCall(_index, valueCheck);
			_caller.Received(2).UpdateCall(_index, valueCheck);
			_caller.Received(2).RemoveCall(_index, valueCheck);
		}

		[Test]
		public void InvokeObserveCheck()
		{
			const int valueCheck = 5;
			
			_list.Add(valueCheck);
			
			_observableList.InvokeObserve(_index, ObservableUpdateType.Added, _caller.AddCall);
			_observableList.InvokeObserve(_index, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableList.InvokeObserve(_index, ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverList.InvokeObserve(_index, ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverList.InvokeObserve(_index, ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverList.InvokeObserve(_index, ObservableUpdateType.Removed, _caller.RemoveCall);
			
			_caller.Received(2).AddCall(_index, valueCheck);
			_caller.Received(2).UpdateCall(_index, valueCheck);
			_caller.Received(2).RemoveCall(_index, valueCheck);
		}

		[Test]
		public void InvokeCheck()
		{
			const int valueCheck = 5;
			
			_list.Add(valueCheck);
			
			_observableList.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableList.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableList.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverList.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverList.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverList.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			
			_observableList.InvokeUpdate(_index);
			_observableResolverList.InvokeUpdate(_index);
			
			_caller.DidNotReceive().AddCall(_index, valueCheck);
			_caller.Received(2).UpdateCall(_index, valueCheck);
			_caller.DidNotReceive().RemoveCall(_index, valueCheck);
		}

		[Test]
		public void InvokeCheck_NotObserving_DoesNothing()
		{
			const int valueCheck = 5;
			
			_list.Add(valueCheck);
			
			_observableList.InvokeUpdate(_index);
			_observableResolverList.InvokeUpdate(_index);
			
			_caller.DidNotReceive().AddCall(_index, valueCheck);
			_caller.DidNotReceive().UpdateCall(_index, valueCheck);
			_caller.DidNotReceive().RemoveCall(_index, valueCheck);
		}

		[Test]
		public void StopObserveCheck()
		{
			const int valueCheck = 5;
			
			_observableList.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableList.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableList.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverList.Observe(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverList.Observe(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverList.Observe(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableList.StopObserving(ObservableUpdateType.Added, _caller.AddCall);
			_observableList.StopObserving(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableList.StopObserving(ObservableUpdateType.Removed, _caller.RemoveCall);
			_observableResolverList.StopObserving(ObservableUpdateType.Added, _caller.AddCall);
			_observableResolverList.StopObserving(ObservableUpdateType.Updated, _caller.UpdateCall);
			_observableResolverList.StopObserving(ObservableUpdateType.Removed, _caller.RemoveCall);
			
			_observableList.Add(valueCheck);
			_observableResolverList.Add(valueCheck);
			_observableList[_index] = valueCheck;
			_observableResolverList[_index] = valueCheck;
			_observableList.RemoveAt(_index);
			_observableResolverList.RemoveAt(_index);
			
			_caller.DidNotReceive().AddCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().UpdateCall(Arg.Any<int>(), Arg.Any<int>());
			_caller.DidNotReceive().RemoveCall(Arg.Any<int>(), Arg.Any<int>());
		}
	}
}