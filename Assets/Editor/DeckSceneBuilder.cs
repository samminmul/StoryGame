using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DeckSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/갑판.unity";
    private const string BackgroundImagePath = "Assets/sprites/갑판.png";
    private const string PlayerSheetPath = "Assets/sprites/움직임시트.PNG";
    private const float PlayerTargetHeight = 1.6f;

    // 움직임시트.PNG는 이미 방향별 걷기 프레임으로 슬라이스되어 있음(움직임시트_0 ~ _50).
    private static readonly string[] DownFrameNames = { "움직임시트_49", "움직임시트_46", "움직임시트_45", "움직임시트_44" };
    private static readonly string[] UpFrameNames = { "움직임시트_41", "움직임시트_42", "움직임시트_43", "움직임시트_40" };
    private static readonly string[] LeftFrameNames = { "움직임시트_16", "움직임시트_17", "움직임시트_18", "움직임시트_19" };
    private static readonly string[] RightFrameNames = { "움직임시트_36", "움직임시트_37", "움직임시트_38", "움직임시트_39" };

    private static Sprite backgroundSprite;
    private static Sprite[] downFrames;
    private static Sprite[] upFrames;
    private static Sprite[] leftFrames;
    private static Sprite[] rightFrames;

    [MenuItem("Tools/Story Game/Build Deck Scene (Placeholder)")]
    public static void Build()
    {
        backgroundSprite = EnsureSingleSprite(BackgroundImagePath);
        Sprite[] allPlayerSprites = AssetDatabase.LoadAllAssetsAtPath(PlayerSheetPath).OfType<Sprite>().ToArray();
        downFrames = ResolveNamedFrames(allPlayerSprites, DownFrameNames);
        upFrames = ResolveNamedFrames(allPlayerSprites, UpFrameNames);
        leftFrames = ResolveNamedFrames(allPlayerSprites, LeftFrameNames);
        rightFrames = ResolveNamedFrames(allPlayerSprites, RightFrameNames);

        // 이미 열려있는 씬이면 다시 로드하지 않는다 - 저장 안 한 변경사항이 날아가는 것을 방지.
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = System.IO.File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        Camera camera = BuildCamera();
        BuildBackground();
        Transform player = BuildPlayer();

        CameraFollowTarget follow = camera.GetComponent<CameraFollowTarget>();
        SerializedObject followSO = new SerializedObject(follow);
        followSO.FindProperty("target").objectReferenceValue = player;
        followSO.ApplyModifiedProperties();

        if (GameObject.Find("PlaceholderLabel") == null)
        {
            GameObject label = new GameObject("PlaceholderLabel", typeof(TextMesh));
            var textMesh = label.GetComponent<TextMesh>();
            textMesh.text = "갑판 (준비 중)";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;
            textMesh.characterSize = 0.3f;
            textMesh.fontSize = 48;
            label.transform.position = new Vector3(0f, 3.5f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        var buildScenes = EditorBuildSettings.scenes.ToList();
        if (!buildScenes.Any(s => s.path == ScenePath))
        {
            buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        Debug.Log("갑판 placeholder scene built at " + ScenePath);
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
        camera.backgroundColor = new Color(0.35f, 0.55f, 0.65f, 1f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        if (cameraGO.GetComponent<CameraFollowTarget>() == null)
        {
            cameraGO.AddComponent<CameraFollowTarget>();
        }

        return camera;
    }

    private static void BuildBackground()
    {
        GameObject bg = GameObject.Find("Background");
        bool created = bg == null;
        if (created)
        {
            bg = new GameObject("Background", typeof(SpriteRenderer));
        }

        var sr = bg.GetComponent<SpriteRenderer>();
        sr.sortingOrder = -10;
        // 처음 생성될 때만 기본 이미지/크기/배치를 적용한다 - 이미 있는 오브젝트는 인스펙터에서 바꾼
        // 이미지와 크기(수동으로 조절한 값 포함)를 그대로 유지.
        if (created)
        {
            if (backgroundSprite != null)
            {
                sr.sprite = backgroundSprite;
                sr.color = Color.white;
            }
            // drawMode+size로 크기를 고정해서, 배경 이미지의 실제 픽셀 크기/스케일과 무관하게 항상 20x12 월드 유닛으로 보이게 한다.
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(20f, 12f);
            bg.transform.position = new Vector3(0f, 0f, 0f);
            bg.transform.localScale = Vector3.one;
        }
    }

    private static Transform BuildPlayer()
    {
        GameObject player = GameObject.Find("Player");
        bool created = player == null;
        if (created)
        {
            player = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(DeckPlayerController));
        }
        player.tag = "Player";

        Sprite idleSprite = downFrames != null && downFrames.Length > 0 ? downFrames[0] : null;

        var sr = player.GetComponent<SpriteRenderer>();
        sr.sprite = idleSprite;
        sr.color = Color.white;
        sr.sortingOrder = 1;

        var rb = player.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        if (created && idleSprite != null)
        {
            Vector2 nativeSize = idleSprite.rect.size / idleSprite.pixelsPerUnit;
            float scale = PlayerTargetHeight / nativeSize.y;

            player.transform.position = new Vector3(0f, 0f, 0f);
            player.transform.localScale = new Vector3(scale, scale, 1f);

            var col = player.GetComponent<BoxCollider2D>();
            col.size = nativeSize;
        }

        var controller = player.GetComponent<DeckPlayerController>();
        SerializedObject controllerSO = new SerializedObject(controller);
        AssignFrames(controllerSO, "downFrames", downFrames);
        AssignFrames(controllerSO, "upFrames", upFrames);
        AssignFrames(controllerSO, "leftFrames", leftFrames);
        AssignFrames(controllerSO, "rightFrames", rightFrames);
        controllerSO.ApplyModifiedProperties();

        return player.transform;
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
                Debug.LogWarning($"[DeckSceneBuilder] {names[i]} 프레임을 찾지 못함");
            }
        }
        return result;
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
}
