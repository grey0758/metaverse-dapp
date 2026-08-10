using System.Collections.Generic;
using UnityEngine;

namespace MetaverseGame.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class MatchSessionRegistry : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 12;

        private readonly Dictionary<string, SessionRecord> sessionsById = new();
        private readonly Dictionary<ulong, string> sessionIdByClientId = new();

        public static MatchSessionRegistry Instance { get; private set; }

        public int Capacity => capacity;
        public int ConnectedSessionCount
        {
            get
            {
                int connected = 0;
                foreach (SessionRecord record in sessionsById.Values)
                {
                    if (record.Connected)
                    {
                        connected += 1;
                    }
                }
                return connected;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ResetForNewMatch()
        {
            sessionsById.Clear();
            sessionIdByClientId.Clear();
        }

        public bool TryApproveConnection(
            ulong clientId,
            string sessionId,
            string displayName,
            out SessionRecord session,
            out string rejectionReason)
        {
            session = null;
            rejectionReason = string.Empty;

            string normalizedSessionId = NormalizeSessionId(sessionId);
            if (string.IsNullOrWhiteSpace(normalizedSessionId))
            {
                rejectionReason = "session_id_required";
                return false;
            }

            string normalizedDisplayName = NormalizeDisplayName(displayName);
            if (sessionsById.TryGetValue(normalizedSessionId, out session))
            {
                if (session.Connected && session.ClientId != clientId)
                {
                    rejectionReason = "session_already_connected";
                    return false;
                }
            }
            else
            {
                if (ConnectedSessionCount >= capacity)
                {
                    rejectionReason = "room_full";
                    return false;
                }

                session = new SessionRecord(
                    normalizedSessionId,
                    sessionsById.Count,
                    ResolveRole(sessionsById.Count),
                    normalizedDisplayName);
                sessionsById.Add(normalizedSessionId, session);
            }

            session.MarkConnected(clientId, normalizedDisplayName);
            sessionIdByClientId[clientId] = normalizedSessionId;
            return true;
        }

        public bool TryGetSession(ulong clientId, out SessionRecord session)
        {
            session = null;
            if (!sessionIdByClientId.TryGetValue(clientId, out string sessionId))
            {
                return false;
            }
            return sessionsById.TryGetValue(sessionId, out session);
        }

        public void MarkDisconnected(ulong clientId)
        {
            if (!sessionIdByClientId.TryGetValue(clientId, out string sessionId))
            {
                return;
            }

            sessionIdByClientId.Remove(clientId);
            if (sessionsById.TryGetValue(sessionId, out SessionRecord session))
            {
                session.MarkDisconnected();
            }
        }

        public static string ResolveRole(int sessionIndex)
        {
            return sessionIndex == 0 ? "duck" : "goose";
        }

        private static string NormalizeSessionId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string NormalizeDisplayName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Unity Guest" : value.Trim();
        }
    }

    public sealed class SessionRecord
    {
        public SessionRecord(
            string sessionId,
            int spawnIndex,
            string role,
            string displayName)
        {
            SessionId = sessionId;
            SpawnIndex = spawnIndex;
            Role = role;
            DisplayName = displayName;
        }

        public string SessionId { get; }
        public int SpawnIndex { get; }
        public string Role { get; }
        public string DisplayName { get; private set; }
        public ulong ClientId { get; private set; }
        public bool Connected { get; private set; }
        public int ConnectionCount { get; private set; }

        public void MarkConnected(ulong clientId, string displayName)
        {
            ClientId = clientId;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? DisplayName
                : displayName.Trim();
            Connected = true;
            ConnectionCount += 1;
        }

        public void MarkDisconnected()
        {
            Connected = false;
            ClientId = 0;
        }
    }
}
