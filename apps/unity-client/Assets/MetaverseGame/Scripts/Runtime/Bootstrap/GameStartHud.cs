using MetaverseGame.Gameplay;
using Unity.Netcode;
using UnityEngine;

namespace MetaverseGame.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class GameStartHud : MonoBehaviour
    {
        private const float ReferenceWidth = 1280f;
        private const float ReferenceHeight = 720f;

        private static readonly Color PanelColor = new(0.025f, 0.04f, 0.075f, 0.94f);
        private static readonly Color PanelShadowColor = new(0f, 0f, 0f, 0.42f);
        private static readonly Color CyanColor = new(0.19f, 0.91f, 1f, 1f);
        private static readonly Color LiveColor = new(0.37f, 1f, 0.58f, 1f);
        private static readonly Color DuckColor = new(1f, 0.36f, 0.27f, 1f);
        private static readonly Color GooseColor = new(0.22f, 0.86f, 1f, 1f);
        private static readonly Color PendingColor = new(1f, 0.76f, 0.25f, 1f);

        private Texture2D pixel;
        private GUIStyle titleStyle;
        private GUIStyle statusStyle;
        private GUIStyle detailStyle;
        private GUIStyle liveStyle;
        private GUIStyle roleCaptionStyle;
        private GUIStyle roleStyle;
        private GUIStyle hintStyle;
        private GUIStyle markerStyle;
        private float startedAt;

        private void Awake()
        {
            startedAt = Time.realtimeSinceStartup;
            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Featherfall HUD Pixel",
                hideFlags = HideFlags.HideAndDontSave,
            };
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply(false, true);
        }

        private void OnGUI()
        {
#if UNITY_SERVER
            return;
#else
            if (pixel == null)
            {
                return;
            }

            EnsureStyles();

            float scale = Mathf.Clamp(
                Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight),
                0.65f,
                2.25f);
            Rect screenSafeArea = Screen.safeArea;
            Rect safeArea = new(
                screenSafeArea.x / scale,
                (Screen.height - screenSafeArea.yMax) / scale,
                screenSafeArea.width / scale,
                screenSafeArea.height / scale);

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            int previousDepth = GUI.depth;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.depth = -1000;

            try
            {
                NetworkManager manager = NetworkManager.Singleton;
                NetworkObject playerObject = manager?.LocalClient?.PlayerObject;
                NetworkPlayerController player = playerObject != null
                    ? playerObject.GetComponent<NetworkPlayerController>()
                    : null;
                string role = player?.PrivateRole ?? string.Empty;

                DrawRunStatus(safeArea, manager, playerObject);
                DrawRoleBadge(safeArea, role);
                DrawTouchHint(safeArea);

                if (playerObject == null)
                {
                    DrawSpawningNotice(safeArea);
                }
                else
                {
                    DrawPlayerMarker(playerObject.transform, role, scale);
                }
            }
            finally
            {
                GUI.matrix = previousMatrix;
                GUI.color = previousColor;
                GUI.depth = previousDepth;
            }
