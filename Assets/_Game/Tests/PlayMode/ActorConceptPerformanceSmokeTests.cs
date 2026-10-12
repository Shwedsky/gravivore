using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using NUnit.Framework;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ActorConceptPerformanceSmokeTests
    {
        private const string Output="docs/history/implementation-passes/actors-combat-concept-world";
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        [UnityTest,Timeout(180000)] public IEnumerator ActualCameraSubmissionLodsMemoryAndFragmentCoverageAcrossCompleteChapter()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.PlayerHealth.SetDevelopmentGodMode(true);root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            var camera=UnityEngine.Camera.main;var enabled=camera.enabled;var target=camera.targetTexture;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var names=new[]{"RepairHub","RelayYard","CapacitorField","CuttingFloor","ShieldDump","HaulerGraveyard","Magnetar","Custodian"};
            var positions=new[]{new Vector3(0,0,-30),new Vector3(0,0,-10),new Vector3(-15,0,9),new Vector3(15,0,9),new Vector3(-15,0,31),new Vector3(15,0,31),new Vector3(0,0,51.5f),new Vector3(0,0,73)};
            var counters=new[]{new Counter(ProfilerCategory.Render,"Draw Calls Count"),new Counter(ProfilerCategory.Render,"Triangles Count"),new Counter(ProfilerCategory.Render,"SetPass Calls Count"),new Counter(ProfilerCategory.Render,"Batches Count"),new Counter(ProfilerCategory.Internal,"Main Thread"),new Counter(ProfilerCategory.Internal,"Render Thread"),new Counter(ProfilerCategory.Render,"GPU Frame Time"),new Counter(ProfilerCategory.Memory,"GC Allocated In Frame"),new Counter(ProfilerCategory.Memory,"GC Used Memory"),new Counter(ProfilerCategory.Memory,"Total Used Memory"),new Counter(ProfilerCategory.Memory,"Gfx Used Memory")};
            var available=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(available);
            Directory.CreateDirectory(Output+"/verification");File.WriteAllLines(Output+"/verification/available-profiler-counters.txt",available.Select(h=>ProfilerRecorderHandle.GetDescription(h)).Where(d=>d.Name.Contains("GPU")||d.Name.Contains("Draw")||d.Name.Contains("Triangle")||d.Name.Contains("Memory")||d.Name.Contains("Thread")).Select(d=>d.Category.Name+": "+d.Name+" / "+d.UnitType));
            var observations=new List<Observation>();
            try
            {
                camera.enabled=false;camera.targetTexture=render;
                for(var zone=0;zone<positions.Length;zone++)
                {
                    if(zone==6){root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();}
                    if(zone==7)root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
                    Move(root.PlayerObject.transform,positions[zone]);camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
                    for(var frame=0;frame<45;frame++){camera.Render();yield return null;}
                    foreach(var c in counters)c.Reset();var peakEnemies=0;var peakEffects=0;double submitMs=0;
                    for(var frame=0;frame<240;frame++)
                    {
                        var start=System.Diagnostics.Stopwatch.GetTimestamp();camera.Render();
                        submitMs+=(System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency;
                        yield return null;foreach(var c in counters)c.Sample();
                        peakEnemies=Math.Max(peakEnemies,root.EnemyPopulation.LiveEnemyCount);peakEffects=Math.Max(peakEffects,root.Phase6BCombat.Vfx.ActiveCount);
                    }
                    var views=root.GetComponentsInChildren<LODGroup>(true).Where(l=>l.gameObject.activeInHierarchy).Select(l=>
                        new Lod{actor=l.transform.root.name+" / "+l.name,activeLevels=l.GetLODs().Select((lod,index)=>new{lod,index}).Where(v=>v.lod.renderers.Any(r=>r!=null&&r.enabled&&r.isVisible)).Select(v=>v.index).ToArray(),visibleTriangles=l.GetLODs().SelectMany(lod=>lod.renderers).Where(r=>r!=null&&r.enabled&&r.isVisible).Sum(r=>r is SkinnedMeshRenderer s?(long)s.sharedMesh.GetIndexCount(0)/3:0)}).Where(l=>l.activeLevels.Length>0).ToArray();
                    observations.Add(new Observation{sector=names[zone],frames=240,width=540,height=960,meanCameraRenderSubmissionMs=submitMs/240,counters=counters.Select(c=>c.Result()).ToArray(),activeActorLods=views,peakOrdinaryEnemies=peakEnemies,peakPhase6BEffects=peakEffects,lights=root.GetComponentsInChildren<Light>(true).Count(l=>l.enabled)});
                    Assert.That(peakEnemies,Is.LessThanOrEqualTo(25));
                    CaptureCoverage(camera,root.transform,names[zone]);
                }
                File.WriteAllText(Output+"/verification/complete-chapter-render-observation.json",JsonUtility.ToJson(new Observations{samples=observations.ToArray()},true));
                var shader=Shader.Find("Gravivore/Actor Industrial Lit");
                var util=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.ShaderUtil")).FirstOrDefault(t=>t!=null);
                if(util!=null)
                {
                    var methods=util.GetMethods(System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
                    var code=methods.FirstOrDefault(m=>m.Name=="GetSRPBatcherCompatibilityCode"&&m.GetParameters().Length==2);
                    var reason=methods.FirstOrDefault(m=>m.Name=="GetSRPBatcherCompatibilityIssueReason"&&m.GetParameters().Length==3);
                    if(code!=null){var result=code.Invoke(null,new object[]{shader,0});File.WriteAllText(Output+"/verification/actor-srp-batcher-rendered.txt",Convert.ToString(result)+(reason==null?"":"\n"+Convert.ToString(reason.Invoke(null,new object[]{shader,0,result}))));}
                }
            }
            finally{foreach(var c in counters)c.Dispose();camera.targetTexture=target;camera.enabled=enabled;render.Release();Object.Destroy(render);}
        }
        private static void Move(Transform actor,Vector3 position){var body=actor.GetComponent<CharacterController>();body.enabled=false;actor.position=position;body.enabled=true;Physics.SyncTransforms();}
        public static void CaptureCoverage(UnityEngine.Camera camera,Transform root,string label)
        {
            var renderers=root.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
            var originals=renderers.Select(r=>r.sharedMaterials).ToArray();var opaque=new Material(Shader.Find("Universal Render Pipeline/Unlit"));opaque.SetColor("_BaseColor",Color.black);
            var copies=new Dictionary<Material,Material>();var canvases=root.GetComponentsInChildren<Canvas>(true);var canvasStates=canvases.Select(c=>c.enabled).ToArray();
            var post=camera.GetUniversalAdditionalCameraData();var pp=post.renderPostProcessing;var priorTarget=camera.targetTexture;var active=RenderTexture.active;var priorFlags=camera.clearFlags;var priorBg=camera.backgroundColor;
            var image=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear);var texture=new Texture2D(540,960,TextureFormat.RGBAFloat,false,true);
            try
            {
                foreach(var canvas in canvases)canvas.enabled=false;post.renderPostProcessing=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                for(var i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originals[i].Select(m=>
                {
                    if(m.renderQueue<3000)return opaque;
                    if(copies.TryGetValue(m,out var found))return found;
                    var copy=new Material(Shader.Find("Gravivore/Diagnostics/Actor Combat Coverage"));
                    if(m.HasProperty("_BaseMap"))copy.SetTexture("_BaseMap",m.GetTexture("_BaseMap"));
                    if(m.HasProperty("_BaseColor"))copy.SetColor("_BaseColor",m.GetColor("_BaseColor"));copies.Add(m,copy);return copy;
                }).ToArray();
                camera.targetTexture=image;camera.Render();RenderTexture.active=image;texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();var pixels=texture.GetPixels();
                var covered=pixels.Count(c=>c.r>.03f);var max=pixels.Max(c=>c.r)*16;var mean=pixels.Sum(c=>(double)c.r*16)/pixels.Length;
                Directory.CreateDirectory(Output+"/overdraw");File.WriteAllText(Output+"/overdraw/"+label+".json",JsonUtility.ToJson(new Coverage{coveredPixels=covered,screenPixels=pixels.Length,maxLayers=max,meanLayersAcrossScreen=mean,coverage=(float)covered/pixels.Length},true));
                var heat=new Texture2D(540,960,TextureFormat.RGB24,false);heat.SetPixels(pixels.Select(c=>new Color(Mathf.Clamp01(c.r*4),Mathf.Clamp01(c.r),Mathf.Clamp01(c.r*.25f))).ToArray());heat.Apply();File.WriteAllBytes(Output+"/overdraw/"+label+".png",heat.EncodeToPNG());Object.Destroy(heat);
            }
            finally{for(var i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originals[i];for(var i=0;i<canvases.Length;i++)canvases[i].enabled=canvasStates[i];post.renderPostProcessing=pp;camera.targetTexture=priorTarget;camera.clearFlags=priorFlags;camera.backgroundColor=priorBg;RenderTexture.active=active;image.Release();Object.Destroy(image);Object.Destroy(texture);Object.Destroy(opaque);foreach(var copy in copies.Values)Object.Destroy(copy);}
        }
        private sealed class Counter:IDisposable
        {
            private ProfilerRecorder _recorder;private readonly string _name;private long _sum,_max;private int _frames;
            public Counter(ProfilerCategory category,string name){_name=name;_recorder=ProfilerRecorder.StartNew(category,name,1);}
            public void Reset(){_sum=0;_max=0;_frames=0;}
            public void Sample(){if(!_recorder.Valid)return;_frames++;var value=_recorder.LastValue;_sum+=value;_max=Math.Max(_max,value);}
            public CounterResult Result()=>new CounterResult{name=_name,valid=_recorder.Valid,available=_recorder.Valid&&(_name!="GPU Frame Time"||_max>0),mean=_frames==0?0:(double)_sum/_frames,max=_max};
            public void Dispose()=>_recorder.Dispose();
        }
        [Serializable]private sealed class Observations{public string method="Editor D3D actual Camera.Render submission, 540x960 sRGB output, full Chapter01 loaded. Render counters are submitted draws/triangles, not a mesh inventory. CPU includes Editor/test runner. Submission timing is CPU wall time and does not wait for GPU. Invalid counters are unavailable, not measured zero. No Android 60 FPS claim.";public Observation[] samples;}
        [Serializable]private sealed class Observation{public string sector;public int frames,width,height,peakOrdinaryEnemies,peakPhase6BEffects,lights;public double meanCameraRenderSubmissionMs;public CounterResult[] counters;public Lod[] activeActorLods;}
        [Serializable]private sealed class CounterResult{public string name;public bool valid,available;public double mean;public long max;}
        [Serializable]private sealed class Lod{public string actor;public int[] activeLevels;public long visibleTriangles;}
        [Serializable]private sealed class Coverage{public string method="Depth-tested additive raster of current world transparent geometry with alpha cutoff 0.025; opaque world retains depth. UI excluded. Actual diagnostic fragments, not Android GPU timing or a worst-case guarantee. Heat map red=4 layers.";public int coveredPixels,screenPixels;public float maxLayers,coverage;public double meanLayersAcrossScreen;}
    }
}
