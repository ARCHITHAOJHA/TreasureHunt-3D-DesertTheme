#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DesertTreasureHunt.Editor
{
    [InitializeOnLoad]
    public static class DesertGameSetupWizard
    {
        static DesertGameSetupWizard()
        {
            EditorApplication.delayCall += EnsureScenesInBuild;
        }

        [MenuItem("Desert Hunt/Register Scenes in Build Profiles", false, 0)]
        public static void EnsureScenesInBuild()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainGame.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("<color=green>[Desert Hunt]</color> Registered MainMenu.unity and MainGame.unity in Build Settings!");
        }

        [MenuItem("Desert Hunt/1. Setup Game Scene (MainGame)", false, 1)]
        public static void BuildMainGameScene()
        {
            Scene activeScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Add Master Initializer
            GameObject masterObj = new GameObject("_GameMasterInitializer");
            masterObj.AddComponent<DesertGameInitializer>();

            // Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
            camObj.transform.position = new Vector3(0f, 4f, -6f);
            camObj.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            // Directional Light
            GameObject lightObj = new GameObject("DesertSun");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.94f, 0.78f);
            light.intensity = 1.35f;
            lightObj.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

            // Save Scene
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            string path = "Assets/Scenes/MainGame.unity";
            EditorSceneManager.SaveScene(activeScene, path);
            AssetDatabase.Refresh();

            EnsureScenesInBuild();
            Debug.Log("<color=green>[Desert Hunt]</color> Successfully created and saved MainGame scene at " + path);
        }

        [MenuItem("Desert Hunt/2. Setup Main Menu Scene (MainMenu)", false, 2)]
        public static void BuildMainMenuScene()
        {
            Scene activeScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();

            // Event System
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            // Main Menu UI
            GameObject menuObj = new GameObject("MainMenuManager");
            menuObj.AddComponent<MainMenuUI>();

            // Save Scene
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            string path = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.SaveScene(activeScene, path);
            AssetDatabase.Refresh();

            EnsureScenesInBuild();
            Debug.Log("<color=green>[Desert Hunt]</color> Successfully created and saved MainMenu scene at " + path);
        }

        [MenuItem("Desert Hunt/3. Open Documentation & Controls Guide", false, 3)]
        public static void ShowGuide()
        {
            EditorUtility.DisplayDialog(
                "Desert Treasure Hunt 3D - Controls & Guide",
                "HOW TO PLAY:\n\n" +
                "- Left / Right Arrow or A / D or Swipe: Change Lanes\n" +
                "- Up Arrow / W / Space or Swipe Up: Jump over spikes and gaps\n" +
                "- Down Arrow / S or Swipe Down: Slide under archways and blades\n" +
                "- ESC / P: Pause Game\n\n" +
                "OBJECTIVE:\n" +
                "Run through the ancient desert ruins, collect coins, rubies, sapphires, and keys, open treasure chests, and survive as long as possible!",
                "Let's Hunt!"
            );
        }
    }
}
#endif
