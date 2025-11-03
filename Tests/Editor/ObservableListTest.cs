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
			void Call(int index, T value, T valueChange, ObservableUpdateType updateType);
		}

		private const int _index = 0;
		private const int _previousValue = 5;
		private const int _newValue = 10;

		private ObservableList<int> _list;
		private IList<int> _mockList;
		private IMockCaller<int> _caller;

		[SetUp]
		public void Init()
		{
			_caller = Substitute.For<IMockCaller<int>>();
			_mockList = Substitute.For<IList<int>>();
			_list = new ObservableList<int>(_mockList);
		}

		[Test]
		public void AddValue_AddsValueToList()
		{
			_list.Add(_previousValue);

			Assert.AreEqual(_previousValue, _list[_index]);
		}

		[Test]
		public void SetValue_UpdatesValue()
		{
			const int valueCheck1 = 5;
			const int valueCheck2 = 6;

			_list.Add(valueCheck1);

			Assert.AreEqual(valueCheck1, _list[_index]);

			_list[_index] = valueCheck2;

			Assert.AreEqual(valueCheck2, _list[_index]);
		}

		[Test]
		public void ObserveCheck()
		{
			_list.Observe(_caller.Call);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());

			_list.Add(_previousValue);

			_list[_index] = _newValue;

			_list.RemoveAt(_index);

			_caller.Received().Call(Arg.Any<int>(), Arg.Is(0), Arg.Is(_previousValue), ObservableUpdateType.Added);
			_caller.Received().Call(_index, _previousValue, _newValue, ObservableUpdateType.Updated);
			_caller.Received().Call(_index, _newValue, 0, ObservableUpdateType.Removed);
		}

		[Test]
		public void InvokeObserveCheck()
		{
			_list.Add(_previousValue);

			_list.InvokeObserve(_index, _caller.Call);

			_caller.DidNotReceive().Call(_index, _previousValue, _previousValue, ObservableUpdateType.Added);
			_caller.Received().Call(_index, _previousValue, _previousValue, ObservableUpdateType.Updated);
			_caller.DidNotReceive().Call(_index, _previousValue, _previousValue, ObservableUpdateType.Removed);
		}

		[Test]
		public void InvokeCheck()
		{
			_list.Add(_previousValue);
			_list.Observe(_caller.Call);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());

			_list.InvokeUpdate(_index);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), ObservableUpdateType.Added);
			_caller.Received().Call(_index, _previousValue, _previousValue, ObservableUpdateType.Updated);
			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), ObservableUpdateType.Removed);
		}

		[Test]
		public void InvokeCheck_NotObserving_DoesNothing()
		{
			_list.Add(_previousValue);
			_list.InvokeUpdate(_index);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObserveCheck()
		{
			_list.Observe(_caller.Call);
			_list.StopObserving(_caller.Call);
			_list.Add(_previousValue);

			_list[_index] = _previousValue;

			_list.RemoveAt(_index);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObserve_WhenCalledOnce_RemovesOnlyOneObserverInstance()
		{
			_list.Observe(_caller.Call);
			_list.Observe(_caller.Call);
			_list.StopObserving(_caller.Call);
			_list.Add(_previousValue);

			_list[_index] = _previousValue;

			_list.RemoveAt(_index);

			_caller.Received(1).Call(Arg.Any<int>(), Arg.Is(0), Arg.Is(_previousValue), ObservableUpdateType.Added);
			_caller.Received(1).Call(_index, _previousValue, _previousValue, ObservableUpdateType.Updated);
			_caller.Received(1).Call(_index, _previousValue, 0, ObservableUpdateType.Removed);
		}

		[Test]
		public void StopObservingAllCheck()
		{
			_list.Observe(_caller.Call);
			_list.StopObservingAll(_caller);
			_list.Add(_previousValue);
			_list.InvokeUpdate(_index);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAll_MultipleCalls_StopsAll()
		{
			_list.Observe(_caller.Call);
			_list.Observe(_caller.Call);
			_list.StopObservingAll(_caller);
			_list.Add(_previousValue);
			_list.InvokeUpdate(_index);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAll_Everything_Check()
		{
			_list.Observe(_caller.Call);
			_list.StopObservingAll();

			_list.Add(_previousValue);
			_list.InvokeUpdate(_index);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAll_NotObserving_DoesNothing()
		{
			_list.StopObservingAll();

			_list.Add(_previousValue);
			_list.InvokeUpdate(_index);

			_caller.DidNotReceive().Call(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void RebindCheck_BaseClass()
		{
			// Add initial data
			_list.Add(_previousValue);
			_list.Add(_newValue);

			// Setup observer
			_list.Observe(_caller.Call);

			// Create new list and rebind
			var newList = new List<int> { 100, 200, 300 };
			_list.Rebind(newList);

			// Verify new list is being used
			Assert.AreEqual(3, _list.Count);
			Assert.AreEqual(100, _list[0]);
			Assert.AreEqual(200, _list[1]);
			Assert.AreEqual(300, _list[2]);

			// Verify observer still works
			_list.Add(400);
			_caller.Received(1).Call(Arg.Any<int>(), Arg.Is(0), Arg.Is(400), ObservableUpdateType.Added);
		}
	}
}