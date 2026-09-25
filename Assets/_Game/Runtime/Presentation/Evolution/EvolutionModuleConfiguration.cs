using System;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using UnityEngine;

namespace Gravivore.Presentation.Evolution
{
    public enum EvolutionSocketId
    {
        Core = 0,
        Left = 1,
        Right = 2,
        Rear = 3
    }

    [Serializable]
    public sealed class EvolutionModuleConfiguration
    {
        [SerializeField] private string _id;
        [SerializeField] private EvolutionSocketId _socket;
        [SerializeField] private PrimitiveType _primitive;
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Vector3 _localEulerAngles;
        [SerializeField] private Vector3 _localScale = Vector3.one;
        [SerializeField] private Color _color = Color.white;

        public EvolutionModuleConfiguration(
            string id,
            EvolutionSocketId socket,
            PrimitiveType primitive,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale,
            Color color)
        {
            _id = id;
            _socket = socket;
            _primitive = primitive;
            _localPosition = localPosition;
            _localEulerAngles = localEulerAngles;
            _localScale = localScale;
            _color = color;
            ValidateOrThrow();
        }

        public string Id => _id;
        public EvolutionSocketId Socket => _socket;
        public PrimitiveType Primitive => _primitive;
        public Vector3 LocalPosition => _localPosition;
        public Vector3 LocalEulerAngles => _localEulerAngles;
        public Vector3 LocalScale => _localScale;
        public Color Color => _color;

        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                throw new InvalidOperationException("Evolution module id is required.");
            }

            if (!Enum.IsDefined(typeof(EvolutionSocketId), _socket))
            {
                throw new InvalidOperationException($"Evolution module {_id} uses an invalid socket.");
            }

            if (!Enum.IsDefined(typeof(PrimitiveType), _primitive))
            {
                throw new InvalidOperationException($"Evolution module {_id} uses an invalid primitive type.");
            }

            if (!IsFinitePositive(_localScale.x) ||
                !IsFinitePositive(_localScale.y) ||
                !IsFinitePositive(_localScale.z))
            {
                throw new InvalidOperationException($"Evolution module {_id} requires a finite positive scale.");
            }

            ValidateFinite(_localPosition, "position");
            ValidateFinite(_localEulerAngles, "rotation");
            if (!IsFinite(_color.r) || !IsFinite(_color.g) || !IsFinite(_color.b) || !IsFinite(_color.a))
            {
                throw new InvalidOperationException($"Evolution module {_id} requires a finite color.");
            }
        }

        private void ValidateFinite(Vector3 value, string field)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y) || !IsFinite(value.z))
            {
                throw new InvalidOperationException($"Evolution module {_id} requires a finite {field}.");
            }
        }

        private static bool IsFinitePositive(float value) => IsFinite(value) && value > 0f;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    [Serializable]
    public sealed class EvolutionTierModuleSet
    {
        [SerializeField] private EvolutionTier _tier;
        [SerializeField] private EvolutionModuleConfiguration[] _modules = Array.Empty<EvolutionModuleConfiguration>();

        public EvolutionTierModuleSet(EvolutionTier tier, EvolutionModuleConfiguration[] modules)
        {
            _tier = tier;
            _modules = modules ?? throw new ArgumentNullException(nameof(modules));
        }

        public EvolutionTier Tier => _tier;
        public EvolutionModuleConfiguration[] Modules => _modules;
    }

    [Serializable]
    public sealed class EvolutionAccentModule
    {
        [SerializeField] private PlayerStatType _stat;
        [SerializeField] private EvolutionModuleConfiguration _module;

        public EvolutionAccentModule(PlayerStatType stat, EvolutionModuleConfiguration module)
        {
            _stat = stat;
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public PlayerStatType Stat => _stat;
        public EvolutionModuleConfiguration Module => _module;
    }
}
