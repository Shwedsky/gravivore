using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Editor
{
    [Serializable]
    public sealed class ArtSpikeCharacterSnapshot
    {
        public string prefab;
        public int childRenderers, skinnedMeshRenderers, meshRenderers, uniqueMaterials, materialSlots;
        public int uniqueMeshes, meshInstances, maximumTextureSize, animators, animations, colliders;
        public long triangles;
        public Vector3 boundsMetres;
        public Vector2 projectedPixelsAt1080x1920;
        public string[] materialNames, donorFiles;
    }

    public static class ArtSpikeAudit
    {
        public static ArtSpikeCharacterSnapshot Inspect(string path)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new FileNotFoundException("Missing art-spike prefab", path);
            var root = Object.Instantiate(source);
            var camera = ArtSpikeScene.GameplayCamera("ArtAudit_TemporaryS20Camera", Vector3.zero);
            try
            {
                var materials = new HashSet<Material>();
                var meshes = new HashSet<Mesh>();
                var donors = new HashSet<string>();
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                var bounds = renderers[0].bounds;
                var snapshot = new ArtSpikeCharacterSnapshot
                {
                    prefab = path,
                    childRenderers = renderers.Length,
                    skinnedMeshRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,
                    meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true).Length,
                    colliders = root.GetComponentsInChildren<Collider>(true).Length,
                    animators = root.GetComponentsInChildren<Animator>(true).Length,
                    animations = root.GetComponentsInChildren<Animation>(true).Length
                };
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                    snapshot.materialSlots += renderer.sharedMaterials.Length;
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material == null) throw new InvalidOperationException("Unassigned art material: " + path);
                        materials.Add(material);
                        foreach (var property in material.GetTexturePropertyNames())
                        {
                            var texture = material.GetTexture(property);
                            if (texture != null)
                                snapshot.maximumTextureSize = Math.Max(snapshot.maximumTextureSize, Math.Max(texture.width, texture.height));
                        }
                    }
                }
                foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    var mesh = filter.sharedMesh;
                    if (mesh == null) throw new InvalidOperationException("Missing art mesh: " + path);
                    snapshot.meshInstances++;
                    meshes.Add(mesh);
                    var donorPath = AssetDatabase.GetAssetPath(mesh);
                    if (donorPath.StartsWith(ArtSpikeBuilder.Root + "/Imported/", StringComparison.Ordinal)) donors.Add(donorPath);
                    for (var sub = 0; sub < mesh.subMeshCount; sub++)
                    {
                        if (mesh.GetTopology(sub) != MeshTopology.Triangles)
                            throw new InvalidOperationException("Triangle count unavailable for non-triangle mesh.");
                        snapshot.triangles += (long)mesh.GetIndexCount(sub) / 3;
                    }
                }
                snapshot.uniqueMaterials = materials.Count;
                snapshot.uniqueMeshes = meshes.Count;
                snapshot.materialNames = materials.Select(m => m.name).OrderBy(n => n).ToArray();
                snapshot.donorFiles = donors.OrderBy(n => n).ToArray();
                snapshot.boundsMetres = bounds.size;
                var minimum = new Vector2(float.MaxValue, float.MaxValue);
                var maximum = new Vector2(float.MinValue, float.MinValue);
                for (var x = -1; x <= 1; x += 2)
                    for (var y = -1; y <= 1; y += 2)
                        for (var z = -1; z <= 1; z += 2)
                        {
                            var point = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                            var screen = camera.WorldToViewportPoint(point);
                            minimum = Vector2.Min(minimum, screen);
                            maximum = Vector2.Max(maximum, screen);
                        }
                snapshot.projectedPixelsAt1080x1920 = Vector2.Scale(maximum - minimum, new Vector2(1080, 1920));
                return snapshot;
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        [MenuItem("Gravivore/Art Spike/Write Performance Snapshot")]
        public static void WriteSnapshot()
        {
            Directory.CreateDirectory("docs/art-spike");
            var report = new StringBuilder("# Art Spike measured prefab snapshot\n\n");
            report.AppendLine("Counts include inactive children. Triangles sum mesh index counts per instance, including Unity primitives. Unique meshes/materials are distinct shared asset references; material slots count draw submissions before batching. Projected pixels are a conservative bounding box at the settled S20 camera (1080×1920), not a pixel-accurate silhouette mask.\n");
            foreach (var path in ArtSpikeBuilder.CharacterPaths)
            {
                var snapshot = Inspect(path);
                File.WriteAllText("docs/art-spike/" + Path.GetFileNameWithoutExtension(path) + ".json", JsonUtility.ToJson(snapshot, true));
                report.AppendLine("## " + Path.GetFileNameWithoutExtension(path) + "\n");
                report.AppendLine($"- Child renderers: {snapshot.childRenderers}; SkinnedMeshRenderer: {snapshot.skinnedMeshRenderers}; MeshRenderer: {snapshot.meshRenderers}.");
                report.AppendLine($"- Materials: {snapshot.uniqueMaterials} unique; {snapshot.materialSlots} slots. Meshes: {snapshot.uniqueMeshes} unique; {snapshot.meshInstances} instances.");
                report.AppendLine($"- Triangles: {snapshot.triangles:N0}; maximum texture size: {snapshot.maximumTextureSize} (0 means no textures).");
                report.AppendLine($"- Animation: {snapshot.animators} Animator, {snapshot.animations} legacy Animation; colliders: {snapshot.colliders}.");
                report.AppendLine($"- Bounds (metres): {snapshot.boundsMetres:F3}; S20 projected bounding box: {snapshot.projectedPixelsAt1080x1920:F1} pixels.\n");
            }
            report.AppendLine("## Performance limits\n\nThis is a structural snapshot, not Android frame-time evidence. Piston donors expose three separate renderers each; Tier 2 retains many renderer submissions. Consolidation/batching and animation must be measured before production adoption. No dynamic lights, colliders, or gameplay scripts live in character prefabs. The comparison scene has one directional light and a small bloom pass.\n");
            File.WriteAllText("docs/art-spike/PERFORMANCE.md", report.ToString());
        }
    }
}
