using System;

namespace MetaverseGame.Networking
{
    [Serializable]
    public sealed class ClientMessage
    {
        public int v = 1;
        public string type;
        public string requestId;
        public string name;
        public string roomCode;
        public bool ready;
        public int sequence;
        public float x;
        public float z;
        public long sentAt;
    }

    [Serializable]
    public sealed class ServerMessage
    {
        public int v;
        public string type;
        public string requestId;
        public string playerId;
        public string roomCode;
        public string phase;
        public string role;
        public string code;
        public string message;
        public int tick;
        public long sentAt;
        public long serverAt;
        public PublicPlayerState[] players;
    }

    [Serializable]
    public sealed class PublicPlayerState
    {
        public string id;
        public string name;
        public bool ready;
        public float x;
        public float z;
    }
}
