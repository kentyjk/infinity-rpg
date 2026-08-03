// ═══════════════════════════════════════════════════════
// BuildScript.cs — CI/CD build automation for Unity
// ═══════════════════════════════════════════════════════
//
// PURPOSE: This script handles both local and CI builds.
// On first run, it creates the necessary scene if missing.
// GitHub Actions calls: Unity -executeMethod InfinityRPG.Editor.BuildScript.BuildAndroid
//
// USAGE:
//   Local:  Unity → Tools → Build Android APK
//   CI:     /opt/unity/Editor/Unity -quit -batchmode -executeMethod InfinityRPG.Editor.BuildScript.BuildAndroid

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace InfinityRPG.Editor
{
    public static class BuildScript
    {
        private const string SCENE_PATH = "Assets/Scenes/Main.unity";
        private const string BUILD_DIR = "Builds/Android";

        // ═══════════════════════════════════════════════
        //  MENU ITEMS
        // ═══════════════════════════════════════════════

        [MenuItem("Tools/Infinity RPG/Setup Scene")]
        public static void SetupScene()
        {
            if (EditorUtility.DisplayDialog("Setup Scene",
                "This will create/replace the Main scene with a fully configured setup. Continue?",
                "Yes", "Cancel"))
            {
                CreateMainScene();
                Debug.Log("[BuildScript] Scene created at: " + SCENE_PATH);
            }
        }

        [MenuItem("Tools/Infinity RPG/Build Android APK")]
        public static void BuildAndroidMenu()
        {
            BuildAndroid();
        }

        // ═══════════════════════════════════════════════
        //  SCENE CREATION (called by CI if scene missing)
        // ═══════════════════════════════════════════════

        public static Scene CreateMainScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- CAMERA ---
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.07f); // #0a0a12
            cam.orthographic = false;
            cam.transform.position = new Vector3(0, 0, -10);
            camGo.tag = "MainCamera";

            // --- CANVAS ---
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Add UIManager to Canvas
            var uiManager = canvasGo.AddComponent<UIManager>();

            // --- EVENT SYSTEM ---
            var eventGo = new GameObject("EventSystem");
            eventGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // --- GAME MANAGER ---
            var gmGo = new GameObject("GameManager");
            var gameManager = gmGo.AddComponent<GameManager>();
            gmGo.AddComponent<BattleSystem>();
            gmGo.AddComponent<MapManager>();
            gmGo.AddComponent<EquipmentManager>();

            // --- MAP OBJECT ---
            var mapGo = new GameObject("Map");
            mapGo.transform.SetParent(gmGo.transform);
            mapGo.AddComponent<PlayerController>();

            // --- CREATE HUD PANEL (under Canvas) ---
            CreateHUDPanel(canvasGo.transform, gameManager);

            // --- CREATE SHOP PANEL ---
            CreateShopPanel(canvasGo.transform, gameManager);

            // --- CREATE EQUIP PANEL ---
            CreateEquipPanel(canvasGo.transform, gameManager);

            // --- CREATE LEVEL UP PANEL ---
            CreateLevelUpPanel(canvasGo.transform, gameManager);

            // --- CREATE BATTLE LOG ---
            var logGo = CreateText(canvasGo.transform, "BattleLog", "", 24,
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0.06f),
                new Vector2(0, 0.94f), TextAlignmentOptions.Center);
            logGo.name = "BattleLog";

            // --- CREATE TOAST ---
            var toastGo = CreateText(canvasGo.transform, "Toast", "", 20,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.6f, 0.06f),
                new Vector2(0.5f, 0.08f), TextAlignmentOptions.Center);
            toastGo.name = "Toast";
            toastGo.SetActive(false);

            // --- HUB BUTTONS ---
            var hubGo = new GameObject("HubButtons");
            hubGo.transform.SetParent(canvasGo.transform);
            var hubLayout = hubGo.AddComponent<HorizontalLayoutGroup>();
            hubLayout.spacing = 8;
            hubLayout.padding = new RectOffset(8, 8, 8, 8);
            hubLayout.childAlignment = TextAnchor.MiddleCenter;
            var hubRect = hubGo.GetComponent<RectTransform>();
            hubRect.anchorMin = new Vector2(0, 0.88f);
            hubRect.anchorMax = new Vector2(1, 0.94f);
            hubRect.offsetMin = Vector2.zero;
            hubRect.offsetMax = Vector2.zero;

            CreateButton(hubGo.transform, "StartRun", "▶ Start Run", new Color(1f, 0.8f, 0.27f));
            CreateButton(hubGo.transform, "Shop", "🏪 Shop", new Color(0.27f, 0.53f, 1f));
            CreateButton(hubGo.transform, "Equip", "⚙️ Equip", new Color(0.53f, 0.27f, 1f));
            CreateButton(hubGo.transform, "Reset", "🔄 Reset", new Color(1f, 0.27f, 0.27f));

            // --- WIRE REFERENCES ---
            // Find objects and wire them to UIManager
            WireReferences(uiManager, gameManager, canvasGo.transform);

            // --- SAVE SCENE ---
            // Ensure directory exists
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, SCENE_PATH);

            // Add to build settings
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene(SCENE_PATH, true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log("[BuildScript] Scene created and added to build settings.");
            return scene;
        }

        // ═══════════════════════════════════════════════
        //  BUILD
        // ═══════════════════════════════════════════════

        public static void BuildAndroid()
        {
            // Ensure scene exists
            if (!System.IO.File.Exists(SCENE_PATH))
            {
                Debug.Log("[BuildScript] Scene not found. Creating...");
                CreateMainScene();
            }

            // Ensure GameConfig exists
            EnsureGameConfig();

            // Set build settings
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            // Player settings
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.infinityrpg.game");
            PlayerSettings.productName = "Infinity RPG";
            PlayerSettings.companyName = "InfinityRPG";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Keystore (from environment or default)
            string keystorePass = System.Environment.GetEnvironmentVariable("KEYSTORE_PASS") ?? "android";
            string keyaliasPass = System.Environment.GetEnvironmentVariable("KEYALIAS_PASS") ?? "android";
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keyaliasPass;

            // Build
            System.IO.Directory.CreateDirectory(BUILD_DIR);
            string outputPath = $"{BUILD_DIR}/InfinityRPG.apk";

            var report = BuildPipeline.BuildPlayer(
                new[] { SCENE_PATH },
                outputPath,
                BuildTarget.Android,
                BuildOptions.None
            );

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] BUILD SUCCESS: {outputPath}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] BUILD FAILED: {report.summary}");
                EditorApplication.Exit(1);
            }
        }

        // ═══════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════

        private static void EnsureGameConfig()
        {
            // Check if GameConfig exists in Resources
            var config = Resources.Load<GameConfig>("GameConfig");
            if (config == null)
            {
                Debug.Log("[BuildScript] Creating default GameConfig...");
                config = ScriptableObject.CreateInstance<GameConfig>();
                System.IO.Directory.CreateDirectory("Assets/Resources");

                // Create default zones, weapons, etc. (minimal for build)
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

                AssetDatabase.CreateAsset(config, "Assets/Resources/GameConfig.asset");
                AssetDatabase.SaveAssets();
            }
        }

        private static void WireReferences(UIManager uiManager, GameManager gameManager, Transform canvasRoot)
        {
            // Use SerializedObject to wire references via reflection
            var uiSo = new SerializedObject(uiManager);
            var gmSo = new SerializedObject(gameManager);

            // Wire GameManager reference
            uiSo.FindProperty("gameManager").objectReferenceValue = gameManager;
            gmSo.FindProperty("config").objectReferenceValue = Resources.Load<GameConfig>("GameConfig");

            // Wire UI panel references by finding them in the scene
            var hud = canvasRoot.GetComponentInChildren<HUDController>();
            var shop = canvasRoot.GetComponentInChildren<ShopPanel>();
            var equip = canvasRoot.GetComponentInChildren<EquipPanel>();
            var levelUp = canvasRoot.GetComponentInChildren<LevelUpPanel>();

            if (hud != null) uiSo.FindProperty("hud").objectReferenceValue = hud;
            if (shop != null) uiSo.FindProperty("shopPanel").objectReferenceValue = shop;
            if (equip != null) uiSo.FindProperty("equipPanel").objectReferenceValue = equip;
            if (levelUp != null) uiSo.FindProperty("levelUpPanel").objectReferenceValue = levelUp;

            // Wire hub buttons
            var hubButtons = canvasRoot.Find("HubButtons");
            if (hubButtons != null)
                uiSo.FindProperty("hubButtons").objectReferenceValue = hubButtons.gameObject;

            // Wire battle log
            var battleLog = canvasRoot.Find("BattleLog");
            if (battleLog != null)
                uiSo.FindProperty("battleLogText").objectReferenceValue = battleLog.GetComponent<TextMeshProUGUI>();

            // Wire toast
            var toast = canvasRoot.Find("Toast");
            if (toast != null)
            {
                uiSo.FindProperty("toastObject").objectReferenceValue = toast.gameObject;
                uiSo.FindProperty("toastText").objectReferenceValue = toast.GetComponent<TextMeshProUGUI>();
            }

            uiSo.ApplyModifiedProperties();
            gmSo.ApplyModifiedProperties();
        }

        private static void CreateHUDPanel(Transform parent, GameManager gameManager)
        {
            var go = new GameObject("HUD");
            go.transform.SetParent(parent);
            var hud = go.AddComponent<HUDController>();
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(8, 8, 4, 4);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 40);

            // Create stat labels (simplified — just a single text)
            var label = CreateText(go.transform, "StatLabel", "⚔️ Lv1 ❤️ 100/100 💪 BP:0 🪙 0g",
                18, Vector2.zero, Vector2.one, Vector2.one, Vector2.zero, TextAlignmentOptions.Center);
        }

        private static void CreateShopPanel(Transform parent, GameManager gameManager)
        {
            var go = new GameObject("ShopPanel");
            go.transform.SetParent(parent);
            go.AddComponent<ShopPanel>();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.2f);
            rect.anchorMax = new Vector2(1, 0.85f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.SetActive(false);

            // Title
            CreateText(go.transform, "Title", "🏪 Equipment Shop", 24,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.05f),
                new Vector2(0, -30), TextAlignmentOptions.Center);

            // Scroll area for items
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(go.transform);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            var scrollRT = scrollGo.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(8, 8);
            scrollRT.offsetMax = new Vector2(-8, -40);

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollGo.transform);
            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4;
            contentLayout.padding = new RectOffset(4, 4, 4, 4);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            var contentRT = contentGo.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRT;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Store content reference via SerializedField
            var shopPanel = go.GetComponent<ShopPanel>();
            var so = new SerializedObject(shopPanel);
            so.FindProperty("contentParent").objectReferenceValue = contentGo.transform;
            so.ApplyModifiedProperties();
        }

        private static void CreateEquipPanel(Transform parent, GameManager gameManager)
        {
            var go = new GameObject("EquipPanel");
            go.transform.SetParent(parent);
            go.AddComponent<EquipPanel>();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.4f);
            rect.anchorMax = new Vector2(1, 0.75f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.SetActive(false);

            CreateText(go.transform, "Title", "⚙️ Equipment Loadout", 24,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.1f),
                new Vector2(0, -30), TextAlignmentOptions.Center);

            // Create 3 dropdown rows (weapon, armor, accessory)
            CreateEquipRow(go.transform, "⚔️ Weapon", 0);
            CreateEquipRow(go.transform, "🛡️ Armor", 1);
            CreateEquipRow(go.transform, "💍 Accessory", 2);
        }

        private static void CreateEquipRow(Transform parent, string label, int index)
        {
            var row = new GameObject($"EquipRow_{index}");
            row.transform.SetParent(parent);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(0, 40);

            var labelGo = CreateText(row.transform, "Label", label, 18,
                Vector2.zero, Vector2.one, new Vector2(0.3f, 1),
                Vector2.zero, TextAlignmentOptions.Left);

            var dropdownGo = new GameObject("Dropdown");
            dropdownGo.transform.SetParent(row.transform);
            dropdownGo.AddComponent<Dropdown>();
            var dropRT = dropdownGo.GetComponent<RectTransform>();
            dropRT.sizeDelta = new Vector2(200, 30);
        }

        private static void CreateLevelUpPanel(Transform parent, GameManager gameManager)
        {
            var go = new GameObject("LevelUpPanel");
            go.transform.SetParent(parent);
            go.AddComponent<LevelUpPanel>();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.3f);
            rect.anchorMax = new Vector2(0.9f, 0.7f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.SetActive(false);

            // Background
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.18f, 0.95f);

            CreateText(go.transform, "Title", "🎉 LEVEL UP!", 28,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.1f),
                new Vector2(0, -20), TextAlignmentOptions.Center);

            CreateText(go.transform, "SPLabel", "SP remaining: 4", 22,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0.06f),
                new Vector2(0, -55), TextAlignmentOptions.Center);

            // Stat allocation rows
            string[] stats = { "❤️ HP    (+5)", "⚡ ATK  (+2)", "🛡️ DEF  (+1)", "💨 AGI  (+1)" };
            for (int i = 0; i < stats.Length; i++)
            {
                CreateStatAllocRow(go.transform, stats[i], i);
            }

            // Confirm button
            var btnGo = CreateButton(go.transform, "ConfirmBtn", "Confirm ▶",
                new Color(1f, 0.8f, 0.27f));
            var btnRT = btnGo.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.3f, 0);
            btnRT.anchorMax = new Vector2(0.7f, 0.1f);

            // Wire confirm button
            var lp = go.GetComponent<LevelUpPanel>();
            var so = new SerializedObject(lp);
            so.FindProperty("confirmButton").objectReferenceValue = btnGo.GetComponent<Button>();
            so.ApplyModifiedProperties();
        }

        private static void CreateStatAllocRow(Transform parent, string label, int index)
        {
            var row = new GameObject($"StatRow_{index}");
            row.transform.SetParent(parent);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.7f - index * 0.12f);
            rt.anchorMax = new Vector2(1, 0.82f - index * 0.12f);
            rt.offsetMin = new Vector2(20, 0);
            rt.offsetMax = new Vector2(-20, 0);

            CreateText(row.transform, "Label", label, 18,
                Vector2.zero, new Vector2(0.7f, 1), new Vector2(0.7f, 1),
                Vector2.zero, TextAlignmentOptions.Left);

            var valGo = CreateText(row.transform, "Value", "0", 20,
                new Vector2(0.7f, 0), new Vector2(0.85f, 1), new Vector2(0.15f, 1),
                Vector2.zero, TextAlignmentOptions.Center);

            var addBtn = CreateButton(row.transform, "AddBtn", "+",
                new Color(0.2f, 0.2f, 0.4f));
            var addRT = addBtn.GetComponent<RectTransform>();
            addRT.anchorMin = new Vector2(0.85f, 0.1f);
            addRT.anchorMax = new Vector2(0.95f, 0.9f);
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;

            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var img = go.AddComponent<Image>();
            img.color = color;

            var btn = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 36);

            var label = new GameObject("Label");
            label.transform.SetParent(go.transform);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.04f, 0.04f, 0.07f);
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.sizeDelta = Vector2.zero;

            return go;
        }
    }
}
#endif
