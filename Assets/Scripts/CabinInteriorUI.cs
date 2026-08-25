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

    [SerializeField] private NewDialogueManager dialogueManager;
    [SerializeField] private Text dialogueText;
    [SerializeField] private Button choiceButton1;
    [SerializeField] private Text choiceButton1Text;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private Text choiceButton2Text;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image backgroundImage;

    [SerializeField] private string firstVisitDialogueCode = "a";
    [SerializeField] private string revisitDialogueCode = "b";

    // key: "{speakerCode}_{face}" (e.g. "victor_smile")
    [SerializeField] private List<SpriteMapping> faceSprites = new();
    // key: CSV의 background 값
    [SerializeField] private List<SpriteMapping> backgroundSprites = new();

    private const string HasVisitedPrefKey = "CabinInterior_HasVisited";

    private NewDialogue currentDialogue;
    private bool hasVisitedBefore;
    private Dictionary<string, Sprite> faceSpriteMap;
    private Dictionary<string, Sprite> backgroundSpriteMap;
    private readonly Dictionary<string, string> lastFaceBySpeaker = new();

    private void Awake()
    {
        hasVisitedBefore = PlayerPrefs.GetInt(HasVisitedPrefKey, 0) == 1;
        faceSpriteMap = faceSprites.ToDictionary(entry => entry.key, entry => entry.sprite);
        backgroundSpriteMap = backgroundSprites.ToDictionary(entry => entry.key, entry => entry.sprite);

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
        string dialogueCode = hasVisitedBefore ? revisitDialogueCode : firstVisitDialogueCode;
        if (!hasVisitedBefore)
        {
            hasVisitedBefore = true;
            PlayerPrefs.SetInt(HasVisitedPrefKey, 1);
            PlayerPrefs.Save();
        }

        currentDialogue = dialogueManager.GetDialogue(dialogueCode);
        if (currentDialogue == null)
        {
            Debug.LogError($"CabinInteriorUI: dialogue '{dialogueCode}' not found.");
            gameObject.SetActive(false);
            return;
        }
        lastFaceBySpeaker.Clear();
        currentDialogue.Initialize(0);
        ShowCurrentPage();
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
        if (choiceIndex >= currentDialogue.GetCurrentLines().Count)
        {
            return;
        }
        currentDialogue.Choose(choiceIndex);
        ShowCurrentPage();
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
            SetChoiceButton(choiceButton1, choiceButton1Text, lines.Count > 0 ? lines[0].lineKr : null);
            SetChoiceButton(choiceButton2, choiceButton2Text, lines.Count > 1 ? lines[1].lineKr : null);
        }
        else
        {
            NewDialogueLineData line = currentDialogue.GetCurrentLine();
            dialogueText.text = string.IsNullOrEmpty(line.speakerCode) || line.speakerCode == "description"
                ? line.lineKr
                : $"<b>{line.speakerCode}</b>\n{line.lineKr}";
            SetChoiceButton(choiceButton1, choiceButton1Text, null);
            SetChoiceButton(choiceButton2, choiceButton2Text, null);
            UpdatePortrait(line);
            UpdateBackground(line);
        }
    }

    private bool IsCurrentPageEnd()
    {
        return !currentDialogue.IsCurrentPageChoice() && currentDialogue.GetCurrentLine().speakerCode == "end";
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
}
