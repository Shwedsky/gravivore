using System;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    [Serializable]
    public sealed class PresentationModelBinding
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Vector3 _localEulerAngles;
        [SerializeField] private Vector3 _localScale = Vector3.one;

        public GameObject Prefab => _prefab;
        public bool HasPrefab => _prefab != null;
        public Vector3 LocalPosition => _localPosition;
        public Quaternion LocalRotation => Quaternion.Euler(_localEulerAngles);
        public Vector3 LocalScale => _localScale;

        public void ValidateOrThrow()
        {
            ValidateTransform(_localPosition, _localEulerAngles, _localScale);
            if (HasPrefab) PresentationPrefabValidation.ValidateOrThrow(_prefab);
        }

        public GameObject InstantiateUnder(Transform visualParent)
        {
            if (visualParent == null) throw new ArgumentNullException(nameof(visualParent));
            ValidateOrThrow(); // Reject unsafe content before Instantiate can invoke lifecycle callbacks.
            if (!HasPrefab) return null;
            var instance = UnityEngine.Object.Instantiate(_prefab, visualParent, false);
            instance.name = _prefab.name;
            instance.transform.localPosition = _localPosition;
            instance.transform.localRotation = LocalRotation;
            instance.transform.localScale = _localScale;
            return instance;
        }

        public static void ValidateTransform(Vector3 position, Vector3 angles, Vector3 scale)
        {
            if (!Finite(position) || !Finite(angles) || !Finite(scale) || scale.x <= 0 || scale.y <= 0 || scale.z <= 0)
                throw new InvalidOperationException("Visual offsets must be finite and scale must be positive on every axis.");
        }

        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
