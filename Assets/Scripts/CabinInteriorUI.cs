using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 선실 내부 UI 패널에 붙어서 미연시 스타일 대사 진행(엔터로 다음 대사, 분기에서는 답장 버튼)을 담당한다.
public class CabinInteriorUI : MonoBehaviour
{
    [System.Serializable]
    private class DialogueNode
    {
        public string speaker;
        public string text;
        public string[] choiceLabels;
        public int[] choiceNextIndices;
        public int nextIndex = -1;
    }

    [SerializeField] private Text dialogueText;
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Text choiceButton1Text;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private Text choiceButton2Text;

    private const int FirstVisitStartIndex = 0;
    private const int RevisitStartIndex = 5;
    private const string HasVisitedPrefKey = "CabinInterior_HasVisited";

    // TODO: 임시 대사. 실제 대사가 정해지면 교체한다.
    private readonly DialogueNode[] nodes =
    {
        // 0~4: 첫 방문
        new DialogueNode { speaker = "", text = "선실 안은 조용하고 아늑하다.", nextIndex = 1 },
        new DialogueNode
        {
            speaker = "선원",
            text = "여기가 네 방이야. 짐은 잘 챙겼어?",
            choiceLabels = new[] { "네, 챙겼어요", "아직이요" },
            choiceNextIndices = new[] { 2, 3 },
        },
        new DialogueNode { speaker = "선원", text = "다행이네. 그럼 푹 쉬도록 해.", nextIndex = 4 },
        new DialogueNode { speaker = "선원", text = "그럼 서둘러 챙기는 게 좋을 거야.", nextIndex = 4 },
        new DialogueNode { speaker = "선원", text = "필요한 게 있으면 언제든 불러.", nextIndex = -1 },

        // 5~: 재방문 (한 번이라도 대화한 뒤 다시 들어왔을 때)
        new DialogueNode { speaker = "선원", text = "또 왔네. 별일 없지?", nextIndex = -1 },
    };

    private int currentIndex;
    private bool hasVisitedBefore;

    private void Awake()
    {
        hasVisitedBefore = PlayerPrefs.GetInt(HasVisitedPrefKey, 0) == 1;

        if (choiceButton1 != null)
        {
            choiceButton1.onClick.AddListener(() => OnChoiceSelected(0));
        }
        if (choiceButton2 != null)
        {
            choiceButton2.onClick.AddListener(() => OnChoiceSelected(1));
        }
    }

    private void OnEnable()
    {
        currentIndex = hasVisitedBefore ? RevisitStartIndex : FirstVisitStartIndex;
        if (!hasVisitedBefore)
        {
            hasVisitedBefore = true;
            PlayerPrefs.SetInt(HasVisitedPrefKey, 1);
            PlayerPrefs.Save();
        }
        ShowNode(currentIndex);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            gameObject.SetActive(false);
            return;
        }

        bool hasChoices = nodes[currentIndex].choiceLabels != null && nodes[currentIndex].choiceLabels.Length > 0;
        if (!hasChoices && keyboard.enterKey.wasPressedThisFrame)
        {
            Advance();
        }
    }

    private void Advance()
    {
        int nextIndex = nodes[currentIndex].nextIndex;
        if (nextIndex < 0)
        {
            gameObject.SetActive(false);
            return;
        }
        currentIndex = nextIndex;
        ShowNode(currentIndex);
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        int[] choiceNextIndices = nodes[currentIndex].choiceNextIndices;
        if (choiceNextIndices == null || choiceIndex >= choiceNextIndices.Length)
        {
            return;
        }
        currentIndex = choiceNextIndices[choiceIndex];
        ShowNode(currentIndex);
    }

    private void ShowNode(int index)
    {
        DialogueNode node = nodes[index];
        dialogueText.text = string.IsNullOrEmpty(node.speaker)
            ? node.text
            : $"<b>{node.speaker}</b>\n{node.text}";

        bool hasChoices = node.choiceLabels != null && node.choiceLabels.Length > 0;
        SetChoiceButton(choiceButton1, choiceButton1Text, hasChoices && node.choiceLabels.Length > 0 ? node.choiceLabels[0] : null);
        SetChoiceButton(choiceButton2, choiceButton2Text, hasChoices && node.choiceLabels.Length > 1 ? node.choiceLabels[1] : null);
    }

    private void SetChoiceButton(Button button, Text label, string text)
    {
        if (button == null)
        {
            return;
        }
        bool show = text != null;
        button.gameObject.SetActive(show);
        if (show && label != null)
        {
            label.text = text;
        }
    }
}
