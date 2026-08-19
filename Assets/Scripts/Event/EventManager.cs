using UnityEngine;
using System.Collections.Generic;
using Unity.InferenceEngine;
using System;
using UnityEditor.PackageManager;

public class EventManager : MonoBehaviour
{
    private EventDatabase eventDatabase;

    private Dictionary<string, Action<EventData>> eventActions;
    [SerializeField]
    private string initialEventName;

    public static EventManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        eventDatabase = GetComponent<EventDatabase>(); 
        //eventDatabase should be attached to the same GameObject
        eventActions = new Dictionary<string, Action<EventData>>
        {
            { "day", ExecuteDay },
            { "dialogue", ExecuteDialogue },
            { "item", ExecuteItem },
            { "person", ExecutePerson },
            { "object", ExecuteObject },
            { "itemkill", ExecuteItemKill },
            { "personkill", ExecutePersonKill },
            { "objectkill", ExecuteObjectKill },
            { "sleep", ExecuteSleep },
            { "highlight", ExecuteHighlight },
            { "trigger", ExecuteTrigger },
            { "happiness", ExecuteHappiness },
            { "diary", ExecuteDiary }
        };
        TriggerEvent(initialEventName);
    }



    public void TriggerEvent(string eventSetID)
    {
        if (!eventDatabase.Events.ContainsKey(eventSetID))
        {
            Debug.LogWarning($"Event set ID '{eventSetID}' not found in the database.");
            return;
        }
        foreach (var eventObj in eventDatabase.Events[eventSetID])
        {
           if (eventActions.TryGetValue(eventObj.eventType, out var action))
            {
                action.Invoke(eventObj);
            }
            else
            {
                Debug.LogWarning($"No action defined for event type: {eventObj.eventType}");
            }
        }
    }

    private void ExecuteDay(EventData eventData)
    {
        // Implement day execution logic
        //Debug.Log($"Executing Day: {eventData.arg1}");
    }
    private void ExecuteDialogue(EventData eventData)
    {
        // Implement dialogue execution logic
        //Debug.Log($"Executing Dialogue: {eventData.arg1}");
    }
    private void ExecuteItem(EventData eventData)
    {
        // Implement item execution logic
        //Debug.Log($"Executing Item: {eventData.arg1}");
    }
    private void ExecuteItemKill(EventData eventData)
    {
        // Implement item kill execution logic
        //Debug.Log($"Executing Item Kill: {eventData.arg1}");
    }
    private void ExecuteTrigger(EventData eventData)
    {
        // Implement trigger execution logic
        //Debug.Log($"Executing Trigger: {eventData.arg1}");
    }
    private void ExecutePerson(EventData eventData)
    {
        // Implement person execution logic
        //Debug.Log($"Executing Person: {eventData.arg1}");
    }
    private void ExecutePersonKill(EventData eventData)
    {
        // Implement person kill execution logic
        //Debug.Log($"Executing Person Kill: {eventData.arg1}");
    }
    private void ExecuteObject(EventData eventData)
    {
        // Implement object execution logic
        //Debug.Log($"Executing Object: {eventData.arg1}");
    }
    private void ExecuteObjectKill(EventData eventData)
    {
        // Implement object kill execution logic
        //Debug.Log($"Executing Object Kill: {eventData.arg1}");
    }
    private void ExecuteSleep(EventData eventData)
    {
        // Implement sleep execution logic
        //Debug.Log($"Executing Sleep: {eventData.arg1}");
    }
    private void ExecuteHighlight(EventData eventData)
    {
        // Implement highlight execution logic
        //Debug.Log($"Executing Highlight: {eventData.arg1}");
    }
    private void ExecuteHappiness(EventData eventData)
    {
        // Implement happiness execution logic
        //Debug.Log($"Executing Happiness: {eventData.arg1}");
    }
    private void ExecuteDiary(EventData eventData)
    {
        // Implement diary execution logic
        //Debug.Log($"Executing Diary: {eventData.arg1}");
    }
}