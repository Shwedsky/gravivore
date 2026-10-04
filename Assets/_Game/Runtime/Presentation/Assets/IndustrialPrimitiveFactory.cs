using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Presentation.Assets
{
    public static class IndustrialPrimitiveFactory
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        public static GameObject Create(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material sharedMaterial,
            Color color,
            float metallic = 0.65f,
            float smoothness = 0.28f)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (sharedMaterial == null) throw new ArgumentNullException(nameof(sharedMaterial));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Visual primitive name is required.", nameof(name));

            var visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = name;
            visual.layer = 0;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = localRotation;
            visual.transform.localScale = localScale;

            var collider = visual.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            var renderer = visual.GetComponent<Renderer>();
            renderer.sharedMaterial = sharedMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var properties = new MaterialPropertyBlock();
            properties.SetColor(BaseColorId, color);
            properties.SetColor(ColorId, color);
            properties.SetFloat(MetallicId, Mathf.Clamp01(metallic));
            properties.SetFloat(SmoothnessId, Mathf.Clamp01(smoothness));
            renderer.SetPropertyBlock(properties);
            return visual;
        }

        public static GameObject Beam(
            Transform parent,
            string name,
            Vector3 localFrom,
            Vector3 localTo,
            float thickness,
            Material sharedMaterial,
            Color color,
            float metallic = 0.7f,
            float smoothness = 0.25f)
        {
            var delta = localTo - localFrom;
            if (delta.sqrMagnitude <= 0.000001f) throw new ArgumentException("Beam endpoints must be distinct.");
            if (float.IsNaN(thickness) || float.IsInfinity(thickness) || thickness <= 0f)
                throw new ArgumentOutOfRangeException(nameof(thickness));

            return Create(
                parent,
                name,
                PrimitiveType.Cube,
                (localFrom + localTo) * 0.5f,
                new Vector3(thickness, thickness, delta.magnitude),
                Quaternion.LookRotation(delta.normalized, Vector3.up),
                sharedMaterial,
                color,
                metallic,
                smoothness);
        }

        public static Transform Group(Transform parent, string name, Vector3 localPosition)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = localPosition;
            group.localRotation = Quaternion.identity;
            group.localScale = Vector3.one;
            return group;
        }

        public static int CountEnabledColliders(GameObject root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            var colliders = root.GetComponentsInChildren<Collider>(true);
            var enabled = 0;
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].enabled) enabled++;
            }

            return enabled;
        }
    }
}
