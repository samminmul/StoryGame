using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 선실 내부 UI 패널에 붙어서 미연시 스타일 대사 진행(엔터로 다음 대사, 분기에서는 답장 버튼)을 담당한다.
public class CabinInteriorUI : MonoBehaviour
{
    [System.Serializable]
    private struct SpriteMapping
    {
        public string key;
        public Sprite sprite;
    }

    [SerializeField] private Text dialogueText;
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Text choiceButton1Text;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private Text choiceButton2Text;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float activeSpeakerScale = 1.1f;

    [SerializeField] private InventoryManager inventoryManager;
    // itemCode가 있는 선택지가 아이템을 보유했을 때 바뀌는 배경 이미지
    // TODO: 현재 씬에 미연결(NULL) 상태. 지정 전까지는 interactable만 true가 되고 버튼 이미지는 하이라이트로 바뀌지 않음.
    [SerializeField] private Sprite choiceHighlightSprite;
    // TODO: 씬에 "아이템을 소모하시겠습니까?" 확인 패널(Yes/No 버튼 포함)을 새로 만들어서 아래 세 필드에 연결해야 함. 아직 씬에 없음.
    [SerializeField] private GameObject itemConsumeConfirmPanel;
    [SerializeField] private Button itemConsumeConfirmYesButton;
    [SerializeField] private Button itemConsumeConfirmNoButton;

    // key: "{speakerCode}_{face}" (e.g. "victor_smile")
    [SerializeField] private List<SpriteMapping> faceSprites = new();
    // key: CSV의 background 값
    [SerializeField] private List<SpriteMapping> backgroundSprites = new();

    private NewDialogue currentDialogue;
    private Dictionary<string, Sprite> faceSpriteMap;
    private Dictionary<string, Sprite> backgroundSpriteMap;
    private readonly Dictionary<string, string> lastFaceBySpeaker = new();
    private Sprite choiceButton1NormalSprite;
    private Sprite choiceButton2NormalSprite;
    private int pendingChoiceIndex;

    private void Awake()
    {
        faceSpriteMap = faceSprites.ToDictionary(entry => entry.key, entry => entry.sprite);
        backgroundSpriteMap = backgroundSprites.ToDictionary(entry => entry.key, entry => entry.sprite);

        if (choiceButton1 != null)
        {
            choiceButton1NormalSprite = choiceButton1.image.sprite;
            choiceButton1.onClick.AddListener(() => OnChoiceSelected(0));
        }
        if (choiceButton2 != null)
        {
            choiceButton2NormalSprite = choiceButton2.image.sprite;
            choiceButton2.onClick.AddListener(() => OnChoiceSelected(1));
        }
        if (itemConsumeConfirmYesButton != null)
        {
            itemConsumeConfirmYesButton.onClick.AddListener(OnItemConsumeConfirmed);
        }
        if (itemConsumeConfirmNoButton != null)
        {
            itemConsumeConfirmNoButton.onClick.AddListener(OnItemConsumeCancelled);
        }
    }

    private void Update()
    {
        if (currentDialogue == null)
        {
            return;
        }

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

        if (!currentDialogue.IsCurrentPageChoice() && keyboard.enterKey.wasPressedThisFrame)
        {
            Advance();
        }
    }

    private void Advance()
    {
        currentDialogue.GoToNextPage();
        ShowCurrentPage();
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (currentDialogue == null || !currentDialogue.IsCurrentPageChoice())
        {
            return;
        }
        var lines = currentDialogue.GetCurrentLines();
        if (choiceIndex >= lines.Count)
        {
            return;
        }

        // 아이템이 필요한 선택지는 SetChoiceButton()에서 미보유 시 이미 클릭 불가(interactable=false) 상태이므로,
        // 여기 도달했다면 아이템을 보유한 상태 -> 소모 여부를 먼저 확인한다.
        if (!string.IsNullOrEmpty(lines[choiceIndex].itemCode))
        {
            pendingChoiceIndex = choiceIndex;
            if (itemConsumeConfirmPanel != null)
            {
                itemConsumeConfirmPanel.SetActive(true);
            }
            return;
        }

        currentDialogue.Choose(choiceIndex);
        ShowCurrentPage();
    }

    private void OnItemConsumeConfirmed()
    {
        if (itemConsumeConfirmPanel != null)
        {
            itemConsumeConfirmPanel.SetActive(false);
        }

        // TODO: 인벤토리에서 이 선택지에 연결된 아이템(현재 페이지 lines[pendingChoiceIndex].itemCode)을 실제로 제거하는 코드
        // 예: inventoryManager.DeleteItem(item);

        currentDialogue.Choose(pendingChoiceIndex);
        ShowCurrentPage();
    }

