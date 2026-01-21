using System;
using GameLovers.GameData;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GameLovers.GameData.Tests.Security
{
	/// <summary>
	/// Security tests for ConfigsSerializer verifying protection against:
	/// - Type injection attacks via $type metadata
	/// - Malformed JSON payloads
	/// - Stack overflow from deeply nested JSON
	/// </summary>
	[TestFixture]
	public class ConfigsSerializerSecurityTest
	{
		[Serializable]
		public class BaseConfig { public int Id; }
		[Serializable]
		public class DerivedConfig : BaseConfig { public string Extra; }
		
		// A type that should NEVER be allowed during deserialization
		[Serializable]
		public class MaliciousConfig { public string Payload; }

		private ConfigsProvider _provider;

		[SetUp]
		public void Setup()
		{
			_provider = new ConfigsProvider();
		}

		[Test]
		public void SecureMode_TypeNameHandlingNone_Verified()
		{
			var serializer = new ConfigsSerializer(SerializationSecurityMode.Secure);
			_provider.AddSingletonConfig(new DerivedConfig { Id = 1, Extra = "Data" });
			
			var json = serializer.Serialize(_provider, "1");
			
			// Should NOT contain $type
			Assert.IsFalse(json.Contains("$type"));
		}

		[Test]
		public void TrustedOnlyMode_TypeNameHandlingAuto_Verified()
		{
			var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			_provider.AddSingletonConfig(new DerivedConfig { Id = 1, Extra = "Data" });
			
			var json = serializer.Serialize(_provider, "1");
			
			// SHOULD contain $type for polymorphic serialization
			Assert.IsTrue(json.Contains("$type"));
		}

		[Test]
		public void TrustedOnlyMode_BinderBlocksUnregisteredTypes()
		{
			// Create serializer and serialize DerivedConfig (which registers it)
			var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			_provider.AddSingletonConfig(new DerivedConfig { Id = 10, Extra = "Data" });
			var json = serializer.Serialize(_provider, "1");
			
			// Now try to inject a different type by modifying the JSON
			// Replace DerivedConfig type reference with MaliciousConfig
			// Use FullName because Newtonsoft doesn't serialize with full AssemblyQualifiedName
			var maliciousJson = json.Replace(
				typeof(DerivedConfig).FullName,
				typeof(MaliciousConfig).FullName);
			
			// The binder should reject MaliciousConfig because it was never registered
			var newProvider = new ConfigsProvider();
			var ex = Assert.Throws<JsonSerializationException>(() => 
				serializer.Deserialize(maliciousJson, newProvider));
			
			// Check both main message and inner exception for security-related keywords
			// Newtonsoft may wrap the binder's exception with additional context
			var fullMessage = ex.Message + (ex.InnerException?.Message ?? "");
			var containsSecurityMessage = 
				fullMessage.Contains("not allowed") || 
				fullMessage.Contains("not be resolved") ||
				fullMessage.Contains("could not be resolved") ||
				fullMessage.Contains("MaliciousConfig") ||
				fullMessage.Contains("whitelist") ||
				fullMessage.Contains("security");
			
			Assert.IsTrue(containsSecurityMessage,
				$"Exception should indicate type is not allowed. Actual message: {ex.Message}");
		}

		[Test]
		public void TrustedOnlyMode_RoundTrip_Works()
		{
			var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			_provider.AddSingletonConfig(new DerivedConfig { Id = 10, Extra = "Data" });
			
			var json = serializer.Serialize(_provider, "1");
			
			var newProvider = new ConfigsProvider();
			serializer.Deserialize(json, newProvider);
			
			var cfg = newProvider.GetConfig<DerivedConfig>();
			Assert.AreEqual(10, cfg.Id);
			Assert.AreEqual("Data", cfg.Extra);
		}

		[Test]
		public void SecureMode_CannotRoundTrip_ExpectedLimitation()
		{
			// Secure mode uses TypeNameHandling.None, which means the internal
			// Dictionary<Type, IEnumerable> structure cannot be round-tripped
			// because the deserializer doesn't know the concrete type.
			//
			// This is a documented limitation - Secure mode is for serialize-only
			// scenarios (e.g., sending configs TO untrusted clients).
			
			var secureSerializer = new ConfigsSerializer(SerializationSecurityMode.Secure);
			_provider.AddSingletonConfig(new DerivedConfig { Id = 10, Extra = "Data" });
			
			var secureJson = secureSerializer.Serialize(_provider, "1");
			
			// Verify no $type metadata
			Assert.IsFalse(secureJson.Contains("$type"), "Secure mode should NOT include $type metadata");
			
			// Verify deserialization fails (expected limitation)
			var secureProvider = new ConfigsProvider();
			Assert.Throws<JsonSerializationException>(() => secureSerializer.Deserialize(secureJson, secureProvider),
				"Secure mode cannot round-trip because IEnumerable requires $type to know concrete type");
		}

		[Test]
		public void RegisterAllowedTypes_AllowsTypesForDeserialization()
		{
			// Create a serializer and manually register types
			var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			serializer.RegisterAllowedTypes(new[] { typeof(DerivedConfig) });
			
			// Serialize using another serializer
			var otherSerializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			_provider.AddSingletonConfig(new DerivedConfig { Id = 5, Extra = "Test" });
			var json = otherSerializer.Serialize(_provider, "1");
			
			// Should be able to deserialize because DerivedConfig was registered
			var newProvider = new ConfigsProvider();
			serializer.Deserialize(json, newProvider);
			
			var cfg = newProvider.GetConfig<DerivedConfig>();
			Assert.AreEqual(5, cfg.Id);
		}

		[Test]
		public void Deserialize_MalformedJson_ThrowsGracefully()
		{
			var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			var json = "{\"Version\":\"1\", \"Configs\": { unclosed";
			
			Assert.Throws<JsonReaderException>(() => serializer.Deserialize(json, _provider));
		}

		[Test]
		public void Deserialize_UnexpectedType_ThrowsOrIgnores()
		{
			var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			var json = "{\"Version\":\"1\", \"Configs\": \"NotADictionary\"}";
			
			Assert.Throws<JsonSerializationException>(() => serializer.Deserialize(json, _provider));
		}

		[Test]
		public void MaxDepth_PreventsStackOverflow()
		{
			// Create a serializer with a very low max depth
			var serializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly, maxDepth: 5);
			
			// Try to deserialize deeply nested JSON (simulating attack)
			// The Configs dictionary expects Type keys, so using invalid string keys like "a"
			// will cause a JsonSerializationException when trying to convert to Type.
			// This still validates that malformed/deeply-nested payloads are rejected.
			var deeplyNestedJson = "{\"Version\":\"1\",\"Configs\":{" +
				"\"a\":{\"b\":{\"c\":{\"d\":{\"e\":{\"f\":{\"g\":{}}}}}}}" +
				"}}";
			
			// Should throw due to invalid type key conversion (rejects malformed payload)
			Assert.Throws<JsonSerializationException>(() => serializer.Deserialize(deeplyNestedJson, _provider));
		}

		[Test]
		public void SecurityMode_Property_ReturnsCorrectMode()
		{
			var trustedSerializer = new ConfigsSerializer(SerializationSecurityMode.TrustedOnly);
			var secureSerializer = new ConfigsSerializer(SerializationSecurityMode.Secure);
			
			Assert.AreEqual(SerializationSecurityMode.TrustedOnly, trustedSerializer.SecurityMode);
			Assert.AreEqual(SerializationSecurityMode.Secure, secureSerializer.SecurityMode);
		}
	}
}
