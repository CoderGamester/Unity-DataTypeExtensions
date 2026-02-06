using System;
using System.Collections.Generic;
using System.Reflection;
using GameLovers.GameData;

namespace GameLovers.GameData.Editor
{
	/// <summary>
	/// Stateless service that validates config data by inspecting fields and properties decorated
	/// with <see cref="ValidationAttribute"/> subclasses. Supports both full-provider and
	/// single-entry validation scopes.
	/// </summary>
	internal static class ConfigValidationService
	{
		private const int SingleConfigId = 0;

		/// <summary>
		/// Validates every config entry in the given <paramref name="provider"/> and returns
		/// the collected list of <see cref="ValidationErrorInfo"/> errors.
		/// </summary>
		public static List<ValidationErrorInfo> ValidateAll(IConfigsProvider provider)
		{
			if (provider == null) return new List<ValidationErrorInfo>();

			var errors = new List<ValidationErrorInfo>();
			foreach (var kv in provider.GetAllConfigs())
			{
				if (!ConfigsEditorUtil.TryReadConfigs(kv.Value, out var entries))
				{
					continue;
				}

				for (int i = 0; i < entries.Count; i++)
				{
					ValidateObject(kv.Key, entries[i].Id, entries[i].Value, errors);
				}
			}

			return errors;
		}

		/// <summary>
		/// Validates a single config entry described by <paramref name="selection"/> and returns
		/// the collected list of <see cref="ValidationErrorInfo"/> errors.
		/// </summary>
		public static List<ValidationErrorInfo> ValidateSingle(ConfigSelection selection)
		{
			var errors = new List<ValidationErrorInfo>();
			if (!selection.IsValid) return errors;
			ValidateObject(selection.ConfigType, selection.ConfigId, selection.Value, errors);
			return errors;
		}

		private static void ValidateObject(Type configType, int configId, object instance, List<ValidationErrorInfo> errors)
		{
			if (instance == null) return;

			foreach (var field in configType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				var attrs = field.GetCustomAttributes(typeof(ValidationAttribute), inherit: true);
				AddValidationErrors(configType, configId, field.Name, attrs, field.GetValue(instance), errors);
			}

			foreach (var prop in configType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (!prop.CanRead) continue;
				var attrs = prop.GetCustomAttributes(typeof(ValidationAttribute), inherit: true);
				AddValidationErrors(configType, configId, prop.Name, attrs, prop.GetValue(instance), errors);
			}
		}

		private static void AddValidationErrors(Type configType, int configId, string memberName, object[] attrs, object value, List<ValidationErrorInfo> errors)
		{
			for (int i = 0; i < attrs.Length; i++)
			{
				if (attrs[i] is ValidationAttribute validationAttribute)
				{
					if (!validationAttribute.IsValid(value, out var message))
					{
						errors.Add(new ValidationErrorInfo(configType.Name, configId == SingleConfigId ? null : configId, memberName, message));
					}
				}
			}
		}
	}
}
