using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gravivore.VisualReplacementProofV1
{
    /// <summary>
    /// Isolated visual proof composition for the Chapter 1 replacement art direction.
    /// This component owns no production gameplay state and does not modify Chapter01.
    /// Visual meshes are project-authored at runtime through VisualProofMeshFactory rather
    /// than Unity built-in primitives or Phase3B/Phase3D simple-kit pieces.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class VisualReplacementProofBootstrap : MonoBehaviour
    {
        private const float CameraFov = 46f;
        private static readonly Vector3 MandatoryCameraOffset = new Vector3(0f, 14.8f, -11.2f);

        [SerializeField] private bool autoCaptureReviewFrames = true;
        [SerializeField] private string outputDirectory = "Artifacts/VisualReplacementProofV1";

        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>(StringComparer.Ordinal);
        private readonly List<Texture2D> _generatedTextures = new List<Texture2D>();
        private VolumeProfile _volumeProfile;

        private Transform _environmentRoot;
        private Transform _g0Root;
        private Transform _scoutRoot;
        private Transform _cutterRoot;
        private Transform _reactorRoot;
        private Transform _barrierRoot;
        private Transform _propRoot;
        private Transform _attackProxyRoot;
        private Transform _deathProxyRoot;
        private Transform _g0AttackSocket;
        private Transform _scoutHitSocket;
        private Transform _scoutDeathSocket;
        private Camera _reviewCamera;
        private bool _built;

        private void Start()
        {
            BuildProof();

            if (autoCaptureReviewFrames)
            {
                StartCoroutine(CaptureReviewSet());
            }
        }

        private void OnDestroy()
        {
            foreach (var material in _materials.Values)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }

            foreach (var texture in _generatedTextures)
            {
                if (texture != null)
                {
                    Destroy(texture);
                }
            }

            if (_volumeProfile != null)
            {
                Destroy(_volumeProfile);
            }
        }

        [ContextMenu("Build / Rebuild Visual Replacement Proof")]
        public void BuildProof()
        {
            if (_built)
            {
                return;
            }

            _built = true;
            ConfigureRenderEnvironment();
            CreateMaterials();
            BuildPostProcessing();

            _environmentRoot = NewRoot("Environment_Proof");
            _g0Root = BuildG0(new Vector3(-2.65f, 0f, -2.2f));
            _scoutRoot = BuildScout(new Vector3(2.25f, 0f, -0.25f));
            _cutterRoot = BuildCutter(new Vector3(1.1f, 0f, 4.35f));
            BuildIndustrialArena();
            BuildReviewLighting();
            BuildReviewCamera();
            BuildReadabilityProxies();
        }

        private void ConfigureRenderEnvironment()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.055f, 0.07f, 0.09f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.025f, 0.035f, 0.045f, 1f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 42f;
        }

        private void CreateMaterials()
        {
            AddMaterial("ArmorGunmetal", new Color(0.12f, 0.145f, 0.17f), 0.78f, 0.34f);
            AddMaterial("ArmorPaint", new Color(0.12f, 0.22f, 0.27f), 0.58f, 0.27f);
            AddMaterial("ArmorEdge", new Color(0.26f, 0.31f, 0.34f), 0.72f, 0.38f);
            AddMaterial("JointRubber", new Color(0.025f, 0.032f, 0.038f), 0.08f, 0.18f);
            AddMaterial("StructuralMetal", new Color(0.095f, 0.11f, 0.125f), 0.82f, 0.25f);
            AddMaterial("WornPaint", new Color(0.19f, 0.205f, 0.20f), 0.46f, 0.19f);
            AddMaterial("Floor", new Color(0.065f, 0.078f, 0.088f), 0.63f, 0.21f);
            AddMaterial("FloorEdge", new Color(0.13f, 0.15f, 0.155f), 0.72f, 0.25f);
            AddMaterial("HeatMetal", new Color(0.17f, 0.10f, 0.065f), 0.72f, 0.29f);
            AddMaterial("CableRubber", new Color(0.018f, 0.024f, 0.028f), 0.02f, 0.13f);
            AddMaterial("CyanEnergy", new Color(0.03f, 0.24f, 0.30f), 0.18f, 0.38f, new Color(0.0f, 2.8f, 4.7f));
            AddMaterial("WarmEnergy", new Color(0.28f, 0.09f, 0.025f), 0.25f, 0.32f, new Color(5.4f, 0.85f, 0.08f));
            AddMaterial("DangerPaint", new Color(0.37f, 0.105f, 0.045f), 0.38f, 0.22f);
        }

        private void AddMaterial(
            string key,
            Color baseColor,
            float metallic,
            float smoothness,
            Color? emission = null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("URP/Lit shader was not found. Visual proof requires the project's existing URP pipeline.");
            }

            var material = new Material(shader)
            {
                name = "VRP1_" + key
            };

            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);

            if (NeedsSurfaceTexture(key))
            {
                var size = key == "Floor" || key == "WornPaint" ? 256 : 128;
                var texture = CreateSurfaceTexture(key, size);
                material.SetTexture("_BaseMap", texture);
                _generatedTextures.Add(texture);
            }

            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            _materials.Add(key, material);
        }

        private void BuildPostProcessing()
        {
            var volumeGo = new GameObject("Volume_ProofColorGrade");
            volumeGo.transform.SetParent(transform, false);

            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 100f;

            _volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            _volumeProfile.name = "VRP1_RuntimeProfile";
            volume.sharedProfile = _volumeProfile;

            var tonemapping = _volumeProfile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            var bloom = _volumeProfile.Add<Bloom>(true);
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(0.30f);
            bloom.scatter.Override(0.52f);

            var color = _volumeProfile.Add<ColorAdjustments>(true);
            color.postExposure.Override(-0.10f);
            color.contrast.Override(18f);
            color.saturation.Override(-5f);

            var vignette = _volumeProfile.Add<Vignette>(true);
            vignette.intensity.Override(0.16f);
            vignette.smoothness.Override(0.52f);
        }

        private static bool NeedsSurfaceTexture(string key)
        {
            return key == "ArmorGunmetal"
                || key == "ArmorPaint"
                || key == "StructuralMetal"
                || key == "WornPaint"
                || key == "Floor"
                || key == "HeatMetal";
        }

        private static Texture2D CreateSurfaceTexture(string key, int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "VRP1_" + key + "_ProceduralSurface",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            var seed = StableHash(key);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var hash = Hash2D(x, y, seed);
                    var grain = (hash & 255) / 255f;
                    var fine = 0.90f + grain * 0.10f;

                    if (key == "Floor")
                    {
                        var seam = x % 64 < 2 || y % 64 < 2;
                        var scuff = ((x + y * 3 + seed) % 97) < 3;
                        fine *= seam ? 0.58f : 1f;
                        fine *= scuff ? 0.78f : 1f;
                    }
                    else if (key == "WornPaint" || key == "ArmorPaint")
                    {
                        var chip = ((hash >> 8) & 1023) < (key == "WornPaint" ? 42 : 16);
                        var scratch = ((x * 5 + y + seed) % 151) < 2;
                        fine *= chip ? 0.64f : 1f;
                        fine *= scratch ? 0.72f : 1f;
                    }
                    else if (key == "HeatMetal")
                    {
                        var band = Mathf.Abs(Mathf.Sin((x + seed % 31) * 0.075f)) * 0.08f;
                        fine = Mathf.Clamp01(fine - band);
                    }
                    else
                    {
                        var brushed = 0.96f + Mathf.Sin((x + seed % 19) * 0.21f) * 0.025f;
                        fine *= brushed;
                    }

                    var value = (byte)Mathf.Clamp(Mathf.RoundToInt(fine * 255f), 0, 255);
                    pixels[y * size + x] = new Color32(value, value, value, 255);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                var hash = 23;
                for (var i = 0; i < value.Length; i++)
                {
                    hash = hash * 31 + value[i];
                }

                return hash;
            }
        }

        private static int Hash2D(int x, int y, int seed)
        {
            unchecked
            {
                var h = x * 374761393 + y * 668265263 + seed * 69069;
                h = (h ^ (h >> 13)) * 1274126177;
                return h ^ (h >> 16);
            }
        }
    }
}
