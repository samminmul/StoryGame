using System.Collections.Generic;
using UnityEngine;

// 임시 테스트 전용: 팀원의 엑셀 파서가 아직 없어서, 스크린샷 예시 데이터를 하드코딩해 넣고
// DialogueManager가 정상적으로 도는지 확인하기 위한 부트스트랩. 실제 파서가 붙으면 이 스크립트는 삭제.
public class TestDialogueBootstrap : MonoBehaviour
{
    private void Start()
    {
        DialogueManager.Instance.OnDialogueEnded += result =>
        {
            Debug.Log($"[TestDialogueBootstrap] 대화 종료. 결과 코드: {result}");
        };

        DialogueManager.Instance.StartDialogue(BuildSampleLines());
    }

    // dialogue-code "a" 예시. 엑셀 스크린샷의 행 번호(2~13)를 LineNumber/Jumpto에 그대로 사용.
    private List<DialogueLineData> BuildSampleLines()
    {
        return new List<DialogueLineData>
        {
            new DialogueLineData { LineNumber = 2, DialogueCode = "a", SpeakerCode = "victor", LineKr = "안녕하세요?", LineEng = "Hello?" },
            new DialogueLineData { LineNumber = 3, DialogueCode = "a", SpeakerCode = "victor", LineKr = "만나서 반가워요", LineEng = "Nice to meet you." },
            new DialogueLineData { LineNumber = 4, DialogueCode = "a", SpeakerCode = "description", LineKr = "빅터가 살며시 웃는다", LineEng = "Victor smiles slightly.", OnlyWhen = "<condition-code>" },
            new DialogueLineData { LineNumber = 5, DialogueCode = "a", LineKr = "무시한다", LineEng = "Ignore", Weight = -100, Jumpto = "8" },
            new DialogueLineData { LineNumber = 6, DialogueCode = "a", LineKr = "저도 만나서 반가워요", LineEng = "Nice to meet you too!", Weight = 10, Jumpto = "9" },
            new DialogueLineData { LineNumber = 7, DialogueCode = "a", LineKr = "(거울을 보여준다)", LineEng = "(Show the mirror)", ItemCode = "mirror", Weight = 20, Jumpto = "10" },
            new DialogueLineData { LineNumber = 8, DialogueCode = "a", SpeakerCode = "victor", LineKr = "......", LineEng = "......", Trigger = "B", Jumpto = "11" },
            new DialogueLineData { LineNumber = 9, DialogueCode = "a", SpeakerCode = "victor", LineKr = "날씨가 좋네요.", LineEng = "It's such a good day.", Jumpto = "11" },
            new DialogueLineData { LineNumber = 10, DialogueCode = "a", SpeakerCode = "victor", LineKr = "그 거울은 뭔가요?", LineEng = "What's the mirror for?", Jumpto = "11" },
            new DialogueLineData { LineNumber = 11, DialogueCode = "a", SpeakerCode = "victor", LineKr = "그나저나, 조금 배고프지 않아?", LineEng = "Anyway, aren't you hungry now?" },
            new DialogueLineData { LineNumber = 12, DialogueCode = "a", SpeakerCode = "end", Jumpto = "AA" },
            new DialogueLineData { LineNumber = 13, DialogueCode = "a", SpeakerCode = "end", Jumpto = "AB" },
        };
    }
}
