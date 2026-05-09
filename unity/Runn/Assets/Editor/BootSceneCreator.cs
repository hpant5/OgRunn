using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Runn.Systems;

namespace Runn.EditorTools
{
    public static class BootSceneCreator
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string BootScenePath = "Assets/Scenes/Boot.unity";

        [MenuItem("Runn/Create or Open Boot Scene")]
        public static void CreateOrOpenBootScene()
        {
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (System.IO.File.Exists(BootScenePath))
            {
                EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
                Debug.Log("[Runn] Opened existing Boot scene.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootGo = new GameObject("Bootstrap");
            bootGo.AddComponent<GameBootstrap>();

            EditorSceneManager.SaveScene(scene, BootScenePath);
            EnsureBuildSettings();
            Debug.Log($"[Runn] Created Boot scene at {BootScenePath}.");
        }

        [MenuItem("Runn/Configure Android Build Settings")]
        public static void ConfigureAndroidBuild()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 2);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.companyName = "Runn";
            PlayerSettings.productName = "Runn";
            EnsureBuildSettings();
            Debug.Log("[Runn] Android build settings configured.");
        }

        private static void EnsureBuildSettings()
        {
            if (!System.IO.File.Exists(BootScenePath)) return;
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool exists = false;
            foreach (var s in scenes) if (s.path == BootScenePath) { exists = true; break; }
            if (!exists)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(BootScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
