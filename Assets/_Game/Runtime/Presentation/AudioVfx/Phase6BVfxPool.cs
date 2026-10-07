using System; using UnityEngine;
namespace Gravivore.Presentation.AudioVfx {
[DisallowMultipleComponent] public sealed class Phase6BVfxPool:MonoBehaviour {
[Serializable] public sealed class Binding { [SerializeField] Phase6BVfxCue _cue; [SerializeField] Phase6BVfxInstance _prefab; [SerializeField,Range(1,8)] int _capacity=4; public Binding(){} public Binding(Phase6BVfxCue cue,Phase6BVfxInstance prefab,int capacity){_cue=cue;_prefab=prefab;_capacity=capacity;} public Phase6BVfxCue Cue=>_cue;public Phase6BVfxInstance Prefab=>_prefab;public int Capacity=>_capacity;}
sealed class Bucket{public Phase6BVfxCue Cue;public Phase6BVfxInstance[] Items;public int Cursor;} [SerializeField] Binding[] _bindings=Array.Empty<Binding>(); Bucket[] _buckets; bool _initialized; public Phase6BVfxInstance LastPlayedInstance { get; private set; } public int CreatedInstanceCount{get;private set;} public int ActiveCount{get{if(_buckets==null)return 0;var n=0;foreach(var b in _buckets)foreach(var i in b.Items)if(i.IsPlaying)n++;return n;}}
void Awake(){if(_bindings!=null&&_bindings.Length>0&&!_initialized)Initialize(_bindings);}
public void Initialize(Binding[] bindings){if(_initialized)throw new InvalidOperationException("Already initialized.");if(bindings==null)throw new ArgumentNullException(nameof(bindings));_bindings=bindings;_buckets=new Bucket[bindings.Length];for(var i=0;i<bindings.Length;i++){var x=bindings[i]??throw new InvalidOperationException($"Binding {i} null.");if(x.Prefab==null||x.Prefab.Cue!=x.Cue||x.Capacity<1||x.Capacity>8)throw new InvalidOperationException($"Invalid binding {x.Cue}.");x.Prefab.ValidateOrThrow();var b=new Bucket{Cue=x.Cue,Items=new Phase6BVfxInstance[x.Capacity]};for(var j=0;j<b.Items.Length;j++){var item=Instantiate(x.Prefab,transform,false);item.name=$"{x.Prefab.name} [Pool {j}]";item.StopImmediate();b.Items[j]=item;CreatedInstanceCount++;}_buckets[i]=b;}_initialized=true;}
public bool TryPlay(Phase6BVfxCue cue,Vector3 origin,Vector3 destination,float durationOverride=-1,int playerVariant=0){if(!_initialized){if(_bindings==null||_bindings.Length==0)return false;Initialize(_bindings);}foreach(var b in _buckets)if(b.Cue==cue){var item=b.Items[b.Cursor];b.Cursor=(b.Cursor+1)%b.Items.Length;item.ConfigurePlayerVariant(playerVariant);item.Play(origin,destination,durationOverride);LastPlayedInstance=item;return true;}return false;}
public bool TryPlayTelegraph(Phase6BVfxCue cue, Vector3 origin, Vector3 direction, float range, float width, float halfAngle, float duration)
{
    if (!_initialized) return false;
    foreach (var bucket in _buckets)
    {
        if (bucket.Cue != cue) continue;
        var item = bucket.Items[bucket.Cursor];
        bucket.Cursor = (bucket.Cursor + 1) % bucket.Items.Length;
        item.StopImmediate();
        item.ConfigureTelegraph(direction, range, width, halfAngle);
        item.Play(origin, origin + direction.normalized * range, duration);
        return true;
    }
    return false;
}
public void StopCue(Phase6BVfxCue cue){if(_buckets==null)return;foreach(var b in _buckets)if(b.Cue==cue)foreach(var i in b.Items)i.StopImmediate();}
public void StopAll(){if(_buckets!=null)foreach(var b in _buckets)foreach(var i in b.Items)i.StopImmediate();}
}}