    private void OnItemConsumeCancelled()
    {
        if (itemConsumeConfirmPanel != null)
        {
            itemConsumeConfirmPanel.SetActive(false);
        }
    }

    private void ShowCurrentPage()
    {
        if (IsCurrentPageEnd())
        {
            gameObject.SetActive(false);
            currentDialogue = null;
            return;
        }

        if (currentDialogue.IsCurrentPageChoice())
        {
            var lines = currentDialogue.GetCurrentLines();
            dialogueText.text = string.Empty;
            SetChoiceButton(choiceButton1, choiceButton1Text, choiceButton1NormalSprite, lines.Count > 0 ? lines[0] : null);
            SetChoiceButton(choiceButton2, choiceButton2Text, choiceButton2NormalSprite, lines.Count > 1 ? lines[1] : null);
        }
        else
        {
            NewDialogueLineData line = currentDialogue.GetCurrentLine();
            dialogueText.text = string.IsNullOrEmpty(line.speakerCode) || line.speakerCode == "description"
                ? line.lineKr
                : $"<b>{line.speakerCode}</b>\n{line.lineKr}";
            SetChoiceButton(choiceButton1, choiceButton1Text, choiceButton1NormalSprite, null);
            SetChoiceButton(choiceButton2, choiceButton2Text, choiceButton2NormalSprite, null);
            UpdatePortrait(line);
            UpdateBackground(line);
        }
    }

    private bool IsCurrentPageEnd()
    {
        return !currentDialogue.IsCurrentPageChoice() && currentDialogue.GetCurrentLine().speakerCode == "end";
    }

    // itemCode가 있는 선택지는 항상 보이되, 아이템을 보유했을 때만 하이라이트 이미지로 바뀌고 선택 가능해진다.
    private void SetChoiceButton(Button button, Text label, Sprite normalSprite, NewDialogueLineData line)
    {
        if (button == null)
        {
            return;
        }

        bool show = line != null;
        button.gameObject.SetActive(show);
        if (!show)
        {
            return;
        }

        if (label != null)
        {
            label.text = line.lineKr;
        }

        bool requiresItem = !string.IsNullOrEmpty(line.itemCode);
        bool hasItem = !requiresItem || (inventoryManager != null && inventoryManager.HaveItem(line.itemCode));

        button.interactable = hasItem;
        button.image.sprite = requiresItem && hasItem ? choiceHighlightSprite : normalSprite;
    }

    private void UpdatePortrait(NewDialogueLineData line)
    {
        if (portraitImage == null)
        {
            return;
        }

        // 설명문처럼 화자가 명시되지 않은 줄은 직전에 보여주던 초상화를 그대로 유지한다.
        if (string.IsNullOrEmpty(line.speakerCode) || line.speakerCode == "description")
        {
            return;
        }

        // face 값이 비어있으면 같은 화자가 마지막으로 보여준 표정을 그대로 유지한다.
        string face = line.face;
        if (string.IsNullOrEmpty(face))
        {
            lastFaceBySpeaker.TryGetValue(line.speakerCode, out face);
        }

        string key = $"{line.speakerCode}_{face}";
        if (!string.IsNullOrEmpty(face) && faceSpriteMap.TryGetValue(key, out Sprite sprite))
        {
            portraitImage.sprite = sprite;
            portraitImage.gameObject.SetActive(true);
            portraitImage.rectTransform.localScale = Vector3.one * activeSpeakerScale;
            lastFaceBySpeaker[line.speakerCode] = face;
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    private void UpdateBackground(NewDialogueLineData line)
    {
        // 빈 값이면 이전 배경을 그대로 유지한다.
        if (backgroundImage == null || string.IsNullOrEmpty(line.background))
        {
            return;
        }
        if (backgroundSpriteMap.TryGetValue(line.background, out Sprite sprite))
        {
            backgroundImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"CabinInteriorUI: background sprite for key '{line.background}' not found.");
        }
    }

    public void StartDialogue(NewDialogue dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogError("StartDialogue() called with a null dialogue.");
            return;
        }

        currentDialogue = dialogue;
        lastFaceBySpeaker.Clear();
        currentDialogue.Initialize(0);
        if (itemConsumeConfirmPanel != null)
        {
            itemConsumeConfirmPanel.SetActive(false);
        }
        gameObject.SetActive(true);
        ShowCurrentPage();
    }
}
