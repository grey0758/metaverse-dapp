using System.IO;
using MetaverseGame.Bootstrap;
using MetaverseGame.Config;
using MetaverseGame.Gameplay;
using MetaverseGame.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetaverseGame.Editor
{
    public static class ProjectBootstrap
    {
        private const string SceneDirectory = "Assets/MetaverseGame/Scenes";
        private const string ScenePath = SceneDirectory + "/Bootstrap.unity";
        private const string ResourceDirectory = "Assets/MetaverseGame/Resources";
        private const string EnvironmentPath = ResourceDirectory + "/GameEnvironment.asset";

        [MenuItem("Metaverse DApp/Create Development Scene")]
        public static void CreateDevelopmentScene()
        {
            EnsureDirectory(SceneDirectory);
            EnsureDirectory(ResourceDirectory);

            GameEnvironment environment = AssetDatabase.LoadAssetAtPath<GameEnvironment>(
                EnvironmentPath);
            if (environment == null)
            {
                environment = ScriptableObject.CreateInstance<GameEnvironment>();
                AssetDatabase.CreateAsset(environment, EnvironmentPath);
            }

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject camera = new("Main Camera");
            camera.tag = "MainCamera";
            camera.transform.SetPositionAndRotation(
                new Vector3(0f, 8f, -9f),
                Quaternion.Euler(32f, 0f, 0f));
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();

            GameObject light = new("Sun");
            Light sun = light.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Low Poly Arena";
            ground.transform.localScale = new Vector3(2.4f, 1f, 2.4f);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Local Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            player.AddComponent<CharacterController>();
            PlayerMotor motor = player.AddComponent<PlayerMotor>();

            GameObject systems = new("Game Systems");
            systems.AddComponent<GameSocketClient>();
            GameBootstrap bootstrap = systems.AddComponent<GameBootstrap>();
            SerializedObject serialized = new(bootstrap);
            serialized.FindProperty("environment").objectReferenceValue = environment;
            serialized.FindProperty("localPlayer").objectReferenceValue = motor;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"Development scene created at {ScenePath}");
        }

        private static void EnsureDirectory(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }
    }
}
