using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.VisualReplacementProofV1
{
    public sealed partial class VisualReplacementProofBootstrap
    {
        private Transform BuildG0(Vector3 position)
        {
            var root = NewRoot("Actor_G0_Replacement").transform;
            root.position = position;
            root.rotation = Quaternion.Euler(0f, 18f, 0f);
            AddLoft(root, "Torso_CoreShell", new[] { Ring(-0.72f,0.58f,0.38f), Ring(-0.36f,0.76f,0.47f,0f,0.02f,11f), Ring(0.18f,0.92f,0.52f,0f,0.06f), Ring(0.62f,0.72f,0.43f,0f,0.03f,11f), Ring(0.82f,0.48f,0.34f) }, 10, Vector3.up*2.82f, Quaternion.identity, "ArmorGunmetal");
            AddLoft(root, "Torso_ChestArmor", new[] { Ring(-0.30f,0.54f,0.18f), Ring(0.08f,0.75f,0.24f,0f,-0.02f,11f), Ring(0.40f,0.61f,0.20f) }, 8, new Vector3(0f,3.03f,-0.48f), Quaternion.identity, "ArmorPaint");
            AddLoft(root, "Torso_RearPowerSpine", new[] { Ring(-0.48f,0.22f,0.18f), Ring(0.10f,0.29f,0.22f,0f,0.02f,22.5f), Ring(0.50f,0.18f,0.15f) }, 8, new Vector3(0f,2.9f,0.52f), Quaternion.identity, "StructuralMetal");
            AddLoft(root, "Head_SensorCowl", new[] { Ring(-0.18f,0.34f,0.30f), Ring(0.03f,0.43f,0.37f,0f,-0.03f,22.5f), Ring(0.31f,0.28f,0.26f,0f,-0.09f) }, 8, new Vector3(0f,4.08f,-0.03f), Quaternion.identity, "ArmorEdge");
            AddLoft(root, "Head_SensorLens", new[] { Ring(-0.11f,0.20f,0.055f), Ring(0.10f,0.25f,0.07f) }, 8, new Vector3(0f,4.12f,-0.34f), Quaternion.identity, "CyanEnergy");
            AddTorus(root, "Core_EnergyCollar",0.36f,0.065f,20,7,new Vector3(0f,3.14f,-0.54f),Quaternion.Euler(90f,0f,0f),"CyanEnergy");
            BuildG0Arm(root,-1f); BuildG0Arm(root,1f); BuildG0Leg(root,-1f); BuildG0Leg(root,1f);
            AddArmorFin(root,"Left_BackFin",-0.52f); AddArmorFin(root,"Right_BackFin",0.52f);
            _g0AttackSocket=NewSocket(root,"Socket_Attack_Right",new Vector3(1.12f,2.18f,-0.48f));
            NewSocket(root,"Socket_Hit_CenterMass",new Vector3(0f,3.0f,-0.5f));
            return root;
        }

        private void BuildG0Arm(Transform root,float side)
        {
            var x=side*0.96f;
            AddLoft(root,SideName(side,"Shoulder_Gimbal"),new[]{Ring(-0.22f,0.31f,0.30f),Ring(0.22f,0.39f,0.38f,0f,0f,22.5f)},8,new Vector3(x,3.35f,-0.01f),Quaternion.Euler(0f,0f,side*-8f),"JointRubber");
            AddExtruded(root,SideName(side,"Shoulder_Pauldrons"),ChamferedPolygon(1.10f,0.58f,0.16f),0.34f,new Vector3(side*1.12f,3.48f,-0.02f),Quaternion.Euler(0f,0f,side*-8f),"ArmorPaint");
            AddLoft(root,SideName(side,"UpperArm"),new[]{Ring(-0.50f,0.24f,0.22f),Ring(-0.04f,0.31f,0.28f,0f,0.02f,22.5f),Ring(0.48f,0.21f,0.20f)},8,new Vector3(side*1.03f,2.67f,-0.02f),Quaternion.Euler(0f,0f,side*6f),"ArmorGunmetal");
            AddTorus(root,SideName(side,"Elbow_Joint"),0.23f,0.075f,14,7,new Vector3(side*1.08f,2.11f,-0.02f),Quaternion.Euler(0f,0f,90f),"JointRubber");
            AddLoft(root,SideName(side,"Forearm"),new[]{Ring(-0.47f,0.20f,0.20f),Ring(0.08f,0.31f,0.27f,0f,-0.02f,22.5f),Ring(0.46f,0.25f,0.22f)},8,new Vector3(side*1.09f,1.57f,-0.08f),Quaternion.Euler(side*3f,0f,side*2f),"ArmorGunmetal");
            AddExtruded(root,SideName(side,"Forearm_Guard"),ChamferedPolygon(0.54f,0.78f,0.12f),0.16f,new Vector3(side*1.09f,1.65f,-0.27f),Quaternion.identity,"ArmorEdge");
            AddLoft(root,SideName(side,"Hand"),new[]{Ring(-0.20f,0.20f,0.17f),Ring(0.20f,0.23f,0.19f,0f,-0.02f,22.5f)},8,new Vector3(side*1.10f,1.02f,-0.12f),Quaternion.identity,"JointRubber");
        }

        private void BuildG0Leg(Transform root,float side)
        {
            var hipX=side*0.46f;
            AddTorus(root,SideName(side,"Hip_Ring"),0.27f,0.085f,16,7,new Vector3(hipX,2.08f,0.03f),Quaternion.Euler(0f,0f,90f),"JointRubber");
            AddLoft(root,SideName(side,"Thigh"),new[]{Ring(-0.62f,0.27f,0.28f),Ring(0f,0.38f,0.33f,side*0.03f,0.02f,22.5f),Ring(0.58f,0.31f,0.30f)},8,new Vector3(side*0.49f,1.47f,0.03f),Quaternion.Euler(3f,0f,side*-4f),"ArmorGunmetal");
            AddExtruded(root,SideName(side,"Thigh_OuterArmor"),new[]{new Vector2(-0.24f,-0.55f),new Vector2(0.30f,-0.38f),new Vector2(0.36f,0.28f),new Vector2(0.16f,0.58f),new Vector2(-0.29f,0.46f)},0.16f,new Vector3(side*0.74f,1.48f,0.02f),Quaternion.Euler(0f,side>0f?90f:-90f,0f),"ArmorPaint");
            AddTorus(root,SideName(side,"Knee_Joint"),0.24f,0.075f,14,7,new Vector3(side*0.50f,0.80f,-0.03f),Quaternion.Euler(0f,0f,90f),"JointRubber");
            AddExtruded(root,SideName(side,"Knee_Armor"),new[]{new Vector2(-0.26f,-0.22f),new Vector2(0.26f,-0.22f),new Vector2(0.34f,0.08f),new Vector2(0f,0.42f),new Vector2(-0.34f,0.08f)},0.22f,new Vector3(side*0.50f,0.83f,-0.27f),Quaternion.identity,"ArmorEdge");
            AddLoft(root,SideName(side,"Shin"),new[]{Ring(-0.54f,0.24f,0.25f),Ring(-0.05f,0.31f,0.34f,0f,0.05f,22.5f),Ring(0.45f,0.23f,0.25f)},8,new Vector3(side*0.51f,0.30f,0.03f),Quaternion.Euler(-7f,0f,side*2f),"ArmorGunmetal");
            AddExtruded(root,SideName(side,"Foot"),new[]{new Vector2(-0.35f,-0.16f),new Vector2(0.33f,-0.16f),new Vector2(0.48f,0.01f),new Vector2(0.30f,0.25f),new Vector2(-0.30f,0.25f)},0.52f,new Vector3(side*0.51f,0.12f,-0.33f),Quaternion.Euler(90f,0f,0f),"ArmorEdge");
        }

        private void AddArmorFin(Transform root,string name,float x)
        {
            AddExtruded(root,name,new[]{new Vector2(-0.16f,-0.44f),new Vector2(0.16f,-0.44f),new Vector2(0.24f,0.38f),new Vector2(0f,0.74f),new Vector2(-0.22f,0.34f)},0.15f,new Vector3(x,3.31f,0.55f),Quaternion.Euler(10f,0f,x>0f?-8f:8f),"ArmorEdge");
        }
    }
}
