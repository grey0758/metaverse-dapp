using UnityEngine;

namespace MetaverseGame.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class MobileOrientationLock : MonoBehaviour
    {
        private void Awake()
        {
#if !UNITY_SERVER
            if (!Application.isMobilePlatform)
            {
                return;
            }

            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
#endif
        }
    }
}
