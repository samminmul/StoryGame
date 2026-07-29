using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Tools/Story Game/Build Main Menu Scene")]
    public static void Build()
    {
        Scene scene = System.IO.File.Exists(ScenePath)
            ? EditorSceneManager.OpenScene(ScenePath)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        if (Object.FindAnyObjectByType<Camera>() == null)
        {
            GameObject cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGO.tag = "MainCamera";
            Camera camera = cameraGO.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.15f, 0.18f, 0.22f, 1f);
            camera.orthographic = true;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        if (canvasGO.transform.Find("Background_Placeholder") == null)
        {
            GameObject bg = new GameObject("Background_Placeholder", typeof(Image));
            bg.transform.SetParent(canvasGO.transform, false);
            bg.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.22f, 1f);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bg.transform.SetAsFirstSibling();
        }

        MainMenuController controller = Object.FindAnyObjectByType<MainMenuController>();
        if (controller == null)
        {
            GameObject controllerGO = new GameObject("MainMenuController", typeof(MainMenuController));
            controller = controllerGO.GetComponent<MainMenuController>();
        }

        Transform existingPanel = canvasGO.transform.Find("ButtonPanel");
        GameObject panel;
        bool panelIsNew = existingPanel == null;
        if (panelIsNew)
        {
            panel = new GameObject("ButtonPanel", typeof(RectTransform), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(canvasGO.transform, false);
        }
        else
        {
            panel = existingPanel.gameObject;
        }

        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = new Vector2(0, -120);
        panelRT.sizeDelta = new Vector2(280, 220);

        var layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        if (panelIsNew)
        {
            Button startBtn = CreateButton(panel.transform, "StartButton", "시작");
            Button settingsBtn = CreateButton(panel.transform, "SettingsButton", "설정");
            Button exitBtn = CreateButton(panel.transform, "ExitButton", "나가기");

            UnityEventTools.AddPersistentListener(startBtn.onClick, controller.OnClickStart);
            UnityEventTools.AddPersistentListener(settingsBtn.onClick, controller.OnClickSettings);
            UnityEventTools.AddPersistentListener(exitBtn.onClick, controller.OnClickExit);
        }

        Transform existingSettingsPanel = canvasGO.transform.Find("SettingsPanel");
        if (existingSettingsPanel != null)
        {
            Object.DestroyImmediate(existingSettingsPanel.gameObject);
        }

        GameObject settingsPanelGO;
        {
            settingsPanelGO = new GameObject("SettingsPanel", typeof(Image));
            settingsPanelGO.transform.SetParent(canvasGO.transform, false);
            settingsPanelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            var dimRT = settingsPanelGO.GetComponent<RectTransform>();
            dimRT.anchorMin = Vector2.zero;
            dimRT.anchorMax = Vector2.one;
            dimRT.offsetMin = Vector2.zero;
            dimRT.offsetMax = Vector2.zero;

            GameObject box = new GameObject("Box", typeof(Image));
            box.transform.SetParent(settingsPanelGO.transform, false);
            box.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 1f);
            var boxRT = box.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 0.5f);
            boxRT.anchorMax = new Vector2(0.5f, 0.5f);
            boxRT.pivot = new Vector2(0.5f, 0.5f);
            boxRT.sizeDelta = new Vector2(500, 360);

            GameObject titleGO = new GameObject("Title", typeof(Text));
            titleGO.transform.SetParent(box.transform, false);
            var title = titleGO.GetComponent<Text>();
            title.text = "설정";
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.black;
            title.fontSize = 32;
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0, -20);
            titleRT.sizeDelta = new Vector2(0, 60);

            GameObject volumeLabelGO = new GameObject("VolumeLabel", typeof(Text));
            volumeLabelGO.transform.SetParent(box.transform, false);
            var volumeLabel = volumeLabelGO.GetComponent<Text>();
            volumeLabel.text = "음량";
            volumeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            volumeLabel.alignment = TextAnchor.MiddleCenter;
            volumeLabel.color = Color.black;
            volumeLabel.fontSize = 22;
            var volumeLabelRT = volumeLabelGO.GetComponent<RectTransform>();
            volumeLabelRT.anchorMin = new Vector2(0.5f, 0.5f);
            volumeLabelRT.anchorMax = new Vector2(0.5f, 0.5f);
            volumeLabelRT.pivot = new Vector2(0.5f, 0.5f);
            volumeLabelRT.anchoredPosition = new Vector2(0, 40);
            volumeLabelRT.sizeDelta = new Vector2(200, 40);

            GameObject sliderGO = CreateSlider(box.transform, "VolumeSlider");
            var sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRT.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRT.pivot = new Vector2(0.5f, 0.5f);
            sliderRT.anchoredPosition = new Vector2(0, -10);
            sliderRT.sizeDelta = new Vector2(360, 20);
            sliderGO.GetComponent<Slider>().value = 1f;

            Button closeBtn = CreateButton(box.transform, "CloseButton", "닫기");
            var closeRT = closeBtn.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.5f, 0f);
            closeRT.anchorMax = new Vector2(0.5f, 0f);
            closeRT.pivot = new Vector2(0.5f, 0f);
            closeRT.anchoredPosition = new Vector2(0, 20);
            closeRT.sizeDelta = new Vector2(160, 56);
            UnityEventTools.AddPersistentListener(closeBtn.onClick, controller.OnClickSettings);

            settingsPanelGO.SetActive(false);
        }

        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("settingsPanel").objectReferenceValue = settingsPanelGO;
        controllerSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        var buildScenes = EditorBuildSettings.scenes.ToList();
        if (!buildScenes.Any(s => s.path == ScenePath))
        {
            buildScenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        Selection.activeGameObject = canvasGO;
        Debug.Log("Main menu scene built at " + ScenePath);
    }

    private static Button CreateButton(Transform parent, string name, string label)
    {
        GameObject go = new GameObject(name, typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
        go.GetComponent<LayoutElement>().preferredHeight = 56;

        GameObject textGO = new GameObject("Text", typeof(Text));
        textGO.transform.SetParent(go.transform, false);
        var text = textGO.GetComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = 24;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return go.GetComponent<Button>();
    }

    private static GameObject CreateSlider(Transform parent, string name)
    {
        var resources = new DefaultControls.Resources
        {
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")
        };

        GameObject sliderGO = DefaultControls.CreateSlider(resources);
        sliderGO.name = name;
        sliderGO.transform.SetParent(parent, false);
        return sliderGO;
    }
}
