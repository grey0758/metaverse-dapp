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

        private readonly List<Material> runtimeMaterials = new();
        private readonly List<Texture2D> runtimeTextures = new();
        private readonly HashSet<int> styledPlayers = new();

        private Transform environmentRoot;
        private Shader surfaceShader;
        private Material stoneMaterial;
        private Material wallMaterial;
        private Material walnutMaterial;
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

            Texture2D stoneTexture = CreateStoneTexture();
            Texture2D woodTexture = CreateWoodTexture();
            Texture2D fabricTexture = CreateFabricTexture();
            Texture2D dashboardTexture = CreateDashboardTexture();

            stoneMaterial = CreateMaterial(
                "Graphite Stone",
                new Color(0.18f, 0.20f, 0.21f),
                0.18f,
                0.78f,
                stoneTexture,
                new Vector2(6f, 6f));
            wallMaterial = CreateMaterial(
                "Architectural Plaster",
                new Color(0.32f, 0.34f, 0.34f),
                0.02f,
                0.34f);
            walnutMaterial = CreateMaterial(
                "Smoked Walnut",
                new Color(0.38f, 0.20f, 0.10f),
                0.05f,
                0.67f,
                woodTexture,
                new Vector2(3f, 1f));
            metalMaterial = CreateMaterial(
                "Blackened Steel",
                new Color(0.055f, 0.065f, 0.07f),
                0.86f,
                0.82f);
            brassMaterial = CreateMaterial(
                "Brushed Brass",
                new Color(0.58f, 0.39f, 0.15f),
                0.78f,
                0.72f);
            leatherMaterial = CreateMaterial(
                "Deep Teal Leather",
                new Color(0.045f, 0.14f, 0.15f),
                0.08f,
                0.62f,
                fabricTexture,
                new Vector2(4f, 4f));
            glassMaterial = CreateMaterial(
                "Smoked Glass",
                new Color(0.16f, 0.38f, 0.42f, 0.32f),
                0.12f,
                0.96f,
                transparent: true);
            rugMaterial = CreateMaterial(
                "Boardroom Rug",
                new Color(0.10f, 0.16f, 0.17f),
                0.01f,
                0.24f,
                fabricTexture,
                new Vector2(9f, 7f));
            warmLightMaterial = CreateMaterial(
                "Warm Architectural Light",
                new Color(0.45f, 0.25f, 0.08f),
                0.05f,
                0.8f,
                emission: new Color(2.4f, 1.15f, 0.32f));
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
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.38f, 0.40f);
            RenderSettings.ambientEquatorColor = new Color(0.20f, 0.19f, 0.17f);
            RenderSettings.ambientGroundColor = new Color(0.075f, 0.08f, 0.075f);
            RenderSettings.ambientIntensity = 0.9f;

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.035f, 0.055f, 0.06f);
                camera.fieldOfView = 52f;
            }

            GameObject sunObject = GameObject.Find("Sun");
            Light sun = sunObject != null ? sunObject.GetComponent<Light>() : null;
            if (sun != null)
            {
                sun.color = new Color(1f, 0.88f, 0.72f);
                sun.intensity = 0.78f;
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.62f;
            }
        }

        private void ConfigureExistingRoom()
        {
            GameObject ground = ApplyMaterial("Low Poly Arena", stoneMaterial);
            if (ground != null)
            {
                ground.transform.localScale = new Vector3(3.6f, 1f, 3.6f);
            }

            ApplyMaterial("North Wall", wallMaterial);
            GameObject southWall = ApplyMaterial("South Wall", wallMaterial);
            Renderer southWallRenderer = southWall != null
                ? southWall.GetComponent<Renderer>()
                : null;
            if (southWallRenderer != null)
            {
                southWallRenderer.enabled = false;
            }
            ApplyMaterial("East Wall", wallMaterial);
            ApplyMaterial("West Wall", wallMaterial);

            GameObject dividerLeft = ApplyMaterial("Divider Left", glassMaterial);
            GameObject dividerRight = ApplyMaterial("Divider Right", glassMaterial);
            if (dividerLeft != null)
            {
                dividerLeft.transform.localScale = new Vector3(11f, 3f, 0.12f);
            }
            if (dividerRight != null)
            {
                dividerRight.transform.localScale = new Vector3(11f, 3f, 0.12f);
            }

            ApplyMaterial("Network Door", walnutMaterial);
        }

        private void BuildArchitecture()
        {
            CreateBox(
                "South Baseboard",
                new Vector3(0f, 0.13f, -11.69f),
                new Vector3(23.3f, 0.24f, 0.12f),
                brassMaterial);
            CreateBox(
                "North Baseboard",
                new Vector3(0f, 0.13f, 11.69f),
                new Vector3(23.3f, 0.24f, 0.12f),
                brassMaterial);
            CreateBox(
                "West Baseboard",
                new Vector3(-11.69f, 0.13f, 0f),
                new Vector3(0.12f, 0.24f, 23.3f),
                brassMaterial);
            CreateBox(
                "East Baseboard",
                new Vector3(11.69f, 0.13f, 0f),
                new Vector3(0.12f, 0.24f, 23.3f),
                brassMaterial);

            CreateBox(
                "Glass Frame Top",
                new Vector3(0f, 2.9f, 2f),
                new Vector3(23.5f, 0.16f, 0.18f),
                metalMaterial);
            CreateBox(
                "Glass Frame Bottom",
                new Vector3(0f, 0.1f, 2f),
                new Vector3(23.5f, 0.16f, 0.18f),
                metalMaterial);
            foreach (float x in new[] { -11.65f, -6.5f, -1.08f, 1.08f, 6.5f, 11.65f })
            {
                CreateBox(
                    "Glass Frame Post",
                    new Vector3(x, 1.5f, 2f),
                    new Vector3(0.12f, 2.85f, 0.18f),
                    metalMaterial);
            }

            CreateBox(
                "South Cutaway Wall",
                new Vector3(0f, 0.31f, -11.67f),
                new Vector3(23.3f, 0.5f, 0.12f),
                walnutMaterial);
            for (int index = 0; index < 21; index++)
            {
                float x = -9f + index * 0.9f;
                CreateBox(
                    "North Walnut Wall Slat",
                    new Vector3(x, 1.5f, 11.60f),
                    new Vector3(0.08f, 2.48f, 0.08f),
                    index % 5 == 0 ? brassMaterial : metalMaterial);
            }

            CreateBox(
                "West Acoustic Panel",
                new Vector3(-11.66f, 1.5f, -4.6f),
                new Vector3(0.08f, 2.5f, 8.2f),
                walnutMaterial);
            CreateBox(
                "East Acoustic Panel",
                new Vector3(11.66f, 1.5f, -4.6f),
                new Vector3(0.08f, 2.5f, 8.2f),
                walnutMaterial);

            CreateBox(
                "Strategy Display Frame",
                new Vector3(6.05f, 1.56f, 1.78f),
                new Vector3(6.45f, 2.16f, 0.12f),
                metalMaterial);
            CreateBox(
                "Strategy Display",
                new Vector3(6.05f, 1.56f, 1.69f),
                new Vector3(6.05f, 1.78f, 0.045f),
                displayMaterial);
            CreateBox(
                "Display Camera Bar",
                new Vector3(6.05f, 2.61f, 1.62f),
                new Vector3(1.1f, 0.09f, 0.09f),
                metalMaterial);
            CreateSphere(
                "Display Camera Lens",
                new Vector3(6.05f, 2.61f, 1.55f),
                new Vector3(0.08f, 0.08f, 0.05f),
                cyanLightMaterial);
        }

        private void BuildConferenceArea()
        {
            const float tableCenterZ = -4.5f;

            CreateBox(
                "Conference Rug",
                new Vector3(0f, 0.035f, tableCenterZ),
                new Vector3(9.2f, 0.055f, 10.2f),
                rugMaterial);
            CreateBox(
                "Table Brass Edge",
                new Vector3(0f, 0.91f, tableCenterZ),
                new Vector3(3.76f, 0.18f, 7.36f),
                brassMaterial,
                true);
            CreateBox(
                "Walnut Conference Table",
                new Vector3(0f, 0.98f, tableCenterZ),
                new Vector3(3.56f, 0.18f, 7.16f),
                walnutMaterial,
                true);
            CreateBox(
                "Table Center Inlay",
                new Vector3(0f, 1.08f, tableCenterZ),
                new Vector3(0.28f, 0.025f, 5.9f),
                metalMaterial);

            foreach (float z in new[] { -6.45f, -2.55f })
            {
                CreateBox(
                    "Table Pedestal",
                    new Vector3(0f, 0.47f, z),
                    new Vector3(1.3f, 0.78f, 0.9f),
                    metalMaterial);
            }

            foreach (float z in new[] { -6.65f, -4.5f, -2.35f })
            {
                CreateBox(
                    "Conference Control",
                    new Vector3(0f, 1.105f, z),
                    new Vector3(0.66f, 0.06f, 0.36f),
                    cyanLightMaterial);
            }

            foreach (float z in new[] { -7.2f, -5.4f, -3.6f, -1.8f })
            {
                CreateChair(new Vector3(-2.72f, 0f, z), 90f);
                CreateChair(new Vector3(2.72f, 0f, z), -90f);
            }
            CreateChair(new Vector3(0f, 0f, -9.25f), 0f);
            CreateChair(new Vector3(0f, 0f, 0.25f), 180f);

            CreateBox(
                "West Credenza",
                new Vector3(-10.55f, 0.55f, -5.2f),
                new Vector3(1.25f, 1.02f, 5.2f),
                walnutMaterial,
                true);
            CreateBox(
                "West Credenza Top",
                new Vector3(-10.55f, 1.09f, -5.2f),
                new Vector3(1.34f, 0.08f, 5.3f),
                brassMaterial);
            for (int index = 0; index < 4; index++)
            {
                CreateBox(
                    "Credenza Door",
                    new Vector3(-9.90f, 0.55f, -7.05f + index * 1.23f),
                    new Vector3(0.035f, 0.74f, 1.08f),
                    metalMaterial);
            }

            CreatePlant(new Vector3(-9.6f, 0f, -9.8f), 1.05f);
            CreatePlant(new Vector3(9.6f, 0f, -9.8f), 1.05f);
        }

        private void BuildArrivalLounge()
        {
            CreateBox(
                "Arrival Runner",
                new Vector3(0f, 0.035f, 7.1f),
                new Vector3(7.2f, 0.05f, 7.4f),
                rugMaterial);

            CreateLoungeSeat(new Vector3(-5.4f, 0f, 7.2f), 90f);
            CreateLoungeSeat(new Vector3(5.4f, 0f, 7.2f), -90f);

            CreateCylinder(
                "Arrival Medallion Rim",
                new Vector3(0f, 0.045f, 7.1f),
                new Vector3(1.75f, 0.035f, 1.75f),
                brassMaterial);
            CreateCylinder(
                "Arrival Medallion",
                new Vector3(0f, 0.075f, 7.1f),
                new Vector3(1.48f, 0.025f, 1.48f),
                stoneMaterial);
            CreateBox(
                "Medallion Mark Horizontal",
                new Vector3(0f, 0.11f, 7.1f),
                new Vector3(1.35f, 0.025f, 0.14f),
                cyanLightMaterial);
            CreateBox(
                "Medallion Mark Vertical",
                new Vector3(0f, 0.11f, 7.1f),
                new Vector3(0.14f, 0.025f, 1.35f),
                cyanLightMaterial);

            CreateBox(
                "North Console",
                new Vector3(0f, 0.62f, 10.75f),
                new Vector3(6.6f, 1.1f, 0.75f),
                walnutMaterial,
                true);
            CreateBox(
                "North Console Top",
                new Vector3(0f, 1.2f, 10.75f),
                new Vector3(6.8f, 0.08f, 0.88f),
                brassMaterial);
            CreateBox(
                "North Console Light",
                new Vector3(0f, 0.67f, 10.34f),
                new Vector3(4.8f, 0.08f, 0.035f),
                warmLightMaterial);

            CreatePlant(new Vector3(-9.5f, 0f, 9.7f), 0.92f);
            CreatePlant(new Vector3(9.5f, 0f, 9.7f), 0.92f);
        }

        private void BuildLightFixtures()
        {
            foreach (float x in new[] { -1.1f, 1.1f })
            {
                CreateBox(
                    "Linear Pendant",
                    new Vector3(x, 2.82f, -4.5f),
                    new Vector3(0.09f, 0.055f, 5.9f),
                    warmLightMaterial);
                CreateCylinder(
                    "Pendant Mount South",
                    new Vector3(x, 2.9f, -6.9f),
                    new Vector3(0.055f, 0.08f, 0.055f),
                    metalMaterial);
                CreateCylinder(
                    "Pendant Mount North",
                    new Vector3(x, 2.9f, -2.1f),
                    new Vector3(0.055f, 0.08f, 0.055f),
                    metalMaterial);
            }

            CreateBox(
                "Display Wash Light",
                new Vector3(6.05f, 2.76f, 1.58f),
                new Vector3(5.8f, 0.045f, 0.05f),
                warmLightMaterial);
            CreateBox(
                "Arrival Pendant",
                new Vector3(0f, 2.82f, 7.1f),
                new Vector3(4.5f, 0.055f, 0.09f),
                warmLightMaterial);

            CreatePointLight("Table Light South", new Vector3(0f, 2.6f, -6.3f), 7.2f, 2.2f);
            CreatePointLight("Table Light North", new Vector3(0f, 2.6f, -2.7f), 7.2f, 2.2f);
            CreatePointLight("Arrival Light", new Vector3(0f, 2.6f, 7.1f), 7.5f, 1.8f);
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
            light.color = new Color(1f, 0.75f, 0.46f);
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
            Texture2D texture = CreateTexture("Graphite Stone Texture", size, size, true);
            Color[] pixels = new Color[size * size];
            Color dark = new(0.095f, 0.11f, 0.115f);
            Color light = new(0.20f, 0.22f, 0.22f);
            Color grout = new(0.035f, 0.04f, 0.042f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool onGrout = x % 32 < 2 || y % 32 < 2;
                    float noise = Mathf.PerlinNoise(x * 0.075f, y * 0.075f);
                    float vein = Mathf.Abs(Mathf.Sin(x * 0.14f + y * 0.055f + noise * 4.2f));
                    Color color = onGrout
                        ? grout
                        : Color.Lerp(dark, light, noise * 0.72f);
                    if (!onGrout && vein > 0.965f)
                    {
                        color = Color.Lerp(color, new Color(0.34f, 0.35f, 0.34f), 0.42f);
                    }
                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(true, false);
            return texture;
        }

        private Texture2D CreateWoodTexture()
        {
            const int size = 128;
            Texture2D texture = CreateTexture("Smoked Walnut Texture", size, size, true);
            Color[] pixels = new Color[size * size];
            Color dark = new(0.12f, 0.045f, 0.018f);
            Color light = new(0.45f, 0.20f, 0.075f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float warp = Mathf.PerlinNoise(x * 0.025f, y * 0.095f) * 11f;
                    float grain = (Mathf.Sin((x + warp) * 0.28f) + 1f) * 0.5f;
                    float variation = Mathf.PerlinNoise(x * 0.055f, y * 0.035f);
                    pixels[y * size + x] = Color.Lerp(
                        dark,
                        light,
                        grain * 0.45f + variation * 0.38f);
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
