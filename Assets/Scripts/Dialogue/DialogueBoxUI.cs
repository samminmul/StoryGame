using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 대화창 임시(placeholder) UI. 말풍선 텍스트와 최대 choiceButtons.Length개의 선택지 버튼을 보여준다.
// 실제 아트/연출 붙기 전까지 쓰는 스캐폴딩.
public class DialogueBoxUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Text speakerText;
    [SerializeField] private Text bodyText;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private Text[] choiceButtonTexts;

    private Action pendingAdvance;

    private void Update()
    {
        if (pendingAdvance == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.enterKey.wasPressedThisFrame)
        {
            Action advance = pendingAdvance;
            pendingAdvance = null;
            advance.Invoke();
        }
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        pendingAdvance = null;
        panel.SetActive(false);
    }

    public void ShowLine(string speaker, string body, Action onAdvance)
    {
        SetChoicesActive(false);
        speakerText.text = speaker;
        bodyText.text = body;
        pendingAdvance = onAdvance;
    }

    public void ShowChoices(List<DialogueLineData> choices, Action<DialogueLineData> onSelected)
    {
        pendingAdvance = null;
        speakerText.text = string.Empty;
        bodyText.text = string.Empty;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool show = i < choices.Count;
            choiceButtons[i].gameObject.SetActive(show);
            if (!show)
            {
                continue;
            }

            DialogueLineData choice = choices[i];
            choiceButtonTexts[i].text = choice.LineKr;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => onSelected(choice));
        }
    }

    private void SetChoicesActive(bool active)
    {
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(active);
        }
    }
}
