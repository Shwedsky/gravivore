using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Presentation.AudioVfx;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gravivore.Editor.Rendering
{
    public sealed class RenderingBuildAudit : IPreprocessBuildWithReport, IPostprocessBuildWithReport, IPreprocessShaders
    {
        public const string Output = "docs/render-hotfix/verification";
        public static readonly string[] UiShaders = { "UI/Default", "UI/DefaultETC1" };
        public static readonly string[] V3Prefabs = { "Custodian_V3", "Emitter_M0", "Broken_Edge_V3_0", "Broken_Edge_V3_1", "Service_Trench_V3", "Collapsed_Hull_V3", "HostileChargeV3", "HostileTravelV3" };
        public const string Atlas = "Assets/_Game/Content/ConceptFidelityV2/Materials/Fidelity_IndustrialAtlas.mat";
        public const string Hostile = "Assets/_Game/Content/Presentation/Phase6B/VFX/Materials/M_Phase6B_HostileSoft.mat";
        private static readonly HashSet<string> Compiled = new HashSet<string>();
        public int callbackOrder => int.MaxValue;
        public static SerializedObject GraphicsObject() => new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").Single());
        public static void EnsureRuntimeUiShaders()
        {
            var settings = GraphicsObject(); var included = settings.FindProperty("m_AlwaysIncludedShaders");
            foreach (var name in UiShaders)
            {
                var shader = Shader.Find(name);
                if (shader == null) throw new InvalidOperationException("Runtime uGUI shader not available: " + name);
                var found = false;
                for (var i = 0; i < included.arraySize; i++) if (included.GetArrayElementAtIndex(i).objectReferenceValue == shader) found = true;
                if (found) continue;
                included.arraySize++; included.GetArrayElementAtIndex(included.arraySize - 1).objectReferenceValue = shader;
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        public static void ValidateOrThrow()
        {
            var included = GraphicsObject().FindProperty("m_AlwaysIncludedShaders");
            foreach (var name in UiShaders)
            {
                var found = false;
                for (var i = 0; i < included.arraySize; i++) if ((included.GetArrayElementAtIndex(i).objectReferenceValue as Shader)?.name == name) found = true;
                if (!found) throw new InvalidOperationException("MAGENTA_GUARD runtime uGUI shader would be stripped: " + name);
            }
            foreach (var name in V3Prefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VisualIntegration.Chapter01V3Builder.Prefab(name));
                if (prefab == null) throw new InvalidOperationException("Missing V3 render prefab: " + name);
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true)) ValidateRenderer(renderer);
                foreach (var effect in prefab.GetComponentsInChildren<Phase6BVfxInstance>(true)) ValidateMaterial(effect.Material, name);
            }
            var atlas = AssetDatabase.LoadAssetAtPath<Material>(Atlas); var hostile = AssetDatabase.LoadAssetAtPath<Material>(Hostile);
            ValidateMaterial(atlas, Atlas); ValidateMaterial(hostile, Hostile);
            if (atlas.shader.name != "Universal Render Pipeline/Lit" || !atlas.IsKeywordEnabled("_EMISSION") || !atlas.IsKeywordEnabled("_METALLICSPECGLOSSMAP")) throw new InvalidOperationException("Industrial atlas shader/keywords changed.");
            if (hostile.shader.name != "Universal Render Pipeline/Particles/Unlit" || !hostile.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT") || hostile.renderQueue != 3000 || hostile.GetFloat("_SrcBlend") != 5 || hostile.GetFloat("_DstBlend") != 10) throw new InvalidOperationException("Hostile transparent profile changed.");
        }
        public static void ValidateMaterial(Material material, string owner)
        {
            if (material == null || material.shader == null || material.shader.name == "Hidden/InternalErrorShader") throw new InvalidOperationException("MAGENTA_GUARD invalid material: " + owner);
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null && !material.shader.isSupported) throw new InvalidOperationException("MAGENTA_GUARD unsupported shader: " + owner);
        }
        public static void ValidateRenderer(Renderer renderer)
        {
            var materials = renderer.sharedMaterials;
            if (materials.Length == 0) throw new InvalidOperationException("Renderer has no materials: " + renderer.name);
            foreach (var material in materials) ValidateMaterial(material, renderer.name);
            var mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh != null)
            {
                // A static-batched renderer addresses only its subset of the combined mesh.
                var batch = new SerializedObject(renderer).FindProperty("m_StaticBatchInfo");
                var batchCount = batch?.FindPropertyRelative("subMeshCount")?.intValue ?? 0;
                var first = batch?.FindPropertyRelative("firstSubMesh")?.intValue ?? 0;
                var effectiveCount = batchCount > 0 ? batchCount : mesh.subMeshCount;
                if (first < 0 || (batchCount > 0 && first + batchCount > mesh.subMeshCount)) throw new InvalidOperationException("Invalid static batch subset: " + renderer.name);
                if (materials.Length < effectiveCount) throw new InvalidOperationException("Incomplete submesh material coverage: " + renderer.name);
            }
            var scale = renderer.transform.lossyScale; var bounds = renderer.bounds;
            foreach (var value in new[] { scale.x, scale.y, scale.z, bounds.center.x, bounds.center.y, bounds.center.z, bounds.extents.x, bounds.extents.y, bounds.extents.z })
                if (float.IsNaN(value) || float.IsInfinity(value)) throw new InvalidOperationException("Non-finite renderer geometry: " + renderer.name);
            if (Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)) > 10 || Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) > 40) throw new InvalidOperationException("Absurd V3 renderer bounds/scale: " + renderer.name);
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            Compiled.Clear(); ValidateOrThrow(); Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,"serialized_scene_renderers.txt"),string.Empty);
            CaptureProfile();
        }
        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> variants)
        {
            if (!UiShaders.Contains(shader.name) && shader.name != "Universal Render Pipeline/Lit" && shader.name != "Universal Render Pipeline/Particles/Unlit") return;
            foreach (var variant in variants)
            {
                var keywords = variant.shaderKeywordSet.GetShaderKeywords().Select(k => k.name).ToArray();
                var isUi = UiShaders.Contains(shader.name);
                var isAtlas = shader.name == "Universal Render Pipeline/Lit" && keywords.Contains("_EMISSION") && keywords.Contains("_METALLICSPECGLOSSMAP");
                var isHostile = shader.name == "Universal Render Pipeline/Particles/Unlit" && keywords.Contains("_SURFACE_TYPE_TRANSPARENT");
                if (isUi || isAtlas || isHostile) Compiled.Add(shader.name + " | " + snippet.passName + " | " + snippet.shaderType + " | " + variant.shaderCompilerPlatform + " | " + string.Join(",", keywords.OrderBy(k => k)));
            }
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            var sceneAudit = File.ReadAllText(Path.Combine(Output,"serialized_scene_renderers.txt"));
            foreach (var scene in Build.AndroidBuild.BuildScenes)
                if (!sceneAudit.Contains(scene + ": renderers=")) throw new BuildFailedException("MAGENTA_GUARD scene renderer audit incomplete: " + scene);
            var packed = report.packedAssets.SelectMany(p => p.contents).Select(c => c.sourceAssetPath).Distinct().OrderBy(p => p).ToArray();
            var required = new[] { UrpConfigurator.UrpAssetPath, UrpConfigurator.RendererDataPath, Atlas, Hostile, AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Material>(Atlas).shader), AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Material>(Hostile).shader) };
            foreach (var path in required) if (!packed.Contains(path)) throw new BuildFailedException("MAGENTA_GUARD render dependency omitted: " + path);
            foreach (var name in new[] { "UI/Default", "UI/DefaultETC1", "Universal Render Pipeline/Lit", "Universal Render Pipeline/Particles/Unlit" })
                if (!Compiled.Any(c => c.StartsWith(name + " | ", StringComparison.Ordinal))) throw new BuildFailedException("MAGENTA_GUARD no retained target shader variant: " + name);
            File.WriteAllText(Path.Combine(Output, "compiled_shader_variants.txt"), string.Join("\n", Compiled.OrderBy(c => c)));
            File.WriteAllText(Path.Combine(Output, "render_dependency_report.json"), JsonUtility.ToJson(new Packing { validated=true, required=required, packed=packed }, true));
        }
        [Serializable] private sealed class Packing { public bool validated; public string[] required, packed; }
        [Serializable] private sealed class Profile
        {
            public string pipeline, renderer, colorSpace; public bool hdr, postprocessData, automaticAndroidApis;
            public int msaa, rendererFeatures; public float renderScale; public string[] androidApis, alwaysIncluded;
        }
        public static void CaptureProfile()
        {
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpConfigurator.UrpAssetPath);
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UrpConfigurator.RendererDataPath);
            var included=GraphicsObject().FindProperty("m_AlwaysIncludedShaders");var shaders=new List<string>();
            for(var i=0;i<included.arraySize;i++) shaders.Add((included.GetArrayElementAtIndex(i).objectReferenceValue as Shader)?.name ?? "null");
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,"render_profile.json"),JsonUtility.ToJson(new Profile {pipeline=pipeline.name,renderer=renderer.name,colorSpace=PlayerSettings.colorSpace.ToString(),hdr=pipeline.supportsHDR,msaa=pipeline.msaaSampleCount,renderScale=pipeline.renderScale,rendererFeatures=renderer.rendererFeatures.Count,postprocessData=renderer.postProcessData!=null,automaticAndroidApis=PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android),androidApis=PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(a=>a.ToString()).ToArray(),alwaysIncluded=shaders.ToArray()},true));
        }
    }
}
