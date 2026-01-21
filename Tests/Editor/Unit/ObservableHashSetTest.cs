using System;
using System.Collections.Generic;
using GameLovers.GameData;
using NSubstitute;
using NUnit.Framework;

namespace GameLovers.GameData.Tests
{
	[TestFixture]
	public class ObservableHashSetTest
	{
		private ObservableHashSet<int> _set;
		private Action<int, ObservableUpdateType> _mockObserver;

		[SetUp]
		public void Setup()
		{
			_set = new ObservableHashSet<int>();
			_mockObserver = Substitute.For<Action<int, ObservableUpdateType>>();
		}

		[Test]
		public void Add_NewItem_ReturnsTrue()
		{
			Assert.IsTrue(_set.Add(1));
		}

		[Test]
		public void Add_NewItem_NotifiesAdded()
		{
			_set.Observe(_mockObserver);
			_set.Add(1);
			_mockObserver.Received(1)(1, ObservableUpdateType.Added);
		}

		[Test]
		public void Add_ExistingItem_ReturnsFalse()
		{
			_set.Add(1);
			Assert.IsFalse(_set.Add(1));
		}

		[Test]
		public void Add_ExistingItem_NoNotification()
		{
			_set.Add(1);
			_set.Observe(_mockObserver);
			_set.Add(1);
			_mockObserver.DidNotReceive()(Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void Remove_ExistingItem_ReturnsTrue()
		{
			_set.Add(1);
			Assert.IsTrue(_set.Remove(1));
		}

		[Test]
		public void Remove_ExistingItem_NotifiesRemoved()
		{
			_set.Add(1);
			_set.Observe(_mockObserver);
			_set.Remove(1);
			_mockObserver.Received(1)(1, ObservableUpdateType.Removed);
		}

		[Test]
		public void Remove_MissingItem_ReturnsFalse()
		{
			Assert.IsFalse(_set.Remove(1));
		}

		[Test]
		public void Remove_MissingItem_NoNotification()
		{
			_set.Observe(_mockObserver);
			_set.Remove(1);
			_mockObserver.DidNotReceive()(Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void Contains_ExistingItem_ReturnsTrue()
		{
			_set.Add(10);
			Assert.IsTrue(_set.Contains(10));
		}

		[Test]
		public void Contains_MissingItem_ReturnsFalse()
		{
			Assert.IsFalse(_set.Contains(10));
		}

		[Test]
		public void Clear_NotifiesRemovedForEachItem()
		{
			_set.Add(1);
			_set.Add(2);
			_set.Observe(_mockObserver);
			_set.Clear();
			_mockObserver.Received(1)(1, ObservableUpdateType.Removed);
			_mockObserver.Received(1)(2, ObservableUpdateType.Removed);
			Assert.AreEqual(0, _set.Count);
		}

		[Test]
		public void Clear_EmptySet_NoNotifications()
		{
			_set.Observe(_mockObserver);
			_set.Clear();
			_mockObserver.DidNotReceive()(Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void Count_ReturnsCorrectValue()
		{
			Assert.AreEqual(0, _set.Count);
			_set.Add(1);
			_set.Add(2);
			Assert.AreEqual(2, _set.Count);
		}

		[Test]
		public void Count_TracksComputedDependency()
		{
			var computed = new ComputedField<int>(() => _set.Count);
			Assert.AreEqual(0, computed.Value);

			_set.Add(1);
			Assert.AreEqual(1, computed.Value);
		}

		[Test]
		public void StopObserving_StopsNotifications()
		{
			_set.Observe(_mockObserver);
			_set.StopObserving(_mockObserver);
			_set.Add(1);
			_mockObserver.DidNotReceive()(Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void StopObservingAll_ClearsAllObservers()
		{
			var observer2 = Substitute.For<Action<int, ObservableUpdateType>>();
			_set.Observe(_mockObserver);
			_set.Observe(observer2);
			_set.StopObservingAll();
			_set.Add(1);
			_mockObserver.DidNotReceive()(Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
			observer2.DidNotReceive()(Arg.Any<int>(), Arg.Any<ObservableUpdateType>());
		}

		[Test]
		public void BeginBatch_SuppressesNotifications()
		{
			_set.Observe(_mockObserver);
			using (_set.BeginBatch())
			{
				_set.Add(1);
				_set.Add(2);
			}
			// In current implementation, ResumeNotifications notifies for ALL current items as Added
			_mockObserver.Received(1)(1, ObservableUpdateType.Added);
			_mockObserver.Received(1)(2, ObservableUpdateType.Added);
		}

		[Test]
		public void Constructor_WithCollection_PopulatesSet()
		{
			var list = new List<int> { 1, 2, 3 };
			var set = new ObservableHashSet<int>(list);
			Assert.AreEqual(3, set.Count);
			Assert.IsTrue(set.Contains(1));
			Assert.IsTrue(set.Contains(2));
			Assert.IsTrue(set.Contains(3));
		}

		[Test]
		public void Constructor_WithComparer_UsesComparer()
		{
			var set = new ObservableHashSet<string>(StringComparer.OrdinalIgnoreCase);
			set.Add("Test");
			Assert.IsTrue(set.Contains("test"));
		}

		[Test]
		public void Enumerable_Works()
		{
			_set.Add(1);
			_set.Add(2);
			var items = new List<int>();
			foreach (var item in _set)
			{
				items.Add(item);
			}
			Assert.AreEqual(2, items.Count);
			Assert.Contains(1, items);
			Assert.Contains(2, items);
		}
	}
}
