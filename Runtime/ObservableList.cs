using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
	public interface IObservableListReader<T> : IObservableListReader, IEnumerable<T>
	{
		/// <summary>
		/// Looks up and return the data that is associated with the given <paramref name="index"/>
		/// </summary>
		T this[int index] { get; }

		/// <summary>
		/// Requests this list as a <see cref="IReadOnlyList{T}"/>
		/// </summary>
		IReadOnlyList<T> ReadOnlyList { get; }

		/// <inheritdoc cref="List{T}.Contains"/>
		bool Contains(T value);

		/// <inheritdoc cref="List{T}.IndexOf(T)"/>
		int IndexOf(T value);

		/// <summary>
		/// Observes to this list changes with the given <paramref name="onUpdate"/>
		/// </summary>
		void Observe(Action<int, T, T, ObservableUpdateType> onUpdate);

		/// <summary>
		/// Observes this list with the given <paramref name="onUpdate"/> when any data changes and invokes it with the given <paramref name="index"/>
		/// </summary>
		void InvokeObserve(int index, Action<int, T, T, ObservableUpdateType> onUpdate);

		/// <summary>
		/// Stops observing this dictionary with the given <paramref name="onUpdate"/> of any data changes
		/// </summary>
		void StopObserving(Action<int, T, T, ObservableUpdateType> onUpdate);

		/// <summary>
		/// Stops observing this dictionary changes from all the given <paramref name="subscriber"/> calls.
		/// If the given <paramref name="subscriber"/> is null then will stop observing from everything.
		/// </summary>
		void StopObservingAll(object subscriber = null);
	}

	/// <inheritdoc />
	public interface IObservableList<T> : IObservableListReader<T>
	{
		/// <summary>
		/// Changes the given <paramref name="index"/> in the list. If the data does not exist it will be added.
		/// It will notify any observer listing to its data
		/// </summary>
		new T this[int index] { get; set; }

		/// <inheritdoc cref="List{T}.Add"/>
		void Add(T data);

		/// <inheritdoc cref="List{T}.Remove"/>
		bool Remove(T data);

		/// <inheritdoc cref="List{T}.RemoveAt"/>
		void RemoveAt(int index);

		/// <inheritdoc cref="List{T}.Clear"/>
		void Clear();

		/// <remarks>
		/// It invokes any update method that is observing to the given <paramref name="index"/> on this list
		/// </remarks>
		void InvokeUpdate(int index);
	}

	/// <inheritdoc />
	/// <remarks>
	/// This interface resolves between 2 lists with different types of values
	/// </remarks>
	public interface IObservableResolverListReader<T, TOrigin> : IObservableListReader<T>
	{
		/// <summary>
		/// The Original List that is being resolved across the entire interface
		/// </summary>
		IReadOnlyList<TOrigin> OriginList { get; }
	}

	/// <inheritdoc />
	/// <remarks>
	/// This interface resolves between 2 lists with different types of values
	/// </remarks>
	public interface IObservableResolverList<T, TOrigin> :
		IObservableResolverListReader<T, TOrigin>,
		IObservableList<T>
	{
		/// <summary>
		/// Updates the value in the origin list corresponding to the specified index.
		/// </summary>
		/// <param name="index">The index of the value to update in the origin list.</param>
		/// <param name="value">The new value to set in the origin list.</param>
		void UpdateOrigin(TOrigin value, int index);

		/// <inheritdoc cref="List{T}.Add"/>
		/// <remarks>
		/// Add's the value to the origin list
		/// </remarks>
		void AddOrigin(TOrigin value);

		/// <inheritdoc cref="List{T}.Remove"/>
		/// <remarks>
		/// Remove's the value to the origin list
		/// </remarks>
		bool RemoveOrigin(TOrigin value);

		/// <inheritdoc cref="List{T}.Clear"/>
		/// <remarks>
		/// Clear's to the origin list
		/// </remarks>
		void ClearOrigin();
	}

	/// <inheritdoc />
	public class ObservableList<T> : IObservableList<T>
	{
		private readonly IList<Action<int, T, T, ObservableUpdateType>> _updateActions = new List<Action<int, T, T, ObservableUpdateType>>();

		/// <inheritdoc cref="IObservableList{T}.this" />
		public T this[int index]
		{
			get => List[index];
			set
			{
				var previousValue = List[index];

				List[index] = value;

				InvokeUpdate(index, previousValue);
			}
		}

		/// <inheritdoc />
		public int Count => List.Count;
		/// <inheritdoc />
		public IReadOnlyList<T> ReadOnlyList => new List<T>(List);

		protected virtual List<T> List { get; }

		protected ObservableList() { }

		public ObservableList(IList<T> list)
		{
			List = list as List<T> ?? list.ToList();
		}

		/// <inheritdoc cref="List{T}.GetEnumerator"/>
		public List<T>.Enumerator GetEnumerator()
		{
			return List.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		/// <inheritdoc />
		public bool Contains(T value)
		{
			return List.Contains(value);
		}

		/// <inheritdoc />
		public int IndexOf(T value)
		{
			return List.IndexOf(value);
		}

		/// <inheritdoc />
		public virtual void Add(T data)
		{
			List.Add(data);

			for (var i = 0; i < _updateActions.Count; i++)
			{
				_updateActions[i](List.Count - 1, default, data, ObservableUpdateType.Added);
			}
		}

		/// <inheritdoc />
		public bool Remove(T data)
		{
			var idx = List.IndexOf(data);

			if (idx >= 0)
			{
				RemoveAt(idx);

				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public virtual void RemoveAt(int index)
		{
			var data = List[index];

			List.RemoveAt(index);

			for (var i = _updateActions.Count - 1; i > -1; i--)
			{
				var action = _updateActions[i];

				action(index, data, default, ObservableUpdateType.Removed);

				// Shift the index if an action was unsubscribed
				i = AdjustIndex(i, action);
			}
		}

		/// <inheritdoc />
		public virtual void Clear()
		{
			// Create a copy in case that one of the callbacks modifies the list (Ex: removing a subscriber)
			var copy = _updateActions.ToList();

			for (var i = copy.Count - 1; i > -1; i--)
			{
				for (var j = 0; j < List.Count; j++)
				{
					copy[i](j, List[j], default, ObservableUpdateType.Removed);
				}
			}

			List.Clear();
		}

		/// <inheritdoc />
		public void InvokeUpdate(int index)
		{
			InvokeUpdate(index, List[index]);
		}

		/// <inheritdoc />
		public void Observe(Action<int, T, T, ObservableUpdateType> onUpdate)
		{
			_updateActions.Add(onUpdate);
		}

		/// <inheritdoc />
		public void InvokeObserve(int index, Action<int, T, T, ObservableUpdateType> onUpdate)
		{
			Observe(onUpdate);
			InvokeUpdate(index);
		}

		/// <inheritdoc />
		public void StopObserving(Action<int, T, T, ObservableUpdateType> onUpdate)
		{
			_updateActions.Remove(onUpdate);
		}

		/// <inheritdoc />
		public void StopObservingAll(object subscriber = null)
		{
			if (subscriber == null)
			{
				_updateActions.Clear();
				return;
			}

			for (var i = _updateActions.Count - 1; i > -1; i--)
			{
				if (_updateActions[i].Target == subscriber)
				{
					_updateActions.RemoveAt(i);
				}
			}
		}

		protected void InvokeUpdate(int index, T previousValue)
		{
			var data = List[index];

			for (var i = 0; i < _updateActions.Count; i++)
			{
				_updateActions[i](index, previousValue, data, ObservableUpdateType.Updated);
			}
		}

		private int AdjustIndex(int index, Action<int, T, T, ObservableUpdateType> action)
		{
			if (index < _updateActions.Count && _updateActions[index] == action)
			{
				return index;
			}

			for (var i = index - 1; i > -1; i--)
			{
				if (_updateActions[index] == action)
				{
					return i;
				}
			}

			return index + 1;
		}
	}

	/// <inheritdoc />
	public class ObservableResolverList<T, TOrigin> : ObservableList<T>, IObservableResolverList<T, TOrigin>
	{
		private readonly IList<TOrigin> _originList;
		private readonly Func<TOrigin, T> _fromOrignResolver;
		private readonly Func<T, TOrigin> _toOrignResolver;

		/// <inheritdoc />
		public IReadOnlyList<TOrigin> OriginList => new List<TOrigin>(_originList);

		public ObservableResolverList(IList<TOrigin> originList, 
			Func<TOrigin, T> fromOrignResolver, 
			Func<T, TOrigin> toOrignResolver) :
			base(new List<T>(originList.Count))
		{
			_originList = originList;
			_fromOrignResolver = fromOrignResolver;
			_toOrignResolver = toOrignResolver;

			for (var i = 0; i < originList.Count; i++)
			{
				List.Add(fromOrignResolver(originList[i]));
			}
		}

		/// <inheritdoc />
		public override void Add(T data)
		{
			_originList.Add(_toOrignResolver(data));
			base.Add(data);
		}

		/// <inheritdoc />
		public override void RemoveAt(int index)
		{
			_originList.RemoveAt(index);
			base.RemoveAt(index);
		}

		/// <inheritdoc />
		public override void Clear()
		{
			_originList.Clear();
			base.Clear();
		}

		/// <inheritdoc />
		public void UpdateOrigin(TOrigin value, int index)
		{
			_originList[index] = value;
			List[index] = _fromOrignResolver(value);
		}

		/// <inheritdoc />
		public void AddOrigin(TOrigin value)
		{
			_originList.Add(value);
			List.Add(_fromOrignResolver(value));
		}

		/// <inheritdoc />
		public bool RemoveOrigin(TOrigin value)
		{
			_originList.Remove(value);

			return base.Remove(_fromOrignResolver(value));
		}

		/// <inheritdoc />
		public void ClearOrigin()
		{
			_originList.Clear();
			base.Clear();
		}
	}
}