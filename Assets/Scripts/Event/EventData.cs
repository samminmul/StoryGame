using NUnit.Framework;
using System;
using UnityEngine;

[Serializable]
public class EventData
{
    public string eventCode { get; set; }
    public string eventType { get; set; }
    public string arg1 { get; set; }
    public string arg2 { get; set; }
    public string arg3 { get; set; }
}