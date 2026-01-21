using System;

namespace GameLovers.GameData
{
	/// <summary>
	/// Extension methods for creating computed fields from observable field readers.
	/// </summary>
	public static class ObservableExtensions
	{
		/// <summary>
		/// Creates a computed field that transforms this field's value using the given selector.
		/// The computed field automatically updates when the source field changes.
		/// </summary>
		public static ComputedField<TResult> Select<T, TResult>(
			this IObservableFieldReader<T> source,
			Func<T, TResult> selector)
		{
			return new ComputedField<TResult>(() => selector(source.Value));
		}

		/// <summary>
		/// Combines this observable field with another field into a computed result.
		/// The computed field automatically updates when either source field changes.
		/// </summary>
		public static ComputedField<TResult> CombineWith<T1, T2, TResult>(
			this IObservableFieldReader<T1> first,
			IObservableFieldReader<T2> second,
			Func<T1, T2, TResult> combiner)
		{
			return new ComputedField<TResult>(() => combiner(first.Value, second.Value));
		}

		/// <summary>
		/// Combines this observable field with two other fields into a computed result.
		/// The computed field automatically updates when any source field changes.
		/// </summary>
		public static ComputedField<TResult> CombineWith<T1, T2, T3, TResult>(
			this IObservableFieldReader<T1> first,
			IObservableFieldReader<T2> second,
			IObservableFieldReader<T3> third,
			Func<T1, T2, T3, TResult> combiner)
		{
			return new ComputedField<TResult>(() => combiner(first.Value, second.Value, third.Value));
		}

		/// <summary>
		/// Combines this observable field with three other fields into a computed result.
		/// The computed field automatically updates when any source field changes.
		/// </summary>
		public static ComputedField<TResult> CombineWith<T1, T2, T3, T4, TResult>(
			this IObservableFieldReader<T1> first,
			IObservableFieldReader<T2> second,
			IObservableFieldReader<T3> third,
			IObservableFieldReader<T4> fourth,
			Func<T1, T2, T3, T4, TResult> combiner)
		{
			return new ComputedField<TResult>(() => combiner(first.Value, second.Value, third.Value, fourth.Value));
		}
	}
}
