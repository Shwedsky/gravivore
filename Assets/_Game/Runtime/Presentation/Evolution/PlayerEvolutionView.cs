using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using UnityEngine;

namespace Gravivore.Presentation.Evolution
{
    public interface IPlayerEvolutionView
    {
        void Apply(EvolutionVisualState state);
    }

    [DisallowMultipleComponent]
    public sealed class PlayerEvolutionView : MonoBehaviour, IPlayerEvolutionView
    {
        private readonly struct TierModuleInstance
        {
            public TierModuleInstance(EvolutionTier tier, GameObject gameObject)
            {
                Tier = tier;
                GameObject = gameObject;
            }

            public EvolutionTier Tier { get; }
            public GameObject GameObject { get; }
        }

        private readonly struct AccentModuleInstance
        {
            public AccentModuleInstance(PlayerStatType stat, GameObject gameObject)
            {
                Stat = stat;
                GameObject = gameObject;
            }

            public PlayerStatType Stat { get; }
            public GameObject GameObject { get; }
        }

        private Transform[] _sockets;
        private TierModuleInstance[] _tierModules;
        private AccentModuleInstance[] _accentModules;
        private Material _moduleMaterial;
        private bool _isInitialized;

        public EvolutionTier CurrentTier { get; private set; }

        public PlayerStatType CurrentDominantStat { get; private set; }

        public int SocketCount => _sockets != null ? _sockets.Length : 0;

        public void Initialize(Transform visualRoot, EvolutionVisualCatalog catalog)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Player evolution view is already initialized.");
            }

            if (visualRoot == null)
            {
                throw new ArgumentNullException(nameof(visualRoot));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _sockets = CreateSockets(visualRoot);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                _moduleMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            var tierModules = new List<TierModuleInstance>();
            for (var setIndex = 0; setIndex < catalog.TierModuleSets.Length; setIndex++)
            {
                var set = catalog.TierModuleSets[setIndex];
                for (var moduleIndex = 0; moduleIndex < set.Modules.Length; moduleIndex++)
                {
                    tierModules.Add(new TierModuleInstance(
                        set.Tier,
                        CreateModule(set.Modules[moduleIndex])));
                }
            }

            _tierModules = tierModules.ToArray();
            _accentModules = new AccentModuleInstance[catalog.AccentModules.Length];
            for (var i = 0; i < catalog.AccentModules.Length; i++)
            {
                var accent = catalog.AccentModules[i];
                _accentModules[i] = new AccentModuleInstance(
                    accent.Stat,
                    CreateModule(accent.Module));
            }

            _isInitialized = true;
        }

        public void Apply(EvolutionVisualState state)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Player evolution view must be initialized before use.");
            }

            for (var i = 0; i < _tierModules.Length; i++)
            {
                _tierModules[i].GameObject.SetActive(_tierModules[i].Tier == state.Tier);
            }

            for (var i = 0; i < _accentModules.Length; i++)
            {
                _accentModules[i].GameObject.SetActive(_accentModules[i].Stat == state.DominantStat);
            }

            CurrentTier = state.Tier;
            CurrentDominantStat = state.DominantStat;
        }

        public int GetActiveTierModuleCount()
        {
            var count = 0;
            for (var i = 0; i < _tierModules.Length; i++)
            {
                if (_tierModules[i].GameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        public bool IsAccentActive(PlayerStatType stat)
        {
            for (var i = 0; i < _accentModules.Length; i++)
            {
                if (_accentModules[i].Stat == stat)
                {
                    return _accentModules[i].GameObject.activeSelf;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(stat));
        }

        private Transform[] CreateSockets(Transform visualRoot)
        {
            var socketCount = Enum.GetValues(typeof(EvolutionSocketId)).Length;
            var sockets = new Transform[socketCount];
            for (var i = 0; i < sockets.Length; i++)
            {
                var socket = new GameObject($"Evolution Socket [{(EvolutionSocketId)i}]").transform;
                socket.SetParent(visualRoot, false);
                sockets[i] = socket;
            }

            return sockets;
        }

        private GameObject CreateModule(EvolutionModuleConfiguration configuration)
        {
            var module = GameObject.CreatePrimitive(configuration.Primitive);
            module.name = $"Evolution Module [{configuration.Id}]";
            module.transform.SetParent(_sockets[(int)configuration.Socket], false);
            module.transform.localPosition = configuration.LocalPosition;
            module.transform.localRotation = Quaternion.Euler(configuration.LocalEulerAngles);
            module.transform.localScale = configuration.LocalScale;
            var collider = module.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }

            var renderer = module.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (_moduleMaterial != null)
                {
                    renderer.sharedMaterial = _moduleMaterial;
                }

                var properties = new MaterialPropertyBlock();
                properties.SetColor("_BaseColor", configuration.Color);
                properties.SetColor("_Color", configuration.Color);
                renderer.SetPropertyBlock(properties);
            }

            module.SetActive(false);
            return module;
        }

        private void OnDestroy()
        {
            if (_moduleMaterial != null)
            {
                Destroy(_moduleMaterial);
            }
        }
    }
}
