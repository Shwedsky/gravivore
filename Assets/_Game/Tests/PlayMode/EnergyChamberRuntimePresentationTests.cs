using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Camera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Camera=UnityEngine.Camera;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class EnergyChamberRuntimePresentationTests
    {
        private const string Output="docs/history/implementation-passes/energy-chamber-runtime-presentation-v1";
        private CanonicalSceneTestScope _scene;
        private GameObject _installation;
        private Camera _camera;
        private CharacterController _walker;
        private int _oldQuality;
        private readonly Dictionary<Material,Vector2> _materialState=new Dictionary<Material,Vector2>();
        private readonly Vector3[] _offsets={new Vector3(-6.5f,0,-5.3f),new Vector3(3,0,-5.3f),new Vector3(3,0,-10),new Vector3(0,0,7),new Vector3(3,0,-14),new Vector3(3,0,-5.3f),new Vector3(3,0,-5.3f),new Vector3(0,0,7)};
        private readonly string[] _names={"A_problematic_west_view","B_problematic_south_view","C_normal_approach","D_close_north_view","E_wider_chapter","F_low_emission","G_normal_final","H_material_detail"};
        [UnityTearDown] public IEnumerator Cleanup(){
            foreach(var pair in _materialState)if(pair.Key!=null){pair.Key.SetFloat("_EmissionScale",pair.Value.x);pair.Key.SetFloat("_PreviewTime",pair.Value.y);}
            _materialState.Clear();QualitySettings.SetQualityLevel(_oldQuality,true);if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _oldQuality=QualitySettings.GetQualityLevel();
            // Android's actual configured default is Medium (index 2), lodBias .7.
            QualitySettings.SetQualityLevel(2,true);
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();
            var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
            foreach(var animator in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Animator>(true)))animator.enabled=false;
            _installation=SceneManager.GetActiveScene().GetRootGameObjects().Single(g=>g.name=="Energy Chamber / production");
            _walker=root.PlayerObject.GetComponent<CharacterController>();
            _camera=root.GetComponentsInChildren<Camera>(true).Single(c=>c.CompareTag("MainCamera"));
            Assert.That(_camera.GetComponent<PortraitFollowCamera>(),Is.Not.Null);
            foreach(var material in _installation.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct().Where(m=>m.HasProperty("_PreviewTime")))
                _materialState.Add(material,new Vector2(material.GetFloat("_EmissionScale"),material.GetFloat("_PreviewTime")));
        }
        [UnityTest] public IEnumerator ActualAndroidCameraVisibilitySweep()
        {
            yield return Load();
            var stage=Environment.GetEnvironmentVariable("GRAVIVORE_ENERGY_PRESENTATION_STAGE")??"diagnosis";
            var group=_installation.GetComponent<LODGroup>();var levels=group.GetLODs();
            var renderers=_installation.GetComponentsInChildren<Renderer>(true);
            var records=new List<View>();
            for(var i=0;i<_offsets.Length;i++)
            {
                foreach(var material in _materialState.Keys){material.SetFloat("_PreviewTime",7.2f);material.SetFloat("_EmissionScale",i==5?.05f:1);}
                var anchor=_installation.transform.position+_offsets[i];
                _walker.enabled=false;_walker.transform.position=anchor;_walker.enabled=true;
                _camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
                _camera.aspect=9f/16;
                yield return null;yield return null;
                group.ForceLOD(-1);
                Save(_camera,stage,_names[i]);yield return null;
                var planes=GeometryUtility.CalculateFrustumPlanes(_camera);
                var distance=Vector3.Distance(_camera.transform.position,group.transform.TransformPoint(group.localReferencePoint));
                var relative=group.size*QualitySettings.lodBias/(2*Mathf.Tan(_camera.fieldOfView*.5f*Mathf.Deg2Rad)*distance);
                var level=Enumerable.Range(0,levels.Length).FirstOrDefault(l=>relative>=levels[l].screenRelativeTransitionHeight);
                if(relative<levels.Last().screenRelativeTransitionHeight)level=-1;
                var view=new View {name=_names[i],anchor=anchor,camera=_camera.transform.position,distance=distance,relativeHeight=relative,lod=level,
                    quality=QualitySettings.names[QualitySettings.GetQualityLevel()],bias=QualitySettings.lodBias,fov=_camera.fieldOfView,aspect=_camera.aspect,
                    emissionScale=i==5?.05f:1,reviewTime=7.2f,
                    items=renderers.Select(r=>new Visibility {name=PathOf(r.transform,_installation.transform),enabled=r.enabled,active=r.gameObject.activeInHierarchy,
                        inFrustum=GeometryUtility.TestPlanesAABB(planes,r.bounds),cameraVisible=r.isVisible,
                        member=level>=0&&levels[level].renderers.Contains(r),bounds=r.bounds}).ToArray()};
                if(stage=="diagnosis"&&(i==0||i==1)){
                    group.ForceLOD(0);Save(_camera,stage,_names[i]+"_control_LOD0");yield return null;
                    view.lod0Visible=renderers.Where(r=>r.isVisible).Select(r=>PathOf(r.transform,_installation.transform)).ToArray();
                    group.ForceLOD(-1);
                }
                records.Add(view);
                if(stage!="diagnosis"&&i<5) {
                    Assert.That(level,Is.EqualTo(0),"Android default should retain the complete assembly at ordinary nearby view "+_names[i]);
                    Assert.That(view.items.Where(v=>v.inFrustum).All(v=>v.cameraVisible),Is.True,"In-frustum near parts vanished at "+_names[i]);
                }
            }
            Directory.CreateDirectory(Output+"/verification/"+stage);
            File.WriteAllText(Output+"/verification/"+stage+"/camera-sweep.json",JsonUtility.ToJson(new Views{items=records.ToArray()},true));
            Assert.That(_camera.fieldOfView,Is.EqualTo(48).Within(.01f));
            Assert.That(group.GetLODs().Length,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator AutomaticLodRetainsIdentityAcrossQualityBiasesAndReducesDistantCost()
        {
            yield return Load();
            var group=_installation.GetComponent<LODGroup>();var lods=group.GetLODs();
            var stage=Environment.GetEnvironmentVariable("GRAVIVORE_ENERGY_PRESENTATION_STAGE")??"visibility";
            var oldBias=QualitySettings.lodBias;var receipts=new List<View>();
            try {
                foreach(var bias in new[]{.3f,.4f,.7f,1f,1.5f,2f})
                foreach(var distance in new[]{22f,30f,50f,80f}) {
                    QualitySettings.lodBias=bias;
                    var point=group.transform.TransformPoint(group.localReferencePoint);
                    _camera.GetComponent<PortraitFollowCamera>().enabled=false;
                    var direction=new Vector3(3.4f,16.6f,-13.6f).normalized;
                    _camera.transform.position=point+direction*distance;
                    _camera.transform.rotation=Quaternion.LookRotation(-direction,Vector3.up);
                    group.ForceLOD(-1);_camera.aspect=9f/16;
                    Save(_camera,stage,"quality_"+bias.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+"_distance_"+distance);yield return null;
                    var relative=group.size*bias/(2*Mathf.Tan(_camera.fieldOfView*.5f*Mathf.Deg2Rad)*distance);
                    var level=Enumerable.Range(0,3).First(l=>relative>=lods[l].screenRelativeTransitionHeight);
                    var critical=lods[2].renderers;
                    var planes=GeometryUtility.CalculateFrustumPlanes(_camera);
                    Assert.That(critical.Where(r=>GeometryUtility.TestPlanesAABB(planes,r.bounds)).All(r=>r.isVisible),Is.True,"Identity lost at bias "+bias+", distance "+distance);
                    receipts.Add(new View {name="Automatic quality sweep",bias=bias,distance=distance,relativeHeight=relative,lod=level});
                }
            }
            finally {QualitySettings.lodBias=oldBias;}
            Assert.That(receipts.Any(r=>r.bias==.7f&&r.lod==1),Is.True,"LOD1 never selected at Android default");
            Assert.That(receipts.Any(r=>r.bias==.7f&&r.lod==2),Is.True,"LOD2 never selected at Android default");
            Directory.CreateDirectory(Output+"/verification/"+stage);
            File.WriteAllText(Output+"/verification/"+stage+"/quality-distance-sweep.json",JsonUtility.ToJson(new Views{items=receipts.ToArray()},true));
        }
        [UnityTest] public IEnumerator OperationalEnergyIsVisibleRestrainedAndNeedsNoRuntimePresenter()
        {
            yield return Load();
            Assert.That(_installation.GetComponentsInChildren<MonoBehaviour>(true),Is.Empty);
            Assert.That(_installation.GetComponentsInChildren<Light>(true),Is.Empty);
            Assert.That(_installation.GetComponentsInChildren<ParticleSystem>(true),Is.Empty);
            Assert.That(_materialState.Count,Is.EqualTo(22));
            Assert.That(_materialState.Keys.All(m=>m.shader.isSupported&&m.GetFloat("_OperationStrength")<=.2f),Is.True);
            var anchor=_installation.transform.position+new Vector3(3,0,-5.3f);
            _walker.enabled=false;_walker.transform.position=anchor;_walker.enabled=true;
            _camera.GetComponent<PortraitFollowCamera>().SnapToTarget();_camera.aspect=9f/16;
            yield return null;
            var frames=new List<Color32[]>();
            foreach(var time in new[]{0f,9f,21f}) {
                foreach(var material in _materialState.Keys){material.SetFloat("_PreviewTime",time);material.SetFloat("_EmissionScale",1);}
                frames.Add(Save(_camera,"lookdev","operational_time_"+time));yield return null;
            }
            long difference=0;var changed=0;
            for(var i=0;i<frames[0].Length;i++) {
                var a=frames[0][i];var b=frames[1][i];var delta=Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b);
                difference+=delta;if(delta>3)changed++;
            }
            Assert.That(changed,Is.GreaterThan(100),"GPU energy breathing produced no visible change");
            var mean=(float)difference/(frames[0].Length*3);
            Assert.That(mean,Is.LessThan(12),"Whole scene modulation is excessive");
            File.WriteAllText(Output+"/verification/lookdev/operational-gpu.json",JsonUtility.ToJson(new Operational {changedPixels=changed,meanByteDifference=mean,newRuntimePresenters=0,newRealtimeLights=0,newParticles=0,times=new[]{0f,9f,21f}},true));
        }
        [UnityTest] public IEnumerator PhoneViewportRetainsInFrustumIdentityAtProblematicPositions()
        {
            yield return Load();
            var records=new List<View>();var group=_installation.GetComponent<LODGroup>();
            // Owner JPEGs are 591x1280, including about 50px of OS status bar.
            // Capture the inferred 591x1230 game viewport, without HUD overlays.
            for(var i=0;i<2;i++) {
                foreach(var material in _materialState.Keys){material.SetFloat("_PreviewTime",7.2f);material.SetFloat("_EmissionScale",1);}
                var anchor=_installation.transform.position+_offsets[i];
                _walker.enabled=false;_walker.transform.position=anchor;_walker.enabled=true;
                _camera.GetComponent<PortraitFollowCamera>().SnapToTarget();_camera.aspect=591f/1230;
                group.ForceLOD(-1);yield return null;
                Save(_camera,"phone-aspect",_names[i],591,1230);yield return null;
                var planes=GeometryUtility.CalculateFrustumPlanes(_camera);
                var visible=group.GetLODs()[0].renderers.Where(r=>GeometryUtility.TestPlanesAABB(planes,r.bounds)).ToArray();
                Assert.That(visible.All(r=>r.isVisible),Is.True,"In-frustum assembly disappeared at inferred phone aspect");
                records.Add(new View {name=_names[i],anchor=anchor,camera=_camera.transform.position,aspect=_camera.aspect,fov=_camera.fieldOfView,bias=QualitySettings.lodBias,
                    items=group.GetLODs()[0].renderers.Select(r=>new Visibility {name=PathOf(r.transform,_installation.transform),inFrustum=GeometryUtility.TestPlanesAABB(planes,r.bounds),cameraVisible=r.isVisible,enabled=r.enabled,active=r.gameObject.activeInHierarchy,member=true,bounds=r.bounds}).ToArray()});
            }
            Directory.CreateDirectory(Output+"/verification/phone-aspect");File.WriteAllText(Output+"/verification/phone-aspect/camera-sweep.json",JsonUtility.ToJson(new Views{items=records.ToArray()},true));
        }
        private static string PathOf(Transform t,Transform root)=>t==root?t.name:PathOf(t.parent,root)+"/"+t.name;
        private static Color32[] Save(Camera camera,string stage,string name,int width=1080,int height=1920)
        {
            Directory.CreateDirectory(Output+"/screenshots/"+stage);
            var old=camera.targetTexture;var active=RenderTexture.active;
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;for(var n=0;n<3;n++)camera.Render();RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(Output+"/screenshots/"+stage+"/"+name+".png",pixels.EncodeToPNG());return pixels.GetPixels32();}
            finally{camera.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);}
        }
        [Serializable] private sealed class Views{public View[] items;}
        [Serializable] private sealed class View{public string name,quality;public Vector3 anchor,camera;public float distance,relativeHeight,bias,fov,aspect,emissionScale,reviewTime;public int lod;public Visibility[] items;public string[] lod0Visible;}
        [Serializable] private sealed class Visibility{public string name;public bool enabled,active,inFrustum,cameraVisible,member;public Bounds bounds;}
        [Serializable] private sealed class Operational{public int changedPixels,newRuntimePresenters,newRealtimeLights,newParticles;public float meanByteDifference;public float[] times;}
    }
}
