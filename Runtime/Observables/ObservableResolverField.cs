using System;

namespace GameLovers.GameData
{
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
		/// Implicit operator to convert the field to its value
		/// </summary>
		public static implicit operator T(ObservableResolverField<T> value) => value.Value;

		/// <summary>
		/// Rebinds this field to new resolver functions without losing existing observers
		/// </summary>
		public void Rebind(Func<T> fieldResolver, Action<T> fieldSetter)
		{
			_fieldResolver = fieldResolver;
			_fieldSetter = fieldSetter;
		}

		/// <inheritdoc />
		protected override T GetCurrentValue() => _fieldResolver();
	}
}

