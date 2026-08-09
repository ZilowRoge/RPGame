using System.Collections.Generic;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.UI.Statistics
{
    public sealed class StatisticsWindowUI : MonoBehaviour
    {
        [SerializeField] private StatisticsDataProviderBase dataProvider;
        [SerializeField] private RecordsBuilder recordBuilder = new();
        [SerializeField] private Transform recordsRoot;
        [SerializeField] private StatisticRecordUI recordPrefab;

        private readonly List<StatisticRecordUI> records = new();

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();

            if (Application.isPlaying)
            {
                Rebuild();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public void SetDataProvider(StatisticsDataProviderBase provider)
        {
            if (dataProvider == provider)
            {
                return;
            }

            Unsubscribe();
            dataProvider = provider;
            Subscribe();
            Refresh();
        }

        public void SetRecordDefinitions(IReadOnlyList<RecordDefinition> definitions)
        {
            recordBuilder ??= new RecordsBuilder();
            recordBuilder.Definitions ??= new List<RecordDefinition>();
            recordBuilder.Definitions.Clear();
            if (definitions != null)
            {
                recordBuilder.Definitions.AddRange(definitions);
            }

            Rebuild();
        }

        public void Rebuild()
        {
            ResolveReferences();
            Subscribe();
            ClearRecords();

            if (recordsRoot == null || recordPrefab == null)
            {
                return;
            }

            IReadOnlyList<StatisticRecordData> data = BuildRecords();
            for (int i = 0; i < data.Count; i++)
            {
                StatisticRecordUI record = Instantiate(recordPrefab, recordsRoot);
                record.SetText(data[i].Label, data[i].ValueText);
                records.Add(record);
            }
        }

        public void Refresh()
        {
            IReadOnlyList<StatisticRecordData> data = BuildRecords();
            if (data.Count != records.Count)
            {
                Rebuild();
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                records[i].SetText(data[i].Label, data[i].ValueText);
            }
        }

        private void ResolveReferences()
        {
            if (dataProvider == null)
            {
                dataProvider = GetComponentInParent<StatisticsDataProviderBase>();
            }

            if (dataProvider == null)
            {
                dataProvider = FindAnyObjectByType<StatisticsDataProviderBase>();
            }

            recordBuilder ??= new RecordsBuilder();

            if (recordsRoot == null)
            {
                recordsRoot = transform.Find("Viewport/Content")
                    ?? transform.Find("Content")
                    ?? transform;
            }
        }

        private void Subscribe()
        {
            if (dataProvider != null)
            {
                dataProvider.Changed -= OnProviderChanged;
                dataProvider.Changed += OnProviderChanged;
            }
        }

        private void Unsubscribe()
        {
            if (dataProvider != null)
            {
                dataProvider.Changed -= OnProviderChanged;
            }
        }

        private void OnProviderChanged()
        {
            Refresh();
        }

        private IReadOnlyList<StatisticRecordData> BuildRecords()
        {
            IReadOnlyList<DataEntry> entries = dataProvider != null
                ? dataProvider.GetStatistics()
                : System.Array.Empty<DataEntry>();

            return recordBuilder.Build(entries);
        }

        private void ClearRecords()
        {
            for (int i = records.Count - 1; i >= 0; i--)
            {
                if (records[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(records[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(records[i].gameObject);
                    }
                }
            }

            records.Clear();
        }
    }
}
