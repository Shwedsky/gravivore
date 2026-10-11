using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class Chapter01ActorIntegrationBuildAudit : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 35;
        public void OnPreprocessBuild(BuildReport report)=>Chapter01ActorIntegrationBuilder.Validate();
        public void OnPostprocessBuild(BuildReport report)
        {
            var root=Chapter01ActorIntegrationBuilder.Root;
            var actors=Chapter01ActorIntegrationBuilder.Actors;
            var required=actors.Select(a=>root+"/Models/"+a+"_V2.fbx").ToArray();
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            var names=actors.SelectMany(a=>Enumerable.Range(0,3).Select(l=>a+"_LOD"+l))
                .Concat(new[]{"Chapter 01 Blueprint World R2","Scout_V2","Cutter_V2","Warden_V2","ArcDrone_V2","Carrier_V2","Magnetar_V2","Custodian_V2"}).ToArray();
            var found=new string[names.Length];
            using(var archive=ZipFile.OpenRead(report.summary.outputPath))
                foreach(var entry in archive.Entries)
                {
                    if(!entry.FullName.StartsWith("assets/bin/Data/"))continue;
                    if(!entry.FullName.Contains(".assets")&&!entry.FullName.Contains("/level"))continue;
                    using var stream=entry.Open();using var memory=new MemoryStream();stream.CopyTo(memory);
                    var content=Encoding.UTF8.GetString(memory.ToArray());
                    for(var i=0;i<names.Length;i++)if(found[i]==null&&content.Contains(names[i]))found[i]=names[i]+" => "+entry.FullName;
                    if(entry.FullName.EndsWith("/level0")&&content.Contains("ActorProductionV2_Review"))
                        throw new BuildFailedException("Review scene is startup scene");
                }
            for(var i=0;i<found.Length;i++)if(found[i]==null)throw new BuildFailedException("Integrated APK omitted "+names[i]);
            string digest;using(var sha=SHA256.Create())using(var file=File.OpenRead(report.summary.outputPath))
                digest=BitConverter.ToString(sha.ComputeHash(file)).Replace("-","").ToLowerInvariant();
            var record=new Evidence{validated=true,apkSha256=digest,requiredModels=required,packedActorPaths=packed.Where(p=>p.StartsWith(root)).OrderBy(p=>p).ToArray(),
                reportContainsAllActorModels=required.All(packed.Contains),serializedEvidence=found,buildScenes=Build.AndroidBuild.BuildScenes};
            Directory.CreateDirectory(Chapter01ActorIntegrationBuilder.Evidence+"/verification");
            File.WriteAllText(Chapter01ActorIntegrationBuilder.Evidence+"/verification/apk-actor-world-packing.json",JsonUtility.ToJson(record,true));
            Debug.Log("CHAPTER01_INTEGRATED_APK_PACKING_PASS");
        }
        [Serializable] private sealed class Evidence
        {public bool validated,reportContainsAllActorModels;public string apkSha256;public string[] requiredModels,packedActorPaths,serializedEvidence,buildScenes;}
    }
}
