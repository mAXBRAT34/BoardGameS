# CIRKS — CHARACTER SELECT (7 ORIGINAL CHARACTERS)

This pack was prepared from **Characters.rar** and **2D_Blocks_DevilsWorkShop_v04.rar**, provided by the project owner.

Seven original selectable options:
1. Reaper (EvilMan)
2. Fallen Angel (Fallen_Angels_1)
3. Dark Angel (Fallen_Angels_2)
4. Golem
5. Orc
6. Warrior (clothes_1)
7. Armored Warrior (clothes_2)

Includes **150 real idle/eye-blink animation frames** plus subtle procedural bobbing, breathing, and swaying.

## FAST INSTALL — Unity

Option A: use the `.unitypackage`: Assets > Import Package > Custom Package, choose it, and click Import All.
Option B: unzip the ZIP **into your Unity project root** (the folder with Assets and ProjectSettings); let Assets merge into the existing Assets folder. Do not create `Assets/Assets` by accident.

1. Wait for Unity to finish compiling. The package requires UGUI (Unity UI) and TextMeshPro (already used in your project).
2. If TextMeshPro asks to import Essentials, select `Import TMP Essential Resources`.
3. At the top select: **Tools > Cirks > Create Character Selection Scene (7 Characters)**.
4. Unity will create/open the **real** scene at `Assets/CirksCharacterSelect/Scenes/CharacterSelect.unity`.
   The builder does not overwrite `MainMenu` or `GameScene`, and adds the new scene to Build Profiles / Scenes in Build.
   It also finds existing scenes called `MainMenu` and `GameScene` and includes them if possible.
5. Press Play. The left/right arrow buttons select between the 7 characters. Each character blinks and gently sways, breathes and bobs.
6. Enter a name (maximum 18 characters). PLAY saves the character and name and opens `GameScene` if available in the build scene list.
7. The X button returns to `MainMenu`.

## LINK THE START BUTTON IN YOUR CURRENT MainMenu

Open your existing `MainMenu` scene.
1. Select your current `ScriptHolder` GameObject (or create an empty `NavigationManager`).
2. Add component `CirksSceneNavigation`.
3. Select the existing green START button.
4. Under Button > On Click(), drag ScriptHolder into the event field, and choose:
   `CirksSceneNavigation > OpenCharacterSelection()`.
5. Save MainMenu.

## PLAYER DATA USED BY THE GAME

The user's selection is saved via PlayerPrefs:
- PlayerPrefs.GetString("Cirks_PlayerName", "")
- PlayerPrefs.GetInt("Cirks_CharacterId", 0) // 0..6
- PlayerPrefs.GetString("Cirks_CharacterName", "")

Character order: Reaper, Fallen Angel, Dark Angel, Golem, Orc, Warrior, Armored Warrior.

## PERSONALIZATION

Open the generated scene and select `CharacterSelectionManager` to change animation speeds or drag different sprites. `CharacterSelectCanvas` contains all the UI.
`CirksCharacterSelection` drives both the sprite-frame idle animation and tiny real-time movements; buttons are hooked up automatically.

## IMPORTANT

This is a native Unity scene generated from project assets by an **Editor menu command**; the editor must compile once and run the builder. Unity is not available in this packaging environment, so actual in-editor runtime testing has not been performed. No existing scenes are modified by the scripts until you explicitly link START.
