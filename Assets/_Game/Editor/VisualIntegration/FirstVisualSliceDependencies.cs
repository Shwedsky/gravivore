using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class FirstVisualSliceDependencies : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 20;
        public static readonly string[] Required =
        {
            G0ProductionV3Review.Model,
            FirstVisualSliceBuilder.Root + "/Models/Scout_V1.fbx",
            FirstVisualSliceBuilder.Root + "/Models/Cutter_V1.fbx",
            FirstVisualSliceBuilder.Root + "/Models/Magnetar_V1.fbx",
            FirstVisualSliceBuilder.Prefab("Environment_Slice")
        };
        [Serializable] private sealed class Evidence
        {
            public string scene, apkSha256;
            public string[] required, dependencies, modelSources, packedReportPaths, serializedRequired, serializedArchiveEntries;
            public bool validated, packedReportComplete;
        }
        public static void ValidateOrThrow()
        {
            var dependencies = AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath, true);
            foreach (var path in Required)
                if (!dependencies.Contains(path)) throw new BuildFailedException("Visual slice missing from Chapter01 production dependencies: " + path);
            var env = AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab("Environment_Slice"));
            if (env.GetComponentsInChildren<Collider>(true).Length != 0) throw new BuildFailedException("Environment art contains collision authority.");
            Directory.CreateDirectory("docs/first-visual-slice");
            File.WriteAllText("docs/first-visual-slice/production_dependencies.json", JsonUtility.ToJson(new Evidence
            { scene = FirstVisualSliceBuilder.ScenePath, required = Required, dependencies = dependencies, validated = true }, true));
            Debug.Log("FIRST_VISUAL_SLICE_DEPENDENCIES_PASS: " + string.Join(", ", Required));
        }
        public void OnPreprocessBuild(BuildReport report) => ValidateOrThrow();
        public void OnPostprocessBuild(BuildReport report)
        {
            var packed = report.packedAssets.SelectMany(a => a.contents).Select(c => c.sourceAssetPath).Distinct().ToArray();
            var models = Required.Take(4).Concat(new[] { FirstVisualSliceBuilder.Root + "/Models/Hero_Reactor.fbx", FirstVisualSliceBuilder.Root + "/Models/Deck_Module.fbx" }).ToArray();
            var dependencies = AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath, true);
            foreach (var path in models)
                if (!dependencies.Contains(path)) throw new BuildFailedException("Production scene omitted required visual model: " + path);
            // Incremental script-only reports omit cached asset entries. Inspect the APK itself:
            // exact serialized mesh names must be in sharedassets1, and the authored kit in level1.
            var names = new[] { "G0_LOD0", "Scout_V1_LOD0", "Cutter_V1_LOD0", "Magnetar_V1_LOD0",
                "First Visual Slice Industrial Containment", "Hero_Reactor", "Deck_Module" };
            var found = new string[names.Length];
            using (var archive = ZipFile.OpenRead(report.summary.outputPath))
                foreach (var entry in archive.Entries)
                {
                    var meshes = entry.FullName.StartsWith("assets/bin/Data/sharedassets1.assets", StringComparison.Ordinal);
                    var scene = entry.FullName.StartsWith("assets/bin/Data/level1", StringComparison.Ordinal);
                    if (!meshes && !scene) continue;
                    using var stream = entry.Open(); using var memory = new MemoryStream();
                    stream.CopyTo(memory); var bytes = memory.ToArray();
                    for (var i = 0; i < names.Length; i++)
                        if (found[i] == null && (i < 4 ? meshes : scene) && ContainsSerializedString(bytes, names[i]))
                            found[i] = names[i] + " => " + entry.FullName;
                }
            for (var i = 0; i < names.Length; i++)
                if (found[i] == null) throw new BuildFailedException("APK omitted required serialized visual content: " + names[i]);
            string hash;
            using (var sha = SHA256.Create()) using (var file = File.OpenRead(report.summary.outputPath))
                hash = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
            var evidence = new Evidence { scene = FirstVisualSliceBuilder.ScenePath, required = Required, dependencies = dependencies,
                modelSources = models, packedReportPaths = packed, packedReportComplete = models.All(packed.Contains),
                serializedRequired = names, serializedArchiveEntries = found, apkSha256 = hash, validated = true };
            File.WriteAllText(Path.ChangeExtension(report.summary.outputPath, ".visual-slice.json"), JsonUtility.ToJson(evidence, true));
            File.WriteAllText("docs/first-visual-slice/apk_packed_dependencies.json", JsonUtility.ToJson(evidence, true));
            Debug.Log("FIRST_VISUAL_SLICE_APK_PACKING_PASS: " + string.Join(", ", found));
        }
        private static bool ContainsSerializedString(byte[] bytes, string value)
        {
            var expected = Encoding.UTF8.GetBytes(value);
            for (var offset = 4; offset <= bytes.Length - expected.Length; offset++)
            {
                if (bytes[offset] != expected[0] || BitConverter.ToInt32(bytes, offset - 4) != expected.Length) continue;
                var match = true;
                for (var i = 1; i < expected.Length; i++)
                    if (bytes[offset + i] != expected[i]) { match = false; break; }
                if (match) return true;
            }
            return false;
        }
    }
}
