using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using GameLovers;

// ReSharper disable once CheckNamespace

namespace GameLovers.ConfigsProvider
{
	/// <summary>
	/// Abstract base class for configuration scriptable objects that store key-value pairs.
	/// Provides a foundation for config containers with serializable dictionary collections using Unity's serialization workaround pattern.
	/// </summary>
	/// <typeparam name="TId">The type of the identifier/key.</typeparam>
	/// <typeparam name="TAsset">The type of the asset/value.</typeparam>
	public abstract class ConfigsScriptableObject<TId, TAsset> : 
		ScriptableObject, IPairConfigsContainer<TId, TAsset>, ISerializationCallbackReceiver
	{
		[SerializeField] private List<Pair<TId, TAsset>> _configs = new();

		/// <inheritdoc />
		public List<Pair<TId, TAsset>> Configs
		{
			get => _configs;
			set => _configs = value;
		}

		/// <summary>
		/// Provides the configs as a read-only dictionary for efficient lookup operations.
		/// </summary>
		public IReadOnlyDictionary<TId, TAsset> ConfigsDictionary { get; private set; }

		/// <inheritdoc />
		public void OnBeforeSerialize()
		{
			// Unity serialization handles the list format automatically
			// No conversion needed before serialization
		}

		/// <inheritdoc />
		public virtual void OnAfterDeserialize()
		{
			// Convert the serialized list to a dictionary for efficient lookups
			var dictionary = new Dictionary<TId, TAsset>();

			foreach (var config in Configs)
			{
				dictionary.Add(config.Key, config.Value);
			}

			ConfigsDictionary = new ReadOnlyDictionary<TId, TAsset>(dictionary);
		}
	}
}