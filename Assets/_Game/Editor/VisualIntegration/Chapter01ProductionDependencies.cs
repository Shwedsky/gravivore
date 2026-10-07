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
    public sealed class Chapter01ProductionDependencies : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 31;
        public static string[] Required => Chapter01ProductionBuilder.Machines.Concat(Chapter01ProductionBuilder.Kit)
            .Select(n=>Chapter01ProductionBuilder.Root+"/Models/"+n+".fbx")
            .Concat(new[]{Chapter01ProductionBuilder.Prefab("Full_Chapter_Environment")})
            .Concat(new[]{"relay-yard","cutting-floor","shield-dump","capacitor-field","hauler-graveyard"}.Select(n=>Chapter01ProductionBuilder.Prefab("Zone_"+n)))
            .Concat(FirstVisualSliceDependencies.Required).ToArray();
        [Serializable] private sealed class Evidence { public bool validated; public string sourceSha,apkSha256; public string[] dependencies,required,packedAssets,serializedEntries; }
        public static void ValidateOrThrow()
        {
            var deps=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
            // Zone packages are retained as nested prefab dependencies as well as inspectable art packages.
            foreach(var path in Required) if(!deps.Contains(path)) throw new BuildFailedException("Chapter01 production dependency missing: "+path);
            Directory.CreateDirectory("docs/chapter01-production/verification");
            File.WriteAllText("docs/chapter01-production/verification/production_dependencies.json",JsonUtility.ToJson(new Evidence{validated=true,dependencies=deps,required=Required},true));
            Debug.Log("CHAPTER01_PRODUCTION_DEPENDENCIES_PASS");
        }
        public void OnPreprocessBuild(BuildReport report)=>ValidateOrThrow();
        public void OnPostprocessBuild(BuildReport report)
        {
            var names=Chapter01ProductionBuilder.Machines.Select(n=>n+"_LOD0").Concat(Chapter01ProductionBuilder.Kit)
                .Concat(new[]{"Chapter 01 Full Production","relay-yard","cutting-floor","shield-dump","capacitor-field","hauler-graveyard"}).ToArray();
            var found=new string[names.Length];
            using(var archive=ZipFile.OpenRead(report.summary.outputPath)) foreach(var entry in archive.Entries)
            {
                if(!entry.FullName.StartsWith("assets/bin/Data/sharedassets",StringComparison.Ordinal)&&!entry.FullName.StartsWith("assets/bin/Data/level1",StringComparison.Ordinal)) continue;
                if(entry.FullName.Contains(".resS")) continue;
                using var stream=entry.Open();using var memory=new MemoryStream();stream.CopyTo(memory);var bytes=memory.ToArray();
                for(var i=0;i<names.Length;i++) if(found[i]==null && ContainsSerialized(bytes,names[i]))found[i]=names[i]+" => "+entry.FullName;
            }
            for(var i=0;i<found.Length;i++)if(found[i]==null)throw new BuildFailedException("APK lacks serialized Chapter01 production asset: "+names[i]);
            string hash;using(var sha=SHA256.Create())using(var file=File.OpenRead(report.summary.outputPath))hash=BitConverter.ToString(sha.ComputeHash(file)).Replace("-","").ToLowerInvariant();
            var evidence=new Evidence{validated=true,apkSha256=hash,required=Required,dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true),
                packedAssets=report.packedAssets.SelectMany(a=>a.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray(),serializedEntries=found};
            File.WriteAllText("docs/chapter01-production/verification/apk_packed_dependencies.json",JsonUtility.ToJson(evidence,true));
            Debug.Log("CHAPTER01_APK_PACKING_PASS: "+hash);
        }
        private static bool ContainsSerialized(byte[] bytes,string value)
        {var expected=Encoding.UTF8.GetBytes(value);for(var offset=4;offset<=bytes.Length-expected.Length;offset++)
            {if(bytes[offset]!=expected[0]||BitConverter.ToInt32(bytes,offset-4)!=expected.Length)continue;var match=true;
                for(var i=1;i<expected.Length;i++)if(bytes[offset+i]!=expected[i]){match=false;break;}if(match)return true;}return false;}
    }
}
