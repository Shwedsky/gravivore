using System; using UnityEngine;
namespace Gravivore.Presentation.AudioVfx {
[DisallowMultipleComponent] public sealed class Phase6BAudioPlayer:MonoBehaviour,IPhase6BAudioCueSink {
[SerializeField] Phase6BAudioBank _bank; [SerializeField,Range(1,12)] int _poolSize=6; AudioSource[] _sources; int _cursor; bool _initialized; public int PoolSize=>_sources?.Length??0;
void Awake(){if(_bank!=null&&!_initialized)Initialize(_bank,_poolSize);}
public void Initialize(Phase6BAudioBank bank,int poolSize=6){if(_initialized)throw new InvalidOperationException("Already initialized.");if(bank==null)throw new ArgumentNullException(nameof(bank));if(poolSize<1||poolSize>12)throw new ArgumentOutOfRangeException(nameof(poolSize));bank.ValidateOrThrow();_bank=bank;_poolSize=poolSize;_sources=new AudioSource[poolSize];for(var i=0;i<poolSize;i++){var go=new GameObject($"Phase 6B Audio Voice {i}");go.transform.SetParent(transform,false);var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.dopplerLevel=0;s.rolloffMode=AudioRolloffMode.Linear;s.minDistance=2;s.maxDistance=22;_sources[i]=s;}_initialized=true;}
public bool TryPlay(Phase6BAudioCue cue,Vector3 pos,int variationSeed=0){if(!_initialized){if(_bank==null)return false;Initialize(_bank,_poolSize);}if(!_bank.TryResolve(cue,variationSeed,out var x))return false;var s=_sources[_cursor];_cursor=(_cursor+1)%_sources.Length;s.Stop();s.transform.position=pos;s.clip=x.Clip;s.volume=x.Volume;s.pitch=x.Pitch;s.spatialBlend=x.SpatialBlend;s.loop=x.Loop;s.Play();return true;}
public void StopAll(){if(_sources!=null)foreach(var s in _sources)s.Stop();}
}}
