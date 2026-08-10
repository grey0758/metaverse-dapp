using MetaverseGame.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace MetaverseGame.Tests
{
    public sealed class MatchSessionRegistryTests
    {
        [Test]
        public void ApprovesAndRestoresReconnectSession()
        {
            GameObject gameObject = new("SessionRegistry");
            MatchSessionRegistry registry = gameObject.AddComponent<MatchSessionRegistry>();
            try
            {
                Assert.That(
                    registry.TryApproveConnection(
                        1,
                        "session-a",
                        "Alpha",
                        out SessionRecord first,
                        out string reason),
                    Is.True,
                    reason);
                Assert.That(first.Role, Is.EqualTo("duck"));
                Assert.That(first.SpawnIndex, Is.EqualTo(0));
                Assert.That(first.ConnectionCount, Is.EqualTo(1));

                registry.MarkDisconnected(1);

                Assert.That(
                    registry.TryApproveConnection(
                        2,
                        "session-a",
                        "Alpha Two",
                        out SessionRecord reconnect,
                        out reason),
                    Is.True,
                    reason);
                Assert.That(reconnect.Role, Is.EqualTo("duck"));
                Assert.That(reconnect.SpawnIndex, Is.EqualTo(first.SpawnIndex));
                Assert.That(reconnect.ConnectionCount, Is.EqualTo(2));
                Assert.That(reconnect.DisplayName, Is.EqualTo("Alpha Two"));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void RejectsDuplicateLiveSessionId()
        {
            GameObject gameObject = new("SessionRegistry");
            MatchSessionRegistry registry = gameObject.AddComponent<MatchSessionRegistry>();
            try
            {
                Assert.That(
                    registry.TryApproveConnection(
                        1,
                        "session-a",
                        "Alpha",
                        out _,
                        out string reason),
                    Is.True,
                    reason);

                Assert.That(
                    registry.TryApproveConnection(
                        2,
                        "session-a",
                        "Beta",
                        out _,
                        out reason),
                    Is.False,
                    reason);
                Assert.That(reason, Is.EqualTo("session_already_connected"));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void RejectsMissingSessionId()
        {
            GameObject gameObject = new("SessionRegistry");
            MatchSessionRegistry registry = gameObject.AddComponent<MatchSessionRegistry>();
            try
            {
                Assert.That(
                    registry.TryApproveConnection(
                        1,
                        " ",
                        "Alpha",
                        out _,
                        out string reason),
                    Is.False,
                    reason);
                Assert.That(reason, Is.EqualTo("session_id_required"));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
