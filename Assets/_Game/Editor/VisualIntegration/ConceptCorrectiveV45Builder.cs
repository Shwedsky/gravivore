using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Offline V45 layout and service ecology authoring; no runtime art construction.</summary>
    public static class ConceptCorrectiveV45Builder
    {
        public const string Root="Assets/_Game/Content/ConceptCorrectiveV45";
        public const string Layer="Chapter 01 Active Industrial Facility V45";
        public const string Output="docs/concept-corrective-v45/verification";
        private static Material _steel,_dark,_amber,_cyan,_red,_white,_paint,_housing;
        private static readonly List<(string name,Vector3 center,Vector3 size)> _blockers=new List<(string,Vector3,Vector3)>();
        private static readonly List<string> _manifest=new List<string>();
        private static Transform Group(Transform parent,string name,Vector3 position=default)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.position=position;return t;}

        // Compress only transit; the elite/boss complex is translated as a whole.
        public static Vector3 FromV44(Vector3 p)
        {
            var x=Mathf.Abs(p.x);
            p.x=Mathf.Sign(p.x)*(x<=6?x:x<18?6+(x-6)/3:x-8);
            var z=p.z;
            var reduction=z<=-20?0:z<-4?(z+20)/8:z<12?2+(z+4)*.625f:z<28?12:z<32?12+(z-28):z<48?16:z<60?16+(z-48)*.5f:22;
            p.z-=reduction;return p;
        }
        private static Material Material(string name,Color color,Color emission,bool textured=false)
        {
            var path=Root+"/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",.55f);m.SetFloat("_Smoothness",.32f);
            if(textured)m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Material>(VisualReplacementV3Builder.Root+"/Materials/VR3_WornIndustrialAtlas.mat").GetTexture("_BaseMap"));
            if(emission.maxColorComponent>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
            EditorUtility.SetDirty(m);return m;
        }
        private static void Palette()
        {
            Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
            _steel=Material("V45_ColdSteel",new Color(.28f,.38f,.45f),Color.black);
            _steel.SetTexture("_BaseMap",null);
            var housingPath=Root+"/Materials/V45_WornHousing.mat";_housing=AssetDatabase.LoadAssetAtPath<Material>(housingPath);
            if(_housing==null){_housing=new Material(AssetDatabase.LoadAssetAtPath<Material>(VisualReplacementV3Builder.Root+"/Materials/VR3_WornIndustrialAtlas.mat"));AssetDatabase.CreateAsset(_housing,housingPath);}
            _housing.SetColor("_BaseColor",new Color(1.8f,2.05f,2.2f));_housing.SetFloat("_Smoothness",.32f);EditorUtility.SetDirty(_housing);
            _dark=Material("V45_Graphite",new Color(.055f,.075f,.09f),Color.black);
            _amber=Material("V45_UtilityAmber",new Color(.85f,.28f,.025f),new Color(1.55f,.47f,.055f));
            _paint=Material("V45_HazardOchre",new Color(.82f,.40f,.055f),Color.black);
            _cyan=Material("V45_ServiceCyan",new Color(.06f,.62f,.73f),new Color(.10f,1.3f,1.8f));
            _red=Material("V45_HostileRed",new Color(.72f,.06f,.025f),new Color(2.2f,.16f,.035f));
            _white=Material("V45_UtilityWhite",new Color(.75f,.82f,.79f),new Color(1.15f,1.35f,1.25f));
        }
        public static void Build()
        {
            Directory.CreateDirectory(Output);Palette();_blockers.Clear();_manifest.Clear();
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var env=root.VisualEnvironment;
            var world=AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            // Width is the persistent idempotence guard; all subsequent authoring is replaceable.
            if(world.Configuration.GroundSize.x>60) Compact(env,world);
            var prior=env.Floor.Find(Layer);if(prior!=null)Object.DestroyImmediate(prior.gameObject);
            var layer=Group(env.Floor,Layer);
            var data=new SerializedObject(env);var obstacles=data.FindProperty("_sliceObstacles");
            for(var i=obstacles.arraySize-1;i>=0;i--)
                if(obstacles.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue.StartsWith("V45 ",StringComparison.Ordinal))obstacles.DeleteArrayElementAtIndex(i);
            data.ApplyModifiedPropertiesWithoutUndo();
            PreserveRepairExit(env);
            // Retain the accepted geometry sources, replace broad deck with smaller construction rhythm.
            foreach(var node in env.Floor.GetComponentsInChildren<Transform>(true))
                if(node.name=="Layered worn deck"||node.name=="Continuous worn deck"||node.name=="Facility deck segmentation"||node.name=="Phase3C Routes")
                    foreach(var r in node.GetComponentsInChildren<Renderer>(true))
                    {r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
            Deck(layer,world.Configuration.Bounds.MinZ,world.Configuration.Bounds.MaxZ);
            Repair(layer);
            foreach(var spot in AssetDatabase.FindAssets("t:SpawnSpotDefinition",new[]{"Assets/_Game/Content/Definitions"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<SpawnSpotDefinition>))
            {
                var s=spot.CreateRuntimeConfiguration();
                Dock(layer,s.Id,s.WorldOrigin,s.Id=="capacitor-field"?_amber:s.Id=="relay-yard"?_white:_red,false);
            }
            foreach(var spot in Chapter1StrongOrdinarySpotCatalog.Create(world.Configuration))Dock(layer,spot.Id,spot.Position,_red,true);
            ServiceRoute(layer,world.Configuration.EliteGate.Position.z);
            Elite(layer,AssetDatabase.LoadAssetAtPath<Gravivore.Gameplay.Encounters.MagnetarGuardDefinition>("Assets/_Game/Content/Definitions/S09_MagnetarGuard.asset").Configuration.SpawnPosition);
            Boss(layer,world.Configuration.BossArenaCenter);
            BakePrimitiveBatches(layer);
            // New raised machinery gets explicit offline collision proxies. Low deck paint and
            // suspended service detail stay decorative; gate colliders remain authoritative.
            data=new SerializedObject(env);obstacles=data.FindProperty("_sliceObstacles");var count=obstacles.arraySize;obstacles.arraySize=count+_blockers.Count;
            for(var i=0;i<_blockers.Count;i++)
            {var p=obstacles.GetArrayElementAtIndex(count+i);p.FindPropertyRelative("_name").stringValue=_blockers[i].name;p.FindPropertyRelative("_center").vector3Value=_blockers[i].center;p.FindPropertyRelative("_size").vector3Value=_blockers[i].size;}
            data.ApplyModifiedPropertiesWithoutUndo();
            var settings=AssetDatabase.LoadAssetAtPath<EnemyAmbientMotionSettings>(Root+"/EnemyAmbientMotion.asset");
            if(settings==null){settings=ScriptableObject.CreateInstance<EnemyAmbientMotionSettings>();AssetDatabase.CreateAsset(settings,Root+"/EnemyAmbientMotion.asset");}
            data=new SerializedObject(root);data.FindProperty("_enemyAmbientMotionSettings").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();
            var fidelity=new SerializedObject(env.Definition.Fidelity);
            fidelity.FindProperty("_localLightRange").floatValue=7.5f;fidelity.FindProperty("_localLightIntensity").floatValue=3.6f;fidelity.ApplyModifiedPropertiesWithoutUndo();
            // Existing two reused local lights now sample the actual service sources.
            env.KeyLight.intensity=1.5f;env.KeyLight.color=new Color(.78f,.85f,.94f);
            foreach(var t in layer.GetComponentsInChildren<Transform>(true))GameObjectUtility.SetStaticEditorFlags(t.gameObject,StaticEditorFlags.BatchingStatic);
            env.ValidateOrThrow();settings.ValidateOrThrow();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText("docs/visual-replacement-v3/verification/collision_fingerprint.txt",VisualReplacementV3Builder.AuthorityFingerprint(env));
            File.WriteAllLines(Output+"/service_ecology_manifest.txt",_manifest);
            File.WriteAllText(Output+"/collision_fingerprint.txt",VisualReplacementV3Builder.AuthorityFingerprint(env));
            Debug.Log("CONCEPT_CORRECTIVE_V45_AUTHOR_PASS");
        }
        private static void Compact(ChapterVisualEnvironment env,Chapter01WorldDefinition world)
        {
            File.WriteAllText(Output+"/v44_collision_fingerprint.txt",VisualReplacementV3Builder.AuthorityFingerprint(env));
            // Keep prefab dependencies and historical review assets intact; the compact layout
            // is a set of scene overrides on the existing production art.
            var poses=env.GetComponentsInChildren<Transform>(true).Where(t=>t!=env.transform).Select(t=>(node:t,position:t.position)).ToArray();
            foreach(var pose in poses){pose.node.position=FromV44(pose.position);PrefabUtility.RecordPrefabInstancePropertyModifications(pose.node);}
            var data=new SerializedObject(env);var obstacles=data.FindProperty("_sliceObstacles");
            for(var i=0;i<obstacles.arraySize;i++)
            {
                var p=obstacles.GetArrayElementAtIndex(i);var c=p.FindPropertyRelative("_center");var size=p.FindPropertyRelative("_size");
                var original=c.vector3Value;var s=size.vector3Value;c.vector3Value=FromV44(original);
                // Broad walls bridge bands; adapt their span, retain ordinary machine dimensions.
                if(s.x>10)s.x=FromV44(original+Vector3.right*s.x/2).x-FromV44(original-Vector3.right*s.x/2).x;
                if(s.z>10)s.z=FromV44(original+Vector3.forward*s.z/2).z-FromV44(original-Vector3.forward*s.z/2).z;
                size.vector3Value=s;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            data=new SerializedObject(world);
            data.FindProperty("_groundSize").vector2Value=new Vector2(56,118);data.FindProperty("_groundCenter").vector3Value=new Vector3(0,0,19);
            var zones=data.FindProperty("_zones");for(var i=0;i<zones.arraySize;i++)foreach(var field in new[]{"_center","_landmarkPosition"})
            {var p=zones.GetArrayElementAtIndex(i).FindPropertyRelative(field);p.vector3Value=FromV44(p.vector3Value);}
            foreach(var field in new[]{"_eliteGate","_bossGate"}){var p=data.FindProperty(field).FindPropertyRelative("_position");p.vector3Value=FromV44(p.vector3Value);}
            var boss=data.FindProperty("_bossArenaCenter");boss.vector3Value=FromV44(boss.vector3Value);data.ApplyModifiedPropertiesWithoutUndo();
            foreach(var path in AssetDatabase.FindAssets("t:SpawnSpotDefinition",new[]{"Assets/_Game/Content/Definitions"}).Select(AssetDatabase.GUIDToAssetPath))RemapAsset(path,"_worldOrigin");
            RemapAsset("Assets/_Game/Content/Definitions/S09_MagnetarGuard.asset","_spawnPosition");RemapAsset("Assets/_Game/Content/Definitions/S09_CustodianM0.asset","_startPosition");
            File.WriteAllText(Output+"/compaction.json","{\"oldWidth\":72,\"newWidth\":56,\"oldLength\":140,\"newLength\":118,\"oldRepairToEliteGate\":90,\"newRepairToEliteGate\":68,\"eliteAndBossArenaTranslation\":-22,\"arenaDimensionsPreserved\":true,\"idsAndProgressionPreserved\":true}");
        }
        private static void RemapAsset(string path,string field)
        {var obj=AssetDatabase.LoadMainAssetAtPath(path);if(obj==null)throw new InvalidOperationException(path);var d=new SerializedObject(obj);var p=d.FindProperty(field);p.vector3Value=FromV44(p.vector3Value);d.ApplyModifiedPropertiesWithoutUndo();}
        private static void PreserveRepairExit(ChapterVisualEnvironment env)
        {
            var data=new SerializedObject(env);var obstacles=data.FindProperty("_sliceObstacles");
            for(var i=0;i<obstacles.arraySize;i++)
            {
                var p=obstacles.GetArrayElementAtIndex(i);
                if(p.FindPropertyRelative("_name").stringValue!="Chapter01 Service corridors Maintenance_Station 101")continue;
                var center=p.FindPropertyRelative("_center");var original=center.vector3Value;
                if(original.x>=7.5f)return;
                // Compaction moved the existing console into the accepted right exit.
                // Move its real prefab and matching proxy together, once.
                var node=env.Floor.GetComponentsInChildren<Transform>(true).Single(t=>
                    t.name=="Maintenance_Station"&&t.GetComponentsInChildren<Renderer>(true).Any(r=>Vector3.Distance(r.bounds.center,original)<.05f));
                var offset=Vector3.right*(7.5f-original.x);
                node.position+=offset;PrefabUtility.RecordPrefabInstancePropertyModifications(node);
                center.vector3Value=original+offset;data.ApplyModifiedPropertiesWithoutUndo();return;
            }
            throw new InvalidOperationException("Repair exit console proxy is missing.");
        }
        private static void BakePrimitiveBatches(Transform layer)
        {
            Directory.CreateDirectory(Root+"/Meshes");AssetDatabase.Refresh();
            // Original service strips, ribs and cradle shells are consolidated offline by
            // sector/material. Runtime never allocates a mesh or clones a material.
            for(var i=0;i<layer.childCount;i++)
            {
                var sector=layer.GetChild(i);
                var cubes=sector.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh!=null&&f.sharedMesh.name=="Cube").ToArray();
                foreach(var group in cubes.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial))
                {
                    var path=Root+"/Meshes/"+i+"_"+group.Key.name+".asset";
                    var mesh=new Mesh{name="V45 "+sector.name+" "+group.Key.name};
                    mesh.CombineMeshes(group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=sector.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray(),true,true);
                    var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(existing!=null){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
                    var obj=new GameObject("Baked service detail "+group.Key.name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(sector,false);
                    obj.GetComponent<MeshFilter>().sharedMesh=mesh;var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=group.Key;
                    if(group.Key.IsKeywordEnabled("_EMISSION")){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
                }
                foreach(var cube in cubes)Object.DestroyImmediate(cube.gameObject);
            }
        }
        private static GameObject Box(Transform parent,string name,Vector3 p,Vector3 size,Material material,bool solid=false,float yaw=0)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.DestroyImmediate(obj.GetComponent<Collider>());obj.name=name;obj.transform.SetParent(parent,false);
            obj.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));obj.transform.localScale=size;var r=obj.GetComponent<Renderer>();r.sharedMaterial=material;
            if(material==_amber||material==_cyan||material==_red||material==_white){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
            if(solid){var b=r.bounds;_blockers.Add(("V45 "+name,b.center,b.size));}return obj;
        }
        private static GameObject Machine(Transform parent,string donor,Vector3 p,Vector3 size,bool solid=true,float yaw=0)
        {
            var obj=VisualReplacementV3Builder.Fit(parent,donor,p,size,yaw);var rs=obj.GetComponentsInChildren<Renderer>();
            foreach(var r in rs)r.sharedMaterial=_housing;
            var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
            if(solid)_blockers.Add(("V45 "+parent.name+" "+donor,b.center,b.size));return obj;
        }
        private static void Cable(Transform parent,Vector3 a,Vector3 b,Material accent)
        {
            var length=Vector3.Distance(a,b);var o=Box(parent,"Terminated power conduit",(a+b)/2,new Vector3(.22f,.15f,length),_dark);
            o.transform.rotation=Quaternion.LookRotation(b-a);var strip=Box(parent,"Conduit powered strip",(a+b)/2+Vector3.up*.08f,new Vector3(.045f,.018f,length*.92f),accent);strip.transform.rotation=o.transform.rotation;
        }
        private static void Beacon(Transform parent,Vector3 p,Material accent,string color)
        {
            Box(parent,"Utility beacon pedestal",p+Vector3.up*.65f,new Vector3(.32f,1.3f,.32f),_dark,true);
            Box(parent,"Inset utility beacon",p+Vector3.up*1.3f,new Vector3(.18f,.42f,.18f),accent);
            Group(parent,"Fidelity light "+color,p+Vector3.up*1.5f);
        }
        private static void Warning(Transform parent,Vector3 p,float width)
        {
            Box(parent,"Machine service boundary",p,new Vector3(width,.022f,.3f),_dark);
            for(var x=-width/2+.12f;x<width/2;x+=.48f)Box(parent,"Amber hazard paint",p+new Vector3(x,.012f,0),new Vector3(.16f,.012f,.25f),_paint,false,-28);
        }
        private static void Deck(Transform parent,float min,float max)
        {
            var deck=Group(parent,"Compact small-panel deck");
            for(var x=-26;x<=26;x+=4)for(var z=min+2;z<max;z+=4)
                VisualReplacementV3Builder.Fit(deck,(Mathf.Abs(Mathf.RoundToInt(x/4+z/4))%7)==0?"VR3_FracturedDeck":((Mathf.RoundToInt(x+z)/4)&3)==0?"VR3_platformdarkplates":"VR3_platformmetal",new Vector3(x,.075f,z),new Vector3(3.97f,.06f,3.97f));
            _manifest.Add("Deck: 4m panel rhythm; 56 x 118m; accepted V44 atlas and URP shader.");
        }
        private static void Repair(Transform parent)
        {
            var g=Group(parent,"Repair Hub cyan diagnostic services");
            foreach(var side in new[]{-1,1})
            {
                Machine(g,"VR3_columnpipes",new Vector3(side*5.5f,0,-32),new Vector3(1,2.9f,1.3f));
                Machine(g,"VR3_generator",new Vector3(side*6.8f,0,-32),new Vector3(1.1f,1.4f,1.5f));
                Beacon(g,new Vector3(side*5.4f,0,-30.7f),_cyan,"cyan");
                Cable(g,new Vector3(side*5.5f,.16f,-32),new Vector3(side*3.6f,.16f,-29.5f),_cyan);
                Warning(g,new Vector3(side*5.6f,.17f,-33.3f),2.2f);
            }
            _manifest.Add("Repair Hub: paired cyan diagnostic power clusters behind the dock; right-hand repair exit remains open.");
        }
        private static void Dock(Transform parent,string id,Vector3 c,Material accent,bool strong)
        {
            var g=Group(parent,(strong?"Heavy fabrication nest ":"Serviced enemy dock ")+id,c);
            var back=c+Vector3.forward*3.8f;
            Machine(g,strong?"VR3_generatorpilelarge":"VR3_command",back,new Vector3(strong?2.2f:1.7f,strong?2.5f:1.8f,1.3f));
            foreach(var side in new[]{-1,1})
            {
                var rack=c+new Vector3(side*3.5f,0,3.7f);
                Machine(g,"VR3_columnpipes",rack,new Vector3(.6f,strong?3.1f:2.2f,.7f));
                Beacon(g,rack+new Vector3(-side*.55f,0,-.55f),accent,accent==_white?"white":accent==_amber?"amber":"red");
                Cable(g,back+Vector3.up*.18f,rack+Vector3.up*.18f,accent);
                var service=c+new Vector3(side*4.1f,0,-4.8f);
                Machine(g,id=="capacitor-field"?"VR3_centrifuge":id=="hauler-graveyard"?"VR3_propcrate":"VR3_generator",service,new Vector3(1.8f,2.15f,2.4f));
                Machine(g,"VR3_proppipeholder",service+new Vector3(side*.5f,0,1.7f),new Vector3(.65f,1.7f,.65f));
                Box(g,"Powered service housing core",service+new Vector3(-side*.92f,1.25f,0),new Vector3(.06f,1.15f,.55f),accent);
                Cable(g,service+new Vector3(-side*.65f,.17f,1),rack+new Vector3(0,.17f,-.6f),accent);
                Warning(g,service+new Vector3(0,.17f,-1.3f),2.2f);
                // Low open cradles advertise origins without closing the combat apron.
                foreach(var z in new[]{-1.5f,1.5f})
                {
                    var dock=c+new Vector3(side*1.5f,.15f,z);
                    Box(g,"Open charging cradle",dock,new Vector3(1.25f,.10f,1.2f),_dark);
                    Box(g,"Cradle service indicator",dock+new Vector3(0,.06f,.53f),new Vector3(.8f,.025f,.055f),accent);
                    Cable(g,dock+Vector3.forward*.62f,back+new Vector3(side*.5f,.15f,-.8f),accent);
                }
            }
            Warning(g,back+new Vector3(0,.18f,-.9f),3.4f);
            Group(g,"Fidelity service fault",back+new Vector3(.45f,1.3f,0));
            _manifest.Add(id+": open charging cradles -> terminated conduits -> service console and powered rear gantry; "+(strong?"heavy fabrication":"ordinary maintenance"));
        }
        private static void ServiceRoute(Transform parent,float eliteGate)
        {
            var g=Group(parent,"Connected corridor machinery and maintenance alcoves");
            foreach(var z in new[]{-23f,-18f,-8f,-3f,2f,12f,17f,27f,32f})foreach(var side in new[]{-1,1})
            {
                var c=new Vector3(side*3.7f,0,z+(side<0?0:1.0f));
                Machine(g,"VR3_generatorpilelarge",c,new Vector3(3.1f,3.0f,2.8f));
                Machine(g,"VR3_propventbig",c+new Vector3(side*1.25f,0,1),new Vector3(.9f,1.5f,1.6f));
                Machine(g,"VR3_proppipeholder",c+new Vector3(-side*.75f,0,2),new Vector3(.7f,1.8f,.75f));
                Beacon(g,c+new Vector3(-side*.55f,0,-1.5f),z==14?_red:_amber,z==14?"red":"amber");
                Box(g,"Recessed amber machinery core",c+new Vector3(-side*1.55f,1.3f,0),new Vector3(.07f,.8f,.45f),_amber);
                Cable(g,c+new Vector3(0,.18f,0),c+new Vector3(-side*1.1f,.18f,2.2f),_amber);
                Warning(g,c+new Vector3(0,.16f,-1.6f),2.5f);
            }
            for(var z=-22f;z<eliteGate-2;z+=4)foreach(var side in new[]{-1,1})
            {
                Box(g,"Route utility inset",new Vector3(side*2.8f,.17f,z),new Vector3(.075f,.025f,1.25f),z<0?_white:_amber);
                Box(g,"Perimeter service rail",new Vector3(side*4.4f,.25f,z),new Vector3(.18f,.18f,3.3f),_steel);
            }
            // Flush walkable service channels connect the hub power feed to the elite
            // induction grid. The deck remains usable over the grate; no raised obstacle.
            for(var z=-21f;z<eliteGate-2;z+=5)foreach(var side in new[]{-1,1})
            {
                var p=new Vector3(side*1.65f,.155f,z);
                Box(g,"Recessed service channel",p,new Vector3(1.1f,.055f,4.7f),_dark);
                foreach(var edge in new[]{-1,1})Box(g,"Service channel steel shoulder",p+new Vector3(edge*.52f,.055f,0),new Vector3(.08f,.075f,4.7f),_steel);
                for(var offset=-2.15f;offset<=2.15f;offset+=.29f)
                    Box(g,"Walkable service grating",p+new Vector3(0,.055f,offset),new Vector3(.94f,.045f,.07f),_steel);
                Box(g,"Recessed utility power feed",p+new Vector3(side*.32f,.032f,0),new Vector3(.025f,.015f,4.5f),_amber);
            }
            foreach(var z in new[]{-12f,4f,22f,34f})
            {
                foreach(var side in new[]{-1,1})
                {
                    Box(g,"Service gantry upright",new Vector3(side*3.4f,1.85f,z),new Vector3(.3f,3.7f,.45f),_steel,true);
                    Box(g,"Gantry base bracket",new Vector3(side*3.4f,.25f,z),new Vector3(.65f,.5f,.7f),_dark,true);
                }
                Box(g,"Suspended service gantry bearer",new Vector3(0,3.7f,z),new Vector3(7.4f,.35f,.9f),_steel);
                Box(g,"Gantry terminated cable tray",new Vector3(0,3.95f,z),new Vector3(6.8f,.16f,.6f),_dark);
                Box(g,"Gantry utility inspection lights",new Vector3(0,3.51f,z-.46f),new Vector3(1.1f,.035f,.025f),_white);
            }
            _manifest.Add("Main lane: at least 4m clear central combat lane, paired serviced generator alcoves, amber/white insets, broken service rails.");
        }
        private static void Elite(Transform parent,Vector3 c)
        {
            var g=Group(parent,"Magnetar magnetic induction infrastructure",c);
            foreach(var side in new[]{-1,1})
            {
                var p=c+new Vector3(side*5.7f,0,3.8f);
                Machine(g,"VR3_generatorpilelarge",p,new Vector3(1.8f,3.1f,2.3f));
                Machine(g,"VR3_columnpipes",p+new Vector3(-side*1.0f,0,1.9f),new Vector3(.6f,3.5f,.65f));
                for(var z=0;z<4;z++)Box(g,"Magnetic lamination",p+new Vector3(-side*.9f,.9f+z*.46f,0),new Vector3(.18f,.23f,1.4f),_steel);
                Box(g,"Powered induction bank",p+new Vector3(-side*.99f,1.6f,0),new Vector3(.035f,1.2f,.65f),_amber);
                Beacon(g,p+new Vector3(-side*.9f,0,-1.4f),_amber,"amber");
                Cable(g,p+Vector3.up*.18f,c+new Vector3(side*3.5f,.18f,1),_amber);
                Warning(g,p+new Vector3(0,.18f,-1.4f),2.6f);
            }
            _manifest.Add("Magnetar: paired magnetic banks, exposed laminations, amber powered cores and connected stabilizer services; arena footprint preserved.");
        }
        private static void Boss(Transform parent,Vector3 c)
        {
            var g=Group(parent,"Custodian boss-grade containment services",c);
            foreach(var side in new[]{-1,1})
            {
                var p=c+new Vector3(side*6.2f,0,3.0f);
                Machine(g,"VR3_cryotube",p,new Vector3(1.4f,4.0f,1.5f));
                Machine(g,"VR3_generator",p+new Vector3(side*.8f,0,-2.7f),new Vector3(1.4f,1.8f,1.5f));
                Box(g,"Containment powered spine",p+new Vector3(-side*.68f,2,0),new Vector3(.09f,2.8f,.55f),_red);
                Beacon(g,p+new Vector3(-side*.9f,0,-1.2f),_red,"red");
                Cable(g,p+new Vector3(0,.19f,0),c+new Vector3(side*4.8f,.19f,3.3f),_red);
                Warning(g,p+new Vector3(0,.19f,-1),2.3f);
            }
            // A rear service gantry frames the boss but does not cross its charge/telegraph apron.
            Machine(g,"VR3_command",c+new Vector3(0,0,5.7f),new Vector3(3,2.5f,1.1f));
            Box(g,"Boss containment control status",c+new Vector3(0,1.5f,5.05f),new Vector3(1.5f,.22f,.08f),_red);
            Group(g,"Fidelity light red",c+new Vector3(0,1.9f,5));
            _manifest.Add("Custodian: taller red containment towers, pressure services and a rear boss control gantry outside the charge apron; telegraph geometry unchanged.");
        }
    }
}
