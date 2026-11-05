using UnityEngine;
using UnityEngine.EventSystems;

namespace DesertTreasureHunt
{
    [DefaultExecutionOrder(-100)]
    public class DesertGameInitializer : MonoBehaviour
    {
        private void Awake()
        {
            SetupEventSystem();
            SetupManagers();
            SetupEnvironment();
            SetupPlayer();
            SetupCamera();
            SetupTrack();
            SetupUI();
        }

        private void SetupEventSystem()
        {
            EventSystem existingEs = FindFirstObjectByType<EventSystem>();
            GameObject es = (existingEs != null) ? existingEs.gameObject : new GameObject("EventSystem");
            EventSystemHelper.EnsureCorrectInputModule(es);
        }

        private void SetupManagers()
        {
            if (FindFirstObjectByType<GameManager>() == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }

            if (FindFirstObjectByType<SoundManager>() == null)
            {
                GameObject smObj = new GameObject("SoundManager");
                smObj.AddComponent<SoundManager>();
            }
        }

        private void SetupEnvironment()
        {
            if (FindFirstObjectByType<DesertEnvironmentBuilder>() == null)
            {
                GameObject envObj = new GameObject("DesertEnvironment");
                envObj.AddComponent<DesertEnvironmentBuilder>();
            }
        }

        private void SetupPlayer()
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc == null)
            {
                GameObject player = new GameObject("Player_Adventurer");
                player.transform.position = new Vector3(0f, 0.1f, 5f);
                player.tag = "Player";

                CharacterController cc = player.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.4f;
                cc.center = new Vector3(0f, 0.9f, 0f);

                pc = player.AddComponent<PlayerController>();
                player.AddComponent<PlayerHealth>();

                GameObject visualsObj = new GameObject("Visuals");
                visualsObj.transform.SetParent(player.transform, false);
                visualsObj.AddComponent<PlayerVisuals>();
            }
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            if (cam.GetComponent<CameraFollow>() == null)
            {
                CameraFollow cf = cam.gameObject.AddComponent<CameraFollow>();
                PlayerController pc = FindFirstObjectByType<PlayerController>();
                if (pc != null) cf.target = pc.transform;
            }
        }

        private void SetupTrack()
        {
            if (FindFirstObjectByType<TrackManager>() == null)
            {
                GameObject trackObj = new GameObject("TrackManager");
                trackObj.AddComponent<TrackManager>();
            }
        }

        private void SetupUI()
        {
            if (FindFirstObjectByType<UIManager>() == null)
            {
                GameObject uiObj = new GameObject("UIManager");
                uiObj.AddComponent<UIManager>();
            }
        }
    }
}
