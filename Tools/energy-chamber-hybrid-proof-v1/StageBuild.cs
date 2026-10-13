// Copied only to the ignored standalone visual project, never a gameplay assembly.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class EnergyChamberStageBuild
{
    [Serializable] private sealed class Config {public string scene,output,result;}
    [Serializable] private sealed class Result {public string result,path,validation;public int errors;public ulong bytes;}
    public static void Build()
    {
        var config=JsonUtility.FromJson<Config>(File.ReadAllText("stage-build.json"));
        var scene=EditorSceneManager.OpenScene(config.scene);
        var roots=scene.GetRootGameObjects();
        var assembly=roots.Single(g=>g.name=="Energy Chamber / hybrid proof V1");
        if(assembly.GetComponentsInChildren<MonoBehaviour>(true).Length!=0 || assembly.GetComponentsInChildren<Collider>(true).Length!=0)throw new InvalidDataException("Device installation has gameplay authority.");
        foreach(var root in roots)
        {
            foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)!=0)throw new InvalidDataException("Missing snapshot script.");
            foreach(var script in root.GetComponentsInChildren<MonoBehaviour>(true))
                if(!(script is Volume) && !(script is UniversalAdditionalCameraData) && !(script is UniversalAdditionalLightData))throw new InvalidDataException("Unexpected project behaviour in visual device snapshot: "+script.GetType().FullName);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
                foreach(var material in renderer.sharedMaterials)
                    if(material==null || material.shader==null || !material.shader.isSupported)throw new InvalidDataException("Missing/unsupported device material on "+renderer.name);
        }
        if(GraphicsSettings.defaultRenderPipeline==null || RenderSettings.customReflectionTexture==null)throw new InvalidDataException("Snapshot lighting/pipeline dependency missing.");
        var camera=roots.SelectMany(g=>g.GetComponentsInChildren<Camera>()).Single();
        if(camera.orthographic || Mathf.Abs(camera.fieldOfView-48f)>.001f)throw new InvalidDataException("Device lens differs from reviewed gameplay camera.");
        var g0=roots.Single(g=>g.name.StartsWith("G-0 /",StringComparison.Ordinal));
        if(g0.transform.localScale!=Vector3.one)throw new InvalidDataException("Device G-0 scale changed.");
        PlayerSettings.companyName="Project Gravivore";PlayerSettings.productName="GRAVIVORE Energy Chamber Proof";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.gravivore.energychamberproof");
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.forceInternetPermission=false;PlayerSettings.Android.forceSDCardPermission=false;PlayerSettings.colorSpace=ColorSpace.Linear;
        AssetDatabase.SaveAssets();Directory.CreateDirectory(Path.GetDirectoryName(config.output));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{config.scene},target=BuildTarget.Android,locationPathName=config.output,options=BuildOptions.Development|BuildOptions.AllowDebugging|BuildOptions.DetailedBuildReport});
        File.WriteAllText(config.result,JsonUtility.ToJson(new Result{result=report.summary.result.ToString(),path=report.summary.outputPath,errors=report.summary.totalErrors,bytes=report.summary.totalSize,validation="Visual dependency closure; no project scripts; no missing scripts/materials; exact camera lens and G-0 unit scale"},true));
        if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Standalone proof build failed: "+report.summary.result);
        Debug.Log("ENERGY_CHAMBER_STANDALONE_ANDROID_SUCCEEDED");
    }
}
