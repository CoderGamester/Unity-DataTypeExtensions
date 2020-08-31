using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <summary>
	/// A field with the possibility to observe changes to it's elements defined <see cref="ObservableUpdateType"/> rules
	/// </summary>
	public interface IObservableFieldReader<out T>
	{
		/// <summary>
		/// The field value
		/// </summary>
		T Value { get; }
		
		/// <summary>
		/// Observes this field with the given <paramref name="onUpdate"/> when any data changes
		/// </summary>
		void Observe(Action<T> onUpdate);
		
		/// <inheritdoc cref="Observe" />
		/// <remarks>
		/// It invokes the given <paramref name="onUpdate"/> method before starting to observe to this field
		/// </remarks>
		void InvokeObserve(Action<T> onUpdate);
		
		/// <summary>
		/// Stops observing this field with the given <paramref name="onUpdate"/> of any data changes
		/// </summary>
		void StopObserving(Action<T> onUpdate);
	}

	/// <inheritdoc />
	public interface IObservableField<T> : IObservableFieldReader<T>
	{
		/// <summary>
		/// The field value with possibility to be changed
		/// </summary>
		new T Value { get; set; }
	}
	
	/// <inheritdoc />
	public class ObservableField<T> : IObservableField<T>
	{
		private readonly Func<T> _fieldResolver;
		private readonly Action<T> _fieldSetter;
		private readonly IList<Action<T>> _updateActions = new List<Action<T>>();

		/// <inheritdoc cref="IObservableField{T}.Value" />
		public T Value
		{
			get => _fieldResolver();
			set
			{
				_fieldSetter(value);
				InvokeUpdates(value);
			}
		}

		private ObservableField() {}
 
		public ObservableField(Func<T> fieldResolver, Action<T> fieldSetter)
		{
			_fieldResolver = fieldResolver;
			_fieldSetter = fieldSetter;
		}
		
		public static implicit operator T(ObservableField<T> value) => value.Value;

		/// <inheritdoc />
		public void Observe(Action<T> onUpdate)
		{
			_updateActions.Add(onUpdate);
		}

		/// <inheritdoc />
		public void InvokeObserve(Action<T> onUpdate)
		{
			onUpdate(Value);
			
			Observe(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(Action<T> onUpdate)
		{
			_updateActions.Remove(onUpdate);
		}

		private void InvokeUpdates(T value)
		{
			for (var i = 0; i < _updateActions.Count; i++)
			{
				_updateActions[i].Invoke(value);
			}
		}
	}
}