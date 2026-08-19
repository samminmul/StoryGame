using UnityEngine;

public class NewDialogueLineData //csv 행 하나의 data
{
    public string dialogueCode { get; set; }
    public int dialoueNum { get; set; }
    public string speakerCode { get; set; }
    public string face { get; set; }
    public int? jumpto { get; set; }
    public string lineKr { get; set; }
    public string lineEng { get; set; }
    public string itemCode { get; set; }
    public int? weight { get; set; }
    public string onlyWhen { get; set; }
    public string trigger { get; set; }
    public string background { get; set; }
}
