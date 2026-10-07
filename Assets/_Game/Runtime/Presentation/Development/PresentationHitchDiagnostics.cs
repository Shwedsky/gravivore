#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Player;
using UnityEngine;
using UnityEngine.Profiling;

namespace Gravivore.Presentation.Development
{
    public sealed class HitchSampleWindow
    {
        private readonly float[] _frames = new float[64];
        private readonly float _threshold, _cooldown;
        private double _nextAllowed;
        private int _cursor;
        public HitchSampleWindow(float threshold,float cooldown,double warmupUntil)
        { _threshold=threshold; _cooldown=cooldown; _nextAllowed=warmupUntil; }
        public bool Record(float seconds,double sessionSeconds)
        {
            _frames[_cursor]=seconds; _cursor=(_cursor+1)%_frames.Length;
            if(seconds<_threshold || sessionSeconds<_nextAllowed) return false;
            _nextAllowed=sessionSeconds+_cooldown; return true;
        }
        public void CopyChronological(float[] destination)
        { for(var i=0;i<_frames.Length;i++) destination[i]=_frames[(_cursor+i)%_frames.Length]; }
        public void SuppressUntil(double seconds) => _nextAllowed=Math.Max(_nextAllowed,seconds);
    }

    /// <summary>DEV only; fixed ring, threshold, cooldown and rotating bounded files.</summary>
    [DisallowMultipleComponent]
    public sealed class PresentationHitchDiagnostics : MonoBehaviour
    {
        [Serializable] private sealed class Snapshot
        {
            public string utc, session;
            public double sessionSeconds;
            public float[] recentFrameSeconds = new float[64];
            public int activeEnemies, activeVfx, vfxCapacity, activeCombatText, combatTextCapacity,
                activeShutdowns, audioSources, playingAudioSources, lineRenderers, enabledLines,
                particleSystems, playingParticles, transforms, renderers;
            public long managedBytes, unityAllocatedBytes;
        }
        private readonly List<Transform> _transforms=new List<Transform>(4096);
        private readonly List<Renderer> _renderers=new List<Renderer>(1024);
        private readonly List<AudioSource> _audio=new List<AudioSource>(32);
        private readonly List<LineRenderer> _lines=new List<LineRenderer>(256);
        private readonly List<ParticleSystem> _particles=new List<ParticleSystem>(256);
        private readonly List<Phase6BVfxPool> _vfx=new List<Phase6BVfxPool>(8);
        private readonly Snapshot _snapshot=new Snapshot();
        private S01SceneCompositionRoot _root;
        private PostDevicePresentationDefinition _settings;
        private VisualSliceAnimationBridge _animation;
        private HitchSampleWindow _window;
        private double _start,_previous;
        private int _fileCursor;
        private bool _paused;
        private string _directory;
        public int WrittenSnapshotCount { get; private set; }
        public string LastSnapshotPath { get; private set; }
        public void Initialize(S01SceneCompositionRoot root,PostDevicePresentationDefinition settings,string outputDirectory=null)
        {
            _root=root; _settings=settings; _animation=root.GetComponent<VisualSliceAnimationBridge>();
            _directory=outputDirectory??Path.Combine(Application.persistentDataPath,"hitch-diagnostics");
            _start=_previous=Time.realtimeSinceStartupAsDouble;
            _window=new HitchSampleWindow(settings.HitchThreshold,settings.HitchCooldown,5);
            _snapshot.session=Guid.NewGuid().ToString("N");
            RefreshCounts(); // Prewarm lists while composition is already allocating.
        }
        private void Update()
        {
            if(_root==null || _paused) return;
            var now=Time.realtimeSinceStartupAsDouble; var dt=(float)(now-_previous); _previous=now;
            RecordFrame(dt,now-_start);
        }
        public void RecordFrame(float seconds,double sessionSeconds)
        {
            if(!_window.Record(seconds,sessionSeconds)) return;
            _window.CopyChronological(_snapshot.recentFrameSeconds);
            _snapshot.utc=DateTime.UtcNow.ToString("O"); _snapshot.sessionSeconds=sessionSeconds;
            RefreshCounts();
            _snapshot.managedBytes=Profiler.GetMonoUsedSizeLong(); _snapshot.unityAllocatedBytes=Profiler.GetTotalAllocatedMemoryLong();
            try
            {
                Directory.CreateDirectory(_directory);
                LastSnapshotPath=Path.Combine(_directory,"hitch-"+_fileCursor+".json");
                _fileCursor=(_fileCursor+1)%_settings.HitchSnapshots;
                File.WriteAllText(LastSnapshotPath,JsonUtility.ToJson(_snapshot,true)); WrittenSnapshotCount++;
                Debug.Log("[Hitch] Bounded diagnostic snapshot: "+LastSnapshotPath);
            }
            catch(Exception exception) { Debug.LogException(exception); }
        }
        private void RefreshCounts()
        {
            _root.GetComponentsInChildren(true,_transforms); _root.GetComponentsInChildren(true,_renderers);
            _root.GetComponentsInChildren(true,_audio); _root.GetComponentsInChildren(true,_lines);
            _root.GetComponentsInChildren(true,_particles); _root.GetComponentsInChildren(true,_vfx);
            _snapshot.transforms=_transforms.Count; _snapshot.renderers=_renderers.Count;
            _snapshot.audioSources=_audio.Count; _snapshot.playingAudioSources=0;
            foreach(var source in _audio) if(source.isPlaying) _snapshot.playingAudioSources++;
            _snapshot.lineRenderers=_lines.Count; _snapshot.enabledLines=0;
            foreach(var line in _lines) if(line.enabled && line.gameObject.activeInHierarchy) _snapshot.enabledLines++;
            _snapshot.particleSystems=_particles.Count; _snapshot.playingParticles=0;
            foreach(var particle in _particles) if(particle.isPlaying) _snapshot.playingParticles++;
            _snapshot.activeVfx=_snapshot.vfxCapacity=0;
            foreach(var pool in _vfx) { _snapshot.activeVfx+=pool.ActiveCount; _snapshot.vfxCapacity+=pool.CreatedInstanceCount; }
            _snapshot.activeEnemies=_root.EnemyPopulation.LiveEnemyCount+(_root.MagnetarGuard.IsAlive?1:0)+(_root.CustodianBoss.IsAlive?1:0);
            _snapshot.activeCombatText=_root.CombatReadability?.ActiveCombatTextCount??0;
            _snapshot.combatTextCapacity=_root.CombatReadability?.TextCapacity??0;
            _snapshot.activeShutdowns=_animation!=null?_animation.ActiveShutdownCount:0;
        }
        private void OnApplicationPause(bool paused)
        {
            _paused=paused; _previous=Time.realtimeSinceStartupAsDouble;
            _window?.SuppressUntil(_previous-_start+5);
        }
    }
}
#endif
