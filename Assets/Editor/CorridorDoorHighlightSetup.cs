using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 선실 복도의 문 오브젝트(선실복도_방문/식당문 스프라이트를 쓰는 오브젝트)에
// 충돌체 + Interactable을 붙이고, 플레이어가 닿으면 "_하라" 스프라이트로 바뀌도록 연결한다.
// 문 배치 자체는 손으로 관리하므로, 이 도구는 이미 씬에 놓인 문 오브젝트를 찾아서 연결만 해준다.
public static class CorridorDoorHighlightSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    private const string DoorSpritePath = "Assets/sprites/선실복도_방문.PNG";
    private const string DoorHighlightPath = "Assets/sprites/선실복도_방문_하라.PNG";
    private const string DoorHighlightFrameName = "선실복도_방문_하라_0";

    private const string DiningDoorSpritePath = "Assets/sprites/선실복도_식당문.PNG";
    private const string DiningDoorHighlightPath = "Assets/sprites/선실복도_식당문_하라.PNG";
    private const string DiningDoorHighlightFrameName = "선실복도_식당문_하라_6";

    [MenuItem("Tools/Story Game/Wire Corridor Door Highlights")]
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

        Sprite[] doorFrames = AssetDatabase.LoadAllAssetsAtPath(DoorSpritePath).OfType<Sprite>().ToArray();
        Sprite doorHighlight = LoadNamedSprite(DoorHighlightPath, DoorHighlightFrameName);

        Sprite[] diningDoorFrames = AssetDatabase.LoadAllAssetsAtPath(DiningDoorSpritePath).OfType<Sprite>().ToArray();
        Sprite diningDoorHighlight = LoadNamedSprite(DiningDoorHighlightPath, DiningDoorHighlightFrameName);

        if (doorHighlight == null || diningDoorHighlight == null)
        {
            Debug.LogError("하이라이트 스프라이트를 찾지 못해서 중단함.");
            return;
        }

        int wiredCount = 0;
        foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (doorFrames.Contains(sr.sprite))
            {
                WireDoor(sr, doorHighlight);
                wiredCount++;
            }
            else if (diningDoorFrames.Contains(sr.sprite))
            {
                WireDoor(sr, diningDoorHighlight);
                wiredCount++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        Debug.Log($"문 {wiredCount}개에 하이라이트 연결 완료");
    }

    private static void WireDoor(SpriteRenderer sr, Sprite highlightSprite)
    {
        GameObject door = sr.gameObject;

        var col = door.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = door.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        col.size = sr.sprite.rect.size / sr.sprite.pixelsPerUnit;

        var interactable = door.GetComponent<Interactable>();
        if (interactable == null)
        {
            interactable = door.AddComponent<Interactable>();
        }

        SerializedObject so = new SerializedObject(interactable);
        so.FindProperty("spriteRenderer").objectReferenceValue = sr;
        so.FindProperty("highlightSprite").objectReferenceValue = highlightSprite;
        so.ApplyModifiedProperties();
    }

    private static Sprite LoadNamedSprite(string assetPath, string spriteName)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault(s => s.name == spriteName);
        if (sprite == null)
        {
            Debug.LogWarning($"[CorridorDoorHighlightSetup] {spriteName} 스프라이트를 {assetPath}에서 찾지 못함");
        }
        return sprite;
    }
}
