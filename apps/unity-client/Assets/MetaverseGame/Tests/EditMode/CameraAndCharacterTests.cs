using MetaverseGame.Bootstrap;
using MetaverseGame.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MetaverseGame.Tests
{
    public sealed class CameraAndCharacterTests
    {
        [Test]
        public void OrbitMathKeepsConfiguredDistanceAndTargetsThePlayer()
        {
            Vector3 target = new(2f, 1f, -3f);
            Vector3 position = FollowLocalPlayer.CalculateOrbitPosition(
                target,
                90f,
                17f,
                4.6f);

            Assert.That(Vector3.Distance(position, target), Is.EqualTo(4.6f).Within(0.0001f));
            Assert.That(position.x, Is.LessThan(target.x));
            Assert.That(position.y, Is.GreaterThan(target.y));
        }

        [Test]
        public void CameraLimitsRejectInvalidPitchAndDistance()
        {
            Assert.That(FollowLocalPlayer.ClampPitch(-90f, -8f, 48f), Is.EqualTo(-8f));
            Assert.That(FollowLocalPlayer.ClampPitch(90f, -8f, 48f), Is.EqualTo(48f));
            Assert.That(FollowLocalPlayer.ClampDistance(0f, 1.35f, 6.2f), Is.EqualTo(1.35f));
            Assert.That(FollowLocalPlayer.ClampDistance(99f, 1.35f, 6.2f), Is.EqualTo(6.2f));
        }

        [Test]
        public void MovementUsesCameraYawInFreeView()
        {
            Vector2 forwardAtRightAngle = FollowLocalPlayer.ConvertMoveInput(
                Vector2.up,
                90f);

            Assert.That(forwardAtRightAngle.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(forwardAtRightAngle.y, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void FreeViewAcceptsDragWhileLockedViewIgnoresIt()
        {
            GameObject cameraObject = new("Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            FollowLocalPlayer controller = cameraObject.AddComponent<FollowLocalPlayer>();
            try
            {
                float lockedPitch = controller.CurrentPitch;
                controller.ApplyLookDelta(new Vector2(80f, 40f));
                Assert.That(controller.CurrentPitch, Is.EqualTo(lockedPitch));

                controller.SetViewMode(FollowLocalPlayer.ViewMode.Free);
                controller.ApplyLookDelta(new Vector2(80f, 40f));
                Assert.That(controller.CurrentPitch, Is.LessThan(lockedPitch));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void LookSurfaceOwnsOnlyOnePointer()
        {
            GameObject cameraObject = new("Camera");
            GameObject surfaceObject = new("Look Surface");
            Camera camera = cameraObject.AddComponent<Camera>();
            FollowLocalPlayer controller = cameraObject.AddComponent<FollowLocalPlayer>();
            CameraLookSurface surface = surfaceObject.AddComponent<CameraLookSurface>();
            controller.SetViewMode(FollowLocalPlayer.ViewMode.Free);
            surface.Configure(controller);

            try
            {
                PointerEventData first = new(null) { pointerId = 21, position = Vector2.zero };
                PointerEventData second = new(null) { pointerId = 22, position = Vector2.zero };

                surface.OnPointerDown(first);
                surface.OnPointerDown(second);
                Assert.That(surface.IsActive, Is.True);

                surface.OnPointerUp(second);
                Assert.That(surface.IsActive, Is.True);
                surface.OnPointerUp(first);
                Assert.That(surface.IsActive, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(surfaceObject);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void CommunityCharacterResourceIncludesIdleAndWalkAnimations()
        {
            const string resourcePath = "Characters/character-j";
            GameObject character = Resources.Load<GameObject>(resourcePath);
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(resourcePath);

            Assert.That(character, Is.Not.Null, "The community character must ship in Resources.");
            Assert.That(clips, Is.Not.Empty, "The community character must include animation clips.");
            Assert.That(ContainsClip(clips, "idle"), Is.True, "The idle clip is missing.");
            Assert.That(ContainsClip(clips, "walk"), Is.True, "The walk clip is missing.");
        }

        [Test]
        public void PlayerVisualInstantiatesTheCommunityCharacterAndAnimations()
        {
            GameObject player = new("Player Visual Test");
            try
            {
                NetworkPlayerVisual visual = player.AddComponent<NetworkPlayerVisual>();
                visual.EnsureInitialized();

                Assert.That(visual.UsesCommunityModel, Is.True);
                Assert.That(visual.HasCommunityAnimation, Is.True);
                Assert.That(visual.VisualRoot, Is.Not.Null);
                Transform model = visual.VisualRoot.Find("Kenney Blocky Character J");
                Assert.That(model, Is.Not.Null);
                Animation animation = model.GetComponentInChildren<Animation>(true);
                Assert.That(animation, Is.Not.Null);
                Assert.That(animation.GetClip("idle"), Is.Not.Null);
                Assert.That(animation.GetClip("walk"), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        private static bool ContainsClip(AnimationClip[] clips, string expectedName)
        {
            foreach (AnimationClip clip in clips)
            {
                if (clip != null && string.Equals(
                        clip.name,
                        expectedName,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
