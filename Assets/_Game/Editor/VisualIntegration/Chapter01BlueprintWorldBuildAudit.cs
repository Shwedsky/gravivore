using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class Chapter01BlueprintWorldBuildAudit : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 720;
        public void OnPreprocessBuild(BuildReport report)
        { if (Chapter01BlueprintWorldBuilder.IsBlueprintWorld()) Chapter01BlueprintWorldBuilder.Audit(); }
        public void OnPostprocessBuild(BuildReport report)
        {
            if (!Chapter01BlueprintWorldBuilder.IsBlueprintWorld()) return;
            var packed = report.packedAssets.SelectMany(p => p.contents).Select(c => c.sourceAssetPath).Distinct().ToArray();
            var deps = AssetDatabase.GetDependencies(Chapter01BlueprintWorldBuilder.ScenePath,true);
            var required = deps.Where(p => p.StartsWith(Chapter01BlueprintWorldBuilder.Root + "/Textures/",StringComparison.Ordinal) ||
                p.StartsWith(Chapter01BlueprintWorldBuilder.Root + "/Materials/",StringComparison.Ordinal)).Concat(
                ConceptConvergenceV47Builder.Actors.Select(a => "Assets/_Game/Content/" + a.folder + "/Models/" + a.name + ".fbx")).ToArray();
            foreach (var path in required)
                if (!packed.Contains(path)) throw new BuildFailedException("Rebuilt world or preserved actor resource not packed: " + path);
            var names = Chapter01BlueprintWorldBuilder.ReadLayout().sectors.Select(s => s.model + " / authored industrial sector")
                .Concat(new[] { Chapter01BlueprintWorldBuilder.Layer,"Authored cross-sector service network" }).ToArray();
            var found = new bool[names.Length];
            using (var archive = ZipFile.OpenRead(report.summary.outputPath))
                foreach (var entry in archive.Entries.Where(e => e.FullName.StartsWith("assets/bin/Data/level",StringComparison.Ordinal) && !e.FullName.EndsWith(".resS",StringComparison.Ordinal)))
                {
                    using var source = entry.Open(); using var memory = new MemoryStream(); source.CopyTo(memory); var bytes = memory.ToArray();
                    for (var i = 0; i < names.Length; i++) found[i] |= ContainsSerialized(bytes,names[i]);
                    foreach (var old in new[] { ConceptCorrectiveV45Builder.Layer,SurfaceHeroV46Builder.Layer,ConceptConvergenceV47Builder.Layer,"Flush industrial panel deck V47","Compact small-panel deck" })
                        if (ContainsSerialized(bytes,old)) throw new BuildFailedException("Historical world remains in serialized production scene: " + old);
                }
            for (var i = 0; i < found.Length; i++) if (!found[i]) throw new BuildFailedException("Rebuilt sector absent from APK: " + names[i]);
            using var file = File.OpenRead(report.summary.outputPath); using var hash = SHA256.Create();
            var sha = BitConverter.ToString(hash.ComputeHash(file)).Replace("-","").ToLowerInvariant();
            File.WriteAllText(Chapter01BlueprintWorldBuilder.Output + "/apk-sha256.txt",sha + "\n");
            File.WriteAllLines(Chapter01BlueprintWorldBuilder.Output + "/apk-required-resources.txt",required);
            File.WriteAllLines(Chapter01BlueprintWorldBuilder.Output + "/apk-world-serialized-sectors.txt",names);
            File.WriteAllLines(Chapter01BlueprintWorldBuilder.Output + "/apk-packed-assets.txt",packed);
        }
        private static bool ContainsSerialized(byte[] bytes, string value)
        {
            var needle = Encoding.UTF8.GetBytes(value);
            for (var offset = 4; offset <= bytes.Length - needle.Length; offset++)
            {
                if (bytes[offset] != needle[0] || BitConverter.ToInt32(bytes,offset - 4) != needle.Length) continue;
                var match = true;
                for (var i = 1; i < needle.Length; i++) if (bytes[offset+i] != needle[i]) { match=false; break; }
                if (match) return true;
            }
            return false;
        }
    }
}
