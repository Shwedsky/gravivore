using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class ConnectedConceptSectorsBuildAudit : IPreprocessBuildWithReport,IPostprocessBuildWithReport
    {
        public int callbackOrder=>730;
        public void OnPreprocessBuild(BuildReport report){if(ConnectedConceptSectorsBuilder.IsInstalled)ConnectedConceptSectorsBuilder.ValidateOrThrow();}
        public void OnPostprocessBuild(BuildReport report)
        {
            if(!ConnectedConceptSectorsBuilder.IsInstalled)return;
            var cube=ConnectedConceptSectorsBuilder.Root+"/Textures/CS_IndustrialReflection.asset";
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            if(!packed.Contains(cube))throw new BuildFailedException("CS_APK shared reflection resource omitted.");
            var names=new[]{ConnectedConceptSectorsBuilder.TransferName,"CuttingFloor / authored industrial sector","CS_work_center_1","CF_pressure vessels_1"};
            var found=new bool[names.Length];
            using(var apk=ZipFile.OpenRead(report.summary.outputPath))
                foreach(var entry in apk.Entries.Where(e=>e.FullName.StartsWith("assets/bin/Data/level",StringComparison.Ordinal)&&!e.FullName.EndsWith(".resS",StringComparison.Ordinal)))
                {
                    using var source=entry.Open();using var memory=new MemoryStream();source.CopyTo(memory);var text=Encoding.UTF8.GetString(memory.ToArray());
                    for(var i=0;i<names.Length;i++)found[i]|=text.Contains(names[i]);
                }
            for(var i=0;i<names.Length;i++)if(!found[i])throw new BuildFailedException("CS_APK connected facility missing from serialized scene: "+names[i]);
            Directory.CreateDirectory(ConnectedConceptSectorsBuilder.Output);
            File.WriteAllText(ConnectedConceptSectorsBuilder.Output+"/apk-connected-facilities.json",JsonUtility.ToJson(new Evidence{validated=true,reflection=cube,serialized=names},true));
        }
        [Serializable] private sealed class Evidence{public bool validated;public string reflection;public string[] serialized;}
    }
}
