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

        [SerializeField] private float modelScale = 1f;
        [SerializeField] private Color fallbackColor = new(0.10f, 0.62f, 0.72f);

        private GameObject visualRoot;
        private Material fallbackMaterial;

        public bool UsesCommunityModel { get; private set; }
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
                return;
            }

            BuildFallbackVisual(visualRoot.transform);
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
