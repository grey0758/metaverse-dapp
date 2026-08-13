using MetaverseGame.Bootstrap;
using MetaverseGame.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace MetaverseGame.Tests
{
    public sealed class BoardroomLayoutTests
    {
        [Test]
        public void PlatoRoomLayoutMatchesReferenceComposition()
        {
            Assert.That(BoardroomEnvironment.ConferenceTableCount, Is.EqualTo(3));
            Assert.That(BoardroomEnvironment.SeatsPerTableSide, Is.EqualTo(4));
            Assert.That(BoardroomEnvironment.WindowBayCount, Is.EqualTo(5));
            Assert.That(BoardroomEnvironment.AcousticSlatCount, Is.EqualTo(35));
            Assert.That(BoardroomEnvironment.RoomHalfExtent, Is.EqualTo(12f));
            Assert.That(
                BoardroomEnvironment.StrategyDisplayWidth /
                    BoardroomEnvironment.StrategyDisplayHeight,
                Is.EqualTo(16f / 9f).Within(0.0001f));
        }

        [Test]
        public void SpawnPositionsStayOutsideConferenceFurnitureAndInsideWalls()
        {
            for (int index = 0; index < 6; index++)
            {
                Vector3 position = NetworkPlayerController.ResolveSpawnPosition(index);

                bool outsideConferenceFurniture =
                    Mathf.Abs(position.x) >= 5.5f ||
                    position.z <= -8.55f ||
                    position.z >= 2.5f;
                Assert.That(outsideConferenceFurniture, Is.True);
                Assert.That(Mathf.Abs(position.x), Is.LessThan(11f));
                Assert.That(position.z, Is.InRange(-11f, 11f));
                Assert.That(position.y, Is.EqualTo(1f));
            }
        }
    }
}
