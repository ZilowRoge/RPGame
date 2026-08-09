using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Progression;
using RPGame.Core.Spells;
using RPGame.Core.Statistics;
using RPGame.Core.Statistics.Attributes;
using RPGame.Core.Statistics.CombatStats;
using RPGame.UI.Statistics;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Tests
{
    public sealed class StatisticsWindowUITests
    {
        private readonly List<UnityEngine.Object> objectsToDestroy = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (objectsToDestroy[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(objectsToDestroy[i]);
                }
            }

            objectsToDestroy.Clear();
        }

        [Test]
        public void Format_WithSingleNumber_ReturnsIntegerText()
        {
            string result = ValueFactory.Single(25f).Format(Format.Integer);

            Assert.AreEqual("25", result);
        }

        [Test]
        public void Format_WithCurrentAndMax_ReturnsPairText()
        {
            string result = ValueFactory.CurrentAndMax(10f, 100f).Format(Format.CurrentAndMax);

            Assert.AreEqual("10 / 100", result);
        }

        [Test]
        public void Format_WithPercentage_ReturnsPercentText()
        {
            string result = ValueFactory.Single(0.3f).Format(Format.Percentage());

            Assert.AreEqual("30%", result);
        }

        [Test]
        public void Format_WithSignedPercentage_ReturnsPlusSign()
        {
            string result = ValueFactory.Single(0.1f).Format(Format.Percentage(showPlusSign: true));

            Assert.AreEqual("+10%", result);
        }

        [Test]
        public void Format_WithRange_ReturnsRangeText()
        {
            string result = ValueFactory.Range(12f, 18f).Format(Format.Range());

            Assert.AreEqual("12 - 18", result);
        }

        [Test]
        public void Format_WithDecimalAndPerSecond_ReturnsDecimalPerSecondText()
        {
            string decimalResult = ValueFactory.Single(2.5f).Format(Format.Decimal(1));
            string perSecondResult = ValueFactory.Single(3.5f).Format(Format.PerSecond());

            Assert.AreEqual("2.5", decimalResult);
            Assert.AreEqual("3.5/s", perSecondResult);
        }

        [Test]
        public void Format_WithRounding_ReturnsRoundedText()
        {
            Format format = Format.Decimal(1);

            Assert.AreEqual("2.5", ValueFactory.Single(2.54f).Format(format));
            Assert.AreEqual("2.6", ValueFactory.Single(2.56f).Format(format));
        }

        [Test]
        public void Format_WithNoValue_ReturnsEmptyValueText()
        {
            string result = ValueFactory.None().Format(Format.Integer);

            Assert.AreEqual("\u2014", result);
        }

        [Test]
        public void Builder_UsesDefinitionOrderAndLabels()
        {
            RecordsBuilder builder = new()
            {
                Definitions = new List<RecordDefinition>
                {
                    RecordDefinition.HealthRegeneration(),
                    RecordDefinition.Health()
                }
            };
            DataEntry[] entries =
            {
                new(RecordId.Health, "10 / 100"),
                new(RecordId.HealthRegeneration, "1.5/s")
            };

            IReadOnlyList<StatisticRecordData> records = builder.Build(entries);

            Assert.AreEqual("Health Regeneration", records[0].Label);
            Assert.AreEqual("1.5/s", records[0].ValueText);
            Assert.AreEqual("Health", records[1].Label);
            Assert.AreEqual("10 / 100", records[1].ValueText);
        }

        [Test]
        public void Builder_WhenDefinitionIsMissingAndConfigured_DisplaysEmptyValue()
        {
            RecordsBuilder builder = new()
            {
                Definitions = new List<RecordDefinition>
                {
                    RecordDefinition.LastSpell()
                }
            };

            IReadOnlyList<StatisticRecordData> records = builder.Build(System.Array.Empty<DataEntry>());

            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("Last spell", records[0].Label);
            Assert.AreEqual("\u2014", records[0].ValueText);
        }

        [Test]
        public void Builder_WhenDefinitionIsMissingAndNotConfigured_SkipsRecord()
        {
            RecordsBuilder builder = new()
            {
                Definitions = new List<RecordDefinition>
                {
                    new(RecordId.Health, "Health")
                }
            };

            IReadOnlyList<StatisticRecordData> records = builder.Build(System.Array.Empty<DataEntry>());

            Assert.AreEqual(0, records.Count);
        }

        [Test]
        public void Builder_WhenWeaponDamageIsMissing_DisplaysEmptyValue()
        {
            RecordsBuilder builder = new()
            {
                Definitions = new List<RecordDefinition>
                {
                    RecordDefinition.WeaponDamage()
                }
            };

            IReadOnlyList<StatisticRecordData> records = builder.Build(System.Array.Empty<DataEntry>());

            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("Weapon damage", records[0].Label);
            Assert.AreEqual("\u2014", records[0].ValueText);
        }

        [Test]
        public void Builder_WithAvailableExperience_DisplaysAvailableExperienceRecord()
        {
            RecordsBuilder builder = new()
            {
                Definitions = new List<RecordDefinition>
                {
                    RecordDefinition.AvailableExperience()
                }
            };
            DataEntry[] entries =
            {
                new(RecordId.AvailableExperience, "250")
            };

            IReadOnlyList<StatisticRecordData> records = builder.Build(entries);

            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("Available XP", records[0].Label);
            Assert.AreEqual("250", records[0].ValueText);
        }

        [Test]
        public void Builder_WithDefaultDefinitions_DisplaysAvailableExperienceFirst()
        {
            RecordsBuilder builder = new();
            DataEntry[] entries =
            {
                new(RecordId.Health, "10 / 100"),
                new(RecordId.AvailableExperience, "250")
            };

            IReadOnlyList<StatisticRecordData> records = builder.Build(entries);

            Assert.AreEqual("Available XP", records[0].Label);
            Assert.AreEqual("250", records[0].ValueText);
        }

        [Test]
        public void Builder_WhenAvailableExperienceIsMissing_DisplaysEmptyValue()
        {
            RecordsBuilder builder = new()
            {
                Definitions = new List<RecordDefinition>
                {
                    RecordDefinition.AvailableExperience()
                }
            };

            IReadOnlyList<StatisticRecordData> records = builder.Build(System.Array.Empty<DataEntry>());

            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("Available XP", records[0].Label);
            Assert.AreEqual("\u2014", records[0].ValueText);
        }

        [Test]
        public void StatisticRecordUI_SetText_UsesOnlyProvidedLabelAndValue()
        {
            StatisticRecordUI record = CreateRecordPrefab("Record");

            record.SetText("Custom Label", "+10%");

            Assert.AreEqual("Custom Label", GetText(record.transform, "Content/Label").text);
            Assert.AreEqual("+10%", GetText(record.transform, "Content/Value").text);
        }

        [Test]
        public void StatisticRecordUI_DoesNotDependOnGameplaySystems()
        {
            Type[] forbiddenTypes =
            {
                typeof(StatisticsController),
                typeof(CharacterAttributes),
                typeof(CombatStatsProvider),
                typeof(ILastUsedSpellDamageRangeProvider),
                typeof(IExperienceProvider)
            };

            Type[] referencedTypes = typeof(StatisticRecordUI)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(field => field.FieldType)
                .Concat(typeof(StatisticRecordUI)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(property => property.PropertyType))
                .Concat(typeof(StatisticRecordUI)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(method => method.ReturnType))
                .Concat(typeof(StatisticRecordUI)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .SelectMany(method => method.GetParameters())
                    .Select(parameter => parameter.ParameterType))
                .ToArray();

            foreach (Type forbiddenType in forbiddenTypes)
            {
                Assert.IsFalse(referencedTypes.Any(type => forbiddenType.IsAssignableFrom(type)), forbiddenType.FullName);
            }
        }

        [Test]
        public void Window_Rebuild_DisplaysRecordsBuiltFromProviderData()
        {
            GameObject windowObject = CreateObject("Window");
            GameObject content = CreateObject("Content", windowObject.transform);
            StatisticRecordUI prefab = CreateRecordPrefab("StatisticRecord");
            TestStatisticsDataProvider provider = windowObject.AddComponent<TestStatisticsDataProvider>();
            provider.SetEntries(
                new DataEntry(RecordId.Health, "10 / 100"),
                new DataEntry(RecordId.Mana, "20 / 50"));
            StatisticsWindowUI window = windowObject.AddComponent<StatisticsWindowUI>();
            SetWindowReferences(window, provider, content.transform, prefab);
            window.SetRecordDefinitions(new[]
            {
                RecordDefinition.Health(),
                RecordDefinition.Mana()
            });

            window.Rebuild();

            Assert.AreEqual(2, content.transform.childCount);
            Assert.AreEqual("Health", GetText(content.transform.GetChild(0), "Content/Label").text);
            Assert.AreEqual("10 / 100", GetText(content.transform.GetChild(0), "Content/Value").text);
            Assert.AreEqual("Mana", GetText(content.transform.GetChild(1), "Content/Label").text);
            Assert.AreEqual("20 / 50", GetText(content.transform.GetChild(1), "Content/Value").text);
        }

        [Test]
        public void Window_WhenProviderChanges_RefreshesValues()
        {
            GameObject windowObject = CreateObject("Window");
            GameObject content = CreateObject("Content", windowObject.transform);
            StatisticRecordUI prefab = CreateRecordPrefab("StatisticRecord");
            TestStatisticsDataProvider provider = windowObject.AddComponent<TestStatisticsDataProvider>();
            provider.SetEntries(new DataEntry(RecordId.LastSpellDamage, "6 - 6"));
            StatisticsWindowUI window = windowObject.AddComponent<StatisticsWindowUI>();
            SetWindowReferences(window, provider, content.transform, prefab);
            window.SetRecordDefinitions(new[] { RecordDefinition.LastSpell() });
            window.Rebuild();

            provider.SetEntriesAndNotify(new DataEntry(RecordId.LastSpellDamage, "7 - 7"));

            Assert.AreEqual("7 - 7", GetText(content.transform.GetChild(0), "Content/Value").text);
        }

        private GameObject CreateObject(string name, Transform parent = null)
        {
            GameObject gameObject = new(name);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }

            objectsToDestroy.Add(gameObject);
            return gameObject;
        }

        private StatisticRecordUI CreateRecordPrefab(string name)
        {
            GameObject recordObject = CreateObject(name);
            GameObject content = CreateObject("Content", recordObject.transform);
            CreateText("Label", content.transform);
            CreateText("Value", content.transform);
            return recordObject.AddComponent<StatisticRecordUI>();
        }

        private static TMP_Text CreateText(string name, Transform parent)
        {
            GameObject textObject = new(name);
            textObject.transform.SetParent(parent);
            return textObject.AddComponent<TextMeshProUGUI>();
        }

        private static TMP_Text GetText(Transform root, string path)
        {
            return root.Find(path).GetComponent<TMP_Text>();
        }

        private static void SetWindowReferences(
            StatisticsWindowUI window,
            StatisticsDataProviderBase provider,
            Transform recordsRoot,
            StatisticRecordUI recordPrefab)
        {
            UnityEditor.SerializedObject serializedWindow = new(window);
            serializedWindow.FindProperty("dataProvider").objectReferenceValue = provider;
            serializedWindow.FindProperty("recordsRoot").objectReferenceValue = recordsRoot;
            serializedWindow.FindProperty("recordPrefab").objectReferenceValue = recordPrefab;
            serializedWindow.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class TestStatisticsDataProvider : StatisticsDataProviderBase
        {
            private readonly List<DataEntry> entries = new();

            public override IReadOnlyList<DataEntry> GetStatistics()
            {
                return entries;
            }

            public void SetEntries(params DataEntry[] entries)
            {
                this.entries.Clear();
                this.entries.AddRange(entries);
            }

            public void SetEntriesAndNotify(params DataEntry[] entries)
            {
                SetEntries(entries);
                NotifyChanged();
            }
        }
    }
}
