using UnityEngine;
using System.Collections.Generic;

public class NewDialogueManager : MonoBehaviour
{
    [SerializeField]
    private NewDialogueDatabase database;

    void Start()
    {
        
    }

    public NewDialogue GetDialogue(string dialogueCode)
    {
        return database.Dialogues.GetValueOrDefault(dialogueCode);
    }
}
