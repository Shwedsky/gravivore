using System;
using Gravivore.Presentation.World;
using UnityEngine;

namespace Gravivore.Presentation.Composition
{
    /// <summary>Moves imported metre-length servo meshes. Healing owns activation.</summary>
    [DisallowMultipleComponent]
    public sealed class RepairManipulatorPresenter : MonoBehaviour
    {
        private sealed class Arm
        {
            public Vector3 Mount, Shoulder;
            public Transform Elbow, Upper, Forearm, Tool;
            public Renderer ToolRenderer;
        }
        private readonly Arm[] _arms=new Arm[2];
        private RepairHubProductionPresenter _hub;
        private ConceptFidelityDefinition _settings;
        private Transform _player,_scanner;
        private float _blend,_phase;
        private MaterialPropertyBlock _toolEmission;
        private static readonly int EmissionColor=Shader.PropertyToID("_EmissionColor");
        public float Engagement=>_blend;
        public int ArmCount=>_settings==null?0:_arms.Length;
        public void Initialize(RepairHubProductionPresenter hub,Transform player,ConceptFidelityDefinition settings)
        {
            if(_hub!=null)throw new InvalidOperationException("Repair arms are already initialized.");
            _hub=hub!=null?hub:throw new ArgumentNullException(nameof(hub));
            _player=player!=null?player:throw new ArgumentNullException(nameof(player));
            _settings=settings;transform.position=hub.RepairPosition;
            // Legacy isolated fixtures may omit art; no runtime primitive substitute.
            if(settings==null)return;
            settings.ValidateOrThrow();
            _toolEmission=new MaterialPropertyBlock();
            for(var i=0;i<_arms.Length;i++)
            {
                var mount=settings.Mount(i);
                var arm=new Arm{Mount=mount,Shoulder=mount+Vector3.up*settings.ShoulderHeight};
                Part(settings.Pedestal,"Authored repair pedestal "+i).localPosition=mount;
                arm.Elbow=Part(settings.Joint,"Authored servo elbow "+i);
                arm.Upper=Part(settings.Upper,"Authored upper manipulator "+i);
                arm.Forearm=Part(settings.Forearm,"Authored telescopic forearm "+i);
                arm.Tool=Part(settings.Tool,"Authored repair tool "+i);
                arm.ToolRenderer=arm.Tool.GetComponentInChildren<Renderer>();
                _arms[i]=arm;
            }
            _scanner=Part(settings.Scanner,"Authored repair scanner");_scanner.gameObject.SetActive(false);
            Tick(0);
        }
        private Transform Part(GameObject prefab,string label)
        {
            var obj=Instantiate(prefab,transform,false);obj.name=label;return obj.transform;
        }
        private void Update(){if(_hub!=null)Tick(Time.deltaTime);}
        public void Tick(float deltaTime)
        {
            if(_hub==null)return;
            _blend=Mathf.MoveTowards(_blend,_hub.IsRepairing?1:0,deltaTime*(_settings!=null?_settings.EngagementSpeed:2.5f));
            _phase+=deltaTime;if(_settings==null)return;
            var target=transform.InverseTransformPoint(_player.position)+Vector3.up*.85f;
            for(var i=0;i<_arms.Length;i++)
            {
                var arm=_arms[i];var side=i==0?-1f:1f;
                var idle=arm.Mount+new Vector3(-side*.25f,1.45f,.2f);
                var tip=Vector3.Lerp(idle,target+new Vector3(side*.48f,.1f+Mathf.Sin(_phase*6+i)*.06f,-.25f),_blend);
                var elbow=Vector3.Lerp(arm.Mount+new Vector3(-side*.3f,2.15f,.25f),(arm.Shoulder+tip)*.5f+Vector3.up*.8f,_blend);
                arm.Elbow.localPosition=elbow;arm.Tool.localPosition=tip;
                Segment(arm.Upper,arm.Shoulder,elbow);Segment(arm.Forearm,elbow,tip);
                var direction=target-tip;
                if(direction.sqrMagnitude>.00001f)arm.Tool.localRotation=Quaternion.LookRotation(direction);
                _toolEmission.SetColor(EmissionColor,Color.white*Mathf.Lerp(.35f,4.5f,_blend));
                arm.ToolRenderer.SetPropertyBlock(_toolEmission);
            }
            _scanner.gameObject.SetActive(_hub.IsRepairing);
            _scanner.localPosition=new Vector3(target.x,.4f+Mathf.PingPong(_phase*1.1f,.8f),target.z);
        }
        private static void Segment(Transform segment,Vector3 a,Vector3 b)
        {
            var direction=b-a;segment.localPosition=a;
            if(direction.sqrMagnitude>.00001f)segment.localRotation=Quaternion.FromToRotation(Vector3.up,direction);
            segment.localScale=new Vector3(1,direction.magnitude,1);
        }
        private void OnDisable(){_blend=0;if(_scanner!=null)_scanner.gameObject.SetActive(false);}
    }
}
