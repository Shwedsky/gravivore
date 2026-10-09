using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.VisualReplacementProofV1
{
    internal static class VisualProofMeshFactory
    {
        internal readonly struct LoftRing
        {
            public readonly float Y;
            public readonly float RadiusX;
            public readonly float RadiusZ;
            public readonly float OffsetX;
            public readonly float OffsetZ;
            public readonly float RotationDegrees;

            public LoftRing(
                float y,
                float radiusX,
                float radiusZ,
                float offsetX = 0f,
                float offsetZ = 0f,
                float rotationDegrees = 0f)
            {
                Y = y;
                RadiusX = radiusX;
                RadiusZ = radiusZ;
                OffsetX = offsetX;
                OffsetZ = offsetZ;
                RotationDegrees = rotationDegrees;
            }
        }

        internal static Mesh CreateLoft(
            string name,
            IReadOnlyList<LoftRing> rings,
            int sides = 8,
            bool capStart = true,
            bool capEnd = true)
        {
            if (rings == null || rings.Count < 2)
            {
                throw new ArgumentException("A loft requires at least two rings.", nameof(rings));
            }

            sides = Mathf.Max(3, sides);
            var vertices = new List<Vector3>(rings.Count * sides + 2);
            var triangles = new List<int>((rings.Count - 1) * sides * 6 + sides * 6);

            for (var r = 0; r < rings.Count; r++)
            {
                var ring = rings[r];
                var phase = ring.RotationDegrees * Mathf.Deg2Rad;

                for (var i = 0; i < sides; i++)
                {
                    var angle = ((float)i / sides) * Mathf.PI * 2f + phase;
                    vertices.Add(new Vector3(
                        ring.OffsetX + Mathf.Cos(angle) * ring.RadiusX,
                        ring.Y,
                        ring.OffsetZ + Mathf.Sin(angle) * ring.RadiusZ));
                }
            }

            for (var r = 0; r < rings.Count - 1; r++)
            {
                var row = r * sides;
                var nextRow = (r + 1) * sides;

                for (var i = 0; i < sides; i++)
                {
                    var next = (i + 1) % sides;
                    var a = row + i;
                    var b = row + next;
                    var c = nextRow + i;
                    var d = nextRow + next;

                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);

                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            if (capStart)
            {
                var center = vertices.Count;
                var first = rings[0];
                vertices.Add(new Vector3(first.OffsetX, first.Y, first.OffsetZ));

                for (var i = 0; i < sides; i++)
                {
                    triangles.Add(center);
                    triangles.Add((i + 1) % sides);
                    triangles.Add(i);
                }
            }

            if (capEnd)
            {
                var center = vertices.Count;
                var last = rings[rings.Count - 1];
                vertices.Add(new Vector3(last.OffsetX, last.Y, last.OffsetZ));
                var row = (rings.Count - 1) * sides;

                for (var i = 0; i < sides; i++)
                {
                    triangles.Add(center);
                    triangles.Add(row + i);
                    triangles.Add(row + (i + 1) % sides);
                }
            }

            return FinalizeMesh(name, vertices, triangles);
        }

        internal static Mesh CreateExtrudedPolygon(string name, IReadOnlyList<Vector2> polygon, float depth)
        {
            if (polygon == null || polygon.Count < 3)
            {
                throw new ArgumentException("An extrusion requires at least three polygon points.", nameof(polygon));
            }

            var half = Mathf.Abs(depth) * 0.5f;
            var count = polygon.Count;
            var vertices = new List<Vector3>(count * 2);
            var triangles = new List<int>((count - 2) * 6 + count * 6);

            for (var i = 0; i < count; i++)
            {
                vertices.Add(new Vector3(polygon[i].x, polygon[i].y, -half));
            }

            for (var i = 0; i < count; i++)
            {
                vertices.Add(new Vector3(polygon[i].x, polygon[i].y, half));
            }

            for (var i = 1; i < count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);

                triangles.Add(count);
                triangles.Add(count + i);
                triangles.Add(count + i + 1);
            }

            for (var i = 0; i < count; i++)
            {
                var next = (i + 1) % count;
                var a = i;
                var b = next;
                var c = count + i;
                var d = count + next;

                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
            }

            return FinalizeMesh(name, vertices, triangles);
        }

        internal static Mesh CreateTube(
            string name,
            IReadOnlyList<Vector3> points,
            float radius,
            int sides = 8)
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException("A tube requires at least two points.", nameof(points));
            }

            sides = Mathf.Max(5, sides);
            var vertices = new List<Vector3>(points.Count * sides);
            var triangles = new List<int>((points.Count - 1) * sides * 6);

            var priorNormal = Vector3.up;

            for (var p = 0; p < points.Count; p++)
            {
                Vector3 tangent;
                if (p == 0)
                {
                    tangent = (points[1] - points[0]).normalized;
                }
                else if (p == points.Count - 1)
                {
                    tangent = (points[p] - points[p - 1]).normalized;
                }
                else
                {
                    tangent = (points[p + 1] - points[p - 1]).normalized;
                }

                var normal = Vector3.Cross(tangent, priorNormal);
                if (normal.sqrMagnitude < 0.001f)
                {
                    normal = Vector3.Cross(tangent, Vector3.right);
                }

                normal.Normalize();
                var binormal = Vector3.Cross(tangent, normal).normalized;
                priorNormal = binormal;

                for (var i = 0; i < sides; i++)
                {
                    var angle = ((float)i / sides) * Mathf.PI * 2f;
                    var offset = normal * (Mathf.Cos(angle) * radius)
                               + binormal * (Mathf.Sin(angle) * radius);
                    vertices.Add(points[p] + offset);
                }
            }

            for (var p = 0; p < points.Count - 1; p++)
            {
                var row = p * sides;
                var nextRow = (p + 1) * sides;

                for (var i = 0; i < sides; i++)
                {
                    var next = (i + 1) % sides;
                    var a = row + i;
                    var b = row + next;
                    var c = nextRow + i;
                    var d = nextRow + next;

                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);

                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            return FinalizeMesh(name, vertices, triangles);
        }

        internal static Mesh CreateTorus(
            string name,
            float majorRadius,
            float minorRadius,
            int majorSegments = 18,
            int minorSegments = 8,
            float arcDegrees = 360f)
        {
            majorSegments = Mathf.Max(3, majorSegments);
            minorSegments = Mathf.Max(3, minorSegments);

            var closed = Mathf.Approximately(Mathf.Abs(arcDegrees), 360f);
            var majorRingCount = closed ? majorSegments : majorSegments + 1;
            var vertices = new List<Vector3>(majorRingCount * minorSegments);
            var triangles = new List<int>(majorSegments * minorSegments * 6);

            for (var major = 0; major < majorRingCount; major++)
            {
                var u = (float)major / majorSegments;
                var theta = u * arcDegrees * Mathf.Deg2Rad;
                var center = new Vector3(Mathf.Cos(theta) * majorRadius, 0f, Mathf.Sin(theta) * majorRadius);
                var radial = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));

                for (var minor = 0; minor < minorSegments; minor++)
                {
                    var phi = ((float)minor / minorSegments) * Mathf.PI * 2f;
                    var offset = radial * (Mathf.Cos(phi) * minorRadius)
                               + Vector3.up * (Mathf.Sin(phi) * minorRadius);
                    vertices.Add(center + offset);
                }
            }

            var segmentCount = closed ? majorSegments : majorSegments;
            for (var major = 0; major < segmentCount; major++)
            {
                var nextMajor = closed ? (major + 1) % majorRingCount : major + 1;
                if (nextMajor >= majorRingCount)
                {
                    break;
                }

                var row = major * minorSegments;
                var nextRow = nextMajor * minorSegments;

                for (var minor = 0; minor < minorSegments; minor++)
                {
                    var nextMinor = (minor + 1) % minorSegments;
                    var a = row + minor;
                    var b = row + nextMinor;
                    var c = nextRow + minor;
                    var d = nextRow + nextMinor;

                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(b);

                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }

            return FinalizeMesh(name, vertices, triangles);
        }

        internal static Mesh CreateChamferedPanel(
            string name,
            float width,
            float height,
            float depth,
            float chamfer)
        {
            var x = width * 0.5f;
            var y = height * 0.5f;
            var c = Mathf.Clamp(chamfer, 0.001f, Mathf.Min(x, y) * 0.45f);

            return CreateExtrudedPolygon(
                name,
                new[]
                {
                    new Vector2(-x + c, -y),
                    new Vector2(x - c, -y),
                    new Vector2(x, -y + c),
                    new Vector2(x, y - c),
                    new Vector2(x - c, y),
                    new Vector2(-x + c, y),
                    new Vector2(-x, y - c),
                    new Vector2(-x, -y + c)
                },
                depth);
        }

        internal static Mesh CreateBlade(
            string name,
            float length,
            float rootHeight,
            float tipHeight,
            float depth)
        {
            return CreateExtrudedPolygon(
                name,
                new[]
                {
                    new Vector2(-length * 0.5f, -rootHeight * 0.5f),
                    new Vector2(length * 0.34f, -tipHeight * 0.5f),
                    new Vector2(length * 0.5f, 0f),
                    new Vector2(length * 0.34f, tipHeight * 0.5f),
                    new Vector2(-length * 0.5f, rootHeight * 0.5f),
                },
                depth);
        }

        internal static Mesh CreateGrate(string name, float width, float length, int bars)
        {
            bars = Mathf.Max(3, bars);
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var barWidth = width / (bars * 2f + 1f);
            var start = -width * 0.5f + barWidth;

            for (var i = 0; i < bars; i++)
            {
                var x0 = start + i * barWidth * 2f;
                AddQuad(vertices, triangles,
                    new Vector3(x0, 0f, -length * 0.5f),
                    new Vector3(x0 + barWidth, 0f, -length * 0.5f),
                    new Vector3(x0 + barWidth, 0f, length * 0.5f),
                    new Vector3(x0, 0f, length * 0.5f));
            }

            var crossCount = Mathf.Max(2, Mathf.RoundToInt(length / 1.6f));
            var crossDepth = 0.08f;
            for (var i = 0; i < crossCount; i++)
            {
                var z = Mathf.Lerp(-length * 0.5f, length * 0.5f, (i + 0.5f) / crossCount);
                AddQuad(vertices, triangles,
                    new Vector3(-width * 0.5f, 0.002f, z - crossDepth),
                    new Vector3(width * 0.5f, 0.002f, z - crossDepth),
                    new Vector3(width * 0.5f, 0.002f, z + crossDepth),
                    new Vector3(-width * 0.5f, 0.002f, z + crossDepth));
            }

            return FinalizeMesh(name, vertices, triangles);
        }

        private static void AddQuad(
            ICollection<Vector3> vertices,
            ICollection<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d)
        {
            var start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 1);

            triangles.Add(start);
            triangles.Add(start + 3);
            triangles.Add(start + 2);
        }

        private static Mesh FinalizeMesh(string name, List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh
            {
                name = name
            };

            if (vertices.Count > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, true);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
