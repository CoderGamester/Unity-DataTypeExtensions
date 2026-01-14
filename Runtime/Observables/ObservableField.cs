using System;
using System.Collections.Generic;

namespace GameLovers.GameData
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
		/// Starts a batch update for this field. 
		/// Notifications will be suppressed until the returned object is disposed.
		/// </summary>
		IDisposable BeginBatch();

		/// <summary>
		/// Observes this field with the given <paramref name="onUpdate"/> when any data changes
		/// </summary>
		void Observe(Action<T, T> onUpdate);

		/// <inheritdoc cref="Observe" />
		/// <remarks>
		/// It invokes the given <paramref name="onUpdate"/> method before starting to observe to this field
		/// </remarks>
		void InvokeObserve(Action<T, T> onUpdate);

		/// <summary>
		/// Stops observing this field with the given <paramref name="onUpdate"/> of any data changes
		/// </summary>
		void StopObserving(Action<T, T> onUpdate);

		/// <summary>
		/// Stops observing this field from all the given <paramref name="subscriber"/> calls.
		/// If the given <paramref name="subscriber"/> is null then will stop observing from everything.
		/// </summary>
		void StopObservingAll(object subscriber = null);

		/// <remarks>
		/// It invokes any update method that is observing to this field
		/// </remarks>
		void InvokeUpdate();
	}

	/// <inheritdoc />
	public interface IObservableField<T> : IObservableFieldReader<T>
	{
		/// <summary>
		/// The field value with possibility to be changed
		/// </summary>
		new T Value { get; set; }

		/// <summary>
		/// Rebinds this field to a new value without losing existing observers.
		/// </summary>
		/// <param name="initialValue">The new initial value for the field</param>
		void Rebind(T initialValue);
	}

	/// <inheritdoc />
	/// <remarks>
	/// A resolver field with the possibility to rebind to new resolver functions
	/// </remarks>
	public interface IObservableResolverField<T> : IObservableField<T>
	{
		/// <summary>
		/// Rebinds this field to new resolver functions without losing existing observers
		/// </summary>
		/// <param name="fieldResolver">The new getter function for the field</param>
		/// <param name="fieldSetter">The new setter function for the field</param>
		void Rebind(Func<T> fieldResolver, Action<T> fieldSetter);
	}

	/// <inheritdoc />
	public class ObservableField<T> : IObservableField<T>, IBatchable, IComputedDependency
	{
		private readonly IList<Action<T, T>> _updateActions = new List<Action<T, T>>();
		private readonly List<Action> _dependencyActions = new List<Action>();

		private T _value;
		private bool _isBatching;
		private T _batchPreviousValue;

		/// <inheritdoc cref="IObservableField{T}.Value" />
		public virtual T Value
		{
			get
			{
				ComputedTracker.OnRead(this);
				return _value;
			}
			set
			{
				var previousValue = _value;

				_value = value;
				InvokeUpdate(previousValue);
			}
		}

		public ObservableField()
		{
			_value = default;
		}

		public ObservableField(T initialValue)
		{
			_value = initialValue;
		}

		public static implicit operator T(ObservableField<T> value) => value.Value;

		/// <inheritdoc />
		void IComputedDependency.Subscribe(Action onDependencyChanged)
		{
			_dependencyActions.Add(onDependencyChanged);
		}

		/// <inheritdoc />
		void IComputedDependency.Unsubscribe(Action onDependencyChanged)
		{
			_dependencyActions.Remove(onDependencyChanged);
		}

		/// <inheritdoc />
		public IDisposable BeginBatch()
		{
			var batch = new ObservableBatch();
			batch.Add(this);
			return batch;
		}

		/// <inheritdoc />
		void IBatchable.SuppressNotifications()
		{
			if (!_isBatching)
			{
				_isBatching = true;
				_batchPreviousValue = _value;
			}
		}

		/// <inheritdoc />
		void IBatchable.ResumeNotifications()
		{
			if (_isBatching)
			{
				_isBatching = false;
				InvokeUpdate(_batchPreviousValue);
			}
		}

		/// <inheritdoc />
		public void Rebind(T initialValue)
		{
			_value = initialValue;
		}

		/// <inheritdoc />
		public void Observe(Action<T, T> onUpdate)
		{
			_updateActions.Add(onUpdate);
		}

		/// <inheritdoc />
		public void InvokeObserve(Action<T, T> onUpdate)
		{
			onUpdate(Value, Value);

			Observe(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(Action<T, T> onUpdate)
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

		/// <inheritdoc />
		public void InvokeUpdate()
		{
			InvokeUpdate(Value);
		}

		protected void InvokeUpdate(T previousValue)
		{
			if (_isBatching)
			{
				return;
			}

			// Cache value to avoid repeated Value getter calls (which trigger ComputedTracker.OnRead)
			var currentValue = GetCurrentValue();

			for (var i = 0; i < _updateActions.Count; i++)
			{
				_updateActions[i].Invoke(previousValue, currentValue);
			}

			for (var i = 0; i < _dependencyActions.Count; i++)
			{
				_dependencyActions[i].Invoke();
			}
		}

		/// <summary>
		/// Gets the current value without triggering dependency tracking.
		/// Override in derived classes that store values differently.
		/// </summary>
		protected virtual T GetCurrentValue() => _value;
	}

	/// <inheritdoc cref="IObservableResolverField{T}"/>
	public class ObservableResolverField<T> : ObservableField<T>, IObservableResolverField<T>
	{
		private Func<T> _fieldResolver;
		private Action<T> _fieldSetter;

		/// <inheritdoc cref="IObservableField{T}.Value" />
		public override T Value
		{
			get
			{
				ComputedTracker.OnRead(this);
				return _fieldResolver();
			}
			set
			{
				var previousValue = _fieldResolver();

				_fieldSetter(value);

				InvokeUpdate(previousValue);
			}
		}

		private ObservableResolverField() { }

		public ObservableResolverField(Func<T> fieldResolver, Action<T> fieldSetter)
		{
			_fieldResolver = fieldResolver;
			_fieldSetter = fieldSetter;
		}

		/// <summary>
		/// Rebinds this field to new resolver functions without losing existing observers
		/// </summary>
		/// <param name="fieldResolver">The new getter function for the field</param>
		/// <param name="fieldSetter">The new setter function for the field</param>
		public void Rebind(Func<T> fieldResolver, Action<T> fieldSetter)
		{
			_fieldResolver = fieldResolver;
			_fieldSetter = fieldSetter;
		}

		/// <inheritdoc />
		protected override T GetCurrentValue() => _fieldResolver();

		public static implicit operator T(ObservableResolverField<T> value) => value.Value;
	}
}
