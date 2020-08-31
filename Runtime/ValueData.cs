using System;

// ReSharper disable once CheckNamespace

// TODO: Remove this after Unity 2020.1 release. Unity will allow Generic Type Serialization....FINALLY \o/
namespace GameLovers
{
	[Serializable]
	public struct IntPairData
	{
		public int Key;
		public int Value;

		public IntPairData(int key, int value)
		{
			Key = key;
			Value = value;
		}
		
		public override string ToString()
		{
			return $"[{Key.ToString()},{Value.ToString()}]";
		}
	}
	
	[Serializable]
	public struct IntStringData
	{
		public int Key;
		public string Value;

		public IntStringData(int key, string value)
		{
			Key = key;
			Value = value;
		}
		
		public override string ToString()
		{
			return $"[{Key.ToString()},{Value}]";
		}
	}
	
	[Serializable]
	public struct FloatPairData
	{
		public float Key;
		public float Value;

		public FloatPairData(float key, float value)
		{
			Key = key;
			Value = value;
		}
		
		public override string ToString()
		{
			return $"[{Key.ToString("F2")},{Value.ToString("F2")}]";
		}
	}
	
	[Serializable]
	public struct FloatStringData
	{
		public float Key;
		public string Value;

		public FloatStringData(float key, string value)
		{
			Key = key;
			Value = value;
		}
		
		public override string ToString()
		{
			return $"[{Key.ToString("F2")},{Value}]";
		}
	}
	
	[Serializable]
	public struct StringPairData
	{
		public string Key;
		public string Value;

		public StringPairData(string key, string value)
		{
			Key = key;
			Value = value;
		}
		
		public override string ToString()
		{
			return $"[{Key},{Value}]";
		}
	}
	
	[Serializable]
	public struct StringIntData
	{
		public string Key;
		public int Value;

		public StringIntData(string key, int value)
		{
			Key = key;
			Value = value;
		}
		
		public override string ToString()
		{
			return $"[{Key},{Value.ToString()}]";
		}
	}
	
	[Serializable]
	public struct StringFloatData
	{
		public string Key;
		public float Value;

		public StringFloatData(string key, float value)
		{
			Key = key;
			Value = value;
		}
		
		public override string ToString()
		{
			return $"[{Key},{Value.ToString("F2")}]";
		}
	}
}