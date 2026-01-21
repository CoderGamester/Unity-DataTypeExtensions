using System;
using System.Collections.Generic;
using GameLovers.GameData;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace GameLovers.GameData.Tests
{
	[TestFixture]
	public class JsonConvertersTest
	{
		private JsonSerializerSettings _settings;

		[SetUp]
		public void Setup()
		{
			_settings = new JsonSerializerSettings
			{
				Converters = new List<JsonConverter>
				{
					new ColorJsonConverter(),
					new Vector2JsonConverter(),
					new Vector3JsonConverter(),
					new Vector4JsonConverter(),
					new QuaternionJsonConverter()
				}
			};
		}

		[Test]
		public void Color_RoundTrip()
		{
			var color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
			var json = JsonConvert.SerializeObject(color, _settings);
			var result = JsonConvert.DeserializeObject<Color>(json, _settings);
			
			// Hex color format (#RRGGBBAA) has 8-bit precision per channel,
			// so tolerance needs to account for 1/255 ≈ 0.004 quantization error
			Assert.AreEqual(color.r, result.r, 0.01f);
			Assert.AreEqual(color.g, result.g, 0.01f);
			Assert.AreEqual(color.b, result.b, 0.01f);
			Assert.AreEqual(color.a, result.a, 0.01f);
		}

		[Test]
		public void Vector2_RoundTrip()
		{
			var vec = new Vector2(1.1f, 2.2f);
			var json = JsonConvert.SerializeObject(vec, _settings);
			var result = JsonConvert.DeserializeObject<Vector2>(json, _settings);
			
			Assert.AreEqual(vec.x, result.x, 0.0001f);
			Assert.AreEqual(vec.y, result.y, 0.0001f);
		}

		[Test]
		public void Vector3_RoundTrip()
		{
			var vec = new Vector3(1.1f, 2.2f, 3.3f);
			var json = JsonConvert.SerializeObject(vec, _settings);
			var result = JsonConvert.DeserializeObject<Vector3>(json, _settings);
			
			Assert.AreEqual(vec.x, result.x, 0.0001f);
			Assert.AreEqual(vec.y, result.y, 0.0001f);
			Assert.AreEqual(vec.z, result.z, 0.0001f);
		}

		[Test]
		public void Vector4_RoundTrip()
		{
			var vec = new Vector4(1.1f, 2.2f, 3.3f, 4.4f);
			var json = JsonConvert.SerializeObject(vec, _settings);
			var result = JsonConvert.DeserializeObject<Vector4>(json, _settings);
			
			Assert.AreEqual(vec.x, result.x, 0.0001f);
			Assert.AreEqual(vec.y, result.y, 0.0001f);
			Assert.AreEqual(vec.z, result.z, 0.0001f);
			Assert.AreEqual(vec.w, result.w, 0.0001f);
		}

		[Test]
		public void Quaternion_RoundTrip()
		{
			var quat = new Quaternion(0.1f, 0.2f, 0.3f, 0.4f);
			var json = JsonConvert.SerializeObject(quat, _settings);
			var result = JsonConvert.DeserializeObject<Quaternion>(json, _settings);
			
			Assert.AreEqual(quat.x, result.x, 0.0001f);
			Assert.AreEqual(quat.y, result.y, 0.0001f);
			Assert.AreEqual(quat.z, result.z, 0.0001f);
			Assert.AreEqual(quat.w, result.w, 0.0001f);
		}
	}
}
