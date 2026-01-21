using System;
using System.Collections.Generic;
using System.Reflection;
using GameLovers.GameData;

namespace GameLoversEditor.GameData
{
	/// <summary>
	/// Provides editor-only validation utilities for config data.
	/// Uses reflection to validate fields and properties decorated with <see cref="ValidationAttribute"/>.
	/// </summary>
	public static class EditorConfigValidator
	{
		/// <summary>
		/// Validates all configurations in the given provider.
		/// Iterates through every registered config type and validates each config instance.
		/// </summary>
		public static ValidationResult ValidateAll(IConfigsProvider provider)
		{
			var result = new ValidationResult();
			var allConfigs = provider.GetAllConfigs();
			var validateMethod = typeof(EditorConfigValidator)
				.GetMethod(nameof(Validate), BindingFlags.Public | BindingFlags.Static)
				?? throw new InvalidOperationException($"Method {nameof(Validate)} not found");

			foreach (var pair in allConfigs)
			{
				var genericMethod = validateMethod.MakeGenericMethod(pair.Key);
				var typeResult = (ValidationResult)genericMethod.Invoke(null, new object[] { provider });
				result.Errors.AddRange(typeResult.Errors);
				result.ValidConfigs.AddRange(typeResult.ValidConfigs);
			}

			return result;
		}

		/// <summary>
		/// Validates all configurations of a specific type in the given provider.
		/// </summary>
		public static ValidationResult Validate<T>(IConfigsProvider provider)
		{
			var result = new ValidationResult();
			var configs = provider.GetConfigsDictionary<T>();
			var typeName = typeof(T).Name;

			foreach (var pair in configs)
			{
				var errorCountBefore = result.Errors.Count;
				ValidateObject(pair.Value, typeof(T), pair.Key, result);

				if (result.Errors.Count == errorCountBefore)
				{
					result.ValidConfigs.Add(new ValidConfig
					{
						ConfigType = typeName,
						ConfigId = pair.Key
					});
				}
			}
			return result;
		}

		private static void ValidateObject(object obj, Type type, int? id, ValidationResult result)
		{
			if (obj == null) return;

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var field in fields)
			{
				ValidateMember(type, id, field.Name, field.GetCustomAttributes<ValidationAttribute>(), 
					field.GetValue(obj), result);
			}

			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var prop in props)
			{
				ValidateMember(type, id, prop.Name, prop.GetCustomAttributes<ValidationAttribute>(), 
					prop.GetValue(obj), result);
			}
		}

		private static void ValidateMember(
			Type type,
			int? id,
			string memberName,
			IEnumerable<ValidationAttribute> attributes,
			object value,
			ValidationResult result)
		{
			foreach (var attr in attributes)
			{
				if (!attr.IsValid(value, out var message))
				{
					result.Errors.Add(new ValidationError
					{
						ConfigType = type.Name,
						ConfigId = id,
						FieldName = memberName,
						Message = message
					});
				}
			}
		}
	}
}
