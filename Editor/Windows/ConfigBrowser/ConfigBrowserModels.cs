using System;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Represents the currently selected config entry in the Config Browser tree.
	/// An invalid selection (no config chosen) has a null <see cref="ConfigType"/>.
	/// </summary>
	internal readonly struct ConfigSelection
	{
		public readonly Type ConfigType;
		public readonly int ConfigId;
		public readonly object Value;

		/// <summary>Returns true when a config entry is actually selected.</summary>
		public bool IsValid => ConfigType != null;

		public ConfigSelection(Type configType, int configId, object value)
		{
			ConfigType = configType;
			ConfigId = configId;
			Value = value;
		}

		/// <summary>Creates an empty selection.</summary>
		public static ConfigSelection None() => new ConfigSelection(null, 0, null);
	}

	/// <summary>
	/// Describes which subset of validation errors should be displayed.
	/// Either all errors or errors for a single config entry identified by type and id.
	/// </summary>
	internal readonly struct ValidationFilter
	{
		public readonly bool IsAll;
		public readonly Type ConfigType;
		public readonly int ConfigId;

		private ValidationFilter(bool isAll, Type configType, int configId)
		{
			IsAll = isAll;
			ConfigType = configType;
			ConfigId = configId;
		}

		/// <summary>Creates a filter that shows all validation errors.</summary>
		public static ValidationFilter All() => new ValidationFilter(true, null, 0);

		/// <summary>Creates a filter for a single config entry.</summary>
		public static ValidationFilter Single(Type type, int id) => new ValidationFilter(false, type, id);
	}

	/// <summary>
	/// Immutable record holding the details of a single validation error produced by
	/// <see cref="ConfigValidationService"/>. Singleton configs use a null <see cref="ConfigId"/>.
	/// </summary>
	internal readonly struct ValidationErrorInfo
	{
		public readonly string ConfigTypeName;
		public readonly int? ConfigId;
		public readonly string FieldName;
		public readonly string Message;

		public ValidationErrorInfo(string configTypeName, int? configId, string fieldName, string message)
		{
			ConfigTypeName = configTypeName;
			ConfigId = configId;
			FieldName = fieldName;
			Message = message;
		}
	}

	/// <summary>
	/// Discriminates the kind of node in the Config Browser tree view.
	/// </summary>
	internal enum ConfigNodeKind
	{
		Header, // A header node grouping other nodes.
		Type, // A config type node containing entry children.
		Entry // A selectable config entry node.
	}

	/// <summary>
	/// Data payload for a single node in the Config Browser <see cref="UnityEngine.UIElements.TreeView"/>.
	/// Nodes are created via the static factory methods <see cref="Header"/>, <see cref="Type"/>, and <see cref="Entry"/>.
	/// </summary>
	internal readonly struct ConfigNode
	{
		public readonly ConfigNodeKind Kind;
		public readonly string DisplayName;
		public readonly Type ConfigType;
		public readonly int ConfigId;
		public readonly object Value;

		private ConfigNode(ConfigNodeKind kind, string displayName, Type configType, int configId, object value)
		{
			Kind = kind;
			DisplayName = displayName;
			ConfigType = configType;
			ConfigId = configId;
			Value = value;
		}

		/// <summary>Creates a header node with the given display <paramref name="name"/>.</summary>
		public static ConfigNode Header(string name) => new ConfigNode(ConfigNodeKind.Header, name, null, 0, null);

		/// <summary>Creates a type node for the given config <paramref name="type"/>.</summary>
		public static ConfigNode Type(Type type, string name) => new ConfigNode(ConfigNodeKind.Type, name, type, 0, null);

		/// <summary>Creates an entry node representing a single config instance.</summary>
		public static ConfigNode Entry(Type type, int id, object value, string name) => new ConfigNode(ConfigNodeKind.Entry, name, type, id, value);
	}
}
