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

    void Start()
    {
        LoadEvents();
    }

    private void LoadEvents()
    {
        using (StringReader reader = new StringReader(eventCSV.text))
        using (CsvReader csv = new CsvReader(
            reader,
            CultureInfo.InvariantCulture))
        {
            // Read the CSV file and convert it to a list of EventData
        }
    }
}
