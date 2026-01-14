using System.Collections.Generic;

namespace GameLoversEditor.GameData
{
	/// <summary>
	/// Represents a single validation error found during config validation.
	/// Contains information about the config type, ID, field name, and error message.
	/// </summary>
	public class ValidationError
	{
		/// <summary>
		/// The name of the config type that contains the validation error.
		/// </summary>
		public string ConfigType { get; set; }

		/// <summary>
		/// The ID of the config instance, or null for singleton configs.
		/// </summary>
		public int? ConfigId { get; set; }

		/// <summary>
		/// The name of the field or property that failed validation.
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		/// The validation error message describing what went wrong.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Returns a formatted string representation of the validation error.
		/// Format: "[ConfigType ID:X] FieldName: Message" or "[ConfigType] FieldName: Message" for singletons.
		/// </summary>
		public override string ToString()
		{
			var idStr = ConfigId.HasValue ? $" ID:{ConfigId.Value}" : "";
			return $"[{ConfigType}{idStr}] {FieldName}: {Message}";
		}
	}

	/// <summary>
	/// Contains the results of a config validation operation.
	/// Provides access to any validation errors found and a convenience property to check overall validity.
	/// </summary>
	public class ValidationResult
	{
		/// <summary>
		/// Gets whether the validation passed with no errors.
		/// Returns true if <see cref="Errors"/> is empty, false otherwise.
		/// </summary>
		public bool IsValid => Errors.Count == 0;

		/// <summary>
		/// Gets the list of validation errors found during validation.
		/// Empty if all validations passed.
		/// </summary>
		public List<ValidationError> Errors { get; } = new List<ValidationError>();
	}
}
