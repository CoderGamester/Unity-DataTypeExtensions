using GameLovers;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.DataExtensions.Tests
{
	[TestFixture]
	public class ObservableFieldTest
	{
		/// <summary>
		/// Mocking interface to check method calls received
		/// </summary>
		public interface IMockCaller<in T>
		{
			void AddCall(T value);
			void UpdateCall(T value);
			void RemoveCall(T value);
		}
		
		private ObservableField<int> _observableField;
		private ObservableResolverField<int> _observableResolverField;
		private int _mockInt;
		private IMockCaller<int> _caller;
		
		[SetUp]
		public void Init()
		{
			_caller = Substitute.For<IMockCaller<int>>();
			_observableField = new ObservableField<int>(_mockInt);
			_observableResolverField = new ObservableResolverField<int>(() => _mockInt, i => _mockInt = i);
		}

		[Test]
		public void ValueCheck()
		{
			Assert.AreEqual(_mockInt, _observableField.Value);
			Assert.AreEqual(_mockInt, _observableResolverField.Value);
		}

		[Test]
		public void ValueSetCheck()
		{
			const int valueCheck = 6;
			
			_mockInt = 5;
			
			Assert.AreNotEqual(_mockInt, _observableField.Value);
			Assert.AreEqual(_mockInt, _observableResolverField.Value);

			_observableField.Value = _mockInt;
			
			Assert.AreEqual(_mockInt, _observableField.Value);

			_observableResolverField.Value = valueCheck;
			
			Assert.AreEqual(valueCheck, _mockInt);
			Assert.AreNotEqual(_mockInt, _observableField.Value);
			Assert.AreEqual(_mockInt, _observableResolverField.Value);
		}

		[Test]
		public void ObserveCheck()
		{
			const int valueCheck = 6;
			
			_observableField.Observe(_caller.UpdateCall);
			_observableResolverField.Observe(_caller.UpdateCall);
			
			_caller.DidNotReceive().UpdateCall(Arg.Any<int>());

			_observableResolverField.Value = valueCheck;
			_observableField.Value = valueCheck;
			
			_caller.Received(2).UpdateCall(valueCheck);
		}

		[Test]
		public void InvokeObserveCheck()
		{
			_observableField.InvokeObserve(_caller.UpdateCall);
			_observableResolverField.InvokeObserve(_caller.UpdateCall);
			
			_caller.Received(2).UpdateCall(0);
		}

		[Test]
		public void InvokeCheck()
		{
			_observableField.Observe(_caller.UpdateCall);
			_observableResolverField.Observe(_caller.UpdateCall);
			
			_observableField.InvokeUpdate();
			_observableResolverField.InvokeUpdate();
			
			_caller.Received(2).UpdateCall(0);
		}

		[Test]
		public void InvokeCheck_NotObserving_DoesNothing()
		{
			_observableField.InvokeUpdate();
			_observableResolverField.InvokeUpdate();
			
			_caller.DidNotReceive().UpdateCall(0);
		}

		[Test]
		public void StopObserveCheck()
		{
			const int valueCheck = 6;
			
			_observableField.Observe(_caller.UpdateCall);
			_observableResolverField.Observe(_caller.UpdateCall);
			_observableField.StopObserving(_caller.UpdateCall);
			_observableResolverField.StopObserving(_caller.UpdateCall);

			_observableResolverField.Value = valueCheck;
			_observableField.Value = valueCheck;
			
			_caller.DidNotReceive().UpdateCall(valueCheck);
		}
	}
}