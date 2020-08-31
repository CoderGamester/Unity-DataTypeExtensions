using GameLovers;
using System;

// ReSharper disable once CheckNamespace

namespace GameLovers.Samples
{
	public enum EnumExample
	{
		Value1
	}
	
	/// <summary>
	/// Simple Selector implementation
	/// </summary>
	[Serializable]
	public class EnumSelectorExample : EnumSelector<EnumExample>
	{
		public EnumSelectorExample() : base(EnumExample.Value1)
		{
		}
		
		public EnumSelectorExample(EnumExample data) : base(data)
		{
		}
		
		public static implicit operator EnumSelectorExample(EnumExample value)
		{
			return new EnumSelectorExample(value);
		}
	}
}