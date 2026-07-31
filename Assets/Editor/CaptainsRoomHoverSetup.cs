using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 선장실의 독서대(조타실_독서대_N) 오브젝트에 충돌체 + HoverHighlight를 붙여서,
// 마우스 커서를 올리면 조타실_독서대_하라_0으로 바뀌도록 연결한다.
public static class CaptainsRoomHoverSetup
{
    private const string ScenePath = "Assets/Scenes/선장실.unity";

    private const string LecternSpritePath = "Assets/sprites/조타실_독서대.PNG";
    private const string LecternHighlightPath = "Assets/sprites/조타실_독서대_하라.PNG";
    private const string LecternHighlightFrameName = "조타실_독서대_하라_0";

    [MenuItem("Tools/Story Game/Wire Captain's Room Hover Highlights")]
    public static void Wire()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = System.IO.File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath)
                : default;
        }

        if (!scene.IsValid())
        {
            Debug.LogError($"씬을 찾을 수 없음: {ScenePath}");
            return;
        }

        Sprite[] normalFrames = AssetDatabase.LoadAllAssetsAtPath(LecternSpritePath).OfType<Sprite>().ToArray();
        Sprite highlightSprite = AssetDatabase.LoadAllAssetsAtPath(LecternHighlightPath)
            .OfType<Sprite>()
            .FirstOrDefault(s => s.name == LecternHighlightFrameName);

        if (highlightSprite == null)
        {
            Debug.LogError($"{LecternHighlightFrameName} 스프라이트를 {LecternHighlightPath}에서 찾지 못해서 중단함.");
            return;
        }

        int wiredCount = 0;
        foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (sr.sprite == null || !normalFrames.Contains(sr.sprite))
            {
                continue;
            }

            Wire(sr, highlightSprite);
            wiredCount++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        Debug.Log($"독서대 {wiredCount}개에 호버 하이라이트 연결 완료");
    }

    private static void Wire(SpriteRenderer sr, Sprite highlightSprite)
    {
        GameObject go = sr.gameObject;

        var col = go.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = go.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        col.size = sr.sprite.rect.size / sr.sprite.pixelsPerUnit;

        var hover = go.GetComponent<HoverHighlight>();
        if (hover == null)
        {
            hover = go.AddComponent<HoverHighlight>();
        }

        SerializedObject so = new SerializedObject(hover);
        so.FindProperty("spriteRenderer").objectReferenceValue = sr;
        so.FindProperty("highlightSprite").objectReferenceValue = highlightSprite;
        so.ApplyModifiedProperties();
    }
}
