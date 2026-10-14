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
        private readonly Vector3[] _offsets={new Vector3(-6.5f,0,-5.3f),new Vector3(3,0,-5.3f),new Vector3(3,0,-10),new Vector3(0,0,7),new Vector3(3,0,-14),new Vector3(3,0,-5.3f),new Vector3(3,0,-5.3f),new Vector3(0,0,7)};
        private readonly string[] _names={"A_problematic_west_view","B_problematic_south_view","C_normal_approach","D_close_north_view","E_wider_chapter","F_low_emission","G_normal_final","H_material_detail"};
        [UnityTearDown] public IEnumerator Cleanup(){QualitySettings.SetQualityLevel(_oldQuality,true);if(_scene!=null)yield return _scene.Cleanup();}
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
            _installation=SceneManager.GetActiveScene().GetRootGameObjects().Single(g=>g.name=="Energy Chamber / production");
            _walker=root.PlayerObject.GetComponent<CharacterController>();
            _camera=root.GetComponentsInChildren<Camera>(true).Single(c=>c.CompareTag("MainCamera"));
            Assert.That(_camera.GetComponent<PortraitFollowCamera>(),Is.Not.Null);
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
                    items=renderers.Select(r=>new Visibility {name=PathOf(r.transform,_installation.transform),enabled=r.enabled,active=r.gameObject.activeInHierarchy,
                        inFrustum=GeometryUtility.TestPlanesAABB(planes,r.bounds),cameraVisible=r.isVisible,
                        member=level>=0&&levels[level].renderers.Contains(r),bounds=r.bounds}).ToArray()};
                if(stage=="diagnosis"&&(i==0||i==1)){
                    group.ForceLOD(0);Save(_camera,stage,_names[i]+"_control_LOD0");yield return null;
                    view.lod0Visible=renderers.Where(r=>r.isVisible).Select(r=>PathOf(r.transform,_installation.transform)).ToArray();
                    group.ForceLOD(-1);
                }
                records.Add(view);
            }
            Directory.CreateDirectory(Output+"/verification/"+stage);
            File.WriteAllText(Output+"/verification/"+stage+"/camera-sweep.json",JsonUtility.ToJson(new Views{items=records.ToArray()},true));
            Assert.That(_camera.fieldOfView,Is.EqualTo(48).Within(.01f));
            Assert.That(group.GetLODs().Length,Is.EqualTo(3));
        }
        private static string PathOf(Transform t,Transform root)=>t==root?t.name:PathOf(t.parent,root)+"/"+t.name;
        private static void Save(Camera camera,string stage,string name)
        {
            Directory.CreateDirectory(Output+"/screenshots/"+stage);
            var old=camera.targetTexture;var active=RenderTexture.active;
            var target=new RenderTexture(1080,1920,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var pixels=new Texture2D(1080,1920,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;for(var n=0;n<3;n++)camera.Render();RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,1080,1920),0,0);pixels.Apply();File.WriteAllBytes(Output+"/screenshots/"+stage+"/"+name+".png",pixels.EncodeToPNG());}
            finally{camera.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);}
        }
        [Serializable] private sealed class Views{public View[] items;}
        [Serializable] private sealed class View{public string name,quality;public Vector3 anchor,camera;public float distance,relativeHeight,bias,fov,aspect;public int lod;public Visibility[] items;public string[] lod0Visible;}
        [Serializable] private sealed class Visibility{public string name;public bool enabled,active,inFrustum,cameraVisible,member;public Bounds bounds;}
    }
}
