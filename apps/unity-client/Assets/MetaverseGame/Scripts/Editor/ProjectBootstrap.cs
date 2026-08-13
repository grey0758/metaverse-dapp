using System.IO;
using MetaverseGame.Bootstrap;
using MetaverseGame.Config;
using MetaverseGame.Gameplay;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
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
        private const string PrefabDirectory = "Assets/MetaverseGame/Prefabs";
        private const string PlayerPrefabPath = PrefabDirectory + "/NetworkPlayer.prefab";

        [MenuItem("Metaverse DApp/Create Development Scene")]
        public static void CreateDevelopmentScene()
        {
            EnsureDirectory(SceneDirectory);
            EnsureDirectory(ResourceDirectory);
            EnsureDirectory(PrefabDirectory);

            GameEnvironment environment = AssetDatabase.LoadAssetAtPath<GameEnvironment>(
                EnvironmentPath);
            if (environment == null)
            {
                environment = ScriptableObject.CreateInstance<GameEnvironment>();
                AssetDatabase.CreateAsset(environment, EnvironmentPath);
            }

            GameObject playerPrefab = CreateNetworkPlayerPrefab();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject camera = new("Main Camera");
            camera.tag = "MainCamera";
            camera.transform.SetPositionAndRotation(
                new Vector3(0f, 2.8f, -14.5f),
                Quaternion.Euler(10f, 0f, 0f));
            Camera sceneCamera = camera.AddComponent<Camera>();
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
            sceneCamera.backgroundColor = new Color(0.035f, 0.055f, 0.06f);
            sceneCamera.fieldOfView = 60f;
            camera.AddComponent<AudioListener>();
            camera.AddComponent<FollowLocalPlayer>();

            GameObject light = new("Sun");
            Light sun = light.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.88f, 0.72f);
            sun.intensity = 0.78f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.62f;
            light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Low Poly Arena";
            ground.transform.localScale = new Vector3(2.4f, 1f, 2.4f);

            CreateBlock("North Wall", new Vector3(0f, 1.5f, 12f), new Vector3(24f, 3f, 0.5f));
            CreateBlock("South Wall", new Vector3(0f, 1.5f, -12f), new Vector3(24f, 3f, 0.5f));
            CreateBlock("East Wall", new Vector3(12f, 1.5f, 0f), new Vector3(0.5f, 3f, 24f));
            CreateBlock("West Wall", new Vector3(-12f, 1.5f, 0f), new Vector3(0.5f, 3f, 24f));
            CreateBlock("Divider Left", new Vector3(-6.5f, 1.5f, 2f), new Vector3(11f, 3f, 0.5f));
            CreateBlock("Divider Right", new Vector3(6.5f, 1.5f, 2f), new Vector3(11f, 3f, 0.5f));

            GameObject door = CreateBlock(
                "Network Door",
                new Vector3(0f, 1.5f, 2f),
                new Vector3(2f, 3f, 0.35f));
            door.AddComponent<NetworkObject>();
            door.AddComponent<NetworkDoor>();

            GameObject systems = new("Game Systems");
            NetworkManager networkManager = systems.AddComponent<NetworkManager>();
            UnityTransport transport = systems.AddComponent<UnityTransport>();
            systems.AddComponent<DirectNetworkBootstrap>();
            systems.AddComponent<GameStartHud>();
            systems.AddComponent<BoardroomEnvironment>();
            systems.AddComponent<MetaverseGame.Input.MobileInputRouter>();
            systems.AddComponent<MobileOrientationLock>();
            systems.AddComponent<MobileTouchControls>();
            networkManager.NetworkConfig.NetworkTransport = transport;
            networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
            networkManager.NetworkConfig.TickRate = 30;
            networkManager.NetworkConfig.EnableSceneManagement = true;
            networkManager.NetworkConfig.ForceSamePrefabs = true;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"Development scene created at {ScenePath}");
        }

        private static GameObject CreateNetworkPlayerPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existing == null)
            {
                GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Network Player";
                ConfigureNetworkPlayer(player);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
                Object.DestroyImmediate(player);
                return prefab;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            ConfigureNetworkPlayer(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        }

        private static void ConfigureNetworkPlayer(GameObject player)
        {
            player.name = "Network Player";
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.transform.localScale = Vector3.one;

            CapsuleCollider capsuleCollider = player.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                Object.DestroyImmediate(capsuleCollider, true);
            }

            MeshRenderer rootRenderer = player.GetComponent<MeshRenderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }

            GetOrAddComponent<CharacterController>(player);
            GetOrAddComponent<NetworkObject>(player);
            GetOrAddComponent<NetworkTransform>(player);
            GetOrAddComponent<NetworkPlayerController>(player);
            GetOrAddComponent<NetworkPlayerVisual>(player);
        }

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            return block;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void EnsureDirectory(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }
    }
}
