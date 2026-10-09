using UnityEngine;

namespace Gravivore.VisualReplacementProofV1
{
    public sealed partial class VisualReplacementProofBootstrap
    {
        private void BuildIndustrialArena()
        {
            var floorCollider=_environmentRoot.gameObject.AddComponent<BoxCollider>();
            floorCollider.center=new Vector3(0f,-0.16f,2f); floorCollider.size=new Vector3(18.2f,0.32f,23f);
            var tiles=new[]{new Vector3(-4.4f,-0.11f,-3.6f),new Vector3(0f,-0.11f,-3.6f),new Vector3(4.4f,-0.11f,-3.6f),new Vector3(-4.4f,-0.11f,1f),new Vector3(0f,-0.11f,1f),new Vector3(4.4f,-0.11f,1f),new Vector3(-4.4f,-0.11f,5.6f),new Vector3(0f,-0.11f,5.6f),new Vector3(4.4f,-0.11f,5.6f)};
            for(var i=0;i<tiles.Length;i++) AddExtruded(_environmentRoot,"Floor_Panel_"+i,ChamferedPolygon(4.15f,4.25f,0.19f),0.22f,tiles[i],Quaternion.Euler(90f,0f,0f),i%2==0?"Floor":"StructuralMetal");
            AddTorus(_environmentRoot,"CombatCenter_OuterSeam",2.65f,0.055f,34,6,new Vector3(0f,0.018f,0.95f),Quaternion.identity,"FloorEdge");
            AddTorus(_environmentRoot,"CombatCenter_EnergyTrace",2.25f,0.027f,34,5,new Vector3(0f,0.024f,0.95f),Quaternion.identity,"CyanEnergy");
            AddMesh(_environmentRoot,"Center_Grate_A",VisualProofMeshFactory.CreateGrate("VRP1_CenterGrateA",1.25f,5.5f,7),new Vector3(-3f,0.025f,1.05f),Quaternion.identity,"FloorEdge");
            AddMesh(_environmentRoot,"Center_Grate_B",VisualProofMeshFactory.CreateGrate("VRP1_CenterGrateB",1.25f,5.5f,7),new Vector3(3f,0.025f,1.05f),Quaternion.identity,"FloorEdge");
            _reactorRoot=BuildReactor(new Vector3(5.8f,0f,6.4f));
            _barrierRoot=BuildBarrierFamily();
            _propRoot=BuildPropCluster(new Vector3(-5.6f,0f,6.2f));
            AddTube(_environmentRoot,"Pipe_Reactor_ToFloor",new[]{new Vector3(5f,1.02f,6.4f),new Vector3(4.15f,0.92f,6.4f),new Vector3(4.05f,0.22f,5.45f),new Vector3(3.65f,0.16f,4.55f)},0.13f,8,"HeatMetal");
            AddTube(_environmentRoot,"Cable_Reactor_Control",new[]{new Vector3(6f,0.25f,5.45f),new Vector3(5.25f,0.09f,4.82f),new Vector3(4.25f,0.07f,4.18f),new Vector3(3.35f,0.07f,3.8f)},0.055f,7,"CableRubber");
            AddTube(_environmentRoot,"Peripheral_Pipe_Run",new[]{new Vector3(-7.65f,0.32f,-5.5f),new Vector3(-7.6f,0.34f,-1.9f),new Vector3(-7.3f,0.48f,1.2f),new Vector3(-7.1f,0.65f,4.1f)},0.11f,8,"StructuralMetal");
            for(var i=0;i<5;i++) AddExtruded(_environmentRoot,"Floor_ServiceCover_"+i,ChamferedPolygon(0.78f,1.15f,0.08f),0.045f,new Vector3(-6.8f+i*2.05f,0.022f,-7.1f+(i%2)*0.35f),Quaternion.Euler(90f,i*7f,0f),"FloorEdge");
        }

