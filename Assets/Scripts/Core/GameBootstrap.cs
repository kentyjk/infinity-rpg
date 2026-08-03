// ═══════════════════════════════════════════════════════
// GameBootstrap.cs — Auto-creates all game systems on Play
// ═══════════════════════════════════════════════════════
//
// Attach to ANY GameObject in the scene (or create an empty one).
// On Awake(), if GameManager doesn't exist, it creates the entire
// game setup: GameManager, Canvas, UI panels, EventSystem, Camera.
//
// This means NO setup tool needed — just hit Play.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InfinityRPG
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Optional: assign a pre-made GameManager prefab")]
        [SerializeField] private GameManager existingGameManager;

        private void Awake()
        {
            // Check if already set up
            if (GameManager.Instance != null) return;

            Debug.Log("[Bootstrap] Creating game systems...");

            // 1. Camera
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.07f);
            cam.orthographic = false;
            cam.transform.position = new Vector3(0, 0, -10);

            // 2. EventSystem
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            // 3. GameManager
            GameObject gmGo;
            if (existingGameManager != null)
            {
                gmGo = existingGameManager.gameObject;
            }
            else
            {
                gmGo = new GameObject("GameManager");
                gmGo.AddComponent<GameManager>();
                gmGo.AddComponent<BattleSystem>();
                gmGo.AddComponent<MapManager>();
                gmGo.AddComponent<EquipmentManager>();
            }

            // 4. Canvas + UI
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<UIManager>();

            // 5. PlayerController (map input)
            var mapGo = new GameObject("Map");
            mapGo.transform.SetParent(gmGo.transform);
            mapGo.AddComponent<PlayerController>();

            Debug.Log("[Bootstrap] Game systems created. Hit Play!");
        }
    }
}
