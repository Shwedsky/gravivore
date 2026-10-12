using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Assets;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class ActorConceptWorldBuildAudit:IPreprocessBuildWithReport,IPostprocessBuildWithReport,IPreprocessShaders
    {
        public const string ShaderName="Gravivore/Actor Industrial Lit";
        private static readonly HashSet<string> Variants=new HashSet<string>();
        public int callbackOrder=>10000;
        public static void Validate()
        {
            var profiles=AssetDatabase.LoadAssetAtPath<HostilePresentationDefinition>(Chapter01ActorIntegrationBuilder.Root+"/HostileMotion.asset");
            if(profiles.Profiles.Any(p=>p.MaximumWallOffset>.1f))throw new InvalidOperationException("Actor wall clearance must use the physical envelope");
            if(profiles.Profiles[3].AttackColor.b<profiles.Profiles[3].AttackColor.r*3)throw new InvalidOperationException("Arc Drone blue authority lost");
            foreach(var actor in Chapter01ActorIntegrationBuilder.Actors)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01ActorIntegrationBuilder.Prefab(actor));
                foreach(var material in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).SelectMany(s=>s.sharedMaterials))
                {
                    if(material.shader.name!=ShaderName||material.renderQueue!=2000)throw new InvalidOperationException(actor+" industrial opaque material lost");
                    foreach(var map in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap","_EmissionMap"})if(material.GetTexture(map)==null)throw new InvalidOperationException(actor+" missing "+map);
                }
            }
        }
        public void OnPreprocessBuild(BuildReport report){Validate();Variants.Clear();}
        public void OnProcessShader(Shader shader,ShaderSnippetData snippet,IList<ShaderCompilerData> data)
        {
            if(shader.name!=ShaderName)return;
            foreach(var variant in data)Variants.Add(snippet.passName+" | "+snippet.shaderType+" | "+variant.shaderCompilerPlatform+" | "+string.Join(",",variant.shaderKeywordSet.GetShaderKeywords().Select(k=>k.name).OrderBy(k=>k)));
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            var dependencies=AssetDatabase.GetDependencies(Chapter01BlueprintWorldBuilder.ScenePath,true);
            var required=dependencies.Where(p=>p.StartsWith(Chapter01ActorIntegrationBuilder.Root+"/Materials/")||p.StartsWith(Chapter01ActorIntegrationBuilder.Root+"/Textures/")||p.EndsWith("ActorIndustrialLit.shader")).Distinct().ToArray();
            foreach(var path in required)if(!packed.Contains(path))throw new BuildFailedException("Actor concept APK omitted "+path);
            if(Variants.Count==0||!Variants.Any(v=>v.Contains("GLES3x"))||!Variants.Any(v=>v.Contains("Vulkan")))throw new BuildFailedException("Actor industrial target variants absent");
            Directory.CreateDirectory(ActorConceptWorldBuilder.Evidence+"/verification");
            File.WriteAllLines(ActorConceptWorldBuilder.Evidence+"/verification/actor-target-shader-variants.txt",Variants.OrderBy(v=>v));
            File.WriteAllText(ActorConceptWorldBuilder.Evidence+"/verification/actor-apk-resources.json",JsonUtility.ToJson(new Evidence{passed=true,required=required,packed=packed.Where(p=>p.StartsWith(Chapter01ActorIntegrationBuilder.Root)).OrderBy(p=>p).ToArray(),shaderVariants=Variants.Count},true));
        }
        [Serializable]private sealed class Evidence{public bool passed;public string[] required,packed;public int shaderVariants;}
    }
}