        private Transform BuildReactor(Vector3 position)
        {
            var root=NewRoot("Environment_ReactorGenerator").transform; root.position=position; root.SetParent(_environmentRoot,true);
            AddLoft(root,"Reactor_Base",new[]{Ring(-0.25f,1.22f,1.22f),Ring(0f,1.38f,1.38f,0f,0f,15f),Ring(0.35f,1.08f,1.08f)},12,Vector3.up*0.25f,Quaternion.identity,"StructuralMetal");
            AddLoft(root,"Reactor_Containment",new[]{Ring(-1.45f,0.72f,0.72f),Ring(-0.75f,0.92f,0.92f,0f,0f,15f),Ring(0f,0.78f,0.78f),Ring(0.75f,0.92f,0.92f,0f,0f,15f),Ring(1.45f,0.70f,0.70f)},12,Vector3.up*2.05f,Quaternion.identity,"ArmorGunmetal");
            AddLoft(root,"Reactor_EnergyCore",new[]{Ring(-1.15f,0.37f,0.37f),Ring(-0.45f,0.48f,0.48f,0f,0f,22.5f),Ring(0.45f,0.48f,0.48f),Ring(1.15f,0.36f,0.36f,0f,0f,22.5f)},10,Vector3.up*2.05f,Quaternion.identity,"CyanEnergy");
            for(var i=0;i<4;i++) AddTorus(root,"Reactor_ContainmentRing_"+i,1.02f,0.095f,24,8,new Vector3(0f,0.95f+i*0.74f,0f),Quaternion.identity,i==1||i==2?"HeatMetal":"ArmorEdge");
            for(var side=-1;side<=1;side+=2) AddExtruded(root,"Reactor_Buttress_"+side,new[]{new Vector2(-0.30f,-0.25f),new Vector2(0.34f,-0.25f),new Vector2(0.62f,0.12f),new Vector2(0.25f,1.45f),new Vector2(-0.22f,1.45f),new Vector2(-0.42f,0.16f)},0.48f,new Vector3(side*1.13f,0.74f,0f),Quaternion.Euler(0f,90f,0f),"WornPaint");
            AddTube(root,"Reactor_CoolingLoop_Left",new[]{new Vector3(-0.64f,3.1f,0.35f),new Vector3(-1.26f,3.2f,0.48f),new Vector3(-1.46f,2.2f,0.62f),new Vector3(-1.17f,1.1f,0.53f)},0.12f,8,"HeatMetal");
            AddTube(root,"Reactor_CoolingLoop_Right",new[]{new Vector3(0.64f,3.1f,0.35f),new Vector3(1.26f,3.2f,0.48f),new Vector3(1.46f,2.2f,0.62f),new Vector3(1.17f,1.1f,0.53f)},0.12f,8,"HeatMetal");
            var c=root.gameObject.AddComponent<CapsuleCollider>(); c.center=new Vector3(0f,1.9f,0f); c.radius=1.32f; c.height=4.15f; return root;
        }

        private Transform BuildBarrierFamily()
        {
            var root=NewRoot("Environment_WallBarrierFamily").transform; root.SetParent(_environmentRoot,true);
            BuildBarrier(root,new Vector3(-5.6f,0f,9.4f),Quaternion.identity,0); BuildBarrier(root,new Vector3(-1.85f,0f,9.4f),Quaternion.identity,1); BuildBarrier(root,new Vector3(1.9f,0f,9.4f),Quaternion.identity,2); BuildBarrier(root,new Vector3(-8.45f,0f,4.7f),Quaternion.Euler(0f,90f,0f),3); BuildBarrier(root,new Vector3(-8.45f,0f,0.95f),Quaternion.Euler(0f,90f,0f),4); return root;
        }

