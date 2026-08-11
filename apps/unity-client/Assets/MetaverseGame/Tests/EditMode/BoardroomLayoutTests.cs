using MetaverseGame.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace MetaverseGame.Tests
{
    public sealed class BoardroomLayoutTests
    {
        [Test]
        public void SpawnPositionsStayOutsideConferenceFurnitureAndInsideWalls()
        {
            for (int index = 0; index < 6; index++)
            {
                Vector3 position = NetworkPlayerController.ResolveSpawnPosition(index);

                bool outsideConferenceFurniture =
                    Mathf.Abs(position.x) >= 5.5f ||
                    position.z <= -10.25f ||
                    position.z >= 2.5f;
                Assert.That(outsideConferenceFurniture, Is.True);
                Assert.That(Mathf.Abs(position.x), Is.LessThan(11f));
                Assert.That(position.z, Is.InRange(-11f, 11f));
                Assert.That(position.y, Is.EqualTo(1f));
            }
        }
    }
}
