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
	}
}