        private void BuildBarrier(Transform parent,Vector3 position,Quaternion rotation,int index)
        {
            var m=NewRoot("Barrier_Module_"+index).transform; m.SetParent(parent,false); m.position=position; m.rotation=rotation;
            AddExtruded(m,"Barrier_LowerShell",new[]{new Vector2(-1.72f,-0.10f),new Vector2(1.72f,-0.10f),new Vector2(1.50f,0.48f),new Vector2(1.18f,0.68f),new Vector2(-1.18f,0.68f),new Vector2(-1.50f,0.48f)},0.42f,new Vector3(0f,0.24f,0f),Quaternion.identity,"StructuralMetal");
            for(var side=-1;side<=1;side+=2) AddExtruded(m,"Barrier_Buttress_"+side,new[]{new Vector2(-0.25f,-0.08f),new Vector2(0.34f,-0.08f),new Vector2(0.52f,0.28f),new Vector2(0.19f,1.56f),new Vector2(-0.18f,1.56f),new Vector2(-0.38f,0.25f)},0.50f,new Vector3(side*1.32f,0.75f,0f),Quaternion.identity,"WornPaint");
            AddLoft(m,"Barrier_UpperRail",new[]{Ring(-1.42f,0.12f,0.14f),Ring(0f,0.17f,0.18f,0f,0f,22.5f),Ring(1.42f,0.12f,0.14f)},8,new Vector3(0f,1.50f,0f),Quaternion.Euler(0f,0f,90f),"ArmorEdge");
            AddExtruded(m,"Barrier_EnergyMarker",ChamferedPolygon(0.72f,0.14f,0.04f),0.045f,new Vector3(0f,1.20f,-0.27f),Quaternion.identity,index%3==2?"WarmEnergy":"CyanEnergy");
            var c=m.gameObject.AddComponent<BoxCollider>(); c.center=new Vector3(0f,0.74f,0f); c.size=new Vector3(3.45f,1.52f,0.56f);
        }

        private Transform BuildPropCluster(Vector3 position)
        {
            var root=NewRoot("Environment_IndustrialPropCluster").transform; root.position=position; root.SetParent(_environmentRoot,true);
            for(var i=0;i<3;i++){var x=(i-1)*0.86f; AddLoft(root,"PressureVessel_"+i,new[]{Ring(-0.82f,0.31f,0.31f),Ring(-0.63f,0.42f,0.42f,0f,0f,22.5f),Ring(0.50f,0.42f,0.42f),Ring(0.78f,0.29f,0.29f,0f,0f,22.5f)},8,new Vector3(x,0.88f,0f),Quaternion.identity,i==1?"WornPaint":"StructuralMetal"); AddTorus(root,"Vessel_Ring_"+i,0.43f,0.055f,16,6,new Vector3(x,1.06f,0f),Quaternion.identity,"ArmorEdge");}
            AddTube(root,"Manifold_Header",new[]{new Vector3(-1.2f,1.42f,0f),new Vector3(0f,1.58f,0.03f),new Vector3(1.2f,1.42f,0f)},0.09f,8,"HeatMetal");
            AddExtruded(root,"ControlConsole",new[]{new Vector2(-0.62f,-0.40f),new Vector2(0.62f,-0.40f),new Vector2(0.52f,0.48f),new Vector2(-0.40f,0.61f),new Vector2(-0.64f,0.20f)},0.48f,new Vector3(0f,0.56f,-0.90f),Quaternion.identity,"ArmorPaint");
            AddExtruded(root,"ControlConsole_Display",ChamferedPolygon(0.72f,0.28f,0.05f),0.035f,new Vector3(0f,0.84f,-1.15f),Quaternion.Euler(-13f,0f,0f),"CyanEnergy");
            var c=root.gameObject.AddComponent<BoxCollider>(); c.center=new Vector3(0f,0.78f,-0.20f); c.size=new Vector3(3f,1.65f,1.45f); return root;
        }

