using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Presentation.Composition
{
    /// <summary>Bounded two-arm machine. Healing events are the only activation authority.</summary>
    [DisallowMultipleComponent]
    public sealed class RepairManipulatorPresenter : MonoBehaviour
    {
        private sealed class Arm
        {
            public Vector3 Mount;
            public Transform Shoulder, Elbow, Upper, Forearm, Tool;
        }
        private readonly Arm[] _arms = new Arm[2];
        private RepairHubProductionPresenter _hub;
        private Transform _player, _scanner;
        private Material _metal, _energy;
        private float _blend, _phase;
        public float Engagement => _blend;
        public int ArmCount => _arms.Length;
        public void Initialize(RepairHubProductionPresenter hub, Transform player, Material lit, Material unlit)
        {
            if (_hub != null) throw new InvalidOperationException("Repair arms are already initialized.");
            _hub=hub; _player=player; transform.position=hub.RepairPosition;
            _metal=new Material(lit) { name="Repair manipulator shared alloy", color=new Color(.18f,.27f,.31f) };
            _energy=new Material(unlit) { name="Repair contained scanner", color=new Color(.15f,.9f,.83f) };
            for(var i=0;i<2;i++)
            {
                var mount=new Vector3(i==0?-2.55f:2.55f,.08f,-.45f);
                var a=new Arm { Mount=mount };
                Part("Repair pedestal "+i,PrimitiveType.Cylinder,mount+Vector3.up*.3f,new Vector3(.65f,.3f,.65f),_metal);
                a.Shoulder=Part("Servo shoulder "+i,PrimitiveType.Sphere,mount+Vector3.up*1.25f,Vector3.one*.45f,_metal);
                Part("Repair riser "+i,PrimitiveType.Cube,mount+Vector3.up*.68f,new Vector3(.27f,1.25f,.27f),_metal);
                a.Elbow=Part("Servo elbow "+i,PrimitiveType.Sphere,Vector3.zero,Vector3.one*.32f,_metal);
                a.Upper=Part("Upper manipulator "+i,PrimitiveType.Cube,Vector3.zero,Vector3.one,_metal);
                a.Forearm=Part("Telescopic forearm "+i,PrimitiveType.Cube,Vector3.zero,Vector3.one,_metal);
                a.Tool=Part("Repair scanner head "+i,PrimitiveType.Cube,Vector3.zero,new Vector3(.20f,.20f,.34f),_energy);
                _arms[i]=a;
            }
            Ring("Repair pad perimeter",Vector3.up*.10f,hub.RepairRadius,_energy);
            _scanner=Ring("Contained repair scanner",Vector3.up*.75f,.85f,_energy);
            _scanner.gameObject.SetActive(false);
            for(var i=0;i<2;i++) Part("Repair pad cross "+i,PrimitiveType.Cube,new Vector3(0,.12f,0),i==0?new Vector3(1.4f,.025f,.16f):new Vector3(.16f,.025f,1.4f),_energy);
            Tick(0f);
        }
        private Transform Part(string name,PrimitiveType shape,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(shape);go.name=name;go.transform.SetParent(transform,false);
            go.transform.localPosition=position;go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            var renderer=go.GetComponent<Renderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return go.transform;
        }
        private Transform Ring(string name,Vector3 position,float radius,Material material)
        {
            var go=new GameObject(name,typeof(LineRenderer));go.transform.SetParent(transform,false);go.transform.localPosition=position;
            var line=go.GetComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=false;line.loop=true;
            line.widthMultiplier=.035f;line.positionCount=32;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
            for(var i=0;i<32;i++){var angle=i*Mathf.PI/16;line.SetPosition(i,new Vector3(Mathf.Sin(angle)*radius,0,Mathf.Cos(angle)*radius));}
            return go.transform;
        }
        private void Update(){if(_hub!=null)Tick(Time.deltaTime);}
        public void Tick(float deltaTime)
        {
            _blend=Mathf.MoveTowards(_blend,_hub.IsRepairing?1f:0f,deltaTime*2.5f);_phase+=deltaTime;
            var target=transform.InverseTransformPoint(_player.position)+Vector3.up*.85f;
            for(var i=0;i<2;i++)
            {
                var a=_arms[i];var side=i==0?-1f:1f;
                var idle=a.Mount+new Vector3(-side*.25f,1.45f,.2f);
                var tip=Vector3.Lerp(idle,target+new Vector3(side*.48f,.1f+Mathf.Sin(_phase*6+i)*.06f,-.25f),_blend);
                var elbow=Vector3.Lerp(a.Mount+new Vector3(-side*.3f,2.15f,.25f),(a.Shoulder.localPosition+tip)*.5f+Vector3.up*.8f,_blend);
                a.Elbow.localPosition=elbow;a.Tool.localPosition=tip;
                Segment(a.Upper,a.Shoulder.localPosition,elbow,.22f);Segment(a.Forearm,elbow,tip,.14f);
                a.Tool.localRotation=Quaternion.LookRotation(target-tip);
            }
            _scanner.gameObject.SetActive(_hub.IsRepairing);
            _scanner.localPosition=new Vector3(target.x,.4f+Mathf.PingPong(_phase*1.1f,.8f),target.z);
        }
        private static void Segment(Transform segment,Vector3 a,Vector3 b,float thickness)
        {
            segment.localPosition=(a+b)*.5f;segment.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
            segment.localScale=new Vector3(thickness,(b-a).magnitude,thickness);
        }
        private void OnDestroy(){if(_metal!=null)Destroy(_metal);if(_energy!=null)Destroy(_energy);}
    }
}
