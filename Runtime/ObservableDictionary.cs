using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <summary>
	/// A simple dictionary with the possibility to observe changes to it's elements defined <see cref="ObservableUpdateType"/> rules
	/// </summary>
	public interface IObservableDictionary : IEnumerable
	{
		/// <summary>
		/// Requests the element count of this dictionary
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Defines the configuration for the observable update done when updating elements in this dictionary
		/// </summary>
		ObservableUpdateFlag ObservableUpdateFlag { get; set; }
	}

	/// <inheritdoc cref="IObservableDictionary"/>
	/// <remarks>
	/// This dictionary only allows to read the elements in it and not to modify it
	/// </remarks>
	public interface IObservableDictionaryReader<TKey, TValue> : IObservableDictionary, IEnumerable<KeyValuePair<TKey, TValue>>
	{
		/// <summary>
		/// Looks up and return the data that is associated with the given <paramref name="key"/>
		/// </summary>
		TValue this[TKey key] { get; }

		/// <summary>
		/// Requests this dictionary as a <see cref="IReadOnlyDictionary{TKey,TValue}"/>
		/// </summary>
		ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary { get; }

		/// <inheritdoc cref="Dictionary{TKey,TValue}.TryGetValue" />
		bool TryGetValue(TKey key, out TValue value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.ContainsKey" />
		bool ContainsKey(TKey key);

		/// <summary>
		/// Observes to this dictionary changes with the given <paramref name="onUpdate"/>
		/// </summary>
		/// <remarks>
		/// It needs the <see cref="this.ObservableUpdateFlag"/> to NOT be set as <see cref="ObservableUpdateFlag.KeyUpdateOnly"/>
		/// </remarks>
		void Observe(Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate);

		/// <summary>
		/// Observes to this dictionary changes with the given <paramref name="onUpdate"/> when the given <paramref name="key"/>
		/// data changes
		/// </summary>
		/// <remarks>
		/// It needs the <see cref="this.ObservableUpdateFlag"/> to NOT be set as <see cref="ObservableUpdateFlag.UpdateOnly"/>
		/// </remarks>
		void Observe(TKey key, Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate);

		/// <inheritdoc cref="Observe(TKey,System.Action{TKey,TValue,TValue,FirstLight.ObservableUpdateType})" />
		/// <remarks>
		/// It invokes the given <paramref name="onUpdate"/> method before starting to observe to this dictionary
		/// </remarks>
		/// <remarks>
		/// It needs the <see cref="this.ObservableUpdateFlag"/> to NOT be set as <see cref="ObservableUpdateFlag.UpdateOnly"/>
		/// </remarks>
		void InvokeObserve(TKey key, Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate);

		/// <summary>
		/// Stops observing this dictionary with the given <paramref name="onUpdate"/> of any data changes
		/// </summary>
		/// <remarks>
		/// It needs the <see cref="this.ObservableUpdateFlag"/> to NOT be set as <see cref="ObservableUpdateFlag.KeyUpdateOnly"/>
		/// </remarks>
		void StopObserving(Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate);

		/// <summary>
		/// Stops observing this dictionary updates for the given <paramref name="key"/>
		/// </summary>
		void StopObserving(TKey key);

		/// <summary>
		/// Stops observing this dictionary changes from all the given <paramref name="subscriber"/> calls.
		/// If the given <paramref name="subscriber"/> is null then will stop observing from everything.
		/// </summary>
		void StopObservingAll(object subscriber = null);
	}

	/// <inheritdoc cref="IObservableDictionary"/>
	public interface IObservableDictionary<TKey, TValue> : IObservableDictionaryReader<TKey, TValue>
	{
		/// <summary>
		/// Changes the given <paramref name="key"/> in the dictionary.
		/// It will notify any observer listing to its data
		/// </summary>
		new TValue this[TKey key] { get; set; }

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Add" />
		void Add(TKey key, TValue value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Remove" />
		bool Remove(TKey key);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Clear"/>
		void Clear();

		/// <remarks>
		/// It invokes any update method that is observing to the given <paramref name="key"/> on this dictionary
		/// </remarks>
		void InvokeUpdate(TKey key);
	}

	/// <inheritdoc />
	/// <remarks>
	/// This interface resolves between 2 dictionaries with different types of keys and values
	/// </remarks>
	public interface IObservableResolverDictionaryReader<TKey, TValue, TKeyOrigin, TValueOrigin> :
		IObservableDictionaryReader<TKey, TValue>
	{
		/// <summary>
		/// The Original Dictionary that is being resolved across the entire interface
		/// </summary>
		ReadOnlyDictionary<TKeyOrigin, TValueOrigin> OriginDictionary { get; }

		/// <summary>
		/// Gets the value from the origin dictionary corresponding to the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the origin dictionary.</param>
		/// <returns>The value from the origin dictionary corresponding to the specified key.</returns>
		TValueOrigin GetOriginValue(TKey key);

		/// <summary>
		/// Attempts to get the value from the origin dictionary corresponding to the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the origin dictionary.</param>
		/// <param name="value">When this method returns, contains the value from the origin dictionary corresponding to the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the origin dictionary contains an element with the specified key; otherwise, false.</returns>
		bool TryGetOriginValue(TKey key, out TValueOrigin value);
	}

	/// <inheritdoc cref="IObservableDictionary{TKey,TValue}"/>
	/// <remarks>
	/// This interface resolves between 2 dictionaries with different types of keys and values
	/// </remarks>
	public interface IObservableResolverDictionary<TKey, TValue, TKeyOrigin, TValueOrigin> :
		IObservableResolverDictionaryReader<TKey, TValue, TKeyOrigin, TValueOrigin>,
		IObservableDictionary<TKey, TValue>
	{
		/// <summary>
		/// Updates the value in the origin dictionary corresponding to the specified origin key.
		/// </summary>
		/// <param name="key">The key of the value to update in the origin dictionary.</param>
		/// <param name="value">The new value to set in the origin dictionary.</param>
		void UpdateOrigin(TKeyOrigin key, TValueOrigin value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Add" />
		/// <remarks>
		/// Add's to the origin dictionary
		/// </remarks>
		void AddOrigin(TKeyOrigin key, TValueOrigin value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Remove" />
		/// <remarks>
		/// Remove's to the origin dictionary
		/// </remarks>
		bool RemoveOrigin(TKeyOrigin key);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Clear" />
		/// <remarks>
		/// Clear's to the origin dictionary
		/// </remarks>
		void ClearOrigin();
	}

	/// <inheritdoc />
	public class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, IList<Action<TKey, TValue, TValue, ObservableUpdateType>>> _keyUpdateActions =
			new Dictionary<TKey, IList<Action<TKey, TValue, TValue, ObservableUpdateType>>>();
		private readonly IList<Action<TKey, TValue, TValue, ObservableUpdateType>> _updateActions =
			new List<Action<TKey, TValue, TValue, ObservableUpdateType>>();

		/// <inheritdoc />
		public int Count => Dictionary.Count;
		/// <inheritdoc />
		public ObservableUpdateFlag ObservableUpdateFlag { get; set; }
		/// <inheritdoc />
		public ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary => new ReadOnlyDictionary<TKey, TValue>(Dictionary);

		protected virtual IDictionary<TKey, TValue> Dictionary { get; }

		private ObservableDictionary() { }

		public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
		{
			Dictionary = dictionary;
			ObservableUpdateFlag = ObservableUpdateFlag.KeyUpdateOnly;
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}.this" />
		public TValue this[TKey key]
		{
			get => Dictionary[key];
			set
			{
				var previousValue = Dictionary[key];

				Dictionary[key] = value;

				InvokeUpdate(key, previousValue);
			}
		}

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return Dictionary.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public bool TryGetValue(TKey key, out TValue value)
		{
			return Dictionary.TryGetValue(key, out value);
		}

		/// <inheritdoc />
		public bool ContainsKey(TKey key)
		{
			return Dictionary.ContainsKey(key);
		}

		/// <inheritdoc />
		public virtual void Add(TKey key, TValue value)
		{
			Dictionary.Add(key, value);

			if (ObservableUpdateFlag != ObservableUpdateFlag.UpdateOnly && _keyUpdateActions.TryGetValue(key, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](key, default, value, ObservableUpdateType.Added);
				}
			}

			if (ObservableUpdateFlag != ObservableUpdateFlag.KeyUpdateOnly)
			{
				for (var i = 0; i < _updateActions.Count; i++)
				{
					_updateActions[i](key, default, value, ObservableUpdateType.Added);
				}
			}
		}

		/// <inheritdoc />
		public virtual bool Remove(TKey key)
		{
			if (!Dictionary.TryGetValue(key, out var value))
			{
				return false;
			}

			Dictionary.Remove(key);

			if (ObservableUpdateFlag != ObservableUpdateFlag.UpdateOnly && _keyUpdateActions.TryGetValue(key, out var actions))
			{
				var listCopy = actions.ToList();
				for (var i = 0; i < listCopy.Count; i++)
				{
					listCopy[i](key, value, default, ObservableUpdateType.Removed);
				}
			}
			if (ObservableUpdateFlag != ObservableUpdateFlag.KeyUpdateOnly)
			{
				var listCopy = _updateActions.ToList();
				for (var i = 0; i < listCopy.Count; i++)
				{
					listCopy[i](key, value, default, ObservableUpdateType.Removed);
				}
			}

			return true;
		}

		/// <inheritdoc />
		public virtual void Clear()
		{
			if (ObservableUpdateFlag != ObservableUpdateFlag.UpdateOnly)
			{
				// Create a copy in case that one of the callbacks modifies the list (Ex: removing a subscriber)
				var copy = new Dictionary<TKey, IList<Action<TKey, TValue, TValue, ObservableUpdateType>>>(_keyUpdateActions);

				foreach (var data in copy)
				{
					var listCopy = data.Value.ToList();
					for (var i = 0; i < listCopy.Count; i++)
					{
						listCopy[i](data.Key, Dictionary[data.Key], default, ObservableUpdateType.Removed);
					}
				}
			}

			if (ObservableUpdateFlag != ObservableUpdateFlag.KeyUpdateOnly)
			{
				foreach (var data in Dictionary)
				{
					var listCopy = _updateActions.ToList();
					for (var i = 0; i < listCopy.Count; i++)
					{
						listCopy[i](data.Key, data.Value, default, ObservableUpdateType.Removed);
					}
				}
			}

			Dictionary.Clear();
		}

		/// <inheritdoc />
		public void InvokeUpdate(TKey key)
		{
			InvokeUpdate(key, Dictionary[key]);
		}

		/// <inheritdoc />
		public void StopObserving(TKey key)
		{
			_keyUpdateActions.Remove(key);
		}

		/// <inheritdoc />
		public void Observe(Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate)
		{
			_updateActions.Add(onUpdate);
		}

		/// <inheritdoc />
		public void Observe(TKey key, Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate)
		{
			var list = new List<Action<TKey, TValue, TValue, ObservableUpdateType>> { onUpdate };

			if (_keyUpdateActions.TryGetValue(key, out var listeners))
			{
				listeners.Add(onUpdate);
			}
			else
			{
				_keyUpdateActions.Add(key, new List<Action<TKey, TValue, TValue, ObservableUpdateType>> { onUpdate });
			}
		}

		/// <inheritdoc />
		public void InvokeObserve(TKey key, Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate)
		{
			Observe(key, onUpdate);
			InvokeUpdate(key);
		}

		/// <inheritdoc />
		public void StopObserving(Action<TKey, TValue, TValue, ObservableUpdateType> onUpdate)
		{
			foreach (var actions in _keyUpdateActions)
			{
				for (var i = actions.Value.Count - 1; i > -1; i--)
				{
					if (actions.Value[i] == onUpdate)
					{
						actions.Value.RemoveAt(i);
						break;
					}
				}
			}

			for (var i = _updateActions.Count - 1; i > -1; i--)
			{
				if (_updateActions[i] == onUpdate)
				{
					_updateActions.RemoveAt(i);
					break;
				}
			}
		}

		/// <inheritdoc />
		public void StopObservingAll(object subscriber = null)
		{
			if (subscriber == null)
			{
				_keyUpdateActions.Clear();
				_updateActions.Clear();
				return;
			}

			foreach (var actions in _keyUpdateActions)
			{
				for (var i = actions.Value.Count - 1; i > -1; i--)
				{
					if (actions.Value[i].Target == subscriber)
					{
						actions.Value.RemoveAt(i);
					}
				}
			}

			for (var i = _updateActions.Count - 1; i > -1; i--)
			{
				if (_updateActions[i].Target == subscriber)
				{
					_updateActions.RemoveAt(i);
				}
			}
		}

		protected void InvokeUpdate(TKey key, TValue previousValue)
		{
			var value = Dictionary[key];

			if (ObservableUpdateFlag != ObservableUpdateFlag.UpdateOnly && _keyUpdateActions.TryGetValue(key, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](key, previousValue, value, ObservableUpdateType.Updated);
				}
			}

			if (ObservableUpdateFlag != ObservableUpdateFlag.KeyUpdateOnly)
			{
				for (var i = 0; i < _updateActions.Count; i++)
				{
					_updateActions[i](key, previousValue, value, ObservableUpdateType.Updated);
				}
			}
		}
	}

	/// <inheritdoc />
	public class ObservableResolverDictionary<TKey, TValue, TKeyOrigin, TValueOrigin> : 
		ObservableDictionary<TKey, TValue>,
		IObservableResolverDictionary<TKey, TValue, TKeyOrigin, TValueOrigin>
	{
		private readonly IDictionary<TKeyOrigin, TValueOrigin> _dictionary;
		private readonly Func<TKey, TValue, KeyValuePair<TKeyOrigin, TValueOrigin>> _toOrignResolver;
		private readonly Func<KeyValuePair<TKeyOrigin, TValueOrigin>, KeyValuePair<TKey, TValue>> _fromOrignResolver;

		/// <inheritdoc />
		public ReadOnlyDictionary<TKeyOrigin, TValueOrigin> OriginDictionary => new ReadOnlyDictionary<TKeyOrigin, TValueOrigin>(_dictionary);

		public ObservableResolverDictionary(IDictionary<TKeyOrigin, TValueOrigin> dictionary,
			Func<KeyValuePair<TKeyOrigin, TValueOrigin>, KeyValuePair<TKey, TValue>> fromOrignResolver,
			Func<TKey, TValue, KeyValuePair<TKeyOrigin, TValueOrigin>> toOrignResolver)
			: base(new Dictionary<TKey, TValue>(dictionary.Count))
		{
			_dictionary = dictionary;
			_toOrignResolver = toOrignResolver;
			_fromOrignResolver = fromOrignResolver;

			foreach (var pair in dictionary)
			{
				Dictionary.Add(fromOrignResolver(pair));
			}
		}

		/// <inheritdoc />
		public TValueOrigin GetOriginValue(TKey key)
		{
			return _dictionary[_toOrignResolver(key, default).Key];
		}

		/// <inheritdoc />
		public bool TryGetOriginValue(TKey key, out TValueOrigin value)
		{
			return _dictionary.TryGetValue(_toOrignResolver(key, default).Key, out value);
		}

		/// <inheritdoc />
		public void UpdateOrigin(TKeyOrigin key, TValueOrigin value)
		{
			var convertPair = _fromOrignResolver(new KeyValuePair<TKeyOrigin, TValueOrigin>(key, value));

			_dictionary[key] = value;
			this[convertPair.Key] = convertPair.Value;
		}

		/// <inheritdoc />
		public override void Add(TKey key, TValue value)
		{
			_dictionary.Add(_toOrignResolver(key, value));
			base.Add(key, value);
		}

		/// <inheritdoc />
		public override bool Remove(TKey key)
		{
			if(!Dictionary.TryGetValue(key, out var value)) return false;

			var pair = _toOrignResolver(key, value);

			_dictionary.Remove(pair.Key);

			return base.Remove(key);
		}

		/// <inheritdoc />
		public override void Clear()
		{
			_dictionary.Clear();
			base.Clear(); ;
		}

		/// <inheritdoc />
		public void AddOrigin(TKeyOrigin key, TValueOrigin value)
		{
			var convertPair = _fromOrignResolver(new KeyValuePair<TKeyOrigin, TValueOrigin>(key, value));

			_dictionary.Add(key, value);
			base.Add(convertPair.Key, convertPair.Value);
		}

		/// <inheritdoc />
		public bool RemoveOrigin(TKeyOrigin key)
		{
			if (!_dictionary.TryGetValue(key, out var value)) return false;

			var convertPair = _fromOrignResolver(new KeyValuePair<TKeyOrigin, TValueOrigin>(key, value));

			_dictionary.Remove(key);
			return base.Remove(convertPair.Key);
		}

		/// <inheritdoc />
		public void ClearOrigin()
		{
			_dictionary.Clear();
			base.Clear();
		}
	}
}