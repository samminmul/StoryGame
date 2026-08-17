using System;
using CsvHelper.Configuration.Attributes;

// 대화 엑셀 시트 한 행에 대응하는 데이터 컨테이너 (엑셀 파싱은 팀원 담당, 이 클래스는 그 결과를 받는 계약).
//
// jumpto 규칙:
//  - jumpto는 원본 엑셀의 "행 번호"를 그대로 가리킨다 (숫자 문자열: 8, 9, 10, 11 ...).
//  - 그래서 각 줄도 자기 자신의 행 번호(LineNumber)를 알고 있어야 jumpto로 대상을 찾을 수 있다.
//    파싱 단계에서 LineNumber에 그 줄의 엑셀 행 번호를 채워서 넘겨줘야 한다.
//  - 숫자가 아닌 jumpto 값(AA, AB 등)은 "다음 줄"이 아니라 대화 종료 후 넘길 결과 코드로 취급한다.
[Serializable]
public class DialogueLineData
{
    // 엑셀 원본의 행 번호. jumpto가 이 값을 참조해서 대상 줄을 찾는다.
    [Ignore]
    public int LineNumber { get; set; }

    [Name("dialogue-code")]
    public string DialogueCode { get; set; }

    [Name("speaker-code")]
    public string SpeakerCode { get; set; }

    [Name("jumpto")]
    public string Jumpto { get; set; }

    [Name("line-kr")]
    public string LineKr { get; set; }

    [Name("line-eng")]
    public string LineEng { get; set; }

    [Name("item-code")]
    public string ItemCode { get; set; }

    [Name("weight")]
    public int? Weight { get; set; }

    [Name("onlywhen")]
    public string OnlyWhen { get; set; }

    [Name("trigger")]
    public string Trigger { get; set; }
}