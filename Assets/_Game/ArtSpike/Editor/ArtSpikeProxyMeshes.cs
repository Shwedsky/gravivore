using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gravivore.ArtSpike.Editor
{
    // Original temporary proxy geometry. Never reads or transforms a third-party character mesh.
    internal static class ArtSpikeProxyMeshes
    {
        internal const string Folder = ArtSpikeBuilder.Root + "/Proxy/Models";

        public static void Build()
        {
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            Write("Chassis", b =>
            {
                b.Box(Vector3.zero, new Vector3(1, .72f, 1), 1);
                foreach (var side in new[] { -1, 1 })
                {
                    b.Box(new Vector3(side * .29f, .40f, -.04f), new Vector3(.36f, .18f, .78f), 0);
                    b.Box(new Vector3(side * .46f, -.08f, 0), new Vector3(.09f, .31f, .65f), 1);
                    for (var n = 0; n < 4; n++)
                        b.Box(new Vector3(side * .30f, .505f, -.29f + n * .12f), new Vector3(.22f, .025f, .025f), 1);
                }
                b.Box(new Vector3(0, -.35f, .39f), new Vector3(.48f, .13f, .19f), 1);
            });
            Write("UpperSupport", b =>
            {
                b.Box(Vector3.zero, new Vector3(.91f, .65f, .73f), 0);
                b.Box(new Vector3(0, -.34f, 0), new Vector3(.62f, .18f, .58f), 1);
                b.Cylinder(new Vector3(-.31f, 0, 0), .23f, .94f, Quaternion.Euler(90, 0, 0), 1);
                b.Cylinder(new Vector3(.31f, 0, 0), .23f, .94f, Quaternion.Euler(90, 0, 0), 1);
            });
            Write("LowerSupport", b =>
            {
                b.Box(Vector3.zero, new Vector3(.72f, .95f, .76f), 0);
                foreach (var side in new[] { -1, 1 })
                    b.Cylinder(new Vector3(side * .35f, 0, .32f), .06f, .94f, Quaternion.identity, 1);
                b.Cylinder(new Vector3(0, .34f, 0), .19f, 1, Quaternion.Euler(90, 0, 0), 1);
            });
            Write("Foot", b =>
            {
                b.Box(new Vector3(0, -.20f, 0), new Vector3(1, .52f, 1), 1);
                b.Box(new Vector3(0, .14f, -.11f), new Vector3(.79f, .46f, .68f), 0);
                for (var n = -1; n <= 1; n++)
                    b.Box(new Vector3(n * .26f, .10f, .35f), new Vector3(.16f, .28f, .24f), 0);
            });
            Write("FlankPlate", b =>
            {
                b.Box(new Vector3(0, -.20f, 0), new Vector3(.60f, .60f, .87f), 1);
                b.Box(new Vector3(0, .13f, 0), new Vector3(1, .58f, 1), 0);
                for (var n = 0; n < 3; n++)
                    b.Box(new Vector3(0, .455f, -.28f + n * .22f), new Vector3(.67f, .04f, .05f), 1);
            });
            Write("WeaponHousing", b =>
            {
                b.Box(new Vector3(0, 0, -.13f), new Vector3(.83f, .90f, .68f), 0);
                foreach (var side in new[] { -1, 1 })
                {
                    b.Box(new Vector3(side * .38f, -.03f, .20f), new Vector3(.20f, .70f, .62f), 1);
                    b.Cylinder(new Vector3(side * .20f, .12f, .05f), .065f, .82f, Quaternion.Euler(90, 0, 0), 1);
                }
                b.Box(new Vector3(0, .44f, -.13f), new Vector3(.61f, .10f, .42f), 1);
            });
            Write("EmitterFork", b =>
            {
                b.Box(new Vector3(0, 0, -.34f), new Vector3(.80f, .80f, .22f), 1);
                foreach (var side in new[] { -1, 1 })
                    b.Box(new Vector3(side * .31f, 0, .03f), new Vector3(.24f, .66f, .87f), 0);
                b.Box(new Vector3(0, -.25f, -.07f), new Vector3(.55f, .15f, .44f), 1);
            });
            Write("ShearBlade", b =>
            {
                b.Box(new Vector3(0, 0, -.24f), new Vector3(1, .72f, .46f), 1);
                b.Blade();
            });
            AssetDatabase.SaveAssets();
        }

        public static void BuildPlayerRefinement()
        {
            Write("MechCarapace", b =>
            {
                b.Box(Vector3.zero, new Vector3(.70f, .76f, .72f), 1, .7f);
                foreach (var side in new[] { -1, 1 })
                    b.Box(new Vector3(side * .25f, .12f, .12f), new Vector3(.43f, .88f, .68f), 0, .65f);
                b.Cylinder(new Vector3(0, -.35f, -.08f), .19f, .55f, Quaternion.Euler(0, 0, 90), 1);
            });
            Write("MechPelvis", b =>
            {
                b.Box(Vector3.zero, new Vector3(1, .8f, 1), 0, .58f);
                b.Cylinder(Vector3.zero, .32f, 1.1f, Quaternion.Euler(0, 0, 90), 1);
            });
            Write("MechForearm", b =>
            {
                b.Cylinder(Vector3.zero, .37f, .88f, Quaternion.Euler(90, 0, 0), 1);
                foreach (var side in new[] { -1, 1 })
                    b.Box(new Vector3(side * .31f, 0, -.06f), new Vector3(.3f, .82f, .8f), 0, .7f);
            });
            AssetDatabase.SaveAssets();
        }

        private static void Write(string name, System.Action<Geometry> author)
        {
            var path = Folder + "/" + name + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = "ProjectProxy_" + name }; AssetDatabase.CreateAsset(mesh, path); }
            var builder = new Geometry();
            author(builder);
            builder.Apply(mesh);
            EditorUtility.SetDirty(mesh);
        }

        private sealed class Geometry
        {
            private readonly List<Vector3> _vertices = new List<Vector3>();
            private readonly List<Vector3> _normals = new List<Vector3>();
            private readonly List<Vector2> _uv = new List<Vector2>();
            private readonly List<int>[] _triangles = { new List<int>(), new List<int>() };

            // Chamfered octagonal footprint and separate top/bottom bevel bands.
            public void Box(Vector3 centre, Vector3 size, int material, float lowerTaper = 1f)
            {
                var footprint = new[] { new Vector2(-.38f,-.5f), new Vector2(.38f,-.5f),
                    new Vector2(.5f,-.38f), new Vector2(.5f,.38f), new Vector2(.38f,.5f),
                    new Vector2(-.38f,.5f), new Vector2(-.5f,.38f), new Vector2(-.5f,-.38f) };
                var levels = new[] { -.5f, -.36f, .36f, .5f };
                var rings = new Vector3[4, 8];
                for (var y = 0; y < 4; y++)
                    for (var p = 0; p < 8; p++)
                    {
                        var bevel = (y == 0 || y == 3 ? .88f : 1f) * Mathf.Lerp(lowerTaper, 1f, (levels[y] + .5f));
                        rings[y, p] = centre + Vector3.Scale(size,
                            new Vector3(footprint[p].x * bevel, levels[y], footprint[p].y * bevel));
                    }
                for (var y = 0; y < 3; y++)
                    for (var p = 0; p < 8; p++)
                        Face(new[] { rings[y,p], rings[y,(p+1)%8], rings[y+1,(p+1)%8], rings[y+1,p] }, centre, material);
                for (var y = 0; y <= 3; y += 3)
                {
                    var cap = new Vector3[8];
                    for (var p = 0; p < 8; p++) cap[p] = rings[y,p];
                    Face(cap, centre, material);
                }
            }

            public void Cylinder(Vector3 centre, float radius, float length, Quaternion rotation, int material)
            {
                const int sides = 12;
                var bottom = new Vector3[sides]; var top = new Vector3[sides];
                for (var i = 0; i < sides; i++)
                {
                    var a = i * Mathf.PI * 2 / sides;
                    var r = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
                    bottom[i] = centre + rotation * (r - Vector3.up * length / 2);
                    top[i] = centre + rotation * (r + Vector3.up * length / 2);
                }
                for (var i = 0; i < sides; i++)
                    Face(new[] { bottom[i], bottom[(i+1)%sides], top[(i+1)%sides], top[i] }, centre, material);
                Face(bottom, centre, material); Face(top, centre, material);
            }

            public void Blade()
            {
                var left = new[] { new Vector3(-.44f,-.3f,-.25f), new Vector3(-.44f,-.3f,.28f), new Vector3(0,-.3f,.53f), new Vector3(.44f,-.3f,-.25f) };
                var right = new Vector3[4];
                for (var i = 0; i < 4; i++) right[i] = left[i] + Vector3.up * .6f;
                Face(left, Vector3.zero, 0); Face(right, Vector3.zero, 0);
                for (var i = 0; i < 4; i++) Face(new[] { left[i], left[(i+1)%4], right[(i+1)%4], right[i] }, Vector3.zero, 0);
            }

            private void Face(Vector3[] points, Vector3 centre, int material)
            {
                var normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;
                var middle = Vector3.zero; foreach (var p in points) middle += p / points.Length;
                var reverse = Vector3.Dot(normal, middle - centre) < 0;
                if (reverse) normal = -normal;
                var start = _vertices.Count;
                foreach (var p in points)
                {
                    _vertices.Add(p); _normals.Add(normal);
                    // Planar UVs at module scale. PBR maps remain shared; no textureless metal substitution.
                    var n = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
                    _uv.Add(n.y >= n.x && n.y >= n.z ? new Vector2(p.x, p.z) :
                        n.x >= n.z ? new Vector2(p.z, p.y) : new Vector2(p.x, p.y));
                }
                for (var i = 1; i < points.Length - 1; i++)
                {
                    _triangles[material].Add(start);
                    _triangles[material].Add(start + (reverse ? i + 1 : i));
                    _triangles[material].Add(start + (reverse ? i : i + 1));
                }
            }

            public void Apply(Mesh mesh)
            {
                mesh.Clear(); mesh.SetVertices(_vertices); mesh.SetNormals(_normals); mesh.SetUVs(0, _uv);
                mesh.subMeshCount = 2;
                mesh.SetTriangles(_triangles[0], 0); mesh.SetTriangles(_triangles[1], 1);
                mesh.RecalculateBounds(); mesh.RecalculateTangents();
            }
        }
    }
}
