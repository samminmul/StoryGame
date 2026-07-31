using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CaptainsRoomSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/선장실.unity";
    private const string DeckSceneName = "갑판";

    [MenuItem("Tools/Story Game/Build Captain's Room Scene (Placeholder)")]
    public static void Build()
    {
        // 이미 열려있는 씬이면 다시 로드하지 않는다 - 저장 안 한 변경사항이 날아가는 것을 방지.
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = System.IO.File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        if (Object.FindAnyObjectByType<Camera>() == null)
        {
            GameObject cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGO.tag = "MainCamera";
            Camera camera = cameraGO.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.10f, 0.08f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        // 예전에 만들어 두었던 "준비 중" 플레이스홀더 라벨은 이제 실제 배경/오브젝트가 들어와서 필요 없다.
        GameObject oldLabel = GameObject.Find("PlaceholderLabel");
        if (oldLabel != null)
        {
            Object.DestroyImmediate(oldLabel);
        }

        BuildEventSystem();
        BuildDeckExitButton();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        var buildScenes = EditorBuildSettings.scenes.ToList();
        if (!buildScenes.Any(s => s.path == ScenePath))
        {
            buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        Debug.Log("선장실 placeholder scene built at " + ScenePath);
    }

    private static void BuildEventSystem()
    {
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        GameObject eventSystemGO = eventSystem != null ? eventSystem.gameObject : new GameObject("EventSystem", typeof(EventSystem));

        var legacyInputModule = eventSystemGO.GetComponent<StandaloneInputModule>();
        if (legacyInputModule != null)
        {
            Object.DestroyImmediate(legacyInputModule);
        }
        if (eventSystemGO.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static void BuildDeckExitButton()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("HudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        Transform existingButton = canvasGO.transform.Find("DeckExitButton");
        bool buttonIsNew = existingButton == null;
        GameObject buttonGO = buttonIsNew
            ? new GameObject("DeckExitButton", typeof(Image), typeof(Button), typeof(SceneExitButton))
            : existingButton.gameObject;
        if (buttonIsNew)
        {
            buttonGO.transform.SetParent(canvasGO.transform, false);
        }

        var rt = buttonGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-24f, 0f);
        rt.sizeDelta = new Vector2(64f, 64f);
        buttonGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.85f);

        Transform existingText = buttonGO.transform.Find("Text");
        GameObject textGO = existingText != null ? existingText.gameObject : new GameObject("Text", typeof(Text));
        if (existingText == null)
        {
            textGO.transform.SetParent(buttonGO.transform, false);
        }
        var text = textGO.GetComponent<Text>();
        text.text = "▶";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = 36;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var exitButton = buttonGO.GetComponent<SceneExitButton>();
        SerializedObject exitSO = new SerializedObject(exitButton);
        exitSO.FindProperty("targetScene").stringValue = DeckSceneName;
        exitSO.ApplyModifiedProperties();

        Button button = buttonGO.GetComponent<Button>();
        if (buttonIsNew)
        {
            UnityEventTools.AddPersistentListener(button.onClick, exitButton.OnClickExit);
        }
    }
}
