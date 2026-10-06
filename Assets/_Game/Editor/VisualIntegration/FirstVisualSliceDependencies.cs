using System;
using System.IO;
using System.Linq;
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
        [Serializable] private sealed class Evidence { public string scene; public string[] required, dependencies, packedRequired; public bool validated; }
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
            // Prefab data is flattened into the scene; check actual model/mesh source entries in build packing.
            var models = Required.Take(4).Concat(new[] { FirstVisualSliceBuilder.Root + "/Models/Hero_Reactor.fbx", FirstVisualSliceBuilder.Root + "/Models/Deck_Module.fbx" }).ToArray();
            foreach (var path in models)
                if (!packed.Contains(path)) throw new BuildFailedException("Build report omitted required visual model: " + path);
            var evidence = new Evidence { scene = FirstVisualSliceBuilder.ScenePath, required = Required, dependencies = packed,
                packedRequired = models, validated = true };
            File.WriteAllText(Path.ChangeExtension(report.summary.outputPath, ".visual-slice.json"), JsonUtility.ToJson(evidence, true));
            File.WriteAllText("docs/first-visual-slice/apk_packed_dependencies.json", JsonUtility.ToJson(evidence, true));
            Debug.Log("FIRST_VISUAL_SLICE_APK_PACKING_PASS: " + string.Join(", ", models));
        }
    }
}
