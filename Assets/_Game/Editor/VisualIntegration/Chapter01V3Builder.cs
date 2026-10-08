using System;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Equipment;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    public static class Chapter01V3Builder
    {
        public const string Root = "Assets/_Game/Content/Chapter01V3";
        public static string Prefab(string name) => Root + "/Prefabs/" + name + ".prefab";
        public static readonly string[] Models = { "Custodian_V3", "Emitter_M0", "Broken_Edge_V3_0", "Broken_Edge_V3_1", "Service_Trench_V3", "Collapsed_Hull_V3" };
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Prefabs"); AssetDatabase.Refresh();
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Content/ConceptFidelityV2/Materials/Fidelity_IndustrialAtlas.mat");
            foreach(var name in Models) ImportPrefab(name, material, name == "Custodian_V3");
            var emitter = PrefabUtility.LoadPrefabContents(Prefab("Emitter_M0"));
            var muzzle = new GameObject("Muzzle").transform; muzzle.SetParent(emitter.transform,false); muzzle.localPosition=new Vector3(0,.02f,.56f);
            PrefabUtility.SaveAsPrefabAsset(emitter,Prefab("Emitter_M0"));PrefabUtility.UnloadPrefabContents(emitter);
            var itemPath=Root+"/ImpulseEmitterM0.asset";
            var item=AssetDatabase.LoadAssetAtPath<EquipmentDefinition>(itemPath);
            if(item==null){item=ScriptableObject.CreateInstance<EquipmentDefinition>();AssetDatabase.CreateAsset(item,itemPath);}
            var data=new SerializedObject(item);data.FindProperty("_id").stringValue=Chapter01Weapon.ItemId;
            data.FindProperty("_displayName").stringValue="ИМПУЛЬСНЫЙ ИЗЛУЧАТЕЛЬ М-0";data.FindProperty("_slot").enumValueIndex=(int)EquipmentSlot.Weapon;
            data.FindProperty("_baseDamage").floatValue=12;data.FindProperty("_maximumRank").intValue=5;data.FindProperty("_damagePerRank").floatValue=4;
            data.ApplyModifiedPropertiesWithoutUndo();item.ValidateOrThrow();
            var catalog=AssetDatabase.LoadAssetAtPath<EquipmentCatalogDefinition>("Assets/_Game/Content/Definitions/S10_EquipmentCatalog.asset");
            data=new SerializedObject(catalog);var items=data.FindProperty("_items");
            var found=false;for(var i=0;i<items.arraySize;i++)if(items.GetArrayElementAtIndex(i).objectReferenceValue==item)found=true;
            if(!found){items.InsertArrayElementAtIndex(items.arraySize);items.GetArrayElementAtIndex(items.arraySize-1).objectReferenceValue=item;}
            data.ApplyModifiedPropertiesWithoutUndo();catalog.ValidateOrThrow();
            var definition=AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset");
            data=new SerializedObject(definition);var boss=data.FindProperty("_boss");
            boss.FindPropertyRelative("_prefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("Custodian_V3"));
            boss.FindPropertyRelative("_localPosition").vector3Value=Vector3.zero;boss.FindPropertyRelative("_localScale").vector3Value=Vector3.one;
            data.FindProperty("_weaponPrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("Emitter_M0"));
            data.FindProperty("_hostileCharge").objectReferenceValue=Effect("HostileChargeV3",Phase6BVfxCue.HostileCharge,Phase6BVfxShape.Charge,.35f);
            data.FindProperty("_hostileTravel").objectReferenceValue=Effect("HostileTravelV3",Phase6BVfxCue.HostileTravel,Phase6BVfxShape.Beam,.14f);
            data.FindProperty("_hostileImpact").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/Presentation/Phase6B/VFX/Prefabs/PFX_HostileImpact.prefab").GetComponent<Phase6BVfxInstance>();
            data.ApplyModifiedPropertiesWithoutUndo();
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var composition=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var env=composition.VisualEnvironment;
            // Device-reported repair exit: these two full-mesh AABB proxies intrude into
            // the right-hand lane. Move the authored machine and its matching authority together.
            data=new SerializedObject(env);var blockers=data.FindProperty("_sliceObstacles");
            for(var i=0;i<blockers.arraySize;i++)
            {
                var blocker=blockers.GetArrayElementAtIndex(i);var name=blocker.FindPropertyRelative("_name").stringValue;
                var center=blocker.FindPropertyRelative("_center").vector3Value;
                if((name=="Chapter01 Service corridors Structural_Support 100"||name=="Chapter01 Service corridors Maintenance_Station 101")&&center.x<7)
                {
                    var kind=name.Contains("Structural_Support")?"Structural_Support":"Maintenance_Station";
                    var original=kind=="Structural_Support"?new Vector3(4.9f,0,-28):new Vector3(5.6f,0,-26.5f);
                    var node=env.Floor.GetComponentsInChildren<Transform>(true).Single(t=>t.name==kind&&Vector3.Distance(t.position,original)<.05f);
                    node.position+=Vector3.right*3;blocker.FindPropertyRelative("_center").vector3Value=center+Vector3.right*3;
                }
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            var prior=env.Floor.Find("Chapter 01 V3 richness");if(prior!=null)Object.DestroyImmediate(prior.gameObject);
            var route=Group(env.Floor,"Chapter 01 V3 richness");
            var edges=Group(route,"Broken perimeter and outside-wall depth");
            for(var z=-34;z<=96;z+=10)foreach(var side in new[]{-1,1})
            {
                Place(edges,"Broken_Edge_V3_"+((z+34)/10%2),new Vector3(side*36.2f,.025f,z),side<0?0:180);
                if((z+34)%20==0)Place(edges,"Collapsed_Hull_V3",new Vector3(side*39f,-.25f,z+3),side*25);
                Fault(route,new Vector3(side*35.5f,.14f,z),((z+34)/10%2==0)?"cyan":"amber");
            }
            for(var x=-30;x<=30;x+=10)
            {
                Place(edges,"Broken_Edge_V3_1",new Vector3(x,.025f,100.2f),90);
                Place(edges,"Broken_Edge_V3_0",new Vector3(x,.025f,-40.2f),-90);
            }
            var services=Group(route,"Route recessed conduits and repairs");
            foreach(var z in new[]{-22,-8,8,24,38,52,66,80,92})
            {
                var side=z%4==0?1:-1;
                Place(services,"Service_Trench_V3",new Vector3(side*2.9f,.20f,z),side*8);
                if(z!=66)Place(services,"Collapsed_Hull_V3",new Vector3(-side*8.8f,0,z+2),side*32);
                Fault(route,new Vector3(side*3.1f,.13f,z),side>0?"cyan":"amber");
            }
            var periphery=Group(route,"Noncombat service alleys");
            foreach(var point in new[]{new Vector3(-31,0,-29),new Vector3(31,0,-29),new Vector3(-31,0,1),new Vector3(31,0,1),
                new Vector3(-29,0,46),new Vector3(29,0,46),new Vector3(-29,0,75),new Vector3(29,0,75),new Vector3(-28,0,93),new Vector3(28,0,93)})
            {
                Place(periphery,"Service_Trench_V3",point+Vector3.up*.20f,point.x<0?17:-24);
                Place(periphery,"Collapsed_Hull_V3",point+new Vector3(point.x<0?-2:2,0,3),point.z);
                Fault(route,point+Vector3.up*.6f,point.x<0?"cyan":"amber");
            }
            // A readable capsule-shaped energy cavity on each threshold; no extra colliders.
            var focal=Group(route,"Warm cool industrial thresholds");
            foreach(var point in new[]{new Vector3(-5.8f,0,-24),new Vector3(-5.8f,0,-16),new Vector3(5.8f,0,17),new Vector3(-5.8f,0,42),new Vector3(5.8f,0,62),new Vector3(-5.8f,0,82)})
            {
                PlaceExisting(focal,point.x<0?"Reactor_V2":"Turbine_V2",point,point.x<0?22:-18);
                Fault(route,point+Vector3.up*1.7f,point.x<0?"amber":"cyan");
            }
            env.KeyLight.intensity=1.35f;env.KeyLight.shadowStrength=.62f;
            RenderSettings.ambientLight=new Color(.20f,.25f,.30f);
            // Real local response remains two reused unshadowed lights.
            data=new SerializedObject(definition.Fidelity);data.FindProperty("_atmosphereInterval").floatValue=4.5f;
            data.FindProperty("_localLightIntensity").floatValue=2.6f;data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            ValidateOrThrow();Debug.Log("CHAPTER01_V3_INTEGRATED");
        }
        private static Transform Group(Transform parent,string name){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
        private static void Fault(Transform parent,Vector3 position,string color)
        {var fault=Group(parent,"Fidelity service fault");fault.position=position;var source=Group(parent,"Fidelity light "+color);source.position=position+Vector3.up*.6f;}
        private static GameObject Place(Transform parent,string name,Vector3 position,float yaw)
        {
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name)),parent);
            obj.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            foreach(var t in obj.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,StaticEditorFlags.BatchingStatic);
            return obj;
        }
        private static void PlaceExisting(Transform parent,string name,Vector3 position,float yaw)
        {var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/ConceptFidelityV2/Prefabs/"+name+".prefab"),parent);obj.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));}
        private static Phase6BVfxInstance Effect(string name,Phase6BVfxCue cue,Phase6BVfxShape shape,float duration)
        {
            var obj=new GameObject(name);var fx=obj.AddComponent<Phase6BVfxInstance>();var data=new SerializedObject(fx);
            data.FindProperty("_cue").enumValueIndex=(int)cue;data.FindProperty("_shape").enumValueIndex=(int)shape;
            data.FindProperty("_duration").floatValue=duration;data.FindProperty("_color").colorValue=new Color(1,.18f,.05f,.9f);
            data.FindProperty("_width").floatValue=.085f;data.FindProperty("_particleCount").intValue=8;
            data.FindProperty("_material").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Content/Presentation/Phase6B/VFX/Materials/M_Phase6B_HostileSoft.mat");
            data.ApplyModifiedPropertiesWithoutUndo();fx.ValidateOrThrow();PrefabUtility.SaveAsPrefabAsset(obj,Prefab(name));Object.DestroyImmediate(obj);
            return AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name)).GetComponent<Phase6BVfxInstance>();
        }
        private static void ImportPrefab(string name,Material material,bool animated)
        {
            var path=Root+"/Models/"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
            importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importCameras=false;importer.importLights=false;
            importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.isReadable=false;
            importer.importAnimation=animated;importer.animationType=animated?ModelImporterAnimationType.Generic:ModelImporterAnimationType.None;
            if(animated)
            {
                importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.motionNodeName="ROOT";importer.optimizeGameObjects=false;
                importer.animationCompression=ModelImporterAnimationCompression.Off;importer.skinWeights=ModelImporterSkinWeights.Custom;importer.maxBonesPerVertex=1;importer.SaveAndReimport();
                var clips=importer.defaultClipAnimations;foreach(var clip in clips){clip.name=clip.name.Split('|').Last();clip.loopTime=clip.name=="Idle"||clip.name=="Run";
                    clip.lockRootPositionXZ=clip.lockRootHeightY=clip.lockRootRotation=true;clip.keepOriginalPositionXZ=clip.keepOriginalPositionY=clip.keepOriginalOrientation=true;}importer.clipAnimations=clips;
            }
            importer.SaveAndReimport();GameObject obj;
            if(animated){obj=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));obj.name=name;}
            else{obj=new GameObject(name);Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),obj.transform,false);}
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=new[]{material};
            if(animated)
            {
                var controllerPath=Root+"/"+name+".controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)??AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                var machine=controller.layers[0].stateMachine;foreach(var old in machine.states)machine.RemoveState(old.state);
                foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")))
                {var state=machine.AddState(clip.name);state.motion=clip;state.writeDefaultValues=false;if(clip.name=="Idle")machine.defaultState=state;}
                var animator=obj.GetComponent<Animator>()??obj.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;
                var skins=obj.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(r=>r.name).ToArray();foreach(var old in obj.GetComponentsInChildren<LODGroup>(true))Object.DestroyImmediate(old);
                var lod=obj.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.16f,new Renderer[]{skins[0]}),new LOD(.075f,new Renderer[]{skins[1]}),new LOD(.018f,new Renderer[]{skins[2]})});lod.RecalculateBounds();
                var sockets=Group(obj.transform,"Presentation Sockets");
                foreach(var origin in new[]{"ConeAttackOrigin","LineAttackOrigin","CircleAttackOrigin"})
                {var point=Group(obj.transform,origin);point.localPosition=origin=="CircleAttackOrigin"?new Vector3(0,2.64f,.22f):new Vector3(1.74f,1.36f,2.14f);}
                foreach(var socket in new[]{"AttackOrigin","HitCenter","HealthAnchor","DeathOrigin"})
                {var t=Group(sockets,socket);t.localPosition=new Vector3(socket=="AttackOrigin"?1.74f:0,1.36f,socket=="AttackOrigin"?2.14f:0);}
                EditorUtility.SetDirty(controller);EditorUtility.SetDirty(machine);
            }
            PrefabUtility.SaveAsPrefabAsset(obj,Prefab(name));Object.DestroyImmediate(obj);
        }
        public static void ValidateOrThrow()
        {
            var dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
            foreach(var name in Models){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));PresentationPrefabValidation.ValidateOrThrow(prefab);
                if(!dependencies.Contains(Prefab(name)))throw new InvalidOperationException("V3 production scene lacks "+name);}
            var definition=AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset");
            if(definition.WeaponPrefab==null||definition.HostileTravel==null||definition.HostileCharge==null||definition.HostileImpact==null)throw new InvalidOperationException("V3 weapon and causal attack bindings required.");
            Debug.Log("CHAPTER01_V3_ASSET_VALIDATION_PASS");
        }
    }
}
