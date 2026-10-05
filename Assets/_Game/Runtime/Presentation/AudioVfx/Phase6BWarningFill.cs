using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Presentation.AudioVfx
{
    /// <summary>One prewarmed translucent mesh follows the authoritative warning contour.</summary>
    internal sealed class Phase6BWarningFill : IDisposable
    {
        private readonly Transform _transform;
        private readonly Mesh _mesh;
        private readonly MeshRenderer _renderer;
        private readonly MaterialPropertyBlock _properties = new MaterialPropertyBlock();
        private readonly Vector3[] _vertices;
        private readonly int[] _triangles;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public Phase6BWarningFill(Transform owner, Material material, int boundaryCount)
        {
            var surface = new GameObject("Phase6B Danger Fill", typeof(MeshFilter), typeof(MeshRenderer));
            surface.transform.SetParent(owner, false);
            _transform = surface.transform;
            _vertices = new Vector3[boundaryCount + 1];
            _triangles = new int[boundaryCount * 3];
            for (var i = 0; i < boundaryCount; i++)
            {
                _triangles[i * 3] = 0;
                _triangles[i * 3 + 1] = i + 1;
                _triangles[i * 3 + 2] = (i + 1) % boundaryCount + 1;
            }
            _mesh = new Mesh { name = "Pooled Phase6B Warning Surface" };
            _mesh.MarkDynamic();
            surface.GetComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = surface.GetComponent<MeshRenderer>();
            _renderer.sharedMaterial = material;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.lightProbeUsage = LightProbeUsage.Off;
            _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            _renderer.enabled = false;
        }

        public void Configure(LineRenderer contour, Vector3 center)
        {
            if (contour.positionCount + 1 != _vertices.Length)
                throw new InvalidOperationException("Warning contour topology changed after prewarm.");
            _vertices[0] = _transform.InverseTransformPoint(center + Vector3.up * .025f);
            for (var i = 0; i < contour.positionCount; i++)
                _vertices[i + 1] = _transform.InverseTransformPoint(contour.GetPosition(i));
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
            _mesh.RecalculateBounds();
            _renderer.enabled = true;
        }

        public void SetColor(Color color)
        {
            _properties.SetColor(BaseColor, color);
            _renderer.SetPropertyBlock(_properties);
        }

        public void Stop() => _renderer.enabled = false;
        public void Dispose()
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(_mesh);
            else UnityEngine.Object.DestroyImmediate(_mesh);
        }
    }
}
