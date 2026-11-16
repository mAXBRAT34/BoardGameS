using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Attach to MainMenu > ScriptHolder and call OpenCharacterSelection() from START.</summary>
public class CirksSceneNavigation : MonoBehaviour
{
    public void OpenCharacterSelection()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
