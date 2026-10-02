using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Gravivore.ArtSpike.Editor
{
    public static class ArtSpikeProbe
    {
        public static void Run()
        {
            var output = new StringBuilder();
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Game/ArtSpike/Imported" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var root = Object.Instantiate(prefab);
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                output.AppendLine($"{path}: size {bounds.size}, center {bounds.center}");
                foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                    output.AppendLine($"  {filter.name}: {filter.sharedMesh.vertexCount} vertices; {filter.sharedMesh.subMeshCount} submeshes");
                Object.DestroyImmediate(root);
            }
            Directory.CreateDirectory("Builds/ArtSpike");
            File.WriteAllText("Builds/ArtSpike/donor-probe.txt", output.ToString());
            Debug.Log("ART SPIKE donor probe completed.");
        }
    }
}
