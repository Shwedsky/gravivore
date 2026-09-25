using System;
using Gravivore.Gameplay.Combat;
using UnityEngine;

namespace Gravivore.Presentation.Combat
{
    [DisallowMultipleComponent]
    public sealed class GravityLashVfxPool : MonoBehaviour, IGravityLashVfx
    {
        private sealed class Beam
        {
            public GameObject GameObject;
            public LineRenderer Renderer;
            public float RemainingLifetime;
        }

        private Beam[] _beams;
        private Material _material;
        private float _duration;
        private int _reuseCursor;
        private bool _isInitialized;

        public void Initialize(GravityAttackSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            settings.ValidateOrThrow();
            _duration = settings.VfxDuration;
            _material = CreateMaterial(settings.VfxColor);
            _beams = new Beam[settings.VfxPoolSize];

            for (var i = 0; i < _beams.Length; i++)
            {
                _beams[i] = CreateBeam(i, settings);
            }

            _isInitialized = true;
        }

        public void Play(Vector3 origin, Vector3 destination)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("GravityLashVfxPool must be initialized before use.");
            }

            var beam = FindAvailableBeam();
            beam.Renderer.SetPosition(0, origin);
            beam.Renderer.SetPosition(1, destination);
            beam.RemainingLifetime = _duration;
            beam.GameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            for (var i = 0; i < _beams.Length; i++)
            {
                var beam = _beams[i];
                if (!beam.GameObject.activeSelf)
                {
                    continue;
                }

                beam.RemainingLifetime -= Time.deltaTime;
                if (beam.RemainingLifetime <= 0f)
                {
                    beam.RemainingLifetime = 0f;
                    beam.GameObject.SetActive(false);
                }
            }
        }

        private Beam FindAvailableBeam()
        {
            for (var i = 0; i < _beams.Length; i++)
            {
                var index = (_reuseCursor + i) % _beams.Length;
                if (_beams[index].GameObject.activeSelf)
                {
                    continue;
                }

                _reuseCursor = (index + 1) % _beams.Length;
                return _beams[index];
            }

            var reused = _beams[_reuseCursor];
            _reuseCursor = (_reuseCursor + 1) % _beams.Length;
            return reused;
        }

        private Beam CreateBeam(int index, GravityAttackSettings settings)
        {
            var beamObject = new GameObject($"Gravity Lash {index}", typeof(LineRenderer));
            beamObject.transform.SetParent(transform, false);
            var lineRenderer = beamObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.widthMultiplier = settings.VfxWidth;
            lineRenderer.numCapVertices = 2;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.sharedMaterial = _material;
            lineRenderer.startColor = settings.VfxColor;
            lineRenderer.endColor = new Color(
                settings.VfxColor.r,
                settings.VfxColor.g,
                settings.VfxColor.b,
                0f);
            beamObject.SetActive(false);

            return new Beam
            {
                GameObject = beamObject,
                Renderer = lineRenderer,
                RemainingLifetime = 0f
            };
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private static Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                throw new InvalidOperationException("A shader is required for the Gravity Lash placeholder VFX.");
            }

            return new Material(shader)
            {
                name = "Gravity Lash Placeholder Material",
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
    }
}
