using UnityEngine;
using CsvHelper;
using NUnit.Framework;
using System.Collections.Generic;

public class NewDialogueDatabase : MonoBehaviour
{
    [SerializeField]
    private TextAsset dialogueCSV;
    private Dictionary<string, NewDialogue> dialogues = new();
    public IReadOnlyDictionary<string, NewDialogue> Dialogues => dialogues;

    void Awake()
    {
        LoadDialogues();
        DBLogForDebug();
    }

    private void DBLogForDebug()
    {
        Debug.Log($"NewDialogueDatabase: loaded {dialogues.Count} dialogue codes.");
        foreach (var kv in dialogues)
        {
            Debug.Log($"Dialogue Code '{kv.Key}' => {kv.Value}");
            kv.Value.DataLogForDebug();
        }
        Debug.Log("로그끝");
    }

    private void LoadDialogues()
    {
        using (var reader = new System.IO.StringReader(dialogueCSV.text))
        using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<NewDialogueLineData>();
            foreach (var record in records)
            {
                record.weight ??= 0; // Set default weight to 0 if null
                record.jumpto ??= record.dialoueNum + 1; // Set default jumpto to next line if null
                AddDialogueLine(record);
            }
        }
    }

    private void AddDialogueLine(NewDialogueLineData line)
    {
        if (!dialogues.ContainsKey(line.dialogueCode))
        {
            dialogues[line.dialogueCode] = new NewDialogue();
        }
        dialogues[line.dialogueCode].AddLineToPage(line.dialoueNum, line);
    }
}
