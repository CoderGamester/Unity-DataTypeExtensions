using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GameLovers.GameData
{
	// ═══════════════════════════════════════════════════════════════════════════
	// EDITOR-ONLY: Observable Debug Window Support
	// ═══════════════════════════════════════════════════════════════════════════
	// This file implements an internal registry used by the Observable Debugger
	// window to discover and inspect observable instances without requiring any
	// manual registration calls in user code.
	//
	// Design notes:
	// - Uses weak references so tracked observables do not leak memory.
	// - Stores value/subscriber getters as delegates so the debug window can poll
	//   live data without reflection.
	// - Compiled out in player builds.
	// ═══════════════════════════════════════════════════════════════════════════

	/// <summary>
	/// Static registry used by the Observable Debugger window to discover and inspect observable instances.
	/// Observables automatically register themselves via the self-registration pattern implemented in each
	/// observable class's editor-only code block.
	/// </summary>
	/// <remarks>
	/// <para>Uses weak references so tracked observables do not prevent garbage collection.</para>
	/// <para>All members are compiled out in player builds via <c>#if UNITY_EDITOR</c>.</para>
	/// </remarks>
	public static class ObservableDebugRegistry
	{
#if UNITY_EDITOR
		private static int _nextId = 1;
		private static readonly ConditionalWeakTable<object, Entry> _entries = new ConditionalWeakTable<object, Entry>();
		private static readonly List<WeakReference<object>> _refs = new List<WeakReference<object>>();

		/// <summary>
		/// Registers an observable instance with the debug registry.
		/// Called automatically by observable constructors in editor builds.
		/// </summary>
		/// <param name="instance">The observable instance to register.</param>
		/// <param name="kind">The observable type category (Field, Computed, List, Dictionary, HashSet).</param>
		/// <param name="valueGetter">Delegate to get the current value as a string.</param>
		/// <param name="subscriberCountGetter">Delegate to get the current subscriber count.</param>
		internal static void Register(
			object instance,
			string kind,
			Func<string> valueGetter,
			Func<int> subscriberCountGetter)
		{
			if (instance == null) return;

			if (_entries.TryGetValue(instance, out _))
			{
				return;
			}

			var name = GetAutoName(kind, instance.GetType());
			var info = new ObservableDebugInfo(_nextId++, name, kind, DateTime.UtcNow);
			_entries.Add(instance, new Entry(info, valueGetter, subscriberCountGetter));
			_refs.Add(new WeakReference<object>(instance));
		}

		/// <summary>
		/// Enumerates snapshots of all currently tracked observable instances.
		/// Automatically cleans up entries for garbage-collected instances.
		/// </summary>
		public static IEnumerable<EntrySnapshot> EnumerateSnapshots()
		{
			for (int i = _refs.Count - 1; i >= 0; i--)
			{
				if (_refs[i].TryGetTarget(out var instance) && _entries.TryGetValue(instance, out var entry))
				{
					yield return entry.ToSnapshot(instance);
				}
				else
				{
					_refs.RemoveAt(i);
				}
			}
		}

		private static string GetAutoName(string kind, Type instanceType)
		{
			try
			{
				// Skip a few frames to avoid naming everything as "...ObservableField..ctor".
				var stack = new StackTrace(4, false);
				var frame = stack.GetFrame(0);
				var method = frame?.GetMethod();
				var typeName = method?.DeclaringType?.Name ?? "Unknown";
				return $"{typeName}.{kind}<{instanceType.Name}>";
			}
			catch
			{
				return $"{kind}<{instanceType.Name}>";
			}
		}

		/// <summary>
		/// A point-in-time snapshot of an observable instance's debug information.
		/// </summary>
		public readonly struct EntrySnapshot
		{
			public readonly ObservableDebugInfo Info;
			public readonly string Value;
			public readonly int Subscribers;
			public readonly Type RuntimeType;
			public readonly WeakReference<object> InstanceRef;

			public EntrySnapshot(ObservableDebugInfo info, string value, int subscribers, Type runtimeType, WeakReference<object> instanceRef)
			{
				Info = info;
				Value = value;
				Subscribers = subscribers;
				RuntimeType = runtimeType;
				InstanceRef = instanceRef;
			}
		}

		private sealed class Entry
		{
			private readonly ObservableDebugInfo _info;
			private readonly Func<string> _valueGetter;
			private readonly Func<int> _subscriberCountGetter;

			public Entry(ObservableDebugInfo info, Func<string> valueGetter, Func<int> subscriberCountGetter)
			{
				_info = info;
				_valueGetter = valueGetter;
				_subscriberCountGetter = subscriberCountGetter;
			}

			/// <summary>
			/// Creates a snapshot from this entry with current live data.
			/// </summary>
			public EntrySnapshot ToSnapshot(object instance)
			{
				string value;
				int subs;

				try { value = _valueGetter?.Invoke() ?? string.Empty; }
				catch (Exception ex) { value = $"<error: {ex.Message}>"; }

				try { subs = _subscriberCountGetter?.Invoke() ?? 0; }
				catch { subs = 0; }

				return new EntrySnapshot(_info, value, subs, instance.GetType(), new WeakReference<object>(instance));
			}
		}
#endif
	}
}

