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
        public int uniqueMeshes, meshInstances, maximumTextureSize, uniqueTextures, animators, animations, colliders;
        public long triangles;
        public Vector3 boundsMetres;
        public Vector2 projectedPixelsAt1080x1920;
        public string[] materialNames, donorFiles, ownedMeshFiles, textureFiles;
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
                var owned = new HashSet<string>();
                var textures = new HashSet<string>();
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
                            {
                                snapshot.maximumTextureSize = Math.Max(snapshot.maximumTextureSize, Math.Max(texture.width, texture.height));
                                textures.Add(AssetDatabase.GetAssetPath(texture));
                            }
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
                    else if (donorPath.StartsWith(ArtSpikeBuilder.Root + "/", StringComparison.Ordinal)) owned.Add(donorPath);
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
                snapshot.ownedMeshFiles = owned.OrderBy(n => n).ToArray();
                snapshot.textureFiles = textures.OrderBy(n => n).ToArray();
                snapshot.uniqueTextures = textures.Count;
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
                report.AppendLine($"- Triangles: {snapshot.triangles:N0}; textures: {snapshot.uniqueTextures} shared maps, maximum size {snapshot.maximumTextureSize}.");
                report.AppendLine($"- Animation: {snapshot.animators} Animator, {snapshot.animations} legacy Animation; colliders: {snapshot.colliders}.");
                report.AppendLine($"- Bounds (metres): {snapshot.boundsMetres:F3}; S20 projected bounding box: {snapshot.projectedPixelsAt1080x1920:F1} pixels.\n");
            }
            report.AppendLine("## Performance limits\n\nV3 uses original articulated proxy modules with two PBR submeshes per mechanical module, not a blanket combine of donor characters. Renderer caps: Tier 0 <=30, Tier 1 <=35, Tier 2 <=40, Cutter <=25; target <=50,000 triangles each. Slots above expose the unbatched draw cost, including shadow submissions separately at runtime. Four shared 1024-pixel PBR maps use mipmaps and an Android ASTC 6x6 override. The unchanged source ARM file is retained for provenance but not referenced by renderers. The scene also uses one shared 32×32-per-face static reflection cubemap, one directional light and modest bloom. Static character prefabs have no Animator, Animation, colliders, lights or gameplay scripts. Pivot articulation is an editor-only preview. This structural snapshot does not establish Android frame time or 60 FPS; batching, shadows, repeated enemies and a future gait require device profiling before production adoption.\n");
            File.WriteAllText("docs/art-spike/PERFORMANCE.md", report.ToString());
        }
    }
}
