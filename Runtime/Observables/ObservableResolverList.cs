using System;
using System.Collections.Generic;

namespace GameLovers.GameData
{
	/// <inheritdoc />
	/// <remarks>
	/// This interface resolves between 2 lists with different types of values
	/// </remarks>
	public interface IObservableResolverListReader<T, out TOrigin> : IObservableListReader<T>
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

		/// <summary>
		/// Rebinds this list to a new origin list and resolver functions without losing existing observers.
		/// The internal list will be rebuilt from the new origin list using the new resolvers.
		/// </summary>
		/// <param name="originList">The new origin list to bind to</param>
		/// <param name="fromOrignResolver">The new function to convert from origin type to this list's type</param>
		/// <param name="toOrignResolver">The new function to convert from this list's type to origin type</param>
		void Rebind(IList<TOrigin> originList, Func<TOrigin, T> fromOrignResolver, Func<T, TOrigin> toOrignResolver);
	}

	/// <inheritdoc cref="IObservableResolverList{T, TOrigin}"/>
	/// <remarks>
	/// This class resolves between 2 lists with different types of values
	/// </remarks>
	public class ObservableResolverList<T, TOrigin> : ObservableList<T>, IObservableResolverList<T, TOrigin>
	{
		private IList<TOrigin> _originList;
		private Func<TOrigin, T> _fromOrignResolver;
		private Func<T, TOrigin> _toOrignResolver;

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
		public void Rebind(IList<TOrigin> originList,
			Func<TOrigin, T> fromOrignResolver,
			Func<T, TOrigin> toOrignResolver)
		{
			_originList = originList;
			_fromOrignResolver = fromOrignResolver;
			_toOrignResolver = toOrignResolver;

			// Rebuild the internal list from the new origin list
			List.Clear();
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

