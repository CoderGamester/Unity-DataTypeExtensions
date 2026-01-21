using System;

namespace GameLovers.GameData
{
	public class RangeAttribute : ValidationAttribute
	{
		private readonly double _min;
		private readonly double _max;

		public RangeAttribute(double min, double max)
		{
			_min = min;
			_max = max;
		}

		public override bool IsValid(object value, out string message)
		{
			if (value == null)
			{
				message = null;
				return true;
			}

			try
			{
				var val = Convert.ToDouble(value);
				if (val < _min || val > _max)
				{
					message = $"Value {val} is out of range [{_min}, {_max}]";
					return false;
				}
			}
			catch
			{
				message = "Value is not a number";
				return false;
			}

			message = null;
			return true;
		}
	}
}
