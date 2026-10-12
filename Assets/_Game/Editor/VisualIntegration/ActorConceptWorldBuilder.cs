using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public static class ActorConceptWorldBuilder
    {
        public const string Evidence="docs/history/implementation-passes/actors-combat-concept-world";
        public static void Build()
        {
            AssetDatabase.Refresh();
            var shader=Shader.Find("Gravivore/Actor Industrial Lit");
            if(shader==null)throw new InvalidOperationException("Industrial actor shader missing");
            var paths=AssetDatabase.FindAssets("t:Material",new[]{Chapter01ActorIntegrationBuilder.Root+"/Materials","Assets/_Game/Content/VisualSlice/Materials"})
                .Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("HostileV2_")||p.Contains("G0_V3_Tier")).ToArray();
            foreach(var path in paths)
            {
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);material.shader=shader;
                material.SetColor("_BaseColor",Color.white);
                material.SetFloat("_BumpScale",.4f);material.SetFloat("_Smoothness",.82f);material.SetFloat("_OcclusionStrength",.85f);
                material.SetVector("_ActorSky",new Vector4(.22f,.26f,.32f,1));material.SetVector("_ActorGround",new Vector4(.055f,.065f,.085f,1));
                material.SetFloat("_ActorFill",1.1f);
                material.SetColor("_EmissionColor",Color.white*(path.Contains("Blue")?1.8f:path.Contains("Amber")?1.6f:1.45f));
                material.EnableKeyword("_EMISSION");EditorUtility.SetDirty(material);
            }
            AssetDatabase.SaveAssets();Directory.CreateDirectory(Evidence+"/verification");
            var profiles=AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.Assets.HostilePresentationDefinition>(Chapter01ActorIntegrationBuilder.Root+"/HostileMotion.asset");
            for(var i=0;i<profiles.Profiles.Length;i++)
            {
                var profile=profiles.Profiles[i];profile.MaximumWallOffset=.08f;
                profile.AttackColor=i==3?new Color(.12f,.58f,1,.85f):i==2||i==4||i==5?new Color(1,.48f,.075f,.8f):new Color(1,.18f,.08f,.8f);
                profile.ElectricalTravel=i==3||i>=5;profile.PairedAttackSources=i<=2;
                profile.AttackStartNormalizedTime=i<=2?.28f:0;
            }
            EditorUtility.SetDirty(profiles);AssetDatabase.SaveAssets();
            var redPath=Chapter01ActorIntegrationBuilder.Root+"/Textures/HostileV2_ContainedRedShell.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(redPath);
            EditorUtility.CopySerialized(AssetImporter.GetAtPath(Chapter01ActorIntegrationBuilder.Root+"/Textures/HostileV2_BaseColor.png"),importer);
            importer.SaveAndReimport();
            foreach(var actor in new[]{"Cutter","Custodian"})
            {
                var materialPath=Chapter01ActorIntegrationBuilder.Root+"/Materials/"+actor+"_ConceptShell.mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                var basis=AssetDatabase.LoadAssetAtPath<Material>(Chapter01ActorIntegrationBuilder.Root+"/Materials/HostileV2_Red.mat");
                if(material==null){material=new Material(basis);AssetDatabase.CreateAsset(material,materialPath);}else material.CopyPropertiesFromMaterial(basis);
                material.shader=shader;material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(redPath));EditorUtility.SetDirty(material);
                var prefab=PrefabUtility.LoadPrefabContents(Chapter01ActorIntegrationBuilder.Prefab(actor));
                try{foreach(var skin in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))skin.sharedMaterial=material;PrefabUtility.SaveAsPrefabAsset(prefab,Chapter01ActorIntegrationBuilder.Prefab(actor));}
                finally{PrefabUtility.UnloadPrefabContents(prefab);}
            }
            AssetDatabase.SaveAssets();
            var catalog=new SerializedObject(AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.Assets.S15VisualCatalog>("Assets/_Game/Content/Definitions/S15_VisualCatalog.asset"));
            catalog.FindProperty("_playerWallEnvelopeRadius").floatValue=1.2f;catalog.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();
            foreach(var actor in Chapter01ActorIntegrationBuilder.Actors)
            {
                var prefab=PrefabUtility.LoadPrefabContents(Chapter01ActorIntegrationBuilder.Prefab(actor));
                try
                {
                    var group=prefab.GetComponentInChildren<LODGroup>();var lods=group.GetLODs();
                    lods[0].screenRelativeTransitionHeight=actor=="Magnetar"||actor=="Custodian"?.19f:.24f;
                    lods[1].screenRelativeTransitionHeight=.105f;lods[2].screenRelativeTransitionHeight=.025f;
                    group.SetLODs(lods);PrefabUtility.SaveAsPrefabAsset(prefab,Chapter01ActorIntegrationBuilder.Prefab(actor));
                }
                finally{PrefabUtility.UnloadPrefabContents(prefab);}
            }
            File.WriteAllLines(Evidence+"/verification/actor-materials.txt",paths);
            BuildSupportLayer(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab"),"G0");
            foreach(var actor in new[]{"Magnetar","Custodian"})BuildSupportLayer(AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01ActorIntegrationBuilder.Prefab(actor)),actor);
            AssetDatabase.SaveAssets();
            Debug.Log("ACTOR_CONCEPT_MATERIAL_AUTHOR_PASS");
            var compatibility=typeof(ShaderUtil).GetMethods(System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)
                .FirstOrDefault(m=>m.Name=="GetSRPBatcherCompatibilityCode"&&m.GetParameters().Length==2);
            File.WriteAllText(Evidence+"/verification/actor-srp-batcher.txt",compatibility==null?"Local Unity API unavailable":Convert.ToString(compatibility.Invoke(null,new object[]{shader,0})));
            var reason=typeof(ShaderUtil).GetMethods(System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).FirstOrDefault(m=>m.Name=="GetSRPBatcherCompatibilityIssueReason"&&m.GetParameters().Length==3);
            if(compatibility!=null&&reason!=null)File.AppendAllText(Evidence+"/verification/actor-srp-batcher.txt","\n"+Convert.ToString(reason.Invoke(null,new object[]{shader,0,compatibility.Invoke(null,new object[]{shader,0})})));
        }
        private static void BuildSupportLayer(GameObject prefab,string actor)
        {
            var animator=prefab.GetComponentInChildren<Animator>(true);var controller=(AnimatorController)animator.runtimeAnimatorController;
            const string layerName="Industrial moving supports";const string phaseName="IndustrialGaitPhase";
            var maskPath=Chapter01ActorIntegrationBuilder.Root+"/"+actor+"_MovingSupports.mask";
            var mask=AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
            if(mask==null){mask=new AvatarMask();AssetDatabase.CreateAsset(mask,maskPath);}
            var nodes=animator.GetComponentsInChildren<Transform>(true);mask.transformCount=nodes.Length;
            bool Leg(Transform node)
            {
                for(var t=node;t!=null&&t!=animator.transform;t=t.parent)
                    if(t.name.StartsWith("SUPPORT_")||t.name.StartsWith("KNEE_")||t.name.StartsWith("FOOT_")||t.name=="L_HIP"||t.name=="R_HIP")return true;
                return false;
            }
            for(var i=0;i<nodes.Length;i++){mask.SetTransformPath(i,AnimationUtility.CalculateTransformPath(nodes[i],animator.transform));mask.SetTransformActive(i,Leg(nodes[i]));}
            EditorUtility.SetDirty(mask);
            if(!controller.parameters.Any(p=>p.name==phaseName))controller.AddParameter(phaseName,AnimatorControllerParameterType.Float);
            var index=Array.FindIndex(controller.layers,l=>l.name==layerName);
            if(index<0){controller.AddLayer(layerName);index=controller.layers.Length-1;}
            var layers=controller.layers;var layer=layers[index];layer.avatarMask=mask;layer.defaultWeight=0;layer.blendingMode=AnimatorLayerBlendingMode.Override;
            var state=layer.stateMachine.states.FirstOrDefault(s=>s.state.name=="Distance driven support gait").state;
            if(state==null)state=layer.stateMachine.AddState("Distance driven support gait");
            state.motion=controller.animationClips.First(c=>c.name=="Run");state.timeParameter=phaseName;state.timeParameterActive=true;
            layer.stateMachine.defaultState=state;layers[index]=layer;controller.layers=layers;EditorUtility.SetDirty(controller);
        }
    }
}
