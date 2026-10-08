using System; using UnityEngine; using UnityEngine.Rendering;
namespace Gravivore.Presentation.AudioVfx {
[DisallowMultipleComponent] public sealed class Phase6BVfxInstance:MonoBehaviour {
[SerializeField] Phase6BVfxCue _cue; [SerializeField] Phase6BVfxShape _shape; [SerializeField,Min(.04f)] float _duration=.18f; [SerializeField] Color _color=new(.22f,.9f,1,.8f); [SerializeField,Range(1,64)] int _particleCount=12; [SerializeField,Min(.05f)] float _range=4; [SerializeField,Min(.005f)] float _width=.055f; [SerializeField,Range(5,170)] float _angleDegrees=58; [SerializeField] Material _material;
[SerializeField,Min(.005f)] float _sparkSize=.07f; [SerializeField,Min(.1f)] float _sparkSpeed=2.3f;
[SerializeField] Material _warningFillMaterial; [SerializeField,Range(.05f,.3f)] float _warningFillAlpha=.18f;
Phase6BWarningFill _warningFill;
int _playerVariant;
[SerializeField,Min(1)] float _pulseWidthMultiplier=2.1f, _arcWidthMultiplier=1.35f;
[SerializeField,Min(.01f)] float _arcBend=.30f;
public int PlayerVariant=>_playerVariant;
public void ConfigurePlayerVariant(int variant)
{
    _playerVariant = !IsWarning && _cue<=Phase6BVfxCue.PlayerImpact ? Mathf.Clamp(variant,0,2) : 0;
}
bool IsWarning => _cue==Phase6BVfxCue.HostileTelegraphBase || _cue==Phase6BVfxCue.BossConeTelegraph || _cue==Phase6BVfxCue.BossLineTelegraph || _cue==Phase6BVfxCue.BossCircleTelegraph;
public Phase6BVfxShape Shape => _shape;
float _authoredRange = -1f, _authoredHalfAngle = -1f, _authoredLineWidth;
LineRenderer _line; ParticleSystem _particles; float _remaining,_activeDuration; Vector3 _origin,_destination; bool _built;
public float PresentedRange => _authoredRange > 0f ? _authoredRange : _range;
public float PresentedHalfAngle => _authoredHalfAngle >= 0f ? _authoredHalfAngle : _angleDegrees * .5f;
public void ConfigureTelegraph(Vector3 direction, float range, float width, float halfAngle)
{
    _authoredRange = range; _authoredHalfAngle = halfAngle; _authoredLineWidth = width;
    transform.rotation = direction.sqrMagnitude > .0001f ? Quaternion.LookRotation(direction, Vector3.up) : Quaternion.identity;
}
public Phase6BVfxCue Cue=>_cue; public int ParticleCount=>_particleCount; public Material Material=>_material; public bool IsPlaying=>_remaining>0&&gameObject.activeSelf;
void Awake()=>EnsureBuilt(); void Update()=>Tick(Time.deltaTime); void OnDisable()=>_remaining=0;
public void Play(Vector3 origin,Vector3 destination,float durationOverride=-1){EnsureBuilt();_origin=origin;_destination=destination;transform.position=origin;if(_cue==Phase6BVfxCue.HostileImpact && _shape==Phase6BVfxShape.Sparks)transform.rotation=(destination-origin).sqrMagnitude>.0001f?Quaternion.LookRotation(destination-origin):Quaternion.identity;_activeDuration=durationOverride>0?durationOverride:_duration;_remaining=_activeDuration;gameObject.SetActive(true);if(_line!=null){ConfigureLine(0);ConfigureWarningFill();}if(_particles!=null){_particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);_particles.Emit(Mathf.Clamp(_particleCount,1,64));}}
public void Tick(float dt){if(_remaining<=0||!gameObject.activeSelf)return;_remaining=Mathf.Max(0,_remaining-Mathf.Max(0,dt));if(_line!=null)ConfigureLine(1-_remaining/Mathf.Max(.0001f,_activeDuration));if(_remaining<=0){if(_particles!=null)_particles.Stop(true,ParticleSystemStopBehavior.StopEmitting);gameObject.SetActive(false);}}
public void StopImmediate(){_authoredRange=-1f;_authoredHalfAngle=-1f;_authoredLineWidth=0f;transform.rotation=Quaternion.identity;_remaining=0;_warningFill?.Stop();if(_particles!=null)_particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);gameObject.SetActive(false);}
public void ValidateOrThrow(){if(_duration<=0||_particleCount<1||_particleCount>64||_range<=0||_width<=0)throw new InvalidOperationException($"{name}: invalid VFX limits.");if(IsWarning && _warningFillMaterial==null)throw new InvalidOperationException($"{name}: warning fill material missing.");if(_material==null)throw new InvalidOperationException($"{name}: material missing.");}
static bool UsesLine(Phase6BVfxShape s)=>s==Phase6BVfxShape.Beam||s==Phase6BVfxShape.Circle||s==Phase6BVfxShape.Cone||s==Phase6BVfxShape.Line||s==Phase6BVfxShape.Scanner;
void EnsureBuilt(){if(_built)return;ValidateOrThrow();if(UsesLine(_shape))BuildLine();else BuildParticles();_built=true;}
void BuildLine(){var go=new GameObject("Phase 6B Geometry",typeof(LineRenderer));go.transform.SetParent(transform,false);_line=go.GetComponent<LineRenderer>();_line.useWorldSpace=true;_line.widthMultiplier=_width;_line.numCapVertices=2;_line.numCornerVertices=2;_line.shadowCastingMode=ShadowCastingMode.Off;_line.receiveShadows=false;_line.lightProbeUsage=LightProbeUsage.Off;_line.reflectionProbeUsage=ReflectionProbeUsage.Off;_line.sharedMaterial=_material;if(IsWarning)_warningFill=new Phase6BWarningFill(transform,_warningFillMaterial,_shape==Phase6BVfxShape.Cone?22:_shape==Phase6BVfxShape.Line?4:32);}
void BuildParticles(){var go=new GameObject("Phase 6B Particles",typeof(ParticleSystem));go.transform.SetParent(transform,false);_particles=go.GetComponent<ParticleSystem>();_particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var m=_particles.main;m.playOnAwake=false;m.loop=false;m.duration=Mathf.Max(.05f,_duration);m.startLifetime=Mathf.Min(_duration,_shape==Phase6BVfxShape.Sparks?.28f:.18f);m.startColor=_color;m.maxParticles=Mathf.Clamp(_particleCount,1,64);m.simulationSpace=ParticleSystemSimulationSpace.World;m.startSize=_shape==Phase6BVfxShape.Burst?.16f:_sparkSize;m.startSpeed=_shape==Phase6BVfxShape.Charge?-.7f:_shape==Phase6BVfxShape.Sparks?_sparkSpeed:1.4f;m.gravityModifier=_shape==Phase6BVfxShape.Sparks?.35f:0;var e=_particles.emission;e.enabled=false;var sh=_particles.shape;sh.enabled=true;sh.shapeType=_shape==Phase6BVfxShape.Sparks?ParticleSystemShapeType.Cone:ParticleSystemShapeType.Sphere;sh.radius=_shape==Phase6BVfxShape.Charge?.42f:.08f;if(_shape==Phase6BVfxShape.Sparks)sh.angle=22;var r=_particles.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=_material;r.renderMode=_cue==Phase6BVfxCue.HostileImpact?ParticleSystemRenderMode.Stretch:ParticleSystemRenderMode.Billboard;if(_cue==Phase6BVfxCue.HostileImpact){r.lengthScale=2f;r.velocityScale=.12f;}r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;}
void ConfigureLine(float n){var c=_color;c.a*=IsWarning?(0.9f+0.1f*Mathf.Cos(n*Mathf.PI*4f))*Mathf.Clamp01((1-n)*20f):Mathf.Clamp01(1-n);if(IsWarning){var fill=_color;fill.a=_warningFillAlpha*Mathf.Clamp01((1-n)*20f);_warningFill.SetColor(fill);}_line.startColor=c;_line.endColor=new Color(c.r,c.g,c.b,c.a*.55f);_line.widthMultiplier=_width;if(_shape==Phase6BVfxShape.Line && _authoredLineWidth > 0f){
    _line.loop=true;_line.positionCount=4;
    var side=transform.right*_authoredLineWidth*.5f;
    var end=_origin+transform.forward*PresentedRange;
    _line.SetPosition(0,_origin-side);_line.SetPosition(1,_origin+side);
    _line.SetPosition(2,end+side);_line.SetPosition(3,end-side);return;
}
if(_shape==Phase6BVfxShape.Beam||_shape==Phase6BVfxShape.Line){
    _line.loop=false;
    var end=(_destination-_origin).sqrMagnitude>.0001f?_destination:_origin+transform.forward*_range;
    if(_cue==Phase6BVfxCue.HostileTravel){
        _line.positionCount=2;
        _line.SetPosition(0,Vector3.Lerp(_origin,end,Mathf.Max(0,n-.24f)));
        _line.SetPosition(1,Vector3.Lerp(_origin,end,Mathf.Clamp01(n+.20f)));return;
    }
    if(_cue==Phase6BVfxCue.GravityLashTravel && _playerVariant==1){
        _line.positionCount=2;_line.widthMultiplier=_width*_pulseWidthMultiplier;
        _line.startColor=new Color(.7f,.95f,1,c.a);_line.endColor=new Color(.4f,.85f,1,c.a);
        _line.SetPosition(0,Vector3.Lerp(_origin,end,Mathf.Max(0,n-.28f)));
        _line.SetPosition(1,Vector3.Lerp(_origin,end,Mathf.Clamp01(n+.32f)));return;
    }
    if(_cue==Phase6BVfxCue.GravityLashTravel && _playerVariant==2){
        _line.positionCount=7;_line.widthMultiplier=_width*_arcWidthMultiplier;
        var side=Vector3.Cross((end-_origin).normalized,Vector3.up);
        for(var i=0;i<7;i++){var u=i/6f;_line.SetPosition(i,Vector3.Lerp(_origin,end,u)+side*Mathf.Sin(u*Mathf.PI)*_arcBend*(1-n));}return;
    }
    _line.positionCount=2;_line.SetPosition(0,_origin);_line.SetPosition(1,end);return;
}if(_shape==Phase6BVfxShape.Cone){ConfigureCone(20);return;}ConfigureRing(_shape==Phase6BVfxShape.Scanner?Mathf.Lerp(.2f,PresentedRange,n):PresentedRange,32);}
void ConfigureWarningFill(){if(_warningFill==null)return;if(_line.positionCount<3){_warningFill.Stop();return;}var center=_shape==Phase6BVfxShape.Line?_origin+transform.forward*PresentedRange*.5f:_origin;_warningFill.Configure(_line,center);}
void OnDestroy()=>_warningFill?.Dispose();
void ConfigureRing(float radius,int segments){_line.loop=true;_line.positionCount=segments;for(var i=0;i<segments;i++){var a=i*Mathf.PI*2/segments;_line.SetPosition(i,_origin+new Vector3(Mathf.Cos(a)*radius,.03f,Mathf.Sin(a)*radius));}}
void ConfigureCone(int segments){_line.loop=true;_line.positionCount=segments+2;_line.SetPosition(0,_origin);var half=PresentedHalfAngle;for(var i=0;i<=segments;i++){var a=Mathf.Lerp(-half,half,i/(float)segments);var d=Quaternion.Euler(0,a,0)*transform.forward;_line.SetPosition(i+1,_origin+d*PresentedRange+Vector3.up*.03f);}}
}}
