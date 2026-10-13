using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.ArtProof.EnergyChamber
{
    /// <summary>Mirror the exact scene/dependencies to an ignored visual-only project.
    /// Production Chapter build callbacks intentionally do not apply there.</summary>
    public static class EnergyChamberDeviceExporter
    {
        public static void Export()
        {
            EnergyChamberProofBuilder.Validate();
            var source=Directory.GetCurrentDirectory();var owner=Directory.GetParent(source).Parent.FullName;
            var stage=Path.Combine(owner,".utmp/ec77");
            Directory.CreateDirectory(stage);
            File.WriteAllText(Path.Combine(stage,".gitignore"),"*\n");
            var inputs=new[]{EnergyChamberProofBuilder.ScenePath,AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline),"Assets/UniversalRenderPipelineGlobalSettings.asset"};
            var dependencies=AssetDatabase.GetDependencies(inputs,true).Where(p=>p.StartsWith("Assets/",StringComparison.Ordinal)).Distinct().OrderBy(p=>p).ToArray();
            var scripts=dependencies.Where(p=>p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".dll",StringComparison.Ordinal)).ToArray();
            if(scripts.Length!=0)throw new InvalidDataException("Visual-only export unexpectedly depends on project gameplay scripts: "+string.Join(", ",scripts));
            foreach(var path in dependencies)
            {
                var target=Path.Combine(stage,path);Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(path,target,true);
                if(File.Exists(path+".meta"))File.Copy(path+".meta",target+".meta",true);
            }
            Directory.CreateDirectory(stage+"/Packages");
            foreach(var path in new[]{"Packages/manifest.json","Packages/packages-lock.json"})if(File.Exists(path))File.Copy(path,Path.Combine(stage,path),true);
            Directory.CreateDirectory(stage+"/ProjectSettings");
            foreach(var path in Directory.GetFiles("ProjectSettings"))
                if(Path.GetFileName(path)!="EditorBuildSettings.asset")File.Copy(path,Path.Combine(stage,"ProjectSettings",Path.GetFileName(path)),true);
            Directory.CreateDirectory(stage+"/Assets/Editor");
            File.Copy("Tools/energy-chamber-hybrid-proof-v1/StageBuild.cs",stage+"/Assets/Editor/StageBuild.cs",true);
            Directory.CreateDirectory(stage+"/Assets/StreamingAssets");
            File.Copy("ThirdPartyNotices.md",stage+"/Assets/StreamingAssets/ThirdPartyNotices.txt",true);
            File.Copy(EnergyChamberProofBuilder.Root+"/Licenses/ThirdPartyNotices.md",stage+"/Assets/StreamingAssets/EnergyChamberNotices.txt",true);
            File.WriteAllText(stage+"/stage-build.json",JsonUtility.ToJson(new StageConfig{scene=EnergyChamberProofBuilder.ScenePath,output=Path.Combine(source,"Builds/Android/gravivore-energy-chamber-polish-v1-dev.apk"),result=Path.Combine(source,EnergyChamberProofBuilder.Evidence+"/verification/android-result.json")},true));
            File.WriteAllText(EnergyChamberProofBuilder.Evidence+"/verification/device-project-dependencies.txt",string.Join("\n",dependencies)+"\n");
            Debug.Log("ENERGY_CHAMBER_VISUAL_DEVICE_PROJECT_EXPORTED assets="+dependencies.Length+" projectScripts=0");
        }
        [Serializable] private sealed class StageConfig {public string scene,output,result;}
    }
}
