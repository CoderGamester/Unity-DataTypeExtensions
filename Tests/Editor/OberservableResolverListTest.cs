using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using GameLovers;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.DataExtensions.Tests
{
	[TestFixture]
	public class ObservableResolverListTest
	{
		private int _index = 0;
		private IObservableResolverList<int, string> _list;
		private IList<string> _mockList;

		[SetUp]
		public void SetUp()
		{
			_mockList = Substitute.For<IList<string>>();
			_list = new ObservableResolverList<int, string>(_mockList,
				origin => int.Parse(origin),
				value => value.ToString());
		}

		[Test]
		public void AddOrigin_AddsValueToOriginList()
		{
			var value = "1";

			_list.AddOrigin(value);

			_mockList.Received().Add(value);
		}

		[Test]
		public void UpdateOrigin_UpdatesOriginList()
		{
			var value = "1";

			_list.AddOrigin(value);
			_list.UpdateOrigin(value, _index);

			_mockList.Received()[_index] = value;
		}

		[Test]
		public void RemoveOrigin_RemovesValueFromOriginList()
		{
			var value = "1";

			_list.AddOrigin(value);

			Assert.IsTrue(_list.RemoveOrigin(value));
			_mockList.Received().Remove(value);
		}

		[Test]
		public void ClearOrigin_ClearsOriginList()
		{
			_list.ClearOrigin();

			_mockList.Received().Clear();
		}

		[Test]
		public void Rebind_ChangesOriginList()
		{
			// Add initial data
			_list.AddOrigin("1");
			_list.AddOrigin("2");

			// Create new origin list and rebind
			var newOriginList = new List<string> { "10", "20", "30", "40" };
			_list.Rebind(
				newOriginList,
				origin => int.Parse(origin),
				value => value.ToString());

			// Verify new list is being used
			Assert.AreEqual(4, _list.Count);
			Assert.AreEqual(10, _list[0]);
			Assert.AreEqual(20, _list[1]);
			Assert.AreEqual(30, _list[2]);
			Assert.AreEqual(40, _list[3]);

			// Verify add operation uses new origin list
			_list.Add(50);
			Assert.AreEqual("50", newOriginList[4]);
		}

		[Test]
		public void Rebind_KeepsObservers()
		{
			// Setup observer
			var observerCalls = 0;
			_list.Observe((index, prev, curr, type) => observerCalls++);

			// Create new origin list and rebind
			var newOriginList = new List<string> { "100", "200" };
			_list.Rebind(
				newOriginList,
				origin => int.Parse(origin),
				value => value.ToString());

			// Trigger update and verify observer is still active
			_list.Add(300);
			Assert.AreEqual(1, observerCalls);
		}

		[Test]
		public void Rebind_ChangesResolvers()
		{
			// Create new origin list with different format and rebind with new resolvers
			var newOriginList = new List<string> { "value:10", "value:20" };
			_list.Rebind(
				newOriginList,
				origin => int.Parse(origin.Split(':')[1]),
				value => $"value:{value}");

			// Verify new resolvers are being used
			Assert.AreEqual(2, _list.Count);
			Assert.AreEqual(10, _list[0]);
			Assert.AreEqual(20, _list[1]);

			// Verify add operation uses new resolver
			_list.Add(30);
			Assert.AreEqual("value:30", newOriginList[2]);
		}
	}
}