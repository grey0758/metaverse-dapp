using System;
using MetaverseGame.Gameplay;
using NUnit.Framework;

namespace MetaverseGame.Tests
{
    public sealed class RoomCodeTests
    {
        [Test]
        public void NormalizesAValidRoomCode()
        {
            Assert.That(RoomCode.Normalize(" duck42 "), Is.EqualTo("DUCK42"));
        }

        [Test]
        public void RejectsPunctuation()
        {
            Assert.Throws<ArgumentException>(() => RoomCode.Normalize("duck-42"));
        }
    }
}
