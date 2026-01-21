using System;
using System.Collections.Generic;
using GameLovers.GameData;
using NUnit.Framework;

namespace GameLovers.GameData.Tests.Integration
{
	[TestFixture]
	public class ObservableComputedIntegrationTest
	{
		[Test]
		public void ComputedField_WithMultipleObservables_TracksAll()
		{
			var field1 = new ObservableField<int>(10);
			var field2 = new ObservableField<int>(20);
			var list = new ObservableList<int>(new List<int> { 1, 2 });
			
			var computed = new ComputedField<int>(() => field1.Value + field2.Value + list.Count);
			
			Assert.AreEqual(32, computed.Value);

			field1.Value = 100;
			Assert.AreEqual(122, computed.Value);

			list.Add(3);
			Assert.AreEqual(123, computed.Value);
		}

		[Test]
		public void BatchUpdates_WithComputedField_SingleRecalculation()
		{
			var field1 = new ObservableField<int>(10);
			var field2 = new ObservableField<int>(20);
			var callCount = 0;
			
			var computed = new ComputedField<int>(() =>
			{
				callCount++;
				return field1.Value + field2.Value;
			});

			computed.Observe((p, c) => { }); // Observe to trigger recompute on dependency change
			var val = computed.Value; // initial compute
			callCount = 0;

			// When computed is included in the batch, it should only recompute once
			// when the batch ends (not once per field)
			using (var batch = new ObservableBatch())
			{
				batch.Add(field1);
				batch.Add(field2);
				batch.Add(computed);
				
				field1.Value = 100;
				field2.Value = 200;
			}
			
			Assert.AreEqual(1, callCount);
		}

		[Test]
		public void ChainedComputedFields_PropagateDirtyFlag()
		{
			var field = new ObservableField<int>(10);
			var c1 = field.Select(x => x + 1);
			var c2 = c1.Select(x => x + 1);
			
			Assert.AreEqual(12, c2.Value);
			
			field.Value = 20;
			Assert.AreEqual(22, c2.Value);
		}
	}
}
