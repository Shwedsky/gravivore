using System;
using UnityEngine;

namespace Gravivore.Presentation.Composition
{
    [CreateAssetMenu(menuName = "Gravivore/Presentation Material Palette")]
    public sealed class PresentationMaterialPalette : ScriptableObject
    {
        [SerializeField] private Material _litMaterial;
        [SerializeField] private Material _unlitMaterial;

        public Material LitMaterial => _litMaterial;
        public Material UnlitMaterial => _unlitMaterial;

        public void ValidateOrThrow()
        {
            ValidateMaterial(_litMaterial, "lit");
            ValidateMaterial(_unlitMaterial, "unlit");
        }

        public Material CreateLitInstance(Color color)
        {
            ValidateMaterial(_litMaterial, "lit");
            return CreateInstance(_litMaterial, color);
        }

        public Material CreateUnlitInstance(Color color)
        {
            ValidateMaterial(_unlitMaterial, "unlit");
            return CreateInstance(_unlitMaterial, color);
        }

        private static Material CreateInstance(Material source, Color color)
        {
            return new Material(source)
            {
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private static void ValidateMaterial(Material material, string role)
        {
            if (material == null || material.shader == null)
            {
                throw new InvalidOperationException($"Presentation material palette requires a valid {role} material.");
            }
        }
    }
}
