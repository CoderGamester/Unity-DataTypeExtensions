using System;
using System.Collections;

namespace GameLovers.GameData
{
	public class MinLengthAttribute : ValidationAttribute
	{
		private readonly int _minLength;

		public MinLengthAttribute(int minLength)
		{
			_minLength = minLength;
		}

		public override bool IsValid(object value, out string message)
		{
			if (value == null)
			{
				message = null;
				return true;
			}

			int length = 0;
			if (value is string s) length = s.Length;
			else if (value is ICollection collection) length = collection.Count;
			else if (value is IEnumerable enumerable)
			{
				var enumerator = enumerable.GetEnumerator();
				while (enumerator.MoveNext()) length++;
			}

			if (length < _minLength)
			{
				message = $"Length {length} is less than minimum length {_minLength}";
				return false;
			}

			message = null;
			return true;
		}
	}
}
