using System;
using System.Collections;
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

			foreach (var pair in allConfigs)
			{
				var type = pair.Key;
				var configs = pair.Value;

				// ConfigsProvider stores all configs (including singletons) as Dictionary<int, T>
				// We need to use reflection to access the values since the generic type varies
				var configsType = configs.GetType();
				if (configsType.IsGenericType && configsType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
				{
					var keyType = configsType.GetGenericArguments()[0];
					if (keyType == typeof(int))
					{
						// Use reflection to iterate the dictionary
						foreach (var item in configs)
						{
							// item is KeyValuePair<int, T>
							var itemType = item.GetType();
							var keyProp = itemType.GetProperty("Key");
							var valueProp = itemType.GetProperty("Value");
							
							var key = (int)keyProp.GetValue(item);
							var value = valueProp.GetValue(item);
							
							ValidateObject(value, type, key, result);
						}
					}
				}
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
			foreach (var pair in configs)
			{
				ValidateObject(pair.Value, typeof(T), pair.Key, result);
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
