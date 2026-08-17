using Mono.Cecil.Cil;
using UnityEngine;

public class DialogueLineData : MonoBehaviour
{
    public string code;
    public string speaker = string.Empty;
    public string line;
    public int jumpto;
    public string item = string.Empty;
    public int weight = 0;
    public string onlywhen = string.Empty;
    public string trigger = string.Empty;
}
