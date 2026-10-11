using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class ConceptConvergenceV47BuildAudit:IPreprocessBuildWithReport,IPostprocessBuildWithReport
    {
        public int callbackOrder=>710;
        public void OnPreprocessBuild(BuildReport report)=>ConceptConvergenceV47Builder.Audit();
        public void OnPostprocessBuild(BuildReport report)
        {
            if (Chapter01BlueprintWorldBuilder.IsBlueprintWorld()) return;
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            var dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
            var actorModels=ConceptConvergenceV47Builder.Actors.Select(a=>"Assets/_Game/Content/"+a.folder+"/Models/"+a.name+".fbx").ToArray();
            foreach(var path in actorModels)
                if(!packed.Contains(path))throw new BuildFailedException("V47 actor model not packed: "+path);
            var maps=dependencies.Where(p=>p.StartsWith(SurfaceHeroV46Builder.Root+"/Textures/",StringComparison.Ordinal)).ToArray();
            foreach(var path in maps)
                if(!packed.Contains(path))throw new BuildFailedException("V47 preserved PBR map not packed: "+path);
            File.WriteAllLines(ConceptConvergenceV47Builder.Output+"/apk_actor_and_pbr_dependencies.txt",actorModels.Concat(maps));
            // Static facility/deck FBX instances are baked into combined scene meshes.
            File.WriteAllLines(ConceptConvergenceV47Builder.Output+"/apk_static_geometry_sources.txt",dependencies.Where(p=>p.StartsWith(ConceptConvergenceV47Builder.Root+"/Models/",StringComparison.Ordinal)));
        }
    }
}
