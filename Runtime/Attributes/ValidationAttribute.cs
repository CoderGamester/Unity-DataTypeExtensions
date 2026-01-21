using System;

namespace GameLovers.GameData
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public abstract class ValidationAttribute : Attribute
	{
		public abstract bool IsValid(object value, out string message);
	}
}
