using System;
using System.Collections.Generic;
using GameLovers.GameData;
using GameLoversEditor.GameData;
using NUnit.Framework;

namespace GameLovers.GameData.Tests
{
	[TestFixture]
	public class EditorConfigValidatorTest
	{
		private ConfigsProvider _provider;

		[SetUp]
		public void Setup()
		{
			_provider = new ConfigsProvider();
		}

		[Test]
		public void ValidateAll_NoErrors_ReturnsEmptyResult()
		{
			var config = new MockValidatableConfigBuilder()
				.WithName("Hero")
				.WithHealth(50)
				.WithTag("ABC")
				.Build();
			_provider.AddSingletonConfig(config);

			var result = EditorConfigValidator.ValidateAll(_provider);
			Assert.AreEqual(0, result.Errors.Count);
		}

		[Test]
		public void ValidateAll_WithErrors_ReturnsAllErrors()
		{
			var config = new MockValidatableConfigBuilder().Invalid().Build();
			_provider.AddSingletonConfig(config);

			var result = EditorConfigValidator.ValidateAll(_provider);
			
			Assert.AreEqual(3, result.Errors.Count);
			Assert.IsTrue(result.Errors.Exists(e => e.FieldName == "Name"));
			Assert.IsTrue(result.Errors.Exists(e => e.FieldName == "Health"));
			Assert.IsTrue(result.Errors.Exists(e => e.FieldName == "Tag"));
		}

		[Test]
		public void Validate_SpecificType_OnlyValidatesThatType()
		{
			var config = new MockValidatableConfigBuilder().WithName("").Build();
			_provider.AddSingletonConfig(config);

			var result = EditorConfigValidator.Validate<MockValidatableConfig>(_provider);
			Assert.AreEqual(1, result.Errors.Count);
		}

		[Test]
		public void ValidateAll_ValidConfigs_TracksValidOnes()
		{
			var config = new MockValidatableConfigBuilder().Build();
			_provider.AddSingletonConfig(config);

			var result = EditorConfigValidator.ValidateAll(_provider);
			Assert.AreEqual(1, result.ValidConfigs.Count);
		}

		[Test]
		public void Validate_UsingBuilder_DefaultValues_PassValidation()
		{
			// Default builder creates valid config
			var provider = new ConfigsProviderBuilder()
				.WithSingleton(new MockValidatableConfigBuilder().Build())
				.Build();

			var result = EditorConfigValidator.ValidateAll(provider);
			Assert.AreEqual(0, result.Errors.Count);
		}
	}
}
