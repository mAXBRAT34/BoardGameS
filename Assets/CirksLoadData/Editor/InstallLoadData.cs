#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click, non-destructive setup. Reads the original seven character sprites
/// from CharacterSelect and assigns them to a component on the GameScene Canvas.
/// </summary>
public static class InstallLoadData
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string SelectScenePath = "Assets/Scenes/CharacterSelect.unity";

    [MenuItem("Tools/Cirks/Add Load Data to GameScene")]
    public static void Install()
    {
        if (!System.IO.File.Exists(GameScenePath) || !System.IO.File.Exists(SelectScenePath))
        {
            EditorUtility.DisplayDialog("Cirks", "GameScene.unity or CharacterSelect.unity not found in Assets/Scenes.", "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        Canvas gameCanvas = null;
        foreach (GameObject root in gameScene.GetRootGameObjects())
        {
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            if (canvases.Length > 0) { gameCanvas = canvases[0]; break; }
        }

        if (gameCanvas == null)
        {
            EditorUtility.DisplayDialog("Cirks", "No Canvas found in GameScene.", "OK");
            return;
        }

        GameSceneLoadData loader = gameCanvas.GetComponent<GameSceneLoadData>();
        if (loader == null)
            loader = Undo.AddComponent<GameSceneLoadData>(gameCanvas.gameObject);

        Scene charScene = EditorSceneManager.OpenScene(SelectScenePath, OpenSceneMode.Additive);
        CirksCharacterSelection selector = null;
        foreach (GameObject root in charScene.GetRootGameObjects())
        {
            selector = root.GetComponentInChildren<CirksCharacterSelection>(true);
            if (selector != null) break;
        }

        if (selector == null || selector.characters == null || selector.characters.Length != 7)
        {
            EditorSceneManager.CloseScene(charScene, true);
            EditorUtility.DisplayDialog("Cirks", "Expected 7 heroes in CharacterSelect. Check CharacterSelectionManager.", "OK");
            return;
        }

        Sprite[] portraits = new Sprite[selector.characters.Length];
        for (int i = 0; i < selector.characters.Length; i++)
        {
            CirksCharacterSelection.CharacterDefinition hero = selector.characters[i];
            portraits[i] = hero != null && hero.idleFrames != null && hero.idleFrames.Length > 0
                ? hero.idleFrames[0]
                : null;
        }

        EditorSceneManager.CloseScene(charScene, true);

        Undo.RecordObject(loader, "Assign seven player portraits");
        SerializedObject serialized = new SerializedObject(loader);
        SerializedProperty property = serialized.FindProperty("heroPortraits");
        property.arraySize = portraits.Length;
        for (int i = 0; i < portraits.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = portraits[i];
        serialized.ApplyModifiedProperties();

        EditorUtility.SetDirty(loader);
        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene);
        Selection.activeGameObject = gameCanvas.gameObject;

        Debug.Log("Cirks Load Data installed. Open GameScene and press Play to see LOAD DATA.");
        EditorUtility.DisplayDialog("Cirks", "Ready! GameScene now has a LOAD DATA button. Press Play to test it.", "OK");
    }
}
#endif
