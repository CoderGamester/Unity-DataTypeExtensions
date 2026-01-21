using System;
using System.Collections.Generic;

namespace GameLovers.GameData
{
	/// <summary>
	/// Interface for observables that support batching updates.
	/// </summary>
	public interface IBatchable
	{
		/// <summary>
		/// Starts suppressing individual updates.
		/// </summary>
		void SuppressNotifications();

		/// <summary>
		/// Stops suppressing updates and triggers a consolidated notification.
		/// </summary>
		void ResumeNotifications();
	}

	/// <summary>
	/// A disposable object that suppresses notifications for a group of observables during its lifetime.
	/// Notifications are consolidated and fired once when the object is disposed.
	/// </summary>
	public class ObservableBatch : IDisposable
	{
		private readonly List<IBatchable> _observables = new List<IBatchable>();
		private bool _disposed;

		/// <summary>
		/// Adds an observable to this batch.
		/// </summary>
		public void Add(IBatchable observable)
		{
			if (_disposed) throw new ObjectDisposedException(nameof(ObservableBatch));
			
			_observables.Add(observable);
			observable.SuppressNotifications();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_disposed) return;

			foreach (var observable in _observables)
			{
				observable.ResumeNotifications();
			}

			_observables.Clear();
			_disposed = true;
		}
	}
}
