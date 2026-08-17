using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class Dialogue : MonoBehaviour
{
    private List<DialogueLineData> dialogueLines;
    private DialogueLineData curLineData;
    private EventManager eventManager;
    
    private int curLineIndex;
    private int nextLineIndex;
    private int weightSum = 0;

    void Awake()
    {
        eventManager = FindAnyObjectByType<EventManager>();
        gotoLine(0);
    }

    private void gotoLine(int lineIndex)
    {
        curLineIndex = lineIndex;
        curLineData = dialogueLines[lineIndex];
        nextLineIndex = curLineData.jumpto;
        weightSum += curLineData.weight;

        if (!string.IsNullOrEmpty(curLineData.trigger)) eventManager.TriggerEvent(curLineData.trigger);
    }

    public void activateLine(int lineIndex)
    {

    }




}
