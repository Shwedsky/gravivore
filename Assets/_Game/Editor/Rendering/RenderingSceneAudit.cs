using System;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravivore.Editor.Rendering
{
    public sealed class RenderingSceneAudit : IProcessSceneWithReport
    {
        public int callbackOrder => int.MaxValue;
        public void OnProcessScene(Scene scene,BuildReport report)
        {
            if(report==null)return;
            var count=0;var v3=0;
            foreach(var root in scene.GetRootGameObjects())foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if(renderer.sharedMaterials.Length==0)throw new BuildFailedException("MAGENTA_GUARD serialized renderer has no materials: "+renderer.name);
                foreach(var material in renderer.sharedMaterials)RenderingBuildAudit.ValidateMaterial(material,scene.path+"/"+renderer.name);
                if(RenderingBuildAudit.V3Prefabs.Any(name=>renderer.name.StartsWith(name,StringComparison.Ordinal)))
                {RenderingBuildAudit.ValidateRenderer(renderer);v3++;}
                count++;
            }
            Directory.CreateDirectory(RenderingBuildAudit.Output);
            File.AppendAllText(Path.Combine(RenderingBuildAudit.Output,"serialized_scene_renderers.txt"),$"{scene.path}: renderers={count}; v3={v3}; missing/error materials=0\n");
        }
    }
}
