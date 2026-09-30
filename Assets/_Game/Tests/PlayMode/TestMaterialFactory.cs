using System;
using UnityEngine;

namespace Gravivore.Tests.PlayMode
{
    internal static class TestMaterialFactory
    {
        private static Material _lit;

        public static Material Lit
        {
            get
            {
                if (_lit != null) return _lit;
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new InvalidOperationException("URP Lit shader is required for PlayMode tests.");
                _lit = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                return _lit;
            }
        }

        public static Material CreateLitInstance()
        {
            return new Material(Lit) { hideFlags = HideFlags.HideAndDontSave };
        }
    }
}
