using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.VisualReplacementProofV1
{
    public sealed partial class VisualReplacementProofBootstrap
    {
        private Transform BuildScout(Vector3 position)
        {
            var root=NewRoot("Actor_Scout_Replacement").transform; root.position=position; root.rotation=Quaternion.Euler(0f,-22f,0f);
            AddLoft(root,"Scout_MainHull",new[]{Ring(-0.58f,0.43f,0.54f,0f,0.16f),Ring(-0.12f,0.62f,0.72f,0f,0.08f,22.5f),Ring(0.32f,0.53f,0.61f,0f,-0.08f),Ring(0.54f,0.34f,0.42f,0f,-0.16f)},8,new Vector3(0f,1.46f,0f),Quaternion.Euler(14f,0f,0f),"ArmorGunmetal");
            AddLoft(root,"Scout_SensorProw",new[]{Ring(-0.24f,0.34f,0.28f,0f,0.04f),Ring(0.08f,0.46f,0.32f,0f,-0.04f,22.5f),Ring(0.28f,0.23f,0.18f,0f,-0.18f)},8,new Vector3(0f,1.77f,-0.70f),Quaternion.Euler(9f,0f,0f),"ArmorPaint");
            AddLoft(root,"Scout_Sensor",new[]{Ring(-0.09f,0.18f,0.05f),Ring(0.09f,0.23f,0.065f)},8,new Vector3(0f,1.84f,-0.98f),Quaternion.identity,"CyanEnergy");
            AddTorus(root,"Scout_CoreRing",0.29f,0.065f,18,7,new Vector3(0f,1.55f,-0.65f),Quaternion.Euler(90f,0f,0f),"CyanEnergy");
            BuildScoutLeg(root,-1f); BuildScoutLeg(root,1f);
            AddExtruded(root,"Scout_DorsalFin",new[]{new Vector2(-0.44f,-0.15f),new Vector2(0.35f,-0.15f),new Vector2(0.56f,0.02f),new Vector2(0.08f,0.48f),new Vector2(-0.36f,0.26f)},0.10f,new Vector3(0f,2.10f,0.12f),Quaternion.Euler(0f,90f,0f),"ArmorEdge");
            AddExtruded(root,"Scout_LeftFlankBlade",BladePolygon(0.72f,0.28f),0.11f,new Vector3(-0.60f,1.48f,-0.08f),Quaternion.Euler(0f,58f,-8f),"ArmorEdge");
            AddExtruded(root,"Scout_RightFlankBlade",BladePolygon(0.72f,0.28f),0.11f,new Vector3(0.60f,1.48f,-0.08f),Quaternion.Euler(0f,-58f,8f),"ArmorEdge");
            for(var side=-1;side<=1;side+=2){AddLoft(root,"Scout_RearThruster_"+side,new[]{Ring(-0.24f,0.16f,0.16f),Ring(0.06f,0.23f,0.23f,0f,0.03f,22.5f),Ring(0.25f,0.18f,0.18f)},8,new Vector3(side*0.42f,1.54f,0.60f),Quaternion.Euler(90f,0f,0f),"StructuralMetal"); AddTorus(root,"Scout_ThrusterGlow_"+side,0.16f,0.04f,14,6,new Vector3(side*0.42f,1.54f,0.82f),Quaternion.Euler(90f,0f,0f),"CyanEnergy"); AddExtruded(root,"Scout_LegGuard_"+side,new[]{new Vector2(-0.18f,-0.34f),new Vector2(0.20f,-0.27f),new Vector2(0.25f,0.22f),new Vector2(0f,0.41f),new Vector2(-0.22f,0.20f)},0.10f,new Vector3(side*0.74f,0.62f,0.11f),Quaternion.Euler(0f,side>0?90f:-90f,side*8f),"ArmorPaint");}
            _scoutHitSocket=NewSocket(root,"Socket_Hit_Core",new Vector3(0f,1.62f,-0.74f)); _scoutDeathSocket=NewSocket(root,"Socket_Death_Center",new Vector3(0f,1.45f,-0.08f)); return root;
        }

        private void BuildScoutLeg(Transform root,float side)
        {
            AddTorus(root,SideName(side,"Scout_Hip"),0.20f,0.06f,14,7,new Vector3(side*0.46f,1.38f,0.18f),Quaternion.Euler(0f,0f,90f),"JointRubber");
            AddLoft(root,SideName(side,"Scout_UpperLeg"),new[]{Ring(-0.38f,0.16f,0.17f),Ring(0.06f,0.22f,0.21f,side*0.03f,0.02f,22.5f),Ring(0.34f,0.15f,0.16f)},8,new Vector3(side*0.50f,0.99f,0.24f),Quaternion.Euler(-14f,0f,side*-8f),"ArmorGunmetal");
            AddTorus(root,SideName(side,"Scout_Knee"),0.16f,0.05f,12,6,new Vector3(side*0.55f,0.60f,0.34f),Quaternion.Euler(0f,0f,90f),"JointRubber");
            AddLoft(root,SideName(side,"Scout_LowerLeg"),new[]{Ring(-0.38f,0.12f,0.14f),Ring(0f,0.18f,0.18f,0f,0.02f,22.5f),Ring(0.32f,0.13f,0.15f)},8,new Vector3(side*0.57f,0.28f,0.12f),Quaternion.Euler(16f,0f,side*6f),"ArmorEdge");
            AddExtruded(root,SideName(side,"Scout_Foot"),new[]{new Vector2(-0.23f,-0.12f),new Vector2(0.27f,-0.12f),new Vector2(0.43f,0f),new Vector2(0.22f,0.18f),new Vector2(-0.22f,0.18f)},0.32f,new Vector3(side*0.58f,0.09f,-0.10f),Quaternion.Euler(90f,0f,0f),"ArmorEdge");
        }

        private Transform BuildCutter(Vector3 position)
        {
            var root=NewRoot("Actor_Cutter_Replacement").transform; root.position=position; root.rotation=Quaternion.Euler(0f,154f,0f);
            AddLoft(root,"Cutter_MainMass",new[]{Ring(-0.45f,0.88f,0.62f,0f,0.10f),Ring(-0.08f,1.02f,0.72f,0f,0.04f,15f),Ring(0.34f,0.82f,0.60f,0f,-0.10f),Ring(0.54f,0.54f,0.42f,0f,-0.22f)},10,new Vector3(0f,0.96f,0f),Quaternion.Euler(7f,0f,0f),"ArmorGunmetal");
            AddExtruded(root,"Cutter_FrontalPlow",new[]{new Vector2(-1.12f,-0.28f),new Vector2(1.12f,-0.28f),new Vector2(1.34f,0.06f),new Vector2(0.82f,0.47f),new Vector2(-0.82f,0.47f),new Vector2(-1.34f,0.06f)},0.24f,new Vector3(0f,0.80f,-0.72f),Quaternion.identity,"ArmorPaint");
            AddExtruded(root,"Cutter_LeftBlade",BladePolygon(1.30f,0.34f),0.13f,new Vector3(-0.84f,0.74f,-0.93f),Quaternion.Euler(0f,15f,-8f),"ArmorEdge"); AddExtruded(root,"Cutter_RightBlade",BladePolygon(1.30f,0.34f),0.13f,new Vector3(0.84f,0.74f,-0.93f),Quaternion.Euler(0f,-15f,8f),"ArmorEdge");
            AddLoft(root,"Cutter_LeftDrive",new[]{Ring(-0.54f,0.23f,0.55f),Ring(0.05f,0.31f,0.67f,0f,0.04f,22.5f),Ring(0.40f,0.23f,0.50f)},8,new Vector3(-0.91f,0.51f,0.17f),Quaternion.Euler(0f,0f,90f),"StructuralMetal"); AddLoft(root,"Cutter_RightDrive",new[]{Ring(-0.54f,0.23f,0.55f),Ring(0.05f,0.31f,0.67f,0f,0.04f,22.5f),Ring(0.40f,0.23f,0.50f)},8,new Vector3(0.91f,0.51f,0.17f),Quaternion.Euler(0f,0f,90f),"StructuralMetal");
            AddTorus(root,"Cutter_CoreRing",0.34f,0.075f,20,7,new Vector3(0f,1.16f,-0.61f),Quaternion.Euler(90f,0f,0f),"WarmEnergy"); AddLoft(root,"Cutter_Core",new[]{Ring(-0.10f,0.21f,0.06f),Ring(0.10f,0.25f,0.07f)},8,new Vector3(0f,1.16f,-0.68f),Quaternion.identity,"WarmEnergy");
            AddTube(root,"Cutter_CoolingLoop",new[]{new Vector3(-0.60f,1.28f,0.27f),new Vector3(-0.72f,1.48f,0.05f),new Vector3(0f,1.56f,0.18f),new Vector3(0.72f,1.48f,0.05f),new Vector3(0.60f,1.28f,0.27f)},0.06f,7,"HeatMetal");
            foreach(var side in new[]{-1f,1f}){for(var ring=0;ring<2;ring++)AddTorus(root,"Cutter_DriveRing_"+side+"_"+ring,0.33f+ring*0.03f,0.055f,16,6,new Vector3(side*0.93f,0.55f,-0.12f+ring*0.48f),Quaternion.Euler(90f,0f,0f),ring==0?"ArmorEdge":"StructuralMetal"); AddExtruded(root,"Cutter_SideArmor_"+side,new[]{new Vector2(-0.42f,-0.28f),new Vector2(0.40f,-0.24f),new Vector2(0.53f,0.16f),new Vector2(0.12f,0.48f),new Vector2(-0.46f,0.30f)},0.11f,new Vector3(side*1.15f,0.83f,0.12f),Quaternion.Euler(0f,side>0?90f:-90f,side*6f),"ArmorPaint");}
            for(var vent=0;vent<3;vent++)AddLoft(root,"Cutter_HeatVent_"+vent,new[]{Ring(-0.13f,0.10f,0.10f),Ring(0.13f,0.14f,0.14f,0f,0f,22.5f)},8,new Vector3(-0.38f+vent*0.38f,1.56f,0.18f),Quaternion.identity,"HeatMetal");
            NewSocket(root,"Socket_Hit_Core",new Vector3(0f,1.12f,-0.68f)); NewSocket(root,"Socket_Death_Center",new Vector3(0f,0.95f,-0.02f)); return root;
        }
    }
}
