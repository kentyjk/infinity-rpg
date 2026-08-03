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
            // Work on the currently open scene (don't create a new one)
            var scene = EditorSceneManager.GetActiveScene();
            
            // Check if already set up
            if (GameObject.Find("GameManager") != null)
            {
                Debug.Log("[BuildScript] GameManager already exists — scene is set up.");
                return scene;
            }

            // Add a bootstrap GameObject — it auto-creates everything at runtime
            var bootstrap = new GameObject("_Bootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            // Save scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log("[BuildScript] Bootstrap added to scene. Hit Play to auto-create game systems.");
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
            var config = Resources.Load<GameConfig>("GameConfig");
            if (config == null)
            {
                Debug.Log("[BuildScript] Creating default GameConfig...");
                config = ScriptableObject.CreateInstance<GameConfig>();
                System.IO.Directory.CreateDirectory("Assets/Resources");
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
    }
}
#endif
