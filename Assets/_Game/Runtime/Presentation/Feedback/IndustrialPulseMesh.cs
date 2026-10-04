using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Presentation.Feedback
{
    internal static class IndustrialPulseMesh
    {
        // One original, shared low-poly ring plus four metal/energy slivers per pool.
        public static Mesh Create()
        {
            var vertices = new List<Vector3>();
            var indices = new List<int>();
            const int segments = 16;
            for (var i = 0; i < segments; i++)
            {
                var a = i * Mathf.PI * 2 / segments;
                var b = (i + 1) * Mathf.PI * 2 / segments;
                var start = vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a) * .4f, 0, Mathf.Sin(a) * .4f));
                vertices.Add(new Vector3(Mathf.Cos(a) * .46f, 0, Mathf.Sin(a) * .46f));
                vertices.Add(new Vector3(Mathf.Cos(b) * .46f, 0, Mathf.Sin(b) * .46f));
                vertices.Add(new Vector3(Mathf.Cos(b) * .4f, 0, Mathf.Sin(b) * .4f));
                indices.Add(start); indices.Add(start + 1); indices.Add(start + 2);
                indices.Add(start); indices.Add(start + 2); indices.Add(start + 3);
                indices.Add(start + 2); indices.Add(start + 1); indices.Add(start);
                indices.Add(start + 3); indices.Add(start + 2); indices.Add(start);
            }
            for (var i = 0; i < 4; i++)
            {
                var direction = new Vector3(Mathf.Cos(i * 1.7f), .6f + i * .13f, Mathf.Sin(i * 1.7f)).normalized;
                var rotation = Quaternion.FromToRotation(Vector3.up, direction);
                var centre = direction * (.32f + i * .06f);
                var start = vertices.Count;
                for (var n = 0; n < 8; n++)
                    vertices.Add(centre + rotation * new Vector3((n & 1) == 0 ? -.025f : .025f,
                        (n & 2) == 0 ? -.13f : .13f, (n & 4) == 0 ? -.025f : .025f));
                var faces = new[] { 0,2,3,0,3,1,4,5,7,4,7,6,0,1,5,0,5,4,
                    2,6,7,2,7,3,0,4,6,0,6,2,1,3,7,1,7,5 };
                foreach (var index in faces) indices.Add(start + index);
            }
            var mesh = new Mesh { name = "Original industrial shock ring and slivers", hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
