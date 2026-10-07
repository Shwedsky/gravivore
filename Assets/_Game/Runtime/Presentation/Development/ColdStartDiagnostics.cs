#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using Gravivore.Presentation.Composition;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Gravivore.Presentation.Development
{
    /// <summary>First 90 seconds, including load/composition. Fixed buffers and two session files.</summary>
    [DisallowMultipleComponent]
    public sealed class ColdStartDiagnostics : MonoBehaviour
    {
        public const int MaximumStalls=128, MaximumPhases=32, MaximumCounts=8;
        public const double ObservationSeconds=90;
        [Serializable] public struct Stall
        { public double at; public float milliseconds; public long mainThreadNs,renderThreadNs,gcBytes; public int gcCollections,enemyVisualCreations,liveEnemies; public bool repairActive,mapExpanded; public string phase; }
        [Serializable] public struct Phase
        { public string name; public double at,milliseconds; public long managedBytes; }
        [Serializable] public struct Counts
        { public double at; public int transforms,renderers,materials,particles,audio,enemies,roots; public long managedBytes,allocatedBytes; }
        [Serializable] public struct Creation
        { public string enemyId; public double at,milliseconds; public long managedBytesDelta; }
        [Serializable] public sealed class Report
        {
            public string utc,unity,device,graphicsApi,reason;
            public double observedSeconds;
            public bool mainThreadAvailable,renderThreadAvailable,gcAvailable;
            public int stallCount,droppedStalls,phaseCount,countCount,frameCount;
            public float worstFrameMilliseconds;
            public Stall[] stalls=new Stall[MaximumStalls];
            public Phase[] phases=new Phase[MaximumPhases];
            public Counts[] counts=new Counts[MaximumCounts];
            public Creation[] creations=new Creation[128];
            public int creationCount,droppedCreations;
        }
        private static ColdStartDiagnostics _active;
        private readonly Report _report=new Report();
        private readonly List<Transform> _transforms=new List<Transform>(8192);
        private readonly List<Renderer> _renderers=new List<Renderer>(4096);
        private readonly List<ParticleSystem> _particles=new List<ParticleSystem>(256);
        private readonly List<AudioSource> _audio=new List<AudioSource>(64);
        private readonly List<S01SceneCompositionRoot> _roots=new List<S01SceneCompositionRoot>(4);
        private readonly HashSet<Material> _materials=new HashSet<Material>();
        private readonly List<Material> _slots=new List<Material>(8);
        private ProfilerRecorder _main,_render,_gc;
        private S01SceneCompositionRoot _root;
        private double _started,_previous,_phaseStarted,_nextCount=1;
        private string _phase="scene-load",_path;
        private int _collections;
        private bool _paused,_finished;
        public Report Evidence => _report;
        public string OutputPath => _path;
        public static ColdStartDiagnostics Begin(Transform owner,bool persist=false)
        {
            if(_active!=null)return _active;
            var go=new GameObject("DEV bounded cold-start diagnostics");
            if(persist) DontDestroyOnLoad(go); else go.transform.SetParent(owner,false);
            _active=go.AddComponent<ColdStartDiagnostics>();_active.Initialize();return _active;
        }
        private void Initialize()
        {
            _started=_previous=_phaseStarted=Time.realtimeSinceStartupAsDouble;_collections=GC.CollectionCount(0);
            _report.utc=DateTime.UtcNow.ToString("O");_report.unity=Application.unityVersion;
            _report.device=SystemInfo.deviceModel;_report.graphicsApi=SystemInfo.graphicsDeviceType.ToString();
            _path=Path.Combine(Application.persistentDataPath,"cold-start-current.json");
            try {if(File.Exists(_path))File.Copy(_path,Path.Combine(Application.persistentDataPath,"cold-start-previous.json"),true);}
            catch(Exception exception){Debug.LogException(exception);}
            _main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
            _render=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Render Thread",1);
            _gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
            _report.mainThreadAvailable=_main.Valid;_report.renderThreadAvailable=_render.Valid;_report.gcAvailable=_gc.Valid;
        }
        public void Attach(S01SceneCompositionRoot root) => _root=root;
        public static void RecordEnemyVisual(string enemyId,double started,long managedBefore)
        {
            if(_active==null||_active._finished)return;
            var report=_active._report;var now=Time.realtimeSinceStartupAsDouble;
            if(report.creationCount<report.creations.Length)
                report.creations[report.creationCount++]=new Creation{enemyId=enemyId,at=started-_active._started,
                    milliseconds=(now-started)*1000,managedBytesDelta=Profiler.GetMonoUsedSizeLong()-managedBefore};
            else report.droppedCreations++;
        }
        public void MarkPhase(string phase)
        {
            if(_finished)return;var now=Time.realtimeSinceStartupAsDouble;
            if(_report.phaseCount<MaximumPhases)
                _report.phases[_report.phaseCount++]=new Phase{name=_phase,at=_phaseStarted-_started,milliseconds=(now-_phaseStarted)*1000,managedBytes=Profiler.GetMonoUsedSizeLong()};
            _phase=phase;_phaseStarted=now;
        }
        private void Update()
        {
            if(_finished||_paused)return;
            var now=Time.realtimeSinceStartupAsDouble;var elapsed=now-_started;
            RecordFrame((float)((now-_previous)*1000),elapsed);_previous=now;
            if(_root!=null&&elapsed>=_nextCount)
            {
                CaptureCounts(elapsed);
                _nextCount=_nextCount<5?5:_nextCount<15?15:_nextCount<30?30:_nextCount<60?60:90;
                Write("checkpoint");
            }
            if(elapsed>=ObservationSeconds)Finish("90-second window complete");
        }
        public void RecordFrame(float milliseconds,double elapsed)
        {
            if(_finished)return;_report.observedSeconds=elapsed;_report.frameCount++;
            _report.worstFrameMilliseconds=Mathf.Max(_report.worstFrameMilliseconds,milliseconds);
            var collections=GC.CollectionCount(0);
            if(milliseconds>=50f)
            {
                if(_report.stallCount<MaximumStalls)
                    _report.stalls[_report.stallCount++]=new Stall{at=elapsed,milliseconds=milliseconds,phase=_phase,
                        mainThreadNs=_main.Valid?_main.LastValue:-1,renderThreadNs=_render.Valid?_render.LastValue:-1,
                        gcBytes=_gc.Valid?_gc.LastValue:-1,gcCollections=collections-_collections,enemyVisualCreations=_report.creationCount,
                        liveEnemies=_root!=null&&_root.EnemyPopulation!=null?_root.EnemyPopulation.LiveEnemyCount:0,
                        repairActive=_root!=null&&_root.RepairHub!=null&&_root.RepairHub.IsRepairing,
                        mapExpanded=_root!=null&&_root.MapIntegration!=null&&_root.MapIntegration.MapPresenter!=null&&_root.MapIntegration.MapPresenter.IsExpanded};
                else _report.droppedStalls++;
            }
            _collections=collections;
        }
        private void CaptureCounts(double elapsed)
        {
            if(_report.countCount>=MaximumCounts)return;
            _root.GetComponentsInChildren(true,_transforms);_root.GetComponentsInChildren(true,_renderers);
            _root.GetComponentsInChildren(true,_particles);_root.GetComponentsInChildren(true,_audio);_root.GetComponentsInChildren(true,_roots);
            _materials.Clear();foreach(var renderer in _renderers){renderer.GetSharedMaterials(_slots);foreach(var material in _slots)if(material!=null)_materials.Add(material);}
            _report.counts[_report.countCount++]=new Counts{at=elapsed,transforms=_transforms.Count,renderers=_renderers.Count,
                particles=_particles.Count,audio=_audio.Count,materials=_materials.Count,roots=_roots.Count,
                enemies=_root.EnemyPopulation!=null?_root.EnemyPopulation.LiveEnemyCount:0,
                managedBytes=Profiler.GetMonoUsedSizeLong(),allocatedBytes=Profiler.GetTotalAllocatedMemoryLong()};
        }
        private void Write(string reason)
        {
            _report.reason=reason;
            try {File.WriteAllText(_path,JsonUtility.ToJson(_report,true));}catch(Exception exception){Debug.LogException(exception);}
        }
        public void Finish(string reason)
        {
            if(_finished)return;MarkPhase("finished");_report.observedSeconds=Time.realtimeSinceStartupAsDouble-_started;
            if(_root!=null)CaptureCounts(_report.observedSeconds);
            Write(reason);_finished=true;_main.Dispose();_render.Dispose();_gc.Dispose();enabled=false;
            Debug.Log("[ColdStart] Bounded evidence: "+_path+"; worst ms="+_report.worstFrameMilliseconds);
        }
        private void OnApplicationPause(bool paused)
        { _paused=paused;_previous=Time.realtimeSinceStartupAsDouble; if(paused&&!_finished)Write("application paused"); }
        private void OnApplicationQuit(){if(!_finished)Finish("application quit");}
        private void OnDestroy(){if(!_finished&&_path!=null)Finish("scene/lifecycle ended");if(_active==this)_active=null;}
    }
}
#endif
