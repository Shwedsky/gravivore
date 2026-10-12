using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class Chapter01CompleteConceptBuildAudit : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder=>740;
        public void OnPreprocessBuild(BuildReport report)
        {if(Chapter01CompleteConceptBuilder.IsInstalled)Chapter01CompleteConceptBuilder.ValidateOrThrow();}
        public void OnPostprocessBuild(BuildReport report)
        {
            if(!Chapter01CompleteConceptBuilder.IsInstalled)return;
            var required=AssetDatabase.GetDependencies(Chapter01BlueprintWorldBuilder.ScenePath,true).Where(p=>
                p.StartsWith(Chapter01CompleteConceptBuilder.Root+"/Materials/",StringComparison.Ordinal)||
                p.StartsWith(Chapter01CompleteConceptBuilder.Root+"/Textures/",StringComparison.Ordinal)).Distinct().ToArray();
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            foreach(var path in required)if(!packed.Contains(path))throw new BuildFailedException("CHAPTER_APK PBR resource missing: "+path);
            var names=Chapter01CompleteConceptBuilder.Models.Take(6).Select(m=>m+" / authored industrial sector")
                .Concat(new[]{Chapter01CompleteConceptBuilder.NetworkName,ConnectedConceptSectorsBuilder.TransferName,"CapacitorField / authored industrial sector","CuttingFloor / authored industrial sector"}).ToArray();
            var found=new bool[names.Length];
            using(var apk=ZipFile.OpenRead(report.summary.outputPath))
                foreach(var entry in apk.Entries.Where(e=>e.FullName.StartsWith("assets/bin/Data/level",StringComparison.Ordinal)&&!e.FullName.EndsWith(".resS",StringComparison.Ordinal)))
                {
                    using var source=entry.Open();using var memory=new MemoryStream();source.CopyTo(memory);
                    var text=Encoding.UTF8.GetString(memory.ToArray());for(var i=0;i<names.Length;i++)found[i]|=text.Contains(names[i]);
                }
            for(var i=0;i<names.Length;i++)if(!found[i])throw new BuildFailedException("CHAPTER_APK installation absent: "+names[i]);
            Directory.CreateDirectory(Chapter01CompleteConceptBuilder.Output);
            File.WriteAllText(Chapter01CompleteConceptBuilder.Output+"/apk-chapter-resources.json",JsonUtility.ToJson(new Evidence{validated=true,resources=required,serializedInstallations=names},true));
        }
        [Serializable] private sealed class Evidence{public bool validated;public string[] resources,serializedInstallations;}
    }
}
