using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <summary>
	/// A list with the possibility to observe changes to it's elements defined <see cref="ObservableUpdateType"/> rules
	/// </summary>
	public interface IObservableListReader : IEnumerable
	{
		/// <summary>
		/// Requests the list element count
		/// </summary>
		int Count { get; }
	}
	
	/// <inheritdoc cref="IObservableListReader"/>
	/// <remarks>
	/// Read only observable list interface
	/// </remarks>
	public interface IObservableListReader<out T> :IObservableListReader, IEnumerable<T> where T : struct
	{
		/// <summary>
		/// Looks up and return the data that is associated with the given <paramref name="index"/>
		/// </summary>
		T this[int index] { get; }
		
		/// <summary>
		/// Requests this list as a <see cref="IReadOnlyList{T}"/>
		/// </summary>
		IReadOnlyList<T> ReadOnlyList { get; }
		
		/// <summary>
		/// Observes this list with the given <paramref name="onUpdate"/> when any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void Observe(ObservableUpdateType updateType, Action<int, T> onUpdate);
		
		/// <summary>
		/// Observes this list with the given <paramref name="onUpdate"/> when any data changes following the rule of
		/// the given <paramref name="updateType"/> and invokes the given <paramref name="onUpdate"/> with the given <paramref name="index"/>
		/// </summary>
		void InvokeObserve(int index, ObservableUpdateType updateType, Action<int, T> onUpdate);
		
		/// <summary>
		/// Stops observing this list with the given <paramref name="onUpdate"/> of any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(ObservableUpdateType updateType, Action<int, T> onUpdate);
	}

	/// <inheritdoc />
	public interface IObservableList<T> : IObservableListReader<T> where T : struct
	{
		/// <summary>
		/// Changes the given <paramref name="index"/> in the list. If the data does not exist it will be added.
		/// It will notify any observer listing to its data
		/// </summary>
		new T this[int index] { get; set; }
		
		/// <summary>
		/// Add the given <paramref name="data"/> to the list.
		/// It will notify any observer listing to its data
		/// </summary>
		void Add(T data);
		
		/// <summary>
		/// Removes the data associated with the given <paramref name="index"/>
		/// </summary>
		/// <exception cref="IndexOutOfRangeException">
		/// Thrown if the given <paramref name="index"/> is out of the range of the list size
		/// </exception>
		void Remove(int index);
	}
	
	/// <inheritdoc />
	public class ObservableList<T> : IObservableList<T> where T : struct
	{
		private readonly IReadOnlyDictionary<int, IList<Action<int, T>>> _genericUpdateActions = 
			new ReadOnlyDictionary<int, IList<Action<int, T>>>(new Dictionary<int, IList<Action<int, T>>>
			{
				{(int) ObservableUpdateType.Added, new List<Action<int, T>>()},
				{(int) ObservableUpdateType.Removed, new List<Action<int, T>>()},
				{(int) ObservableUpdateType.Updated, new List<Action<int, T>>()}
			});

		/// <inheritdoc cref="IObservableList{T}.this" />
		public T this[int index]
		{
			get => List[index];
			set
			{
				List[index] = value;
				
				var updates = _genericUpdateActions[(int) ObservableUpdateType.Updated];
				for (var i = 0; i < updates.Count; i++)
				{
					updates[i](i, value);
				}
			}
		}
		
		/// <inheritdoc />
		public int Count => List.Count;
		/// <inheritdoc />
		public IReadOnlyList<T> ReadOnlyList => new ReadOnlyCollection<T>(List);
		
		protected virtual IList<T> List { get; }
		
		protected ObservableList() {}
		
		public ObservableList(IList<T> list)
		{
			List = list;
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			return List.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		/// <inheritdoc />
		public void Add(T data)
		{
			List.Add(data);

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Added];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](i, data);
			}
		}

		/// <inheritdoc />
		public void Remove(int index)
		{
			var data = List[index];
			
			List.RemoveAt(index);

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Removed];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](i, data);
			}
		}

		/// <inheritdoc />
		public void Observe(ObservableUpdateType updateType, Action<int, T> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Add(onUpdate);
		}

		/// <inheritdoc />
		public void InvokeObserve(int index, ObservableUpdateType updateType, Action<int, T> onUpdate)
		{
			onUpdate(index, List[index]);
			
			Observe(updateType, onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(ObservableUpdateType updateType, Action<int, T> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Remove(onUpdate);
		}
	}

	/// <inheritdoc />
	public class ObservableResolverList<T> : ObservableList<T> where T : struct
	{
		private readonly Func<IList<T>> _listResolver;

		protected override IList<T> List => _listResolver();

		public ObservableResolverList(Func<IList<T>> listResolver)
		{
			_listResolver = listResolver;
		}
	}
}