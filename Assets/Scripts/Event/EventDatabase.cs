using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class EventDatabase : MonoBehaviour
{
    [SerializeField]
    private TextAsset eventCSV;

    private Dictionary<string, List<EventData>> events = new();
    public IReadOnlyDictionary<string, List<EventData>> Events => events;

    void Awake()
    {
        LoadEvents();
        //DBLogForDebug();
    }

    private void LoadEvents()
    {
        using (StringReader reader = new StringReader(eventCSV.text))
        using (CsvReader csv = new CsvReader(
            reader,
            CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<EventData>().ToList();
            // eventCode를 키로, 동일 코드의 EventData 리스트를 값으로 가지는 딕셔너리 생성
            events = records.GroupBy(r => r.eventCode ?? string.Empty)
                            .ToDictionary(g => g.Key, g => g.ToList());
        }
    }

    private void DBLogForDebug()
    {
        // Debug output: show loaded record counts and contents per event code
        Debug.Log($"EventDatabase: loaded {events.Sum(kv => kv.Value.Count)} records across {events.Count} keys.");
        foreach (var kv in events)
        {
            Debug.Log($"Event Code '{kv.Key}' => {kv.Value.Count} record(s)");
            for (int i = 0; i < kv.Value.Count; i++)
            {
                var e = kv.Value[i];
                Debug.Log($"[{kv.Key}][{i}] eventCode='{e.eventCode}', eventType='{e.eventType}', arg1='{e.arg1}', arg2='{e.arg2}', arg3='{e.arg3}'");
            }
        }
    }

    public void AddEvent(string eventSetID, EventData newEvent)
    {
        if (!events.ContainsKey(eventSetID))
        {
            events[eventSetID] = new List<EventData>();
        }
        events[eventSetID].Add(newEvent);
    }
}