#endif
        }

        private void DrawRunStatus(
            Rect safeArea,
            NetworkManager manager,
            NetworkObject playerObject)
        {
            Rect panel = new(safeArea.x + 24f, safeArea.y + 24f, 440f, 174f);
            DrawPanel(panel);
            Fill(new Rect(panel.x, panel.y, 6f, panel.height), CyanColor);

            GUI.Label(
                new Rect(panel.x + 24f, panel.y + 15f, 280f, 38f),
                "FEATHERFALL",
                titleStyle);

            float heartbeat = 0.65f + (Mathf.Sin(Time.unscaledTime * 5f) + 1f) * 0.175f;
            Fill(
                new Rect(panel.xMax - 116f, panel.y + 24f, 10f, 10f),
                new Color(LiveColor.r, LiveColor.g, LiveColor.b, heartbeat));
            GUI.Label(
                new Rect(panel.xMax - 98f, panel.y + 16f, 78f, 28f),
                $"LIVE {FormatElapsed(Time.realtimeSinceStartup - startedAt)}",
                liveStyle);

            GUI.Label(
                new Rect(panel.x + 24f, panel.y + 58f, panel.width - 48f, 30f),
                "GAME IS RUNNING",
                statusStyle);
            GUI.Label(
                new Rect(panel.x + 24f, panel.y + 96f, panel.width - 48f, 24f),
                ResolveConnectionLabel(manager, playerObject),
                detailStyle);
            GUI.Label(
                new Rect(panel.x + 24f, panel.y + 128f, panel.width - 48f, 24f),
                "MOBILE INPUT  /  30 HZ  /  SERVER AUTHORITY",
                detailStyle);
        }

        private void DrawRoleBadge(Rect safeArea, string role)
        {
            Rect panel = new(safeArea.xMax - 264f, safeArea.y + 24f, 240f, 174f);
            DrawPanel(panel);

            string roleLabel = string.IsNullOrWhiteSpace(role)
                ? "ASSIGNING..."
                : role.ToUpperInvariant();
            Color roleColor = ResolveRoleColor(role);

            GUI.Label(
                new Rect(panel.x + 18f, panel.y + 14f, panel.width - 36f, 24f),
                "YOUR PRIVATE ROLE",
                roleCaptionStyle);
            Fill(
                new Rect(panel.x + 18f, panel.y + 48f, panel.width - 36f, 70f),
                new Color(roleColor.r, roleColor.g, roleColor.b, 0.22f));
            DrawBorder(
                new Rect(panel.x + 18f, panel.y + 48f, panel.width - 36f, 70f),
                roleColor,
                2f);

            Color previousColor = roleStyle.normal.textColor;
            roleStyle.normal.textColor = roleColor;
            GUI.Label(
                new Rect(panel.x + 18f, panel.y + 56f, panel.width - 36f, 52f),
                roleLabel,
                roleStyle);
            roleStyle.normal.textColor = previousColor;

            GUI.Label(
                new Rect(panel.x + 18f, panel.y + 132f, panel.width - 36f, 22f),
                "OWNER-ONLY  /  VISIBLE TO YOU",
                detailStyle);
        }

        private void DrawTouchHint(Rect safeArea)
        {
            const float panelWidth = 620f;
            Rect panel = new(
                safeArea.center.x - panelWidth * 0.5f,
                safeArea.yMax - 92f,
                panelWidth,
                68f);
            DrawPanel(panel);
            Fill(new Rect(panel.x, panel.y, panel.width, 4f), CyanColor);

            GUI.Label(
                new Rect(panel.x + 20f, panel.y + 13f, 150f, 22f),
                "TOUCH CONTROLS",
                hintStyle);
            GUI.Label(
                new Rect(panel.x + 20f, panel.y + 37f, 150f, 18f),
                "LANDSCAPE MOBILE",
                detailStyle);

            Fill(
                new Rect(panel.x + 176f, panel.y + 13f, 2f, 42f),
                new Color(1f, 1f, 1f, 0.16f));
            GUI.Label(
                new Rect(panel.x + 198f, panel.y + 12f, 190f, 24f),
                "LEFT JOYSTICK",
                hintStyle);
            GUI.Label(
                new Rect(panel.x + 198f, panel.y + 37f, 190f, 18f),
                "DRAG TO MOVE",
                detailStyle);

            Fill(
                new Rect(panel.x + 400f, panel.y + 13f, 2f, 42f),
                new Color(1f, 1f, 1f, 0.16f));
            GUI.Label(
                new Rect(panel.x + 422f, panel.y + 12f, 178f, 24f),
                "RIGHT ACTION",
                hintStyle);
            GUI.Label(
                new Rect(panel.x + 422f, panel.y + 37f, 178f, 18f),
                "USE NEAR A DOOR",
                detailStyle);
        }

        private void DrawSpawningNotice(Rect safeArea)
        {
            Rect notice = new(
                safeArea.center.x - 175f,
                safeArea.center.y - 30f,
                350f,
                60f);
            DrawPanel(notice);
            GUI.Label(notice, "SPAWNING YOUR PLAYER...", statusStyle);
        }

        private void DrawPlayerMarker(Transform player, string role, float scale)
        {
            Camera activeCamera = Camera.main;
            if (activeCamera == null)
            {
                return;
            }

            Vector3 screenPoint = activeCamera.WorldToScreenPoint(
                player.position + Vector3.up * 1.45f);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            float x = screenPoint.x / scale;
            float y = (Screen.height - screenPoint.y) / scale;
            Color roleColor = ResolveRoleColor(role);
            string roleLabel = string.IsNullOrWhiteSpace(role)
                ? "YOU"
                : $"YOU  /  {role.ToUpperInvariant()}";

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.08f;
            Rect reticle = new(
                x - 24f * pulse,
                y - 24f * pulse,
                48f * pulse,
                48f * pulse);
            DrawBorder(reticle, roleColor, 2f);
            Fill(new Rect(x - 2f, y - 2f, 4f, 4f), roleColor);

            Rect marker = new(x - 70f, y - 67f, 140f, 28f);
            Fill(marker, roleColor);
            GUI.Label(marker, roleLabel, markerStyle);
            Fill(new Rect(x - 3f, marker.yMax, 6f, 14f), roleColor);
        }

        private void DrawPanel(Rect rect)
        {
            Fill(new Rect(rect.x + 6f, rect.y + 7f, rect.width, rect.height), PanelShadowColor);
            Fill(rect, PanelColor);
            DrawBorder(rect, new Color(1f, 1f, 1f, 0.12f), 1f);
        }

        private void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, pixel);
            GUI.color = previous;
        }

        private void DrawBorder(Rect rect, Color color, float thickness)
        {
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = CreateStyle(28, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            statusStyle = CreateStyle(20, FontStyle.Bold, TextAnchor.MiddleLeft, LiveColor);
            detailStyle = CreateStyle(
                13,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Color(0.72f, 0.8f, 0.9f, 1f));
            liveStyle = CreateStyle(13, FontStyle.Bold, TextAnchor.MiddleLeft, LiveColor);
            roleCaptionStyle = CreateStyle(
                13,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.82f, 0.88f, 0.96f, 1f));
            roleStyle = CreateStyle(30, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            hintStyle = CreateStyle(15, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            markerStyle = CreateStyle(
                13,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.035f, 0.05f, 0.075f, 1f));
        }

        private static GUIStyle CreateStyle(
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color)
        {
            GUIStyle style = new(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = alignment,
                clipping = TextClipping.Clip,
                wordWrap = false,
            };
            style.normal.textColor = color;
            return style;
        }

        private static Color ResolveRoleColor(string role)
        {
            if (string.Equals(role, "duck", System.StringComparison.OrdinalIgnoreCase))
            {
                return DuckColor;
            }
            if (string.Equals(role, "goose", System.StringComparison.OrdinalIgnoreCase))
            {
                return GooseColor;
            }
            return PendingColor;
        }

        private static string ResolveConnectionLabel(
            NetworkManager manager,
            NetworkObject playerObject)
        {
            if (manager == null)
            {
                return "INITIALIZING NETWORK...";
            }
            if (!manager.IsListening)
            {
                return "STARTING LOCAL SESSION...";
            }
            if (manager.IsHost)
            {
                return playerObject == null
                    ? "LOCAL HOST ONLINE  /  SPAWNING PLAYER"
                    : "LOCAL HOST ONLINE  /  PLAYER READY";
            }
            if (manager.IsServer)
            {
                return "DEDICATED SERVER ONLINE";
            }
            if (manager.IsConnectedClient)
            {
                return playerObject == null
                    ? "CONNECTED  /  SPAWNING PLAYER"
                    : "CONNECTED  /  PLAYER READY";
            }
            if (manager.IsClient)
            {
                return "CONNECTING TO HOST...";
            }
            return "NETWORK READY";
        }

        private static string FormatElapsed(float elapsedSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }

        private void OnDestroy()
        {
            if (pixel != null)
            {
                Destroy(pixel);
            }
        }
    }
}
