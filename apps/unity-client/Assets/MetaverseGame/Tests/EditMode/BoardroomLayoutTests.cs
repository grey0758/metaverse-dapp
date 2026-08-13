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
            Assert.That(BoardroomEnvironment.ConferenceTableCount, Is.EqualTo(4));
            Assert.That(BoardroomEnvironment.SeatsPerTableSide, Is.EqualTo(8));
            Assert.That(BoardroomEnvironment.WindowBayCount, Is.EqualTo(5));
            Assert.That(BoardroomEnvironment.AcousticSlatCount, Is.EqualTo(72));
            Assert.That(BoardroomEnvironment.RoomHalfWidth, Is.EqualTo(7.6f));
            Assert.That(BoardroomEnvironment.RoomHalfLength, Is.EqualTo(12f));
            Assert.That(BoardroomEnvironment.RoomCeilingHeight, Is.EqualTo(4f));
            Assert.That(BoardroomEnvironment.ConferenceTableWidth, Is.EqualTo(11.1f));
            Assert.That(BoardroomEnvironment.ConferenceTableDepth, Is.EqualTo(0.92f));
            Assert.That(BoardroomEnvironment.ConferenceTableHeight, Is.EqualTo(0.79f));
            Assert.That(BoardroomEnvironment.ConferenceChairWidth, Is.EqualTo(0.72f));
            Assert.That(
                BoardroomEnvironment.StrategyDisplayWidth /
                    BoardroomEnvironment.StrategyDisplayHeight,
                Is.EqualTo(16f / 9f).Within(0.0001f));
            Assert.That(
                BoardroomEnvironment.StrategyDisplayTextureScale,
                Is.EqualTo(new Vector2(-1f, -1f)));
            Assert.That(
                BoardroomEnvironment.StrategyDisplayTextureOffset,
                Is.EqualTo(Vector2.one));
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
                Assert.That(
                    Mathf.Abs(position.x),
                    Is.LessThan(BoardroomEnvironment.RoomHalfWidth - 0.5f));
                Assert.That(position.z, Is.InRange(-11f, 11f));
                Assert.That(position.y, Is.EqualTo(1f));
            }
        }

        [Test]
        public void DefaultSpawnLeavesRoomForTheLockedCameraInsideTheSouthWall()
        {
            Vector3 spawn = NetworkPlayerController.ResolveSpawnPosition(0);
            Vector3 cameraPosition = FollowLocalPlayer.CalculateOrbitPosition(
                spawn,
                FollowLocalPlayer.DefaultLockedYawOffset,
                FollowLocalPlayer.DefaultPitch,
                FollowLocalPlayer.DefaultDistance);

            Assert.That(spawn.x, Is.LessThan(-5.5f));
            Assert.That(cameraPosition.x, Is.GreaterThan(spawn.x));
            Assert.That(cameraPosition.x, Is.LessThan(-5f));
            Assert.That(
                Mathf.Abs(cameraPosition.x),
                Is.LessThan(BoardroomEnvironment.RoomHalfWidth - 0.5f));
            Assert.That(
                cameraPosition.z,
                Is.GreaterThan(-BoardroomEnvironment.RoomHalfLength + 0.5f));
        }
    }
}
