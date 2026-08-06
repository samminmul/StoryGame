using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CorridorSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    private const float UpperFloorY = 2.5f;
    private const float LowerFloorY = -2.5f;
    private const float CorridorMinX = -8.5f;
    private const float CorridorMaxX = 8.5f;
    private const float PlayerTargetHeight = 1.6f;

    private const string PlayerSheetPath = "Assets/sprites/움직임시트.PNG";
    private const string BackgroundImagePath = "Assets/sprites/임시 배 배경.jpg";
    private const string MapImagePath = "Assets/sprites/임시 지도 이미지.png";
    private const float SpritePixelsPerUnit = 100f;

    // 움직임시트.PNG는 이미 방향별 걷기 프레임으로 슬라이스되어 있음(움직임시트_0 ~ _50).
    private const string IdleFrameName = "움직임시트_49";
    private static readonly string[] LeftFrameNames = { "움직임시트_16", "움직임시트_17", "움직임시트_18", "움직임시트_19" };
    private static readonly string[] RightFrameNames = { "움직임시트_36", "움직임시트_37", "움직임시트_38", "움직임시트_39" };

    private static Sprite boxSprite;
    private static Sprite playerSprite;
    private static Sprite[] playerLeftSprites;
    private static Sprite[] playerRightSprites;
    private static Sprite backgroundSprite;
    private static Sprite mapSprite;

    [MenuItem("Tools/Story Game/Build Corridor Scene")]
    public static void Build()
    {
        boxSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite[] allPlayerSprites = AssetDatabase.LoadAllAssetsAtPath(PlayerSheetPath).OfType<Sprite>().ToArray();
        playerSprite = allPlayerSprites.FirstOrDefault(s => s.name == IdleFrameName);
        playerLeftSprites = ResolveNamedFrames(allPlayerSprites, LeftFrameNames);
        playerRightSprites = ResolveNamedFrames(allPlayerSprites, RightFrameNames);
        backgroundSprite = EnsureSingleSprite(BackgroundImagePath);
        mapSprite = EnsureSingleSprite(MapImagePath);

        // 이미 열려있는 씬이면 다시 로드하지 않는다 - OpenScene은 디스크에 저장된 마지막 버전으로 되돌리기 때문에,
        // 저장하지 않은 상태에서 재실행하면 인스펙터에서 손으로 고친 내용이 전부 사라진다.
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = System.IO.File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        Camera camera = BuildCamera();
        BuildEventSystem();

        Transform corridorRoot = FindOrCreate("CorridorRoot").transform;
        BuildBackground(corridorRoot);
        BuildFloorDivider(corridorRoot);
        // 문/사다리는 더 이상 여기서 자동 생성하지 않는다 - 실제 문/사다리 배치는 씬에서 손으로 관리한다.
        // (예전 플레이스홀더 문/사다리를 자동으로 되살리는 문제가 있었음.)

        Transform player = BuildPlayer(corridorRoot);

        CameraFollow2D follow = camera.GetComponent<CameraFollow2D>();
        SerializedObject followSO = new SerializedObject(follow);
        followSO.FindProperty("target").objectReferenceValue = player;
        followSO.FindProperty("minX").floatValue = CorridorMinX;
        followSO.FindProperty("maxX").floatValue = CorridorMaxX;
        followSO.ApplyModifiedProperties();

        BuildHud(camera);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        var buildScenes = EditorBuildSettings.scenes.ToList();
        if (!buildScenes.Any(s => s.path == ScenePath))
        {
            buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        Debug.Log("Corridor scene built at " + ScenePath);
    }

    private static Camera BuildCamera()
    {
        Camera camera = Object.FindAnyObjectByType<Camera>();
        GameObject cameraGO;
        if (camera == null)
        {
            cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGO.tag = "MainCamera";
            camera = cameraGO.GetComponent<Camera>();
        }
        else
        {
            cameraGO = camera.gameObject;
        }

        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.10f, 0.12f, 0.16f, 1f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        if (cameraGO.GetComponent<CameraFollow2D>() == null)
        {
            cameraGO.AddComponent<CameraFollow2D>();
        }

        return camera;
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

    private static void BuildBackground(Transform parent)
    {
        GameObject bg = FindOrCreateChild(parent, "Background", out bool created, typeof(SpriteRenderer));

        var sr = bg.GetComponent<SpriteRenderer>();
        sr.sortingOrder = -10;
        // 처음 생성될 때만 기본 이미지/크기/배치를 적용한다 - 이미 있는 오브젝트는 인스펙터에서 바꾼
        // 이미지와 크기(수동으로 조절한 값 포함)를 그대로 유지.
        if (created)
        {
            sr.sprite = backgroundSprite != null ? backgroundSprite : boxSprite;
            sr.color = backgroundSprite != null ? Color.white : new Color(0.10f, 0.12f, 0.16f, 1f);
            // drawMode+size로 크기를 고정해서, 배경 이미지의 실제 픽셀 크기/스케일과 무관하게 항상 20x12 월드 유닛으로 보이게 한다.
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(20f, 12f);
            bg.transform.position = new Vector3(0f, 0f, 0f);
            bg.transform.localScale = Vector3.one;
        }
    }

    private static void BuildFloorDivider(Transform parent)
    {
        GameObject divider = FindOrCreateChild(parent, "FloorDivider", typeof(SpriteRenderer));

        var sr = divider.GetComponent<SpriteRenderer>();
        sr.sprite = boxSprite;
        sr.color = new Color(0.35f, 0.38f, 0.42f, 1f);
        sr.sortingOrder = -5;
        divider.transform.position = new Vector3(0f, (UpperFloorY + LowerFloorY) * 0.5f, 0f);
        divider.transform.localScale = new Vector3(18f, 0.08f, 1f);
    }

    private static Transform BuildPlayer(Transform parent)
    {
        GameObject player = FindOrCreateChild(parent, "Player", out bool created, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(CorridorPlayerController));
        player.tag = "Player";

        var sr = player.GetComponent<SpriteRenderer>();
        sr.sprite = playerSprite;
        sr.color = Color.white;
        sr.sortingOrder = 1;

        var rb = player.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        if (created)
        {
            Vector2 nativeSize = SpriteNativeSize(playerSprite);
            float scale = PlayerTargetHeight / nativeSize.y;

            // UpperFloorY는 발이 닿는 바닥선이므로, 스프라이트 중심이 아니라 발 위치가 거기에 오도록 절반 높이만큼 올려서 배치한다.
            player.transform.position = new Vector3(0f, UpperFloorY + PlayerTargetHeight * 0.5f, 0f);
            player.transform.localScale = new Vector3(scale, scale, 1f);

            var col = player.GetComponent<BoxCollider2D>();
            col.size = nativeSize;
        }

        var controller = player.GetComponent<CorridorPlayerController>();
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("minX").floatValue = CorridorMinX;
        so.FindProperty("maxX").floatValue = CorridorMaxX;
        so.FindProperty("idleSprite").objectReferenceValue = playerSprite;
        AssignFrames(so, "leftFrames", playerLeftSprites);
        AssignFrames(so, "rightFrames", playerRightSprites);
        so.ApplyModifiedProperties();

        return player.transform;
    }

    private static void BuildHud(Camera camera)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("HudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        // Screen Space - Camera로 카메라에 연결해서, Scene 뷰에서도 캔버스가 카메라 프러스텀 크기에 맞춰 보이도록 함
        // (Overlay 모드는 Scene 뷰에서 CanvasScaler 기준 해상도만큼 거대한 평면으로 미리보기가 그려져 월드 오브젝트와 스케일이 안 맞아 보임)
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        // Screen Space - Camera 모드에서는 캔버스도 월드의 SpriteRenderer들과 같은 정렬 순서 경쟁에 들어간다.
        // sortingOrder를 충분히 높여서 플레이어/문 등 어떤 sortingOrder보다도 항상 위에 그려지도록 한다.
        canvas.sortingOrder = 100;

        GameObject hudControllerGO = FindOrCreate("CorridorHUDController");
        if (hudControllerGO.GetComponent<CorridorHUDController>() == null)
        {
            hudControllerGO.AddComponent<CorridorHUDController>();
        }
        var hud = hudControllerGO.GetComponent<CorridorHUDController>();

        GameObject topBar = FindOrCreateChild(canvasGO.transform, "TopBar", typeof(Image));
        var topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0f, 1f);
        topBarRT.anchorMax = new Vector2(1f, 1f);
        topBarRT.pivot = new Vector2(0.5f, 1f);
        topBarRT.anchoredPosition = Vector2.zero;
        topBarRT.sizeDelta = new Vector2(0f, 90f);
        topBar.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.9f);

        GameObject sunIcon = FindOrCreateChild(topBar.transform, "SunIcon", typeof(Image));
        var sunRT = sunIcon.GetComponent<RectTransform>();
        sunRT.anchorMin = new Vector2(0f, 0.5f);
        sunRT.anchorMax = new Vector2(0f, 0.5f);
        sunRT.pivot = new Vector2(0f, 0.5f);
        sunRT.anchoredPosition = new Vector2(24f, 0f);
        sunRT.sizeDelta = new Vector2(36f, 36f);
        sunIcon.GetComponent<Image>().color = new Color(0.95f, 0.8f, 0.25f, 1f);

        Text dayText = CreateHudText(topBar.transform, "DayText", "day 17", 30, TextAnchor.MiddleLeft);
        var dayRT = dayText.GetComponent<RectTransform>();
        dayRT.anchorMin = new Vector2(0f, 0.5f);
        dayRT.anchorMax = new Vector2(0f, 0.5f);
        dayRT.pivot = new Vector2(0f, 0.5f);
        dayRT.anchoredPosition = new Vector2(72f, 0f);
        dayRT.sizeDelta = new Vector2(140f, 60f);

        Text titleText = CreateHudText(topBar.transform, "TitleText", "선실 복도", 30, TextAnchor.MiddleCenter);
        var titleRT = titleText.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = Vector2.zero;
        titleRT.sizeDelta = new Vector2(400f, 60f);

        Button menuBtn = CreateHudButton(topBar.transform, "MenuButton", "≡", 44, out bool menuBtnCreated);
        var menuRT = menuBtn.GetComponent<RectTransform>();
        menuRT.anchorMin = new Vector2(1f, 0.5f);
        menuRT.anchorMax = new Vector2(1f, 0.5f);
        menuRT.pivot = new Vector2(1f, 0.5f);
        menuRT.anchoredPosition = new Vector2(-24f, 0f);
        menuRT.sizeDelta = new Vector2(64f, 64f);
        // 처음 생성될 때만 리스너를 붙인다 - 이미 있는 버튼에 매번 붙이면 클릭할 때마다 중복 호출된다.
        if (menuBtnCreated)
        {
            UnityEventTools.AddPersistentListener(menuBtn.onClick, hud.OnClickMenu);
        }

        GameObject bottomBar = FindOrCreateChild(canvasGO.transform, "BottomBar", typeof(Image));
        var bottomBarRT = bottomBar.GetComponent<RectTransform>();
        bottomBarRT.anchorMin = new Vector2(0f, 0f);
        bottomBarRT.anchorMax = new Vector2(1f, 0f);
        bottomBarRT.pivot = new Vector2(0.5f, 0f);
        bottomBarRT.anchoredPosition = Vector2.zero;
        bottomBarRT.sizeDelta = new Vector2(0f, 80f);
        bottomBar.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.9f);

        Button bagBtn = CreateHudButton(bottomBar.transform, "BagButton", "가방", 26, out bool bagBtnCreated);
        var bagRT = bagBtn.GetComponent<RectTransform>();
        bagRT.anchorMin = new Vector2(0f, 0.5f);
        bagRT.anchorMax = new Vector2(0f, 0.5f);
        bagRT.pivot = new Vector2(0f, 0.5f);
        bagRT.anchoredPosition = new Vector2(24f, 0f);
        bagRT.sizeDelta = new Vector2(140f, 56f);
        if (bagBtnCreated)
        {
            UnityEventTools.AddPersistentListener(bagBtn.onClick, hud.OnClickBag);
        }

        Button mapBtn = CreateHudButton(bottomBar.transform, "MapButton", "지도", 26, out bool mapBtnCreated);
        var mapRT = mapBtn.GetComponent<RectTransform>();
        mapRT.anchorMin = new Vector2(1f, 0.5f);
        mapRT.anchorMax = new Vector2(1f, 0.5f);
        mapRT.pivot = new Vector2(1f, 0.5f);
        mapRT.anchoredPosition = new Vector2(-24f, 0f);
        mapRT.sizeDelta = new Vector2(140f, 56f);
        if (mapBtnCreated)
        {
            UnityEventTools.AddPersistentListener(mapBtn.onClick, hud.OnClickMap);
        }

        GameObject menuOverlay = BuildMenuOverlay(canvasGO.transform, hud);
        GameObject mapOverlay = BuildMapOverlay(canvasGO.transform, hud);
        GameObject bagOverlay = BuildBagOverlay(canvasGO.transform);

        // 이미 연결되어 있는 필드는 덮어쓰지 않는다 - 손으로 다른 오버레이(예: 커스텀 메뉴창)로
        // 바꿔놓은 걸 재실행할 때마다 원래 자동 생성 오버레이로 되돌리면 안 된다.
        SerializedObject hudSO = new SerializedObject(hud);
        SetIfUnassigned(hudSO, "dayText", dayText);
        SetIfUnassigned(hudSO, "menuOverlay", menuOverlay);
        SetIfUnassigned(hudSO, "mapOverlay", mapOverlay);
        SetIfUnassigned(hudSO, "bagOverlay", bagOverlay);
        hudSO.ApplyModifiedProperties();
    }

    private static void SetIfUnassigned(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop.objectReferenceValue == null)
        {
            prop.objectReferenceValue = value;
        }
    }

    private static GameObject BuildMenuOverlay(Transform canvasParent, CorridorHUDController hud)
    {
        // 반투명 검정으로 배경을 어둡게 눌러주는 방식. 실제 가우시안 블러는 URP 포스트 프로세싱이 필요해서 별도 작업이 필요함.
        GameObject overlay = FindOrCreateChild(canvasParent, "MenuOverlay", typeof(Image));
        var overlayRT = overlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        GameObject panel = FindOrCreateChild(overlay.transform, "MenuPanel", typeof(Image));
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(480f, 420f);
        panel.GetComponent<Image>().color = new Color(0.97f, 0.97f, 0.97f, 1f);

        Text title = CreateHudText(panel.transform, "MenuTitle", "메뉴", 32, TextAnchor.MiddleLeft);
        title.color = Color.black;
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(0f, 1f);
        titleRT.pivot = new Vector2(0f, 1f);
        titleRT.anchoredPosition = new Vector2(24f, -20f);
        titleRT.sizeDelta = new Vector2(200f, 50f);

        Button closeBtn = CreateMenuButton(panel.transform, "CloseButton", "X", 22, out bool closeBtnCreated);
        var closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot = new Vector2(1f, 1f);
        closeRT.anchoredPosition = new Vector2(-16f, -16f);
        closeRT.sizeDelta = new Vector2(44f, 44f);
        if (closeBtnCreated)
        {
            UnityEventTools.AddPersistentListener(closeBtn.onClick, hud.OnClickCloseMenu);
        }

        GameObject divider = FindOrCreateChild(panel.transform, "Divider", typeof(Image));
        var dividerRT = divider.GetComponent<RectTransform>();
        dividerRT.anchorMin = new Vector2(0f, 1f);
        dividerRT.anchorMax = new Vector2(1f, 1f);
        dividerRT.pivot = new Vector2(0.5f, 1f);
        dividerRT.offsetMin = new Vector2(24f, -76f);
        dividerRT.offsetMax = new Vector2(-24f, -74f);
        divider.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.15f);

        GameObject buttonList = FindOrCreateChild(panel.transform, "MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        var listRT = buttonList.GetComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0.5f, 0.5f);
        listRT.anchorMax = new Vector2(0.5f, 0.5f);
        listRT.pivot = new Vector2(0.5f, 0.5f);
        listRT.anchoredPosition = new Vector2(0f, -20f);
        listRT.sizeDelta = new Vector2(340f, 220f);

        var layout = buttonList.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button settingsBtn = CreateMenuButton(buttonList.transform, "SettingsButton", "설정", 26, out bool settingsBtnCreated);
        settingsBtn.GetComponent<LayoutElement>().preferredHeight = 56f;
        if (settingsBtnCreated)
        {
            UnityEventTools.AddPersistentListener(settingsBtn.onClick, hud.OnClickMenuSettings);
        }

        Button saveBtn = CreateMenuButton(buttonList.transform, "SaveButton", "저장", 26, out bool saveBtnCreated);
        saveBtn.GetComponent<LayoutElement>().preferredHeight = 56f;
        if (saveBtnCreated)
        {
            UnityEventTools.AddPersistentListener(saveBtn.onClick, hud.OnClickMenuSave);
        }

        Button exitBtn = CreateMenuButton(buttonList.transform, "ExitButton", "나가기", 26, out bool exitBtnCreated);
        exitBtn.GetComponent<LayoutElement>().preferredHeight = 56f;
        if (exitBtnCreated)
        {
            UnityEventTools.AddPersistentListener(exitBtn.onClick, hud.OnClickMenuExit);
        }

        overlay.SetActive(false);
        return overlay;
    }

    private static GameObject BuildMapOverlay(Transform canvasParent, CorridorHUDController hud)
    {
        GameObject overlay = FindOrCreateChild(canvasParent, "MapOverlay", typeof(Image));
        var overlayRT = overlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        GameObject panel = FindOrCreateChild(overlay.transform, "MapPanel", typeof(Image));
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(720f, 540f);
        panel.GetComponent<Image>().color = new Color(0.97f, 0.97f, 0.97f, 1f);

        GameObject mapImageGO = FindOrCreateChild(panel.transform, "MapImage", typeof(Image));
        var mapImageRT = mapImageGO.GetComponent<RectTransform>();
        mapImageRT.anchorMin = Vector2.zero;
        mapImageRT.anchorMax = Vector2.one;
        mapImageRT.offsetMin = new Vector2(16f, 16f);
        mapImageRT.offsetMax = new Vector2(-16f, -16f);
        var mapImage = mapImageGO.GetComponent<Image>();
        mapImage.sprite = mapSprite;
        mapImage.preserveAspect = true;

        Button closeBtn = CreateMenuButton(panel.transform, "CloseButton", "X", 22, out bool closeBtnCreated);
        var closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot = new Vector2(1f, 1f);
        closeRT.anchoredPosition = new Vector2(-16f, -16f);
        closeRT.sizeDelta = new Vector2(44f, 44f);
        if (closeBtnCreated)
        {
            UnityEventTools.AddPersistentListener(closeBtn.onClick, hud.OnClickCloseMap);
        }

        overlay.SetActive(false);
        return overlay;
    }

    private const int BagSlotCount = 8;

    private static GameObject BuildBagOverlay(Transform canvasParent)
    {
        // 배경을 어둡게 누르지 않는다 - 가방은 상황판처럼 배경이 그대로 보여야 함.
        GameObject overlay = FindOrCreateChild(canvasParent, "BagOverlay", typeof(Image));
        var overlayRT = overlay.GetComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        GameObject panel = FindOrCreateChild(overlay.transform, "BagPanel", typeof(Image));
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0f);
        panelRT.anchorMax = new Vector2(0.5f, 0f);
        panelRT.pivot = new Vector2(0.5f, 0f);
        panelRT.anchoredPosition = new Vector2(0f, 100f);
        panelRT.sizeDelta = new Vector2(820f, 240f);
        panel.GetComponent<Image>().color = new Color(0.97f, 0.97f, 0.97f, 0.95f);

        Button prevBtn = CreateMenuButton(panel.transform, "PrevButton", "◁", 22);
        var prevRT = prevBtn.GetComponent<RectTransform>();
        prevRT.anchorMin = new Vector2(0f, 0f);
        prevRT.anchorMax = new Vector2(0f, 0f);
        prevRT.pivot = new Vector2(0f, 0f);
        prevRT.anchoredPosition = new Vector2(16f, 16f);
        prevRT.sizeDelta = new Vector2(40f, 40f);

        Button nextBtn = CreateMenuButton(panel.transform, "NextButton", "▷", 22);
        var nextRT = nextBtn.GetComponent<RectTransform>();
        nextRT.anchorMin = new Vector2(0f, 0f);
        nextRT.anchorMax = new Vector2(0f, 0f);
        nextRT.pivot = new Vector2(0f, 0f);
        nextRT.anchoredPosition = new Vector2(64f, 16f);
        nextRT.sizeDelta = new Vector2(40f, 40f);

        GameObject slotRow = FindOrCreateChild(panel.transform, "SlotRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var slotRowRT = slotRow.GetComponent<RectTransform>();
        slotRowRT.anchorMin = new Vector2(0.5f, 1f);
        slotRowRT.anchorMax = new Vector2(0.5f, 1f);
        slotRowRT.pivot = new Vector2(0.5f, 1f);
        slotRowRT.anchoredPosition = new Vector2(0f, -24f);
        slotRowRT.sizeDelta = new Vector2(660f, 96f);

        var slotLayout = slotRow.GetComponent<HorizontalLayoutGroup>();
        slotLayout.spacing = 12f;
        slotLayout.childAlignment = TextAnchor.MiddleCenter;
        slotLayout.childControlWidth = true;
        slotLayout.childControlHeight = true;
        slotLayout.childForceExpandWidth = false;
        slotLayout.childForceExpandHeight = false;

        for (int i = 0; i < BagSlotCount; i++)
        {
            GameObject slot = FindOrCreateChild(slotRow.transform, $"Slot{i}", typeof(Image), typeof(LayoutElement));
            slot.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 1f);
            var slotLayoutElement = slot.GetComponent<LayoutElement>();
            slotLayoutElement.preferredWidth = 72f;
            slotLayoutElement.preferredHeight = 72f;
        }

        overlay.SetActive(false);
        return overlay;
    }

    private static Button CreateMenuButton(Transform parent, string name, string label, int fontSize)
    {
        return CreateMenuButton(parent, name, label, fontSize, out _);
    }

    private static Button CreateMenuButton(Transform parent, string name, string label, int fontSize, out bool created)
    {
        GameObject go = FindOrCreateChild(parent, name, out created, typeof(Image), typeof(Button), typeof(LayoutElement));
        go.GetComponent<Image>().color = Color.white;

        GameObject textGO = FindOrCreateChild(go.transform, "Text", typeof(Text));
        var text = textGO.GetComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.fontSize = fontSize;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        Button button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.75f, 0.85f, 1f, 1f); // 마우스를 올리면 옅은 파란색으로 강조
        colors.pressedColor = new Color(0.6f, 0.75f, 1f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        return button;
    }

    private static Text CreateHudText(Transform parent, string name, string label, int fontSize, TextAnchor anchor)
    {
        GameObject go = FindOrCreateChild(parent, name, typeof(Text));
        var text = go.GetComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = anchor;
        text.color = Color.white;
        text.fontSize = fontSize;
        return text;
    }

    private static Button CreateHudButton(Transform parent, string name, string label, int fontSize)
    {
        return CreateHudButton(parent, name, label, fontSize, out _);
    }

    private static Button CreateHudButton(Transform parent, string name, string label, int fontSize, out bool created)
    {
        GameObject go = FindOrCreateChild(parent, name, out created, typeof(Image), typeof(Button));
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

        GameObject textGO = FindOrCreateChild(go.transform, "Text", typeof(Text));
        var text = textGO.GetComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = fontSize;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return go.GetComponent<Button>();
    }

    private static Vector2 SpriteNativeSize(Sprite sprite)
    {
        return sprite.rect.size / sprite.pixelsPerUnit;
    }

    private static void AssignFrames(SerializedObject so, string propertyName, Sprite[] frames)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        prop.arraySize = frames?.Length ?? 0;
        for (int i = 0; i < prop.arraySize; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }
    }

    private static Sprite[] ResolveNamedFrames(Sprite[] allSprites, string[] names)
    {
        var result = new Sprite[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            result[i] = allSprites.FirstOrDefault(s => s.name == names[i]);
            if (result[i] == null)
            {
                Debug.LogWarning($"[CorridorSceneBuilder] {names[i]} 프레임을 찾지 못함");
            }
        }
        return result;
    }

    private static Sprite EnsureSlicedSprite(string assetPath, string spriteName, Rect pixelRect, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"텍스처를 찾을 수 없음: {assetPath}");
            return null;
        }

        bool alreadySliced = importer.textureType == TextureImporterType.Sprite
            && importer.spriteImportMode == SpriteImportMode.Multiple
            && Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit)
            && importer.spritesheet != null
            && importer.spritesheet.Any(s => s.name == spriteName && s.rect == pixelRect);

        if (!alreadySliced)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;

            var otherFrames = (importer.spritesheet ?? new SpriteMetaData[0]).Where(s => s.name != spriteName);
            var meta = new SpriteMetaData
            {
                name = spriteName,
                rect = pixelRect,
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
            importer.spritesheet = otherFrames.Append(meta).ToArray();
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault(s => s.name == spriteName);
    }

    private static Sprite EnsureSingleSprite(string assetPath)
    {
        if (!System.IO.File.Exists(assetPath))
        {
            Debug.LogWarning($"배경 이미지가 없음: {assetPath}");
            return null;
        }

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"텍스처를 찾을 수 없음: {assetPath}");
            return null;
        }

        if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static GameObject FindOrCreate(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go : new GameObject(name);
    }

    private static GameObject FindOrCreateChild(Transform parent, string name, params System.Type[] components)
    {
        return FindOrCreateChild(parent, name, out _, components);
    }

    private static GameObject FindOrCreateChild(Transform parent, string name, out bool created, params System.Type[] components)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            created = false;
            return existing.gameObject;
        }

        created = true;
        GameObject go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        return go;
    }
}
