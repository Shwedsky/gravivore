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
    public sealed class PostDeviceCombatDependencies : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder=>30;
        private static readonly string[] Names={"PostDevicePresentation","Step","P0_PlayerStep_01","P0_PlayerStep_02","P0_PlayerStep_03",
            "P0_Servo_01","P0_Servo_02","P0_LashCharge","P0_LashRelease","P0_LashImpact"};
        [Serializable] private sealed class Evidence { public string apkSha256; public string[] serializedEntries,requiredDependencies; public bool validated; }
        public void OnPreprocessBuild(BuildReport report)
        {
            var dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
            if(!dependencies.Contains(PostDeviceCombatBuilder.SettingsPath)) throw new BuildFailedException("Canonical combat readability settings are not wired.");
            var settings=AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.Combat.PostDevicePresentationDefinition>(PostDeviceCombatBuilder.SettingsPath);
            settings.ValidateOrThrow();
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            var found=new string[Names.Length];
            using(var archive=ZipFile.OpenRead(report.summary.outputPath))
            foreach(var entry in archive.Entries)
            {
                // Android splits serialized archives and writes streamed AudioClip
                // metadata into GUID-named files. Inspect both, excluding raw streams
                // and IL2CPP metadata where class names would be false positives.
                if(!IsSerializedArchive(entry.FullName)) continue;
                using var stream=entry.Open(); using var memory=new MemoryStream(); stream.CopyTo(memory); var bytes=memory.ToArray();
                for(var i=0;i<Names.Length;i++) if(found[i]==null && Contains(bytes,Names[i])) found[i]=Names[i]+" => "+entry.FullName;
            }
            for(var i=0;i<Names.Length;i++) if(found[i]==null) throw new BuildFailedException("APK omitted post-device content: "+Names[i]);
            using var sha=SHA256.Create(); using var file=File.OpenRead(report.summary.outputPath);
            var hash=BitConverter.ToString(sha.ComputeHash(file)).Replace("-","").ToLowerInvariant();
            var evidence=new Evidence { apkSha256=hash,serializedEntries=found,validated=true,
                requiredDependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true).Where(p=>p==PostDeviceCombatBuilder.SettingsPath || p.EndsWith(".wav",StringComparison.Ordinal)).ToArray() };
            var json=JsonUtility.ToJson(evidence,true);
            File.WriteAllText(Path.ChangeExtension(report.summary.outputPath,".post-device.json"),json);
            Directory.CreateDirectory("docs/post-device-combat-readability"); File.WriteAllText("docs/post-device-combat-readability/apk_packed_dependencies.json",json);
            Debug.Log("POST_DEVICE_APK_CONTENT_VERIFIED: "+string.Join(", ",found));
        }
        private static bool IsSerializedArchive(string path)
        {
            const string prefix="assets/bin/Data/";
            if(!path.StartsWith(prefix,StringComparison.Ordinal)) return false;
            var name=path.Substring(prefix.Length);
            if(name.StartsWith("sharedassets",StringComparison.Ordinal))
                return name.EndsWith(".assets",StringComparison.Ordinal) || name.Contains(".assets.split");
            if(name.Length!=32) return false;
            foreach(var character in name)
                if(!((character>='0' && character<='9') || (character>='a' && character<='f'))) return false;
            return true;
        }
        private static bool Contains(byte[] bytes,string name)
        {
            var text=Encoding.UTF8.GetBytes(name);
            for(var offset=4;offset<=bytes.Length-text.Length;offset++)
            {
                if(bytes[offset]!=text[0] || BitConverter.ToInt32(bytes,offset-4)!=text.Length) continue;
                var matches=true; for(var i=1;i<text.Length;i++) if(bytes[offset+i]!=text[i]) { matches=false; break; }
                if(matches) return true;
            }
            return false;
        }
    }
}
