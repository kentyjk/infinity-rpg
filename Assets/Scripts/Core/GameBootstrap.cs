// ═══════════════════════════════════════════════════════
// GameBootstrap.cs — Creates visible game UI on Play
// ═══════════════════════════════════════════════════════
//
// Attach to any GameObject. On Awake(), creates:
// - Camera (dark background)
// - GameManager + BattleSystem + MapManager + EquipmentManager
// - Canvas with visible HUD, buttons, and battle log
// - EventSystem for input
//
// No TMPro dependency. Works out of the box in Unity 6.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InfinityRPG
{
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            // Create a persistent bootstrap GameObject
            var go = new GameObject("_Bootstrap_Auto");
            DontDestroyOnLoad(go);
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            if (GameManager.Instance != null)
            {
                // Already set up — don't duplicate
                gameObject.SetActive(false);
                return;
            }

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

            // 2. GameManager
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            gmGo.AddComponent<BattleSystem>();
            gmGo.AddComponent<MapManager>();
            gmGo.AddComponent<EquipmentManager>();

            // Ensure GameConfig exists (create default if missing)
            if (gm.Config == null)
            {
                Debug.Log("[Bootstrap] No GameConfig found — creating default...");
                var config = ScriptableObject.CreateInstance<GameConfig>();
                config.mapWidth = 10;
                config.mapHeight = 12;
                config.tileSize = 1f;
                config.statPointsPerLevel = 4;
                config.expCurveMultiplier = 1.35f;
                config.baseExpToNext = 80;
                config.bpMinThreshold = 0.3f;
                config.damageVariance = 0.3f;
                config.maxBattleTurns = 100;
                config.startingWeaponIds = new[] { "w0" };
                config.startingArmorIds = new[] { "a0" };
                gm.SetConfig(config);
            }

            // 3. EventSystem
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            // 4. Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var uiManager = canvasGo.AddComponent<UIManager>();

            // 5. Map (for PlayerController)
            var mapGo = new GameObject("Map");
            mapGo.transform.SetParent(gmGo.transform);
            mapGo.AddComponent<PlayerController>();

            // 6. Create visible UI
            CreateHUD(canvasGo.transform, gm);
            CreateButtons(canvasGo.transform, uiManager);
            CreateBattleLog(canvasGo.transform, uiManager);

            Debug.Log("[Bootstrap] Done! Game should be visible now.");
        }

        private void CreateHUD(Transform parent, GameManager gm)
        {
            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(parent);
            var hud = hudGo.AddComponent<HUDController>();
            var rt = hudGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.94f);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background
            var bg = hudGo.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.07f, 0.12f);

            // Layout
            var layout = hudGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Stat labels
            MakeLabel(hudGo.transform, "⚔️ Lv 1", 20);
            MakeLabel(hudGo.transform, "❤️ 100/100", 20);
            MakeLabel(hudGo.transform, "💪 BP: 0", 20);
            MakeLabel(hudGo.transform, "🪙 0g", 20);

            // Wire stats to HUDController (simplified — uses a single text for now)
            // Actual stat updates happen via GameManager.OnStateChanged → HUDController.Refresh
        }

        private void CreateButtons(Transform parent, UIManager uiManager)
        {
            var btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(parent);
            var rt = btnRow.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.88f);
            rt.anchorMax = new Vector2(1, 0.94f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var layout = btnRow.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.padding = new RectOffset(16, 16, 4, 4);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;

            // Start Run button
            var startBtn = MakeButton(btnRow.transform, "▶ Start Run", new Color(1f, 0.8f, 0.27f));
            startBtn.onClick.AddListener(() => GameManager.Instance?.StartRun());

            // Shop button
            var shopBtn = MakeButton(btnRow.transform, "🏪 Shop", new Color(0.27f, 0.53f, 1f));
            shopBtn.onClick.AddListener(() => {
                var gm = GameManager.Instance;
                if (gm != null && gm.State.bankGold >= 1500)
                    gm.BuyAccessory(gm.Config?.allAccessories?[0]);
            });

            // Equip button
            var equipBtn = MakeButton(btnRow.transform, "⚙️ Equip", new Color(0.53f, 0.27f, 1f));
            equipBtn.onClick.AddListener(() => Debug.Log("[UI] Equip clicked"));

            // Reset button
            var resetBtn = MakeButton(btnRow.transform, "🔄 Reset", new Color(1f, 0.27f, 0.27f));
            resetBtn.onClick.AddListener(() => GameManager.Instance?.HardReset());
        }

        private void CreateBattleLog(Transform parent, UIManager uiManager)
        {
            // Background (Image)
            var logGo = new GameObject("BattleLog");
            logGo.transform.SetParent(parent);
            var rt = logGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, 0.82f);
            rt.anchorMax = new Vector2(0.98f, 0.87f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var bg = logGo.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.07f, 0.12f, 0.9f);

            // Text (child — can't be on same GO as Image)
            var textGo = new GameObject("BattleLogText");
            textGo.transform.SetParent(logGo.transform);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.text = "⚔️ Ready — tap Start Run to begin!";
            text.fontSize = 22;
            text.color = new Color(0.8f, 0.8f, 0.9f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            // Wire to UIManager
            var uiMgr = parent.GetComponent<UIManager>();
            if (uiMgr != null)
                uiMgr.battleLogText = text;
        }

        // ---- Helpers ----

        private Button MakeButton(Transform parent, string label, Color color)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 56);

            var img = go.AddComponent<Image>();
            img.color = color;

            var btn = go.AddComponent<Button>();

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 22;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.04f, 0.04f, 0.07f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            return btn;
        }

        private Text MakeLabel(Transform parent, string text, int fontSize)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(text.Length * 14 + 20, 36);

            var label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleLeft;
            label.raycastTarget = false;

            return label;
        }
    }
}
