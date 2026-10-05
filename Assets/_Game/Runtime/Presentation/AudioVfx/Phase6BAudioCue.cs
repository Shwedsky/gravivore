using UnityEngine;
namespace Gravivore.Presentation.AudioVfx {
public enum Phase6BAudioCue { PlayerStep,ServoActuator,GravityLashCharge,GravityLashRelease,GravityLashImpact,EnemyMechanicalHit,EnemyShutdown,PlayerDamage,PlayerDeath,MagnetarSignature,CustodianCone,CustodianLine,CustodianCircle,RepairHub,Evolution,ChapterComplete }
public readonly struct Phase6BAudioSelection { public Phase6BAudioSelection(AudioClip clip,float volume,float pitch,float spatialBlend,bool loop){Clip=clip;Volume=volume;Pitch=pitch;SpatialBlend=spatialBlend;Loop=loop;} public AudioClip Clip{get;} public float Volume{get;} public float Pitch{get;} public float SpatialBlend{get;} public bool Loop{get;} }
public interface IPhase6BAudioCueSink { bool TryPlay(Phase6BAudioCue cue,Vector3 worldPosition,int variationSeed=0); }
}
