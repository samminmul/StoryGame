using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DeckSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/갑판.unity";
    private const string BackgroundImagePath = "Assets/sprites/갑판.png";
    private const string PlayerSheetPath = "Assets/sprites/NPC2 (1).png";
    private const float PlayerTargetHeight = 1.6f;
    private const float SpritePixelsPerUnit = 100f;

    // NPC2 (1).png는 3x3 프레임 시트(마지막 2칸은 비어있음). 각 프레임의 픽셀 경계를
    // 실제 캐릭터를 감싸는 타이트한 사각형으로 미리 계산해둔 값.
    // 0,1: 아래쪽 / 2,3: 위쪽(뒷모습) / 4,5,6: 옆모습(왼쪽 기준, 오른쪽은 스프라이트 반전)
    private static readonly Rect[] PlayerFrameRects =
    {
        new Rect(84f, 544f, 88f, 192f),
        new Rect(340f, 548f, 88f, 188f),
        new Rect(596f, 544f, 88f, 192f),
        new Rect(84f, 292f, 88f, 188f),
        new Rect(340f, 296f, 84f, 180f),
        new Rect(596f, 300f, 84f, 176f),
        new Rect(84f, 40f, 84f, 180f),
    };

    private static Sprite backgroundSprite;
    private static Sprite[] playerFrames;

    [MenuItem("Tools/Story Game/Build Deck Scene (Placeholder)")]
    public static void Build()
    {
        backgroundSprite = EnsureSingleSprite(BackgroundImagePath);
        playerFrames = EnsureSlicedSprites(PlayerSheetPath, "Player_", PlayerFrameRects, SpritePixelsPerUnit);

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
        if (backgroundSprite == null)
        {
            return;
        }

        GameObject bg = GameObject.Find("Background");
        if (bg == null)
        {
            bg = new GameObject("Background", typeof(SpriteRenderer));
        }

        var sr = bg.GetComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.color = Color.white;
        sr.sortingOrder = -10;
        // drawMode+size로 크기를 고정해서, 배경 이미지의 실제 픽셀 크기/스케일과 무관하게 항상 20x12 월드 유닛으로 보이게 한다.
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(20f, 12f);
        bg.transform.position = new Vector3(0f, 0f, 0f);
        bg.transform.localScale = Vector3.one;
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

        Sprite idleSprite = playerFrames != null && playerFrames.Length > 0 ? playerFrames[0] : null;

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
        SerializedProperty framesProp = controllerSO.FindProperty("frames");
        framesProp.arraySize = playerFrames?.Length ?? 0;
        for (int i = 0; i < framesProp.arraySize; i++)
        {
            framesProp.GetArrayElementAtIndex(i).objectReferenceValue = playerFrames[i];
        }
        controllerSO.ApplyModifiedProperties();

        return player.transform;
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

    private static Sprite[] EnsureSlicedSprites(string assetPath, string namePrefix, Rect[] pixelRects, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"텍스처를 찾을 수 없음: {assetPath}");
            return new Sprite[pixelRects.Length];
        }

        var expectedNames = Enumerable.Range(0, pixelRects.Length).Select(i => $"{namePrefix}{i}").ToArray();

        // 기존 importer.spritesheet를 읽어서 병합하지 않고, 우리가 원하는 프레임 목록(+기존 Player_Idle 보존)을
        // 항상 통째로 새로 만들어서 덮어쓴다. 병합 방식은 이 텍스처에서 실제로 원인 불명으로 반영이 안 되는
        // 문제가 있었어서, 결정론적으로 매번 동일한 결과가 나오도록 단순화함.
        var desired = expectedNames.Select((name, i) => new SpriteMetaData
        {
            name = name,
            rect = pixelRects[i],
            alignment = (int)SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f)
        }).ToList();

        if (importer.spritesheet != null)
        {
            foreach (var existing in importer.spritesheet)
            {
                if (!expectedNames.Contains(existing.name))
                {
                    desired.Add(existing);
                }
            }
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.spritesheet = desired.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToList();
        Debug.Log($"[DeckSceneBuilder] {assetPath} 슬라이스 결과: {string.Join(", ", sprites.Select(s => s.name))}");

        var result = new Sprite[pixelRects.Length];
        for (int i = 0; i < pixelRects.Length; i++)
        {
            result[i] = sprites.FirstOrDefault(s => s.name == expectedNames[i]);
            if (result[i] == null)
            {
                Debug.LogWarning($"[DeckSceneBuilder] {expectedNames[i]} 프레임을 찾지 못함");
            }
        }
        return result;
    }
}
