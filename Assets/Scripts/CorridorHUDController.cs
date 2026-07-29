using UnityEngine;
using UnityEngine.UI;

public class CorridorHUDController : MonoBehaviour
{
    [SerializeField] private int day = 17;
    [SerializeField] private Text dayText;
    [SerializeField] private GameObject menuOverlay;
    [SerializeField] private GameObject mapOverlay;
    [SerializeField] private GameObject bagOverlay;

    private void Start()
    {
        if (dayText != null)
        {
            dayText.text = $"day {day}";
        }
    }

    public void OnClickMenu()
    {
        if (menuOverlay != null)
        {
            menuOverlay.SetActive(true);
        }
    }

    public void OnClickCloseMenu()
    {
        if (menuOverlay != null)
        {
            menuOverlay.SetActive(false);
        }
    }

    public void OnClickMenuSettings()
    {
        Debug.Log("설정 열기");
    }

    public void OnClickMenuSave()
    {
        Debug.Log("저장하기");
    }

    public void OnClickMenuExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickBag()
    {
        if (bagOverlay != null)
        {
            bagOverlay.SetActive(!bagOverlay.activeSelf);
        }
    }

    public void OnClickMap()
    {
        if (mapOverlay != null)
        {
            mapOverlay.SetActive(true);
        }
    }

    public void OnClickCloseMap()
    {
        if (mapOverlay != null)
        {
            mapOverlay.SetActive(false);
        }
    }
}
