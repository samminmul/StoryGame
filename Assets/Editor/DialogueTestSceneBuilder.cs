using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 대화창 UI(DialogueManager/DialogueBoxUI) 확인용 임시 테스트 씬 빌더.
// 텍스트 위치는 Assets/sprites/대화창/대화창.png(1920x1080 기준) 안의 네임플레이트/본문 사각형을
// 픽셀 스캔해서 얻은 좌표에 맞춰뒀다. 이 아트가 바뀌면 아래 좌표 상수들도 같이 바꿔줘야 한다.
public static class DialogueTestSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/DialogueTest.unity";

    // 대화창.png(1920x1080) 안에서 실제 박스가 그려진 영역을 픽셀 스캔으로 찾은 좌표.
    private static readonly Vector2 NameplateAnchoredPos = new Vector2(89f, -765f);
    private static readonly Vector2 NameplateSize = new Vector2(159f, 36f);
    private static readonly Vector2 BodyAnchoredPos = new Vector2(94f, -840f);
    private static readonly Vector2 BodySize = new Vector2(1485f, 194f);
    private const float ChoiceButtonHeight = 42f;
    private const float ChoiceButtonSpacing = 50f;

    [MenuItem("Tools/Story Game/Build Dialogue Test Scene (Placeholder)")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = System.IO.File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        BuildCamera();
        BuildEventSystem();
        Canvas canvas = BuildCanvas();
        GameObject panel = BuildDialoguePanel(canvas.transform, out Text speakerText, out Text bodyText, out Button[] choiceButtons, out Text[] choiceButtonTexts);
        BuildDialogueSystem(panel, speakerText, bodyText, choiceButtons, choiceButtonTexts);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        Debug.Log("DialogueTest placeholder scene built at " + ScenePath + " - Play를 누르면 샘플 대화(dialogue-code \"a\")가 바로 시작됩니다.");
    }

    // 대화창 스프라이트(1920x1080, PPU 100)를 화면에 꽉 채워서 보여주는 카메라.
    private static void BuildCamera()
    {
        Camera camera = Object.FindAnyObjectByType<Camera>();
        if (camera != null)
        {
            return;
        }

        GameObject cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraGO.tag = "MainCamera";
        camera = cameraGO.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.4f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
    }

    private static void BuildEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static Canvas BuildCanvas()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        return canvas;
    }

    private static GameObject BuildDialoguePanel(Transform canvasTransform, out Text speakerText, out Text bodyText, out Button[] choiceButtons, out Text[] choiceButtonTexts)
    {
        // 패널 자체는 눈에 안 보이는 풀스크린 컨테이너. 실제 배경은 씬에 배치된 "대화창" 스프라이트(월드
        // 스페이스, 1920x1080/PPU100로 카메라 뷰를 꽉 채움)가 담당하고, 여기서는 그 위에 텍스트만 얹는다.
        // 예전 버그(GameObject.Find가 비활성 오브젝트를 못 찾아서 재실행할 때마다 DialoguePanel이
        // 중복 생성되던 문제)로 이미 만들어졌을 수 있는 중복 패널을 정리한다. 하나만 남기고 나머지는 삭제.
        RemoveDuplicateChildren(canvasTransform, "DialoguePanel");

        // DialoguePanel은 평소 비활성 상태라 GameObject.Find로는 못 찾는다(비활성 오브젝트 제외됨).
        // 반드시 Transform.Find로 캔버스의 자식을 직접 찾아야 재실행할 때 중복 생성되지 않는다.
        GameObject panel = canvasTransform.Find("DialoguePanel")?.gameObject;
        bool created = panel == null;
        if (created)
        {
            panel = new GameObject("DialoguePanel", typeof(RectTransform));
            panel.transform.SetParent(canvasTransform, false);
        }

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;

        if (created)
        {
            panel.SetActive(false);
        }

        // 좌표는 대화창.png(1920x1080) 안의 네임플레이트/본문 사각형을 픽셀 스캔해서 얻은 값.
        speakerText = BuildText(panel.transform, "SpeakerText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), NameplateAnchoredPos, NameplateSize, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
        bodyText = BuildText(panel.transform, "BodyText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), BodyAnchoredPos, BodySize, 26, FontStyle.Normal, TextAnchor.UpperLeft);

        const int maxChoices = 4;
        choiceButtons = new Button[maxChoices];
        choiceButtonTexts = new Text[maxChoices];
        for (int i = 0; i < maxChoices; i++)
        {
            RemoveDuplicateChildren(panel.transform, "ChoiceButton" + i);

            float y = BodyAnchoredPos.y - i * ChoiceButtonSpacing;
            GameObject buttonGO = panel.transform.Find("ChoiceButton" + i)?.gameObject;
            bool buttonCreated = buttonGO == null;
            if (buttonCreated)
            {
                buttonGO = new GameObject("ChoiceButton" + i, typeof(Image), typeof(Button));
                buttonGO.transform.SetParent(panel.transform, false);
            }

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.anchoredPosition = new Vector2(BodyAnchoredPos.x, y);
            buttonRect.sizeDelta = new Vector2(BodySize.x, ChoiceButtonHeight);

            // 아트에 버튼 그래픽이 없어서, 클릭 가능한 영역만 살짝 티나게 반투명 흰색으로 표시.
            Image buttonImage = buttonGO.GetComponent<Image>();
            buttonImage.color = new Color(1f, 1f, 1f, 0.12f);

            choiceButtons[i] = buttonGO.GetComponent<Button>();
            choiceButtonTexts[i] = BuildText(buttonGO.transform, "Text", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-32f, 0f), 24, FontStyle.Normal, TextAnchor.MiddleLeft);
            choiceButtonTexts[i].color = Color.white;

            if (buttonCreated)
            {
                buttonGO.SetActive(false);
            }
        }

        return panel;
    }

    // 같은 이름의 자식이 여러 개 있으면 첫 번째만 남기고 나머지는 삭제한다.
    private static void RemoveDuplicateChildren(Transform parent, string name)
    {
        bool keptOne = false;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name != name)
            {
                continue;
            }

            if (!keptOne)
            {
                keptOne = true;
                continue;
            }

            Object.DestroyImmediate(child.gameObject);
        }
    }

    private static Text BuildText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, int fontSize, FontStyle style, TextAnchor alignment)
    {
        GameObject existing = parent.Find(name)?.gameObject;
        GameObject go = existing != null ? existing : new GameObject(name, typeof(Text));
        if (existing == null)
        {
            go.transform.SetParent(parent, false);
        }

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = string.Empty;

        return text;
    }

    private static void BuildDialogueSystem(GameObject panel, Text speakerText, Text bodyText, Button[] choiceButtons, Text[] choiceButtonTexts)
    {
        GameObject systemGO = GameObject.Find("DialogueSystem");
        bool created = systemGO == null;
        if (created)
        {
            systemGO = new GameObject("DialogueSystem", typeof(DialogueBoxUI), typeof(DialogueManager), typeof(TestDialogueBootstrap));
        }

        DialogueBoxUI boxUI = systemGO.GetComponent<DialogueBoxUI>();
        SerializedObject boxSO = new SerializedObject(boxUI);
        boxSO.FindProperty("panel").objectReferenceValue = panel;
        boxSO.FindProperty("speakerText").objectReferenceValue = speakerText;
        boxSO.FindProperty("bodyText").objectReferenceValue = bodyText;

        SerializedProperty buttonsProp = boxSO.FindProperty("choiceButtons");
        buttonsProp.arraySize = choiceButtons.Length;
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceButtons[i];
        }

        SerializedProperty buttonTextsProp = boxSO.FindProperty("choiceButtonTexts");
        buttonTextsProp.arraySize = choiceButtonTexts.Length;
        for (int i = 0; i < choiceButtonTexts.Length; i++)
        {
            buttonTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceButtonTexts[i];
        }
        boxSO.ApplyModifiedProperties();

        DialogueManager manager = systemGO.GetComponent<DialogueManager>();
        SerializedObject managerSO = new SerializedObject(manager);
        managerSO.FindProperty("dialogueBoxUI").objectReferenceValue = boxUI;
        managerSO.ApplyModifiedProperties();
    }
}