        private void BuildReviewLighting()
        {
            var keyGo=new GameObject("Light_KeyDirectional"); keyGo.transform.SetParent(transform,false); keyGo.transform.rotation=Quaternion.Euler(52f,-34f,0f); var key=keyGo.AddComponent<Light>(); key.type=LightType.Directional; key.color=new Color(0.61f,0.73f,0.86f); key.intensity=0.78f; key.shadows=LightShadows.Soft; key.shadowStrength=0.78f;
            var cyanGo=new GameObject("Light_ReactorCyan"); cyanGo.transform.SetParent(transform,false); cyanGo.transform.position=new Vector3(5.8f,2.05f,6.4f); var cyan=cyanGo.AddComponent<Light>(); cyan.type=LightType.Point; cyan.color=new Color(0.12f,0.70f,1f); cyan.intensity=4.2f; cyan.range=7.5f; cyan.shadows=LightShadows.None;
            var warmGo=new GameObject("Light_WarmMachineryAccent"); warmGo.transform.SetParent(transform,false); warmGo.transform.position=new Vector3(4f,5.2f,7.8f); warmGo.transform.rotation=Quaternion.LookRotation(new Vector3(1.8f,-3.8f,-1.2f)); var warm=warmGo.AddComponent<Light>(); warm.type=LightType.Spot; warm.color=new Color(1f,0.34f,0.11f); warm.intensity=7f; warm.range=9.5f; warm.spotAngle=44f; warm.shadows=LightShadows.Soft;
        }

        private void BuildReviewCamera()
        {
            var go=new GameObject("Camera_Proof_GameplayPortrait"); go.transform.SetParent(transform,false); _reviewCamera=go.AddComponent<Camera>(); _reviewCamera.fieldOfView=CameraFov; _reviewCamera.nearClipPlane=0.15f; _reviewCamera.farClipPlane=80f; _reviewCamera.allowHDR=true; _reviewCamera.clearFlags=CameraClearFlags.SolidColor; _reviewCamera.backgroundColor=new Color(0.018f,0.025f,0.032f,1f); go.tag="MainCamera"; AimCamera(new Vector3(0f,1.30f,1.4f));
        }

        private void BuildReadabilityProxies()
        {
            _attackProxyRoot=NewRoot("PROOF_ONLY_AttackReadabilityProxy").transform;
            var start=_g0AttackSocket.position; var end=_scoutHitSocket.position; var mid=(start+end)*0.5f; var len=Vector3.Distance(start,end);
            AddLoft(_attackProxyRoot,"Attack_EnergyLance",new[]{Ring(-len*0.5f,0.055f,0.055f),Ring(0f,0.095f,0.095f,0f,0f,22.5f),Ring(len*0.5f,0.028f,0.028f)},8,mid,Quaternion.FromToRotation(Vector3.up,end-start),"CyanEnergy"); AddTorus(_attackProxyRoot,"Attack_ImpactRing",0.34f,0.045f,20,6,end,Quaternion.LookRotation((start-end).normalized)*Quaternion.Euler(90f,0f,0f),"WarmEnergy");
            _deathProxyRoot=NewRoot("PROOF_ONLY_DeathReadabilityProxy").transform; var center=_scoutDeathSocket.position; AddTorus(_deathProxyRoot,"Death_ContrastRing",0.72f,0.055f,24,7,center+Vector3.up*0.15f,Quaternion.Euler(90f,0f,0f),"WarmEnergy");
            for(var i=0;i<7;i++){var a=i*(360f/7f)*Mathf.Deg2Rad; var off=new Vector3(Mathf.Cos(a),0.35f+(i%3)*0.17f,Mathf.Sin(a))*0.65f; AddExtruded(_deathProxyRoot,"Death_ClearanceShard_"+i,BladePolygon(0.50f+(i%2)*0.18f,0.18f),0.08f,center+off,Quaternion.Euler(20f+i*11f,i*37f,i*13f),i%2==0?"CyanEnergy":"WarmEnergy");}
            _attackProxyRoot.gameObject.SetActive(false); _deathProxyRoot.gameObject.SetActive(false);
        }
    }
}
