using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GameLovers.GameData
{
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

		/// <summary>
		/// Rebinds this dictionary to a new origin dictionary and resolver functions without losing existing observers.
		/// The internal dictionary will be rebuilt from the new origin dictionary using the new resolvers.
		/// </summary>
		/// <param name="dictionary">The new origin dictionary to bind to</param>
		/// <param name="fromOrignResolver">The new function to convert from origin types to this dictionary's types</param>
		/// <param name="toOrignResolver">The new function to convert from this dictionary's types to origin types</param>
		void Rebind(IDictionary<TKeyOrigin, TValueOrigin> dictionary,
			Func<KeyValuePair<TKeyOrigin, TValueOrigin>, KeyValuePair<TKey, TValue>> fromOrignResolver,
			Func<TKey, TValue, KeyValuePair<TKeyOrigin, TValueOrigin>> toOrignResolver);
	}

	/// <inheritdoc cref="IObservableResolverDictionary{TKey, TValue, TKeyOrigin, TValueOrigin}"/>
	public class ObservableResolverDictionary<TKey, TValue, TKeyOrigin, TValueOrigin> :
		ObservableDictionary<TKey, TValue>,
		IObservableResolverDictionary<TKey, TValue, TKeyOrigin, TValueOrigin>
	{
		private IDictionary<TKeyOrigin, TValueOrigin> _dictionary;
		private Func<TKey, TValue, KeyValuePair<TKeyOrigin, TValueOrigin>> _toOrignResolver;
		private Func<KeyValuePair<TKeyOrigin, TValueOrigin>, KeyValuePair<TKey, TValue>> _fromOrignResolver;

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
		public void Rebind(IDictionary<TKeyOrigin, TValueOrigin> dictionary,
			Func<KeyValuePair<TKeyOrigin, TValueOrigin>, KeyValuePair<TKey, TValue>> fromOrignResolver,
			Func<TKey, TValue, KeyValuePair<TKeyOrigin, TValueOrigin>> toOrignResolver)
		{
			_dictionary = dictionary;
			_toOrignResolver = toOrignResolver;
			_fromOrignResolver = fromOrignResolver;

			// Rebuild the internal dictionary from the new origin dictionary
			Dictionary.Clear();
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
			if (!Dictionary.TryGetValue(key, out var value)) return false;

			var pair = _toOrignResolver(key, value);

			_dictionary.Remove(pair.Key);

			return base.Remove(key);
		}

		/// <inheritdoc />
		public override void Clear()
		{
			_dictionary.Clear();
			base.Clear();
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

