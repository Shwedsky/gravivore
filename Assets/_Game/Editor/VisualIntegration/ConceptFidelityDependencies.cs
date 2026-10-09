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
    public sealed class ConceptFidelityDependencies : IPreprocessBuildWithReport,IPostprocessBuildWithReport
    {
        public int callbackOrder=>41;
        public void OnPreprocessBuild(BuildReport report)=>ConceptFidelityBuilder.ValidateOrThrow();
        [Serializable] private sealed class Evidence
        {public bool validated;public string apkSha256;public string[] required,serializedEntries,dependencies;}
        public void OnPostprocessBuild(BuildReport report)
        {
            if (Chapter01BlueprintWorldBuilder.IsBlueprintWorld()) return;
            var names=ConceptFidelityBuilder.StaticAssets.Concat(ConceptFidelityBuilder.AnimatedAssets.Select(n=>(n=="Custodian_V2"?"Custodian_V3":n)+"_LOD0"))
                .Concat(Chapter01V3Builder.Models.Where(n=>n!="Custodian_V3"))
                .Concat(new[]{"ConceptFidelity","Fidelity_IndustrialAtlas","Chapter 01 Concept Fidelity V2"}).ToArray();
            var found=new string[names.Length];
            using(var archive=ZipFile.OpenRead(report.summary.outputPath))foreach(var entry in archive.Entries)
            {
                if(!entry.FullName.StartsWith("assets/bin/Data/sharedassets",StringComparison.Ordinal)&&!entry.FullName.StartsWith("assets/bin/Data/level",StringComparison.Ordinal))continue;
                if(entry.FullName.Contains(".resS"))continue;
                using var source=entry.Open();using var memory=new MemoryStream();source.CopyTo(memory);var bytes=memory.ToArray();
                for(var i=0;i<names.Length;i++)if(found[i]==null&&Serialized(bytes,names[i]))found[i]=names[i]+" => "+entry.FullName;
            }
            for(var i=0;i<found.Length;i++)if(found[i]==null)throw new BuildFailedException("APK is missing authored fidelity content: "+names[i]);
            string hash;using(var sha=SHA256.Create())using(var apk=File.OpenRead(report.summary.outputPath))hash=BitConverter.ToString(sha.ComputeHash(apk)).Replace("-","").ToLowerInvariant();
            Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification");
            File.WriteAllText("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/verification/apk_packed_dependencies.json",JsonUtility.ToJson(new Evidence{
                validated=true,apkSha256=hash,required=names,serializedEntries=found,dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true)},true));
            Debug.Log("CONCEPT_FIDELITY_APK_PACKING_PASS "+hash);
        }
        private static bool Serialized(byte[] data,string value)
        {
            var bytes=Encoding.UTF8.GetBytes(value);
            for(var p=4;p<=data.Length-bytes.Length;p++)
            {
                if(data[p]!=bytes[0]||BitConverter.ToInt32(data,p-4)!=bytes.Length)continue;var match=true;
                for(var i=1;i<bytes.Length;i++)if(data[p+i]!=bytes[i]){match=false;break;}
                if(match)return true;
            }
            return false;
        }
    }
}
