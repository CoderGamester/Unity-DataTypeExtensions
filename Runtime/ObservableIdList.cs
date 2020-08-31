using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <summary>
	/// This interface wraps the access to a list of structs by a defined generic id.
	/// It is possible get, add, remove and set a list element by its Id.
	/// It is possible to observe changes in the list from one of the defined <see cref="ObservableUpdateType"/> rules
	/// </summary>
	public interface IObservableIdList : IEnumerable
	{
		/// <summary>
		/// Requests the list element count
		/// </summary>
		int Count { get; }
	}

	/// <inheritdoc cref="IObservableIdList"/>
	/// <remarks>
	/// Read only uniqueId list interface
	/// </remarks>
	public interface IObservableIdListReader<in TKey, TValue> : IObservableIdList, IEnumerable<TValue>
		where TValue : struct
	{
		/// <summary>
		/// Looks up and return the data that is associated with the given <paramref name="key"/>
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown when there is no data is associated with the given <paramref name="key"/>
		/// </exception>
		TValue this[TKey key] { get; }
		
		/// <summary>
		/// Requests this list as a <see cref="IReadOnlyList{T}"/>
		/// </summary>
		IReadOnlyList<TValue> ReadOnlyList { get; }
			
		/// <summary>
		/// Looks up the data that is associated with the given <paramref name="key"/>.
		/// It return true if is able to delivery out the given <paramref name="value"/>.
		/// The <paramref name="value"/> will be the default if returns false
		/// </summary>
		bool TryGet(TKey key, out TValue value);

		/// <summary>
		/// Observes this list with the given <paramref name="onUpdate"/> when the given <paramref name="id"/> data
		/// changes following the rule of the given <paramref name="updateType"/>
		/// </summary>
		void Observe(TKey id, ObservableUpdateType updateType, Action<TValue> onUpdate);
		
		/// <inheritdoc cref="Observe(TKey,GameLovers.ObservableUpdateType,System.Action{TValue})" />
		/// <remarks>
		/// It invokes the given <paramref name="onUpdate"/> method before starting to observe to this list
		/// </remarks>
		void InvokeObserve(TKey id, ObservableUpdateType updateType, Action<TValue> onUpdate);
		
		/// <summary>
		/// Observes this list with the given <paramref name="onUpdate"/> when any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void Observe(ObservableUpdateType updateType, Action<TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this list with the given <paramref name="onUpdate"/> of the given <paramref name="id"/> data
		/// changes following the rule of the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(TKey id, ObservableUpdateType updateType, Action<TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this list with the given <paramref name="onUpdate"/> of any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(ObservableUpdateType updateType, Action<TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this list updates for the given <paramref name="id"/>
		/// </summary>
		void StopObserving(TKey id);
	}

	/// <inheritdoc cref="IObservableIdList" />
	public interface IObservableIdList<in TKey, TValue> : IObservableIdListReader<TKey, TValue>
		where TValue : struct
	{
		/// <summary>
		/// Changes the given <paramref name="key"/> in the list. If the data does not exist it will be added.
		/// It will notify any observer listing to its data
		/// </summary>
		new TValue this[TKey key] { get; set; }
		
		/// <summary>
		/// Add the given <paramref name="data"/> to the list.
		/// It will notify any observer listing to its data
		/// </summary>
		void Add(TValue data);
		
		/// <summary>
		/// Removes the data associated with the given <paramref name="id"/>
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown if there is no data associated with the given <paramref name="id"/>
		/// </exception>
		void Remove(TKey id);
		
		/// <summary>
		/// Removes the the given <paramref name="data"/>
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown if there is no <paramref name="data"/> in this list
		/// </exception>
		void RemoveData(TValue data);
 
		/// <summary>
		/// Removes the data associated with the given <paramref name="id"/> if present in the list
		/// </summary>
		void TryRemove(TKey id);
 
		/// <summary>
		/// Removes the <paramref name="data"/> if present in the list
		/// </summary>
		void TryRemoveData(TValue data);
	}
 
	/// <inheritdoc />
	public class ObservableIdList<TKey, TValue> : IObservableIdList<TKey, TValue>
		where TValue : struct
	{
		private readonly IList<TValue> _list;
		private readonly Func<TValue, TKey> _referenceIdResolver;
		private readonly EqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;
		private readonly IDictionary<TKey, IList<Action<TValue>>> _onAddActions = new Dictionary<TKey, IList<Action<TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TValue>>> _onUpdateActions = new Dictionary<TKey, IList<Action<TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TValue>>> _onRemoveActions = new Dictionary<TKey, IList<Action<TValue>>>();
		private readonly IReadOnlyDictionary<int, IList<Action<TValue>>> _genericUpdateActions = 
			new ReadOnlyDictionary<int, IList<Action<TValue>>>(new Dictionary<int, IList<Action<TValue>>>
			{
				{(int) ObservableUpdateType.Added, new List<Action<TValue>>()},
				{(int) ObservableUpdateType.Removed, new List<Action<TValue>>()},
				{(int) ObservableUpdateType.Updated, new List<Action<TValue>>()}
			});

		/// <inheritdoc cref="IObservableIdList{TKey,TValue}.this" />
		public TValue this[TKey key]
		{
			get
			{
				if (TryGet(key, out var data))
				{
					return data;
				}

				throw new ArgumentException($"Can not find {typeof(TValue).Name} to id {key.ToString()}");
			}
			set
			{
				var id = _referenceIdResolver(value);
				int index = FindIndex(id);
				if (index < 0)
				{
					Add(value);
				}
				else
				{
					_list[index] = value;
				}
 
				if (_onUpdateActions.TryGetValue(id, out var actions))
				{
					for (var i = 0; i < actions.Count; i++)
					{
						actions[i](value);
					}
				}

				var updates = _genericUpdateActions[(int) ObservableUpdateType.Updated];
				for (var i = 0; i < updates.Count; i++)
				{
					updates[i](value);
				}
			}
		}

		/// <inheritdoc />
		public int Count => _list.Count;
		/// <inheritdoc />
		public IReadOnlyList<TValue> ReadOnlyList => new ReadOnlyCollection<TValue>(_list);
		
		private ObservableIdList() {}
 
		public ObservableIdList(Func<TValue, TKey> referenceIdResolver, IList<TValue> list)
		{
			_referenceIdResolver = referenceIdResolver;
			_list = list;
		}

		/// <inheritdoc />
		public IEnumerator<TValue> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public bool TryGet(TKey id, out TValue value)
		{
			int index = FindIndex(id);
			if (index < 0)
			{
				value = default;
				
				return false;
			}
 
			value = _list[index];

			return true;
		}

		/// <inheritdoc />
		public void Observe(TKey id, ObservableUpdateType updateType, Action<TValue> onUpdate)
		{
			switch (updateType)
			{
				case ObservableUpdateType.Added:
					if (!_onAddActions.TryGetValue(id, out var addList))
					{
						addList = new List<Action<TValue>>();
						
						_onAddActions.Add(id, addList);
					}
					
					addList.Add(onUpdate);
					break;
				case ObservableUpdateType.Updated:
					if (!_onUpdateActions.TryGetValue(id, out var updateList))
					{
						updateList = new List<Action<TValue>>();
						
						_onUpdateActions.Add(id, updateList);
					}
					
					updateList.Add(onUpdate);
					break;
				case ObservableUpdateType.Removed:
					if (!_onRemoveActions.TryGetValue(id, out var removeList))
					{
						removeList = new List<Action<TValue>>();
						
						_onRemoveActions.Add(id, removeList);
					}
					
					removeList.Add(onUpdate);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(updateType), updateType, "Wrong update type");
			}
		}

		/// <inheritdoc />
		public void InvokeObserve(TKey id, ObservableUpdateType updateType, Action<TValue> onUpdate)
		{
			onUpdate(_list[FindIndex(id)]);
			
			Observe(id, updateType, onUpdate);
		}

		/// <inheritdoc />
		public void Observe(ObservableUpdateType updateType, Action<TValue> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Add(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(TKey id, ObservableUpdateType updateType, Action<TValue> onUpdate)
		{
			switch (updateType)
			{
				case ObservableUpdateType.Added:
					if (_onAddActions.TryGetValue(id, out var addList))
					{
						addList.Remove(onUpdate);
					}
					break;
				case ObservableUpdateType.Updated:
					if (_onUpdateActions.TryGetValue(id, out var updateList))
					{
						updateList.Remove(onUpdate);
					}
					break;
				case ObservableUpdateType.Removed:
					if (_onRemoveActions.TryGetValue(id, out var removeList))
					{
						removeList.Remove(onUpdate);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(updateType), updateType, "Wrong update type");
			}
		}

		/// <inheritdoc />
		public void StopObserving(ObservableUpdateType updateType, Action<TValue> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Remove(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(TKey id)
		{
			if (_onAddActions.TryGetValue(id, out var addList))
			{
				addList.Clear();

				_onAddActions.Remove(id);
			}
			if (_onUpdateActions.TryGetValue(id, out var updateList))
			{
				updateList.Clear();

				_onUpdateActions.Remove(id);
			}
			if (_onRemoveActions.TryGetValue(id, out var removeList))
			{
				removeList.Clear();

				_onRemoveActions.Remove(id);
			}
		}

		/// <inheritdoc />
		public void Add(TValue data)
		{
			var id = _referenceIdResolver(data);
			if (FindIndex(id) >= 0)
			{
				throw new ArgumentException($"Cannot add {nameof(TValue)} with id {id.ToString()}, because it already exists");
			}
 
			_list.Add(data);
			
			if (_onAddActions.TryGetValue(id, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](data);
				}
			}

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Added];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](data);
			}
		}

		/// <inheritdoc />
		public void Remove(TKey id)
		{
			int index = FindIndex(id);
			if (index < 0)
			{
				throw new ArgumentException($"Cannot remove {nameof(TValue)} with id {id.ToString()}, because it does not exists");
			}

			Remove(index, id);
		}

		/// <inheritdoc />
		public void RemoveData(TValue data)
		{
			Remove(_referenceIdResolver(data));
		}

		/// <inheritdoc />
		public void TryRemove(TKey id)
		{
			int index = FindIndex(id);
			if (index >= 0)
			{
				Remove(index, id);
			}
		}

		/// <inheritdoc />
		public void TryRemoveData(TValue data)
		{
			TryRemove(_referenceIdResolver(data));
		}
		
		private int FindIndex(TKey id)
		{
			var list = _list;
			for (var i = 0; i < list.Count; i++)
			{
				if (_comparer.Equals(_referenceIdResolver(list[i]), id))
				{
					return i;
				}
			}
 
			return -1;
		}

		private void Remove(int index, TKey id)
		{
			var data = _list[index];
			
			_list.RemoveAt(index);
			
			if (_onRemoveActions.TryGetValue(id, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](data);
				}
			}

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Removed];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](data);
			}
		}
	}
}