using UnityEngine;
using UnityEngine.Rendering;

namespace MetaverseGame.Gameplay
{
    /// <summary>
    /// Loads the licensed Kenney Blocky Characters visual without touching the
    /// network or collision root. A small procedural fallback keeps the player
    /// readable if the imported model is unavailable in a partial checkout.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkPlayerVisual : MonoBehaviour
    {
        private const string CharacterResource = "Characters/character-j";
        private const float CharacterHeight = 2f;
        private const float MovementThreshold = 0.08f;

        [SerializeField] private float modelScale = 1f;
        [SerializeField] private Color fallbackColor = new(0.10f, 0.62f, 0.72f);
        [SerializeField] private bool showGroundMarker = true;

        private GameObject visualRoot;
        private Material fallbackMaterial;
        private Material markerMaterial;
        private Animation legacyAnimation;
        private Animator animator;
        private AnimationClip idleClip;
        private AnimationClip walkClip;
        private string activeClip;
        private Vector3 lastPosition;

        public bool UsesCommunityModel { get; private set; }
        public bool HasCommunityAnimation => idleClip != null || walkClip != null;
        public Transform VisualRoot => visualRoot != null ? visualRoot.transform : null;

        private void Awake()
        {
            BuildVisual();
        }

        private void OnDestroy()
        {
            if (fallbackMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(fallbackMaterial);
                }
                else
                {
                    DestroyImmediate(fallbackMaterial);
                }
            }
            if (markerMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(markerMaterial);
                }
                else
                {
                    DestroyImmediate(markerMaterial);
                }
            }
        }

        private void BuildVisual()
        {
            visualRoot = new GameObject("Community Character Visual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localPosition = new Vector3(0f, -1f, 0f);
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one * Mathf.Max(0.01f, modelScale);

            GameObject source = Resources.Load<GameObject>(CharacterResource);
            if (source != null)
            {
                GameObject instance = Instantiate(source, visualRoot.transform);
                instance.name = "Kenney Blocky Character J";
                NormalizeImportedModel(instance);
                UsesCommunityModel = true;
                ConfigureAnimation(instance);
            }
            else
            {
                BuildFallbackVisual(visualRoot.transform);
            }

            if (showGroundMarker)
            {
                CreateGroundMarker();
            }
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (!UsesCommunityModel || visualRoot == null)
            {
                return;
            }

            float frameSpeed = Time.deltaTime > 0.0001f
                ? Vector3.Distance(transform.position, lastPosition) / Time.deltaTime
                : 0f;
            lastPosition = transform.position;

            if (legacyAnimation != null)
            {
                PlayLegacyClip(frameSpeed > MovementThreshold ? walkClip : idleClip);
            }
            else if (animator != null && animator.runtimeAnimatorController != null)
            {
                string state = frameSpeed > MovementThreshold ? "walk" : "idle";
                if (!string.Equals(activeClip, state, System.StringComparison.OrdinalIgnoreCase))
                {
                    animator.CrossFadeInFixedTime(state, 0.12f);
                    activeClip = state;
                }
            }
        }

        private void ConfigureAnimation(GameObject instance)
        {
            legacyAnimation = instance.GetComponentInChildren<Animation>(true);
            if (legacyAnimation != null)
            {
                legacyAnimation.playAutomatically = false;
                idleClip = FindClip(legacyAnimation, "idle");
                walkClip = FindClip(legacyAnimation, "walk");
                if (idleClip == null)
                {
                    idleClip = FindFirstClip(legacyAnimation);
                }
                if (idleClip != null)
                {
                    idleClip.wrapMode = WrapMode.Loop;
                }
                if (walkClip != null)
                {
                    walkClip.wrapMode = WrapMode.Loop;
                }
                PlayLegacyClip(idleClip);
                return;
            }

            animator = instance.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.enabled = animator.runtimeAnimatorController != null;
            }
        }

        private void PlayLegacyClip(AnimationClip clip)
        {
            if (legacyAnimation == null || clip == null ||
                string.Equals(activeClip, clip.name, System.StringComparison.Ordinal))
            {
                return;
            }

            legacyAnimation.CrossFade(clip.name, 0.12f);
            activeClip = clip.name;
        }

        private static AnimationClip FindClip(Animation animation, string token)
        {
            foreach (AnimationState state in animation)
            {
                AnimationClip clip = state?.clip;
                if (clip != null && clip.name.IndexOf(
                        token,
                        System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return clip;
                }
            }
            return null;
        }

        private static AnimationClip FindFirstClip(Animation animation)
        {
            foreach (AnimationState state in animation)
            {
                if (state?.clip != null)
                {
                    return state.clip;
                }
            }
            return null;
        }

        private void CreateGroundMarker()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Diffuse");
            markerMaterial = new Material(shader)
            {
                name = "Player Ground Marker",
                hideFlags = HideFlags.DontSave,
            };
            Color markerColor = new(0.12f, 0.88f, 0.96f, 1f);
            if (markerMaterial.HasProperty("_BaseColor"))
            {
                markerMaterial.SetColor("_BaseColor", markerColor);
            }
            if (markerMaterial.HasProperty("_Color"))
            {
                markerMaterial.SetColor("_Color", markerColor);
            }
            if (markerMaterial.HasProperty("_EmissionColor"))
            {
                markerMaterial.EnableKeyword("_EMISSION");
                markerMaterial.SetColor("_EmissionColor", markerColor * 1.8f);
            }

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Community Character Ground Marker";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(0f, -0.98f, 0f);
            marker.transform.localScale = new Vector3(0.72f, 0.018f, 0.72f);
            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = markerMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            Collider collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }
        }

        private static void NormalizeImportedModel(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            float height = Mathf.Max(0.01f, bounds.size.y);
            float scale = CharacterHeight / height;
            Transform parent = instance.transform.parent;
            Vector3 localCenter = parent != null
                ? parent.InverseTransformPoint(bounds.center)
                : bounds.center;
            Vector3 localMin = parent != null
                ? parent.InverseTransformPoint(bounds.min)
                : bounds.min;
            instance.transform.localScale *= scale;
            instance.transform.localPosition = new Vector3(
                -localCenter.x * scale,
                -localMin.y * scale,
                -localCenter.z * scale);

            foreach (Renderer renderer in renderers)
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private void BuildFallbackVisual(Transform parent)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Diffuse");
            fallbackMaterial = new Material(shader)
            {
                name = "Character Fallback Material",
                hideFlags = HideFlags.DontSave,
            };
            if (fallbackMaterial.HasProperty("_BaseColor"))
            {
                fallbackMaterial.SetColor("_BaseColor", fallbackColor);
            }
            if (fallbackMaterial.HasProperty("_Color"))
            {
                fallbackMaterial.SetColor("_Color", fallbackColor);
            }

            CreatePrimitive(
                PrimitiveType.Capsule,
                "Fallback Body",
                new Vector3(0f, 0.9f, 0f),
                new Vector3(0.62f, 0.9f, 0.62f),
                fallbackMaterial,
                parent);
            CreatePrimitive(
                PrimitiveType.Sphere,
                "Fallback Head",
                new Vector3(0f, 1.83f, 0f),
                new Vector3(0.58f, 0.52f, 0.58f),
                fallbackMaterial,
                parent);
        }

        private static GameObject CreatePrimitive(
            PrimitiveType type,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            Transform parent)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = position;
            primitive.transform.localScale = scale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }
            return primitive;
        }
    }
}
