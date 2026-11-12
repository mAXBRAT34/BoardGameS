
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneNavigation : MonoBehaviour
{
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
