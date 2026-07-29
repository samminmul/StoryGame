using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExitButton : MonoBehaviour
{
    [SerializeField] private string targetScene;

    public void OnClickExit()
    {
        SceneManager.LoadScene(targetScene);
    }
}
