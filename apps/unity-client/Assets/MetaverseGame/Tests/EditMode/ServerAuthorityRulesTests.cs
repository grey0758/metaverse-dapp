using MetaverseGame.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace MetaverseGame.Tests
{
    public sealed class ServerAuthorityRulesTests
    {
        [Test]
        public void RejectsRepeatedAndOlderInputSequences()
        {
            Assert.That(ServerAuthorityRules.IsNewerSequence(12, 11), Is.True);
            Assert.That(ServerAuthorityRules.IsNewerSequence(11, 11), Is.False);
            Assert.That(ServerAuthorityRules.IsNewerSequence(10, 11), Is.False);
        }

        [Test]
        public void ClampsMovementAndRejectsNonFiniteInput()
        {
            Vector2 clamped = ServerAuthorityRules.SanitizeMoveInput(new Vector2(3f, 4f));
            Assert.That(clamped.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(
                ServerAuthorityRules.SanitizeMoveInput(
                    new Vector2(float.PositiveInfinity, 0f)),
                Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ChecksInteractionDistanceOnTheServer()
        {
            Assert.That(
                ServerAuthorityRules.IsWithinInteractionDistance(
                    Vector3.zero,
                    new Vector3(1.5f, 0f, 0f),
                    2f),
                Is.True);
            Assert.That(
                ServerAuthorityRules.IsWithinInteractionDistance(
                    Vector3.zero,
                    new Vector3(2.1f, 0f, 0f),
                    2f),
                Is.False);
        }
    }
}
