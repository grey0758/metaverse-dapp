using System.Collections.Generic;
using MetaverseGame.Gameplay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace MetaverseGame.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class BoardroomEnvironment : MonoBehaviour
    {
        private const string GeneratedRootName = "Boardroom Environment";
        private const string GraphiteStoneResource = "Boardroom/GraphiteStone";
        private const string SmokedWalnutResource = "Boardroom/SmokedWalnut";
        private const string StrategyDisplayResource = "Boardroom/StrategyDisplay";

        public const int ConferenceTableCount = 3;
        public const int SeatsPerTableSide = 4;
        public const int WindowBayCount = 5;
        public const int AcousticSlatCount = 35;
        public const float RoomHalfExtent = 12f;
        public const float StrategyDisplayWidth = 5f;
        public const float StrategyDisplayHeight = StrategyDisplayWidth * 9f / 16f;

        private readonly List<Material> runtimeMaterials = new();
        private readonly List<Texture2D> runtimeTextures = new();
        private readonly HashSet<int> styledPlayers = new();

        private Transform environmentRoot;
        private Shader surfaceShader;
        private Material stoneMaterial;
        private Material wallMaterial;
        private Material walnutMaterial;
        private Material slatMaterial;
        private Material acousticBlueMaterial;
        private Material ceilingMaterial;
        private Material windowGlassMaterial;
        private Material metalMaterial;
        private Material brassMaterial;
        private Material leatherMaterial;
        private Material glassMaterial;
        private Material rugMaterial;
        private Material warmLightMaterial;
        private Material cyanLightMaterial;
        private Material displayMaterial;
        private Material ceramicMaterial;
        private Material foliageMaterial;
        private Material visorMaterial;
        private Material[] avatarMaterials;
        private float nextPlayerStyleAt;

        private void Awake()
        {
            BuildEnvironment();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextPlayerStyleAt)
            {
                return;
            }

            nextPlayerStyleAt = Time.unscaledTime + 0.75f;
            StyleNetworkPlayers();
        }

        private void OnDestroy()
        {
            foreach (Material material in runtimeMaterials)
            {
                Release(material);
            }
            foreach (Texture2D texture in runtimeTextures)
            {
                Release(texture);
            }
        }

        private void BuildEnvironment()
        {
            Transform previous = transform.Find(GeneratedRootName);
            if (previous != null)
            {
                Release(previous.gameObject);
            }

            GameObject root = new(GeneratedRootName);
            environmentRoot = root.transform;
            environmentRoot.SetParent(transform, false);

            CreateMaterials();
            ConfigureSceneLighting();
            ConfigureExistingRoom();
            BuildArchitecture();
            BuildConferenceArea();
            BuildArrivalLounge();
            BuildLightFixtures();
            StyleNetworkPlayers();

            StaticBatchingUtility.Combine(root);
        }

        private void CreateMaterials()
        {
            bool usingScriptablePipeline = GraphicsSettings.currentRenderPipeline != null;
            surfaceShader = Shader.Find(
                usingScriptablePipeline
                    ? "Universal Render Pipeline/Lit"
                    : "Standard");
            surfaceShader ??= Shader.Find("Standard");
            surfaceShader ??= Shader.Find("Universal Render Pipeline/Lit");
            surfaceShader ??= Shader.Find("Diffuse");

            Texture2D stoneTexture = Resources.Load<Texture2D>(GraphiteStoneResource)
                ?? CreateStoneTexture();
            Texture2D woodTexture = Resources.Load<Texture2D>(SmokedWalnutResource)
                ?? CreateWoodTexture();
            Texture2D fabricTexture = CreateFabricTexture();
            Texture2D dashboardTexture = Resources.Load<Texture2D>(StrategyDisplayResource)
                ?? CreateDashboardTexture();

            stoneMaterial = CreateMaterial(
                "Plato Cool Gray Carpet",
                new Color(0.86f, 0.88f, 0.88f),
                0.0f,
                0.28f,
                stoneTexture,
                new Vector2(7f, 9f));
            wallMaterial = CreateMaterial(
                "Warm White Architectural Wall",
                new Color(0.82f, 0.84f, 0.83f),
                0.02f,
                0.24f);
            walnutMaterial = CreateMaterial(
                "Plato Light Honey Oak",
                new Color(0.94f, 0.79f, 0.56f),
                0.02f,
                0.48f,
                woodTexture,
                new Vector2(2.8f, 1.2f));
            slatMaterial = CreateMaterial(
                "Warm Oak Acoustic Slats",
                new Color(0.58f, 0.38f, 0.22f),
                0.02f,
                0.4f,
                woodTexture,
                new Vector2(0.55f, 3.8f));
            acousticBlueMaterial = CreateMaterial(
                "Cobalt Acoustic Felt",
                new Color(0.025f, 0.12f, 0.35f),
                0.0f,
                0.22f,
                fabricTexture,
                new Vector2(3f, 4f));
            ceilingMaterial = CreateMaterial(
                "Ceiling Tile",
                new Color(0.91f, 0.93f, 0.93f),
                0.0f,
                0.18f);
            windowGlassMaterial = CreateMaterial(
                "Cool Window Glass",
                new Color(0.34f, 0.58f, 0.68f, 0.34f),
                0.0f,
                0.72f,
                transparent: true);
            metalMaterial = CreateMaterial(
                "Window and Chair Graphite",
                new Color(0.025f, 0.032f, 0.04f),
                0.86f,
                0.82f);
            brassMaterial = CreateMaterial(
                "Soft Champagne Detail",
                new Color(0.58f, 0.42f, 0.22f),
                0.5f,
                0.58f);
            leatherMaterial = CreateMaterial(
                "Black Mesh Chair Fabric",
                new Color(0.035f, 0.045f, 0.055f),
                0.0f,
                0.34f,
                fabricTexture,
                new Vector2(7f, 7f));
            glassMaterial = CreateMaterial(
                "Smoked Glass",
                new Color(0.16f, 0.38f, 0.42f, 0.32f),
                0.12f,
                0.96f,
                transparent: true);
            rugMaterial = CreateMaterial(
                "Carpet Accent",
                new Color(0.68f, 0.72f, 0.74f),
                0.01f,
                0.2f,
                fabricTexture,
                new Vector2(9f, 11f));
            warmLightMaterial = CreateMaterial(
                "Ceiling Light Emission",
                new Color(0.92f, 0.9f, 0.78f),
                0.05f,
                0.8f,
                emission: new Color(1.8f, 1.65f, 1.2f));
            cyanLightMaterial = CreateMaterial(
                "Cyan Status Light",
                new Color(0.05f, 0.32f, 0.36f),
                0.08f,
                0.76f,
                emission: new Color(0.12f, 1.7f, 2.1f));
            displayMaterial = CreateMaterial(
                "Strategy Display",
                Color.white,
                0f,
                0.42f,
                dashboardTexture,
                Vector2.one,
                Color.white);
            ceramicMaterial = CreateMaterial(
                "Charcoal Ceramic",
                new Color(0.12f, 0.14f, 0.13f),
                0.04f,
                0.48f);
            foliageMaterial = CreateMaterial(
                "Foliage",
                new Color(0.16f, 0.34f, 0.21f),
                0f,
                0.3f);
            visorMaterial = CreateMaterial(
                "Avatar Visor",
                new Color(0.025f, 0.07f, 0.085f),
                0.35f,
                0.9f,
                emission: new Color(0.02f, 0.34f, 0.46f));
            avatarMaterials = new[]
            {
                CreateMaterial(
                    "Avatar Cyan",
                    new Color(0.10f, 0.62f, 0.72f),
                    0.15f,
                    0.68f),
                CreateMaterial(
                    "Avatar Coral",
                    new Color(0.82f, 0.24f, 0.18f),
                    0.12f,
                    0.66f),
                CreateMaterial(
                    "Avatar Gold",
                    new Color(0.88f, 0.56f, 0.16f),
                    0.15f,
                    0.7f),
                CreateMaterial(
                    "Avatar Green",
                    new Color(0.22f, 0.62f, 0.34f),
                    0.12f,
                    0.65f),
            };
        }

        private void ConfigureSceneLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.78f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.58f, 0.61f, 0.60f);
            RenderSettings.ambientGroundColor = new Color(0.34f, 0.36f, 0.35f);
            RenderSettings.ambientIntensity = 1.15f;

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.68f, 0.75f, 0.78f);
                camera.fieldOfView = 56f;
            }

            GameObject sunObject = GameObject.Find("Sun");
            Light sun = sunObject != null ? sunObject.GetComponent<Light>() : null;
            if (sun != null)
            {
                sun.color = new Color(1f, 0.96f, 0.88f);
                sun.intensity = 0.92f;
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.62f;
            }
        }

        private void ConfigureExistingRoom()
        {
            GameObject ground = ApplyMaterial("Low Poly Arena", stoneMaterial);
            if (ground != null)
            {
                ground.transform.localScale = new Vector3(2.45f, 1f, 2.45f);
            }

            GameObject northWall = ApplyMaterial("North Wall", acousticBlueMaterial);
            // Keep the original collider as the room boundary, but let the
            // authored front wall own the visible surface without overlap.
            Renderer northWallRenderer = northWall != null
                ? northWall.GetComponent<Renderer>()
                : null;
            if (northWallRenderer != null)
            {
                northWallRenderer.enabled = false;
            }
            GameObject southWall = ApplyMaterial("South Wall", wallMaterial);
            Renderer southWallRenderer = southWall != null
                ? southWall.GetComponent<Renderer>()
                : null;
            if (southWallRenderer != null)
            {
                southWallRenderer.enabled = false;
            }
            ApplyMaterial("East Wall", slatMaterial);
            GameObject westWall = ApplyMaterial("West Wall", wallMaterial);
            Renderer westWallRenderer = westWall != null
                ? westWall.GetComponent<Renderer>()
                : null;
            if (westWallRenderer != null)
            {
                westWallRenderer.enabled = false;
            }

            GameObject dividerLeft = ApplyMaterial("Divider Left", glassMaterial);
            GameObject dividerRight = ApplyMaterial("Divider Right", glassMaterial);
            foreach (GameObject divider in new[] { dividerLeft, dividerRight })
            {
                if (divider == null)
                {
                    continue;
                }

                Renderer renderer = divider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }

                Collider collider = divider.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }

            ApplyMaterial("Network Door", walnutMaterial);
        }

        private void BuildArchitecture()
        {
            CreateBox(
                "South Baseboard",
                new Vector3(0f, 0.1f, -11.69f),
                new Vector3(23.3f, 0.18f, 0.12f),
                slatMaterial);
            CreateBox(
                "North Blue Wall Base",
                new Vector3(0f, 0.2f, 11.69f),
                new Vector3(23.3f, 0.4f, 0.14f),
                acousticBlueMaterial);

            // Left-side window bays are intentionally open, like the reference room.
            CreateBox(
                "Window Header",
                new Vector3(-11.72f, 2.88f, 0f),
                new Vector3(0.16f, 0.18f, 23.3f),
                ceilingMaterial);
            CreateBox(
                "Window Sill",
                new Vector3(-11.72f, 0.18f, 0f),
                new Vector3(0.16f, 0.18f, 23.3f),
                wallMaterial);
            foreach (float z in new[] { -9.6f, -5.1f, -0.6f, 3.9f, 8.4f })
            {
                CreateBox(
                    "Window Mullion",
                    new Vector3(-11.7f, 1.52f, z),
                    new Vector3(0.18f, 2.7f, 0.12f),
                    metalMaterial);
                CreateBox(
                    "Window Glass",
                    new Vector3(-11.62f, 1.52f, z + 2.05f),
                    new Vector3(0.04f, 2.52f, 4.0f),
                    windowGlassMaterial);
            }

            CreateBox(
                "North Cobalt Acoustic Wall",
                new Vector3(0f, 1.52f, 11.66f),
                new Vector3(23.25f, 2.66f, 0.12f),
                acousticBlueMaterial);
            for (int index = 0; index < AcousticSlatCount; index++)
            {
                float x = -10.9f + index * 0.64f;
                CreateBox(
                    "East Acoustic Wall Slat",
                    new Vector3(11.62f, 1.5f, x),
                    new Vector3(0.12f, 2.55f, 0.085f),
                    slatMaterial);
            }

            CreateBox(
                "North Display Lower Wood Band",
                new Vector3(0f, 0.62f, 11.54f),
                new Vector3(6.4f, 0.72f, 0.12f),
                walnutMaterial);

            CreateBox(
                "Strategy Display Frame",
                new Vector3(0f, 1.5f, 11.48f),
                new Vector3(5.35f, 3f, 0.15f),
                metalMaterial);
            CreateBox(
                "Strategy Display",
                new Vector3(0f, 1.5f, 11.38f),
                new Vector3(StrategyDisplayWidth, StrategyDisplayHeight, 0.045f),
                displayMaterial);
            CreateBox(
                "Display Camera Bar",
                new Vector3(0f, 2.78f, 11.3f),
                new Vector3(1.1f, 0.09f, 0.09f),
                metalMaterial);
            CreateSphere(
                "Display Camera Lens",
                new Vector3(0f, 2.78f, 11.23f),
                new Vector3(0.08f, 0.08f, 0.05f),
                cyanLightMaterial);

            CreateCeilingGrid();
        }

        private void BuildConferenceArea()
        {
            const float tableWidth = 8.9f;
            const float tableDepth = 1.25f;
            float[] tableCenters = { -7.0f, -3.5f, 0f };

            CreateBox(
                "Conference Carpet Focus",
                new Vector3(0f, 0.035f, -3.5f),
                new Vector3(10.4f, 0.055f, 10.0f),
                rugMaterial);

            for (int row = 0; row < tableCenters.Length; row++)
            {
                float z = tableCenters[row];
                string rowName = $"Plato Table {row + 1}";

                CreateBox(
                    $"{rowName} Edge",
                    new Vector3(0f, 0.83f, z),
                    new Vector3(tableWidth + 0.16f, 0.16f, tableDepth + 0.16f),
                    metalMaterial,
                    true);
                CreateBox(
                    $"{rowName} Honey Oak Top",
                    new Vector3(0f, 0.94f, z),
                    new Vector3(tableWidth, 0.16f, tableDepth),
                    walnutMaterial,
                    true);
                CreateBox(
                    $"{rowName} Modesty Panel",
                    new Vector3(0f, 0.48f, z),
                    new Vector3(tableWidth - 0.32f, 0.62f, 0.09f),
                    metalMaterial,
                    true);
                foreach (float x in new[] { -3.72f, 3.72f })
                {
                    CreateBox(
                        $"{rowName} Leg",
                        new Vector3(x, 0.42f, z),
                        new Vector3(0.14f, 0.72f, 0.84f),
                        metalMaterial,
                        true);
                }

                CreateBox(
                    $"{rowName} Center Control",
                    new Vector3(0f, 1.045f, z),
                    new Vector3(0.52f, 0.045f, 0.22f),
                    cyanLightMaterial);

                for (int seat = 0; seat < SeatsPerTableSide; seat++)
                {
                    float x = -3.3f + seat * 2.2f;
                    CreateChair(new Vector3(x, 0f, z - 1.32f), 0f);
                    CreateChair(new Vector3(x, 0f, z + 1.32f), 180f);
                }
            }

            CreateBox(
                "Presenter Podium",
                new Vector3(-8.9f, 0.58f, 8.85f),
                new Vector3(1.15f, 1.05f, 0.78f),
                slatMaterial,
                true);
            CreateBox(
                "Presenter Podium Top",
                new Vector3(-8.9f, 1.14f, 8.85f),
                new Vector3(1.3f, 0.08f, 0.9f),
                walnutMaterial);
            CreateBox(
                "Presenter Podium Light",
                new Vector3(-8.9f, 0.76f, 8.42f),
                new Vector3(0.72f, 0.05f, 0.035f),
                cyanLightMaterial);
        }

        private void BuildArrivalLounge()
        {
            CreateBox(
                "Rear Entry Carpet",
                new Vector3(0f, 0.035f, 8.8f),
                new Vector3(10.7f, 0.05f, 5.4f),
                rugMaterial);
            CreateBox(
                "Rear Sideboard",
                new Vector3(8.8f, 0.52f, 9.45f),
                new Vector3(1.35f, 0.92f, 2.3f),
                slatMaterial,
                true);
            CreateBox(
                "Rear Sideboard Top",
                new Vector3(8.8f, 1.03f, 9.45f),
                new Vector3(1.45f, 0.08f, 2.4f),
                walnutMaterial);
            CreatePlant(new Vector3(-10.0f, 0f, 9.6f), 0.85f);
            CreatePlant(new Vector3(10.0f, 0f, 9.6f), 0.85f);
        }

        private void BuildLightFixtures()
        {
            foreach (float z in new[] { -7.0f, -3.5f, 0f })
            {
                CreateBox(
                    "Linear Ceiling Light",
                    new Vector3(0f, 2.88f, z),
                    new Vector3(7.2f, 0.055f, 0.12f),
                    warmLightMaterial);
            }

            CreateBox(
                "Display Wash Light",
                new Vector3(0f, 2.72f, 10.9f),
                new Vector3(7.2f, 0.045f, 0.05f),
                warmLightMaterial);
            CreateBox(
                "Rear Ceiling Light",
                new Vector3(0f, 2.88f, 7.8f),
                new Vector3(7.2f, 0.055f, 0.12f),
                warmLightMaterial);

            CreatePointLight("Table Light South", new Vector3(0f, 2.6f, -7.0f), 6.5f, 1.6f);
            CreatePointLight("Table Light Center", new Vector3(0f, 2.6f, -3.5f), 6.5f, 1.6f);
            CreatePointLight("Table Light North", new Vector3(0f, 2.6f, 0f), 6.5f, 1.6f);
            CreatePointLight("Rear Light", new Vector3(0f, 2.6f, 7.8f), 6.5f, 1.4f);
        }

        private void CreateCeilingGrid()
        {
            for (int index = 0; index < 8; index++)
            {
                float z = -10.5f + index * 3f;
                CreateBox(
                    "Ceiling Tile Crossbar",
                    new Vector3(0f, 2.98f, z),
                    new Vector3(23.3f, 0.06f, 0.06f),
                    ceilingMaterial);
            }

            for (int index = 0; index < 6; index++)
            {
                float x = -9f + index * 3.6f;
                CreateBox(
                    "Ceiling Tile Rail",
                    new Vector3(x, 2.98f, 0f),
                    new Vector3(0.06f, 0.06f, 23.3f),
                    ceilingMaterial);
            }
        }

        private void CreateChair(Vector3 position, float yaw)
        {
            GameObject chair = new("Executive Chair");
            chair.transform.SetParent(environmentRoot, false);
            chair.transform.localPosition = position;
            chair.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            CreateBox(
                "Chair Seat",
                new Vector3(0f, 0.56f, 0f),
                new Vector3(0.96f, 0.18f, 0.92f),
                leatherMaterial,
                true,
                chair.transform);
            CreateBox(
                "Chair Back",
                new Vector3(0f, 1.08f, -0.43f),
                new Vector3(0.98f, 1.08f, 0.15f),
                leatherMaterial,
                parent: chair.transform);
            CreateBox(
                "Chair Back Accent",
                new Vector3(0f, 1.08f, -0.515f),
                new Vector3(0.62f, 0.72f, 0.025f),
                brassMaterial,
                parent: chair.transform);
            CreateCylinder(
                "Chair Pedestal",
                new Vector3(0f, 0.28f, 0f),
                new Vector3(0.14f, 0.27f, 0.14f),
                metalMaterial,
                parent: chair.transform);
            CreateCylinder(
                "Chair Base",
                new Vector3(0f, 0.045f, 0f),
                new Vector3(0.48f, 0.035f, 0.48f),
                metalMaterial,
                parent: chair.transform);
            foreach (float x in new[] { -0.53f, 0.53f })
            {
                CreateBox(
                    "Chair Arm",
                    new Vector3(x, 0.78f, 0f),
                    new Vector3(0.07f, 0.1f, 0.72f),
                    metalMaterial,
                    parent: chair.transform);
            }
        }

        private void CreateLoungeSeat(Vector3 position, float yaw)
        {
            GameObject seat = new("Lounge Seat");
            seat.transform.SetParent(environmentRoot, false);
            seat.transform.localPosition = position;
            seat.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            CreateBox(
                "Lounge Base",
                new Vector3(0f, 0.34f, 0f),
                new Vector3(2.4f, 0.58f, 1.1f),
                leatherMaterial,
                true,
                seat.transform);
            CreateBox(
                "Lounge Back",
                new Vector3(0f, 0.85f, -0.47f),
                new Vector3(2.4f, 0.92f, 0.16f),
                leatherMaterial,
                parent: seat.transform);
            CreateBox(
                "Lounge Trim",
                new Vector3(0f, 0.08f, 0f),
                new Vector3(2.48f, 0.09f, 1.18f),
                brassMaterial,
                parent: seat.transform);
        }

        private void CreatePlant(Vector3 position, float scale)
        {
            GameObject plant = new("Architectural Plant");
            plant.transform.SetParent(environmentRoot, false);
            plant.transform.localPosition = position;
            plant.transform.localScale = Vector3.one * scale;

            CreateCylinder(
                "Ceramic Planter",
                new Vector3(0f, 0.34f, 0f),
                new Vector3(0.48f, 0.34f, 0.48f),
                ceramicMaterial,
                parent: plant.transform);
            CreateCylinder(
                "Planter Rim",
                new Vector3(0f, 0.68f, 0f),
                new Vector3(0.52f, 0.055f, 0.52f),
                brassMaterial,
                parent: plant.transform);

            Vector3[] leafPositions =
            {
                new(-0.22f, 1.18f, 0f),
                new(0.22f, 1.22f, 0.08f),
                new(0f, 1.46f, -0.12f),
                new(-0.08f, 1.65f, 0.12f),
                new(0.18f, 1.72f, -0.02f),
            };
            float[] leafAngles = { -22f, 24f, -9f, 14f, -16f };
            for (int index = 0; index < leafPositions.Length; index++)
            {
                CreateCapsule(
                    "Sculpted Leaf",
                    leafPositions[index],
                    new Vector3(0.16f, 0.52f, 0.16f),
                    Quaternion.Euler(0f, index * 67f, leafAngles[index]),
                    foliageMaterial,
                    plant.transform);
            }
        }

        private void CreatePointLight(string name, Vector3 position, float range, float intensity)
        {
            GameObject lightObject = new(name);
            lightObject.transform.SetParent(environmentRoot, false);
            lightObject.transform.localPosition = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.91f, 0.78f);
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.Auto;
        }

        private void StyleNetworkPlayers()
        {
            if (avatarMaterials == null || avatarMaterials.Length == 0)
            {
                return;
            }

            NetworkPlayerController[] players = FindObjectsByType<NetworkPlayerController>(
                FindObjectsSortMode.None);
            foreach (NetworkPlayerController player in players)
            {
                int instanceId = player.gameObject.GetInstanceID();
                if (!styledPlayers.Add(instanceId))
                {
                    continue;
                }

                NetworkObject networkObject = player.GetComponent<NetworkObject>();
                int paletteIndex = networkObject != null
                    ? (int)(networkObject.OwnerClientId % (ulong)avatarMaterials.Length)
                    : 0;
                Renderer renderer = player.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = avatarMaterials[paletteIndex];
                }

                if (player.transform.Find("Avatar Visor") == null)
                {
                    CreateSphere(
                        "Avatar Visor",
                        new Vector3(0f, 0.34f, 0.44f),
                        new Vector3(0.58f, 0.23f, 0.10f),
                        visorMaterial,
                        parent: player.transform);
                    CreateCylinder(
                        "Avatar Ground Ring",
                        new Vector3(0f, -0.975f, 0f),
                        new Vector3(0.72f, 0.025f, 0.72f),
                        cyanLightMaterial,
                        parent: player.transform);
                }
            }
        }

        private GameObject ApplyMaterial(string objectName, Material material)
        {
            GameObject target = GameObject.Find(objectName);
            Renderer renderer = target != null ? target.GetComponent<Renderer>() : null;
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
            return target;
        }

        private Material CreateMaterial(
            string name,
            Color color,
            float metallic,
            float smoothness,
            Texture2D texture = null,
            Vector2? textureScale = null,
            Color? emission = null,
            bool transparent = false)
        {
            Material material = new(surfaceShader)
            {
                name = name,
                hideFlags = HideFlags.DontSave,
            };
            runtimeMaterials.Add(material);

            SetColor(material, "_Color", color);
            SetColor(material, "_BaseColor", color);
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Glossiness", smoothness);
            SetFloat(material, "_Smoothness", smoothness);

            if (texture != null)
            {
                SetTexture(material, "_MainTex", texture);
                SetTexture(material, "_BaseMap", texture);
                Vector2 scale = textureScale ?? Vector2.one;
                SetTextureScale(material, "_MainTex", scale);
                SetTextureScale(material, "_BaseMap", scale);
            }

            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                SetColor(material, "_EmissionColor", emission.Value);
                if (texture != null)
                {
                    SetTexture(material, "_EmissionMap", texture);
                }
            }

            if (transparent)
            {
                material.SetOverrideTag("RenderType", "Transparent");
                SetFloat(material, "_Surface", 1f);
                SetFloat(material, "_Mode", 3f);
                SetFloat(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
                SetFloat(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                SetFloat(material, "_ZWrite", 0f);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)RenderQueue.Transparent;
            }

            return material;
        }

        private GameObject CreateBox(
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool withCollider = false,
            Transform parent = null)
        {
            return CreatePrimitive(
                PrimitiveType.Cube,
                name,
                position,
                scale,
                Quaternion.identity,
                material,
                withCollider,
                parent);
        }

        private GameObject CreateCylinder(
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool withCollider = false,
            Transform parent = null)
        {
            return CreatePrimitive(
                PrimitiveType.Cylinder,
                name,
                position,
                scale,
                Quaternion.identity,
                material,
                withCollider,
                parent);
        }

        private GameObject CreateSphere(
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool withCollider = false,
            Transform parent = null)
        {
            return CreatePrimitive(
                PrimitiveType.Sphere,
                name,
                position,
                scale,
                Quaternion.identity,
                material,
                withCollider,
                parent);
        }

        private GameObject CreateCapsule(
            string name,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            Material material,
            Transform parent = null)
        {
            return CreatePrimitive(
                PrimitiveType.Capsule,
                name,
                position,
                scale,
                rotation,
                material,
                false,
                parent);
        }

        private GameObject CreatePrimitive(
            PrimitiveType primitiveType,
            string name,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            Material material,
            bool withCollider,
            Transform parent)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent != null ? parent : environmentRoot, false);
            primitive.transform.localPosition = position;
            primitive.transform.localRotation = rotation;
            primitive.transform.localScale = scale;

            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            Collider primitiveCollider = primitive.GetComponent<Collider>();
            if (primitiveCollider != null && !withCollider)
            {
                primitiveCollider.enabled = false;
                Release(primitiveCollider);
            }

            return primitive;
        }

        private Texture2D CreateStoneTexture()
        {
            const int size = 128;
            Texture2D texture = CreateTexture("Pale Carpet Texture", size, size, true);
            Color[] pixels = new Color[size * size];
            Color low = new(0.56f, 0.59f, 0.60f);
            Color high = new(0.76f, 0.78f, 0.78f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float weave = Mathf.PerlinNoise(x * 0.09f, y * 0.19f);
                    float heather = Mathf.Sin(y * 0.17f + weave * 2.2f) * 0.05f;
                    float value = Mathf.Clamp01(weave * 0.62f + 0.24f + heather);
                    pixels[y * size + x] = Color.Lerp(low, high, value);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private Texture2D CreateWoodTexture()
        {
            const int size = 128;
            Texture2D texture = CreateTexture("Light Honey Oak Texture", size, size, true);
            Color[] pixels = new Color[size * size];
            Color dark = new(0.30f, 0.14f, 0.045f);
            Color light = new(0.78f, 0.48f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float warp = Mathf.PerlinNoise(x * 0.035f, y * 0.08f) * 10f;
                    float grain = (Mathf.Sin((y + warp) * 0.28f) + 1f) * 0.5f;
                    float variation = Mathf.PerlinNoise(x * 0.055f, y * 0.035f);
                    pixels[y * size + x] = Color.Lerp(
                        dark,
                        light,
                        grain * 0.38f + variation * 0.42f + 0.08f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private Texture2D CreateFabricTexture()
        {
            const int size = 64;
            Texture2D texture = CreateTexture("Fine Weave Texture", size, size, true);
            Color[] pixels = new Color[size * size];
            Color low = new(0.38f, 0.42f, 0.41f);
            Color high = new(0.62f, 0.66f, 0.64f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float weave = ((x + y) & 1) == 0 ? 0.42f : 0.58f;
                    float noise = Mathf.PerlinNoise(x * 0.35f, y * 0.35f) * 0.16f;
                    pixels[y * size + x] = Color.Lerp(low, high, weave + noise);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private Texture2D CreateDashboardTexture()
        {
            const int width = 256;
            const int height = 144;
            Texture2D texture = CreateTexture("Boardroom Strategy Dashboard", width, height, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[width * height];
            Color background = new(0.018f, 0.035f, 0.045f);

            for (int y = 0; y < height; y++)
            {
                float vertical = y / (float)(height - 1);
                Color rowColor = Color.Lerp(background, new Color(0.035f, 0.075f, 0.085f), vertical);
                for (int x = 0; x < width; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.025f;
                    pixels[y * width + x] = rowColor + new Color(noise, noise, noise, 0f);
                }
            }

            Color cyan = new(0.10f, 0.92f, 1f);
            Color gold = new(1f, 0.63f, 0.18f);
            Color green = new(0.32f, 0.92f, 0.55f);
            Color panel = new(0.045f, 0.10f, 0.12f);
            Color grid = new(0.10f, 0.23f, 0.25f);

            FillRect(pixels, width, height, 0, 136, width, 8, cyan);
            FillRect(pixels, width, height, 9, 101, 72, 26, panel);
            FillRect(pixels, width, height, 88, 101, 72, 26, panel);
            FillRect(pixels, width, height, 167, 101, 80, 26, panel);
            FillRect(pixels, width, height, 9, 12, 151, 80, panel);
            FillRect(pixels, width, height, 167, 12, 80, 80, panel);

            FillRect(pixels, width, height, 16, 108, 50, 5, cyan);
            FillRect(pixels, width, height, 95, 108, 42, 5, gold);
            FillRect(pixels, width, height, 174, 108, 58, 5, green);
            FillRect(pixels, width, height, 16, 118, 29, 3, grid);
            FillRect(pixels, width, height, 95, 118, 33, 3, grid);
            FillRect(pixels, width, height, 174, 118, 40, 3, grid);

            for (int index = 0; index < 5; index++)
            {
                int y = 22 + index * 14;
                FillRect(pixels, width, height, 18, y, 132, 1, grid);
            }
            for (int index = 0; index < 7; index++)
            {
                int x = 20 + index * 21;
                FillRect(pixels, width, height, x, 18, 1, 69, grid);
            }

            Vector2Int[] cyanLine =
            {
                new(18, 29), new(39, 38), new(60, 33), new(81, 55),
                new(102, 48), new(123, 72), new(149, 77),
            };
            Vector2Int[] goldLine =
            {
                new(18, 23), new(39, 27), new(60, 44), new(81, 39),
                new(102, 57), new(123, 54), new(149, 68),
            };
            DrawPolyline(pixels, width, height, cyanLine, cyan, 2);
            DrawPolyline(pixels, width, height, goldLine, gold, 2);

            int[] bars = { 24, 47, 33, 61, 53 };
            for (int index = 0; index < bars.Length; index++)
            {
                Color color = index % 2 == 0 ? cyan : gold;
                FillRect(
                    pixels,
                    width,
                    height,
                    178 + index * 12,
                    21,
                    7,
                    bars[index],
                    color);
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private Texture2D CreateTexture(string name, int width, int height, bool mipChain)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, mipChain)
            {
                name = name,
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 2,
            };
            runtimeTextures.Add(texture);
            return texture;
        }

        private static void FillRect(
            Color[] pixels,
            int width,
            int height,
            int x,
            int y,
            int rectWidth,
            int rectHeight,
            Color color)
        {
            int minX = Mathf.Clamp(x, 0, width);
            int minY = Mathf.Clamp(y, 0, height);
            int maxX = Mathf.Clamp(x + rectWidth, 0, width);
            int maxY = Mathf.Clamp(y + rectHeight, 0, height);
            for (int row = minY; row < maxY; row++)
            {
                for (int column = minX; column < maxX; column++)
                {
                    pixels[row * width + column] = color;
                }
            }
        }

        private static void DrawPolyline(
            Color[] pixels,
            int width,
            int height,
            IReadOnlyList<Vector2Int> points,
            Color color,
            int thickness)
        {
            for (int index = 1; index < points.Count; index++)
            {
                DrawLine(
                    pixels,
                    width,
                    height,
                    points[index - 1],
                    points[index],
                    color,
                    thickness);
            }
        }

        private static void DrawLine(
            Color[] pixels,
            int width,
            int height,
            Vector2Int from,
            Vector2Int to,
            Color color,
            int thickness)
        {
            int deltaX = Mathf.Abs(to.x - from.x);
            int stepX = from.x < to.x ? 1 : -1;
            int deltaY = -Mathf.Abs(to.y - from.y);
            int stepY = from.y < to.y ? 1 : -1;
            int error = deltaX + deltaY;
            int x = from.x;
            int y = from.y;

            while (true)
            {
                FillRect(
                    pixels,
                    width,
                    height,
                    x - thickness / 2,
                    y - thickness / 2,
                    thickness,
                    thickness,
                    color);
                if (x == to.x && y == to.y)
                {
                    break;
                }

                int doubledError = error * 2;
                if (doubledError >= deltaY)
                {
                    error += deltaY;
                    x += stepX;
                }
                if (doubledError <= deltaX)
                {
                    error += deltaX;
                    y += stepY;
                }
            }
        }

        private static void SetColor(Material material, string property, Color value)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, value);
            }
        }

        private static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static void SetTexture(Material material, string property, Texture value)
        {
            if (material.HasProperty(property))
            {
                material.SetTexture(property, value);
            }
        }

        private static void SetTextureScale(Material material, string property, Vector2 value)
        {
            if (material.HasProperty(property))
            {
                material.SetTextureScale(property, value);
            }
        }

        private static void Release(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
