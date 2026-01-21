using System;
using System.Collections.Generic;
using GameLovers.GameData;
using GameLoversEditor.GameData;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace GameLovers.GameData.Tests
{
	[TestFixture]
	public class MigrationRunnerTest
	{
		[Serializable]
		public class MockConfig
		{
			public int Value;
			public string NewField;
		}

		[ConfigMigration(typeof(MockConfig))]
		public class MockMigration_v1_v2 : IConfigMigration
		{
			public ulong FromVersion => 1;
			public ulong ToVersion => 2;
			public void Migrate(JObject configJson)
			{
				configJson["Value"] = (int)configJson["Value"] + 10;
			}
		}

		[ConfigMigration(typeof(MockConfig))]
		public class MockMigration_v2_v3 : IConfigMigration
		{
			public ulong FromVersion => 2;
			public ulong ToVersion => 3;
			public void Migrate(JObject configJson)
			{
				configJson["NewField"] = "Migrated";
			}
		}

		[SetUp]
		public void Setup()
		{
			MigrationRunner.Initialize(force: true);
		}

		[Test]
		public void GetConfigTypesWithMigrations_ReturnsCorrectTypes()
		{
			var types = MigrationRunner.GetConfigTypesWithMigrations();
			Assert.Contains(typeof(MockConfig), (System.Collections.ICollection)types);
		}

		[Test]
		public void GetAvailableMigrations_ReturnsOrderedMigrations()
		{
			var migrations = MigrationRunner.GetAvailableMigrations<MockConfig>();
			Assert.AreEqual(2, migrations.Count);
			Assert.AreEqual(1, (int)migrations[0].FromVersion);
			Assert.AreEqual(2, (int)migrations[1].FromVersion);
		}

		[Test]
		public void Migrate_AppliesSequentialMigrations()
		{
			var json = new JObject { ["Value"] = 5 };
			var count = MigrationRunner.Migrate(typeof(MockConfig), json, 1, 3);
			
			Assert.AreEqual(2, count);
			Assert.AreEqual(15, (int)json["Value"]);
			Assert.AreEqual("Migrated", (string)json["NewField"]);
		}

		[Test]
		public void GetLatestVersion_ReturnsCorrectVersion()
		{
			Assert.AreEqual(3, (int)MigrationRunner.GetLatestVersion(typeof(MockConfig)));
		}

		#region Complex Migration Tests

		[Serializable]
		public class MockComplexConfig
		{
			public int Id;
			public string Name;
			public int AttackDamage;
			public string ArmorType;
			public int BaseHealth;
			public int BonusHealth;
			public MockStats Stats;
			public string[] Abilities;
		}

		[Serializable]
		public class MockStats
		{
			public int DamageReduction;
			public int CritChance;
		}

		[ConfigMigration(typeof(MockComplexConfig))]
		public class MockComplex_v1_v2 : IConfigMigration
		{
			public ulong FromVersion => 1;
			public ulong ToVersion => 2;
			public void Migrate(JObject configJson)
			{
				// Rename Damage -> AttackDamage
				configJson["AttackDamage"] = configJson["Damage"];
				configJson.Remove("Damage");

				// Add ArmorType based on Health
				int health = (int)configJson["Health"];
				configJson["ArmorType"] = health >= 100 ? "Heavy" : "Light";
			}
		}

		[ConfigMigration(typeof(MockComplexConfig))]
		public class MockComplex_v2_v3 : IConfigMigration
		{
			public ulong FromVersion => 2;
			public ulong ToVersion => 3;
			public void Migrate(JObject configJson)
			{
				// Split Health -> Base + Bonus
				int totalHealth = (int)configJson["Health"];
				configJson["BaseHealth"] = (int)(totalHealth * 0.8f);
				configJson["BonusHealth"] = totalHealth - (int)configJson["BaseHealth"];
				configJson.Remove("Health");

				// Add Stats object
				configJson["Stats"] = new JObject
				{
					["DamageReduction"] = (string)configJson["ArmorType"] == "Heavy" ? 40 : 10,
					["CritChance"] = 5
				};

				// Add empty array
				configJson["Abilities"] = new JArray();
			}
		}

		[Test]
		public void Migrate_ComplexPatterns_v1ToV2_Works()
		{
			var json = new JObject
			{
				["Id"] = 1,
				["Name"] = "Unit",
				["Health"] = 150,
				["Damage"] = 20
			};

			MigrationRunner.Migrate(typeof(MockComplexConfig), json, 1, 2);

			Assert.IsNull(json["Damage"]);
			Assert.AreEqual(20, (int)json["AttackDamage"]);
			Assert.AreEqual("Heavy", (string)json["ArmorType"]);
		}

		[Test]
		public void Migrate_ComplexPatterns_v2ToV3_Works()
		{
			var json = new JObject
			{
				["Id"] = 1,
				["Name"] = "Unit",
				["AttackDamage"] = 20,
				["ArmorType"] = "Heavy",
				["Health"] = 100
			};

			MigrationRunner.Migrate(typeof(MockComplexConfig), json, 2, 3);

			Assert.IsNull(json["Health"]);
			Assert.AreEqual(80, (int)json["BaseHealth"]);
			Assert.AreEqual(20, (int)json["BonusHealth"]);
			Assert.IsNotNull(json["Stats"]);
			Assert.AreEqual(40, (int)json["Stats"]["DamageReduction"]);
			Assert.IsInstanceOf<JArray>(json["Abilities"]);
		}

		[Test]
		public void Migrate_ComplexPatterns_Chained_v1ToV3_Works()
		{
			var json = new JObject
			{
				["Id"] = 1,
				["Name"] = "Unit",
				["Health"] = 150,
				["Damage"] = 20
			};

			MigrationRunner.Migrate(typeof(MockComplexConfig), json, 1, 3);

			Assert.IsNull(json["Damage"]);
			Assert.IsNull(json["Health"]);
			Assert.AreEqual(20, (int)json["AttackDamage"]);
			Assert.AreEqual("Heavy", (string)json["ArmorType"]);
			Assert.AreEqual(120, (int)json["BaseHealth"]);
			Assert.AreEqual(30, (int)json["BonusHealth"]);
			Assert.AreEqual(40, (int)json["Stats"]["DamageReduction"]);
			Assert.AreEqual(0, ((JArray)json["Abilities"]).Count);
		}

		#endregion
	}
}
