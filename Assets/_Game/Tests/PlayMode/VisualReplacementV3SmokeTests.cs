using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Composition;
using Gravivore.Gameplay.Equipment;
using Gravivore.Presentation.Player;
using Gravivore.Presentation.UI;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.AudioVfx;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class VisualReplacementV3SmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;root.PlayerHealth.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        [UnityTest] public IEnumerator RepairPrototypeHasLayeredArtAndPreservesAuthority()
        {
            yield return Load();var root=_scene.Root;
            var layer=root.VisualEnvironment.Floor.Find("Chapter 01 Visual Replacement V3");Assert.NotNull(layer);
            Assert.That(layer.GetComponentsInChildren<Collider>(true),Is.Empty);
            Assert.That(layer.GetComponentsInChildren<MonoBehaviour>(true),Is.Empty);
            var renderers=layer.GetComponentsInChildren<Renderer>(true);Assert.That(renderers.Length,Is.GreaterThan(10));
            foreach(var r in layer.Find("Layered worn deck").GetComponentsInChildren<Renderer>())
                if(r.name.Contains("platform")){Assert.That(r.bounds.size.x,Is.GreaterThan(5.9f));Assert.That(r.bounds.size.z,Is.GreaterThan(5.9f));Assert.That(r.bounds.size.y,Is.LessThan(.4f));}
            foreach(var r in renderers)
            {
                Assert.That(r.sharedMaterials.Length,Is.EqualTo(1));Assert.NotNull(r.sharedMaterial);
                Assert.That(r.sharedMaterial.shader.name,Is.EqualTo("Universal Render Pipeline/Lit"));
                Assert.That(r.sharedMaterial.shader.isSupported,Is.True);
            }
            var controller=root.PlayerObject.GetComponent<CharacterController>();controller.enabled=false;
            root.PlayerObject.transform.position=new Vector3(0,0,-28);controller.enabled=true;Physics.SyncTransforms();yield return null;
            root.PlayerHealth.ApplyDamage(new DamageRequest(35,DamageType.Physical));root.PlayerHealth.Tick(3.1f);root.PlayerHealth.Tick(.1f);
            root.RepairHub.Manipulators.Tick(.5f);yield return null;
            Capture(root,"01_repair_hub");
            Directory.CreateDirectory("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification");
            File.WriteAllLines("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification/repair_renderers.txt",renderers.Select(r=>r.name+" | "+r.bounds+" | "+r.sharedMaterial.name+" | base="+r.sharedMaterial.GetTexture("_BaseMap")?.name));
            File.WriteAllLines("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification/floor_layers.txt",root.VisualEnvironment.Floor.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.bounds.Contains(new Vector3(0,r.bounds.center.y,-30))).Select(r=>r.name+" | "+r.bounds));
            // Existing repair-exit physics must remain open with the real capsule.
            foreach(var other in root.GetComponentsInChildren<CharacterController>(true))if(other!=controller)other.enabled=false;
            controller.enabled=false;controller.transform.position=new Vector3(0,0,-27);controller.enabled=true;
            for(var i=0;i<24;i++)controller.Move(Vector3.right*.25f);
            Assert.That(controller.transform.position.x,Is.EqualTo(6).Within(.1f));
        }
        internal static void Capture(S01SceneCompositionRoot root,string name)
        {
            var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
            root.MapIntegration.MapPresenter.RefreshNow();var canvas=root.GetComponentInChildren<Canvas>();
            var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/internal");
                File.WriteAllBytes("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/internal/"+name+".png",texture.EncodeToPNG());
            }
            finally
            {canvas.renderMode=mode;canvas.worldCamera=priorCamera;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
        private static void Move(S01SceneCompositionRoot root,Vector3 point)
        {var controller=root.PlayerObject.GetComponent<CharacterController>();controller.enabled=false;controller.transform.position=point;controller.enabled=true;Physics.SyncTransforms();}
        [UnityTest] public IEnumerator ProductionCameraReviewAndRankedEquipmentRestore()
        {
            yield return Load();var root=_scene.Root;
            var points=new[]{root.EnemyPopulation.GetSpot(0).Position,root.EnemyPopulation.GetSpot(1).Position,new Vector3(0,0,-1.25f),root.MagnetarGuard.transform.position+Vector3.back*5};
            var names=new[]{"02_ordinary_sector","03_strong_ordinary","04_service_corridor","05_magnetar_encounter"};
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            for(var i=0;i<points.Length;i++){Move(root,points[i]);yield return null;Capture(root,names[i]);}
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Move(root,root.WorldPresenter.Configuration.BossArenaCenter+Vector3.back*4.5f);root.CustodianBoss.Tick(0);yield return null;
            Capture(root,"06_custodian_arena");
            var bossHud=root.GetComponentInChildren<BossHealthHudPresenter>(true);Assert.NotNull(bossHud);Assert.IsTrue(bossHud.IsVisible);Assert.That(bossHud.FillAmount,Is.EqualTo(1));
            Capture(root,"10_boss_hud");
            root.Equipment.GrantEquipment(Chapter01Weapon.ItemId);root.Equipment.Equip(Chapter01Weapon.ItemId,EquipmentSlot.Weapon);
            var weapon=root.PlayerObject.GetComponent<WeaponEquipmentPresenter>();
            Move(root,new Vector3(0,0,10));root.CustodianBoss.Tick(10);yield return new WaitForSeconds(4.1f);
            Assert.That(weapon.VisualRank,Is.EqualTo(1));Capture(root,"07_g0_rank1");
            var rank1Origin=weapon.ActiveMuzzle.localPosition;
            var hardpoint=weapon.ActiveMuzzle.parent.parent;
            File.WriteAllText("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification/weapon_mount.txt","Player "+root.PlayerObject.transform.position+"\nHardpoint "+hardpoint.position+" forward="+hardpoint.forward+" up="+hardpoint.up+" right="+hardpoint.right+" euler="+hardpoint.eulerAngles+"\nMuzzle "+weapon.ActiveMuzzle.position);
            for(var rank=2;rank<=5;rank++)
            {
                root.Equipment.GrantRankedCopy(Chapter01Weapon.ItemId);Assert.That(weapon.VisualRank,Is.EqualTo(rank));
                for(var tier=0;tier<3;tier++)
                {
                    var form=root.PlayerObject.GetComponent<Gravivore.Presentation.Evolution.PlayerEvolutionView>().GetTierForm((Gravivore.Gameplay.Progression.EvolutionTier)tier);
                    var attachment=form.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="M-0 Equipped Weapon");
                    for(var r=1;r<=5;r++)Assert.That(attachment.Find("Rank"+r).gameObject.activeSelf,Is.EqualTo(r==rank));
                }
            }
            Assert.That(weapon.ActiveMuzzle.localPosition.z,Is.GreaterThan(rank1Origin.z+.5f));
            yield return new WaitForSeconds(4.1f);Capture(root,"08_g0_rank5");Capture(root,"11_minimap_hud");
            var lash=root.GetComponentInChildren<GravityLashVfxPool>();var destination=weapon.ActiveMuzzle.position+weapon.ActiveMuzzle.forward*4;
            var count=lash.Phase6BCreatedVfxCount;
            // Freeze only presentation clocks while the GPU uploads each review frame. A slow
            // ReadPixels must not consume a short production cue before it can be photographed.
            lash.enabled=false;
            foreach(var cue in lash.Vfx.GetComponentsInChildren<Phase6BVfxInstance>(true))cue.enabled=false;
            lash.BeginCharge(Vector3.zero,destination,null,.5f);
            Assert.That(Vector3.Distance(lash.LastPlayedObject.transform.position,weapon.ActiveMuzzle.position),Is.LessThan(.01f));
            AdvanceCaptureParticles(lash.Vfx.LastPlayedInstance);yield return null;
            Capture(root,"12a_weapon_source");
            lash.Play(Vector3.zero,destination);lash.Vfx.LastPlayedInstance.Tick(.025f);
            var travel=lash.Vfx.LastPlayedInstance.GetComponentInChildren<LineRenderer>();
            Assert.That(Vector3.Distance(travel.GetPosition(0),weapon.ActiveMuzzle.position),Is.LessThan(.01f));
            yield return null;Capture(root,"12b_weapon_travel");
            lash.Tick(1);AdvanceCaptureParticles(lash.Vfx.LastPlayedInstance);yield return null;
            Assert.That(Vector3.Distance(lash.Vfx.LastPlayedInstance.transform.position,destination),Is.LessThan(.01f));
            Capture(root,"12c_weapon_impact");lash.Vfx.StopAll();
            Assert.That(lash.Phase6BCreatedVfxCount,Is.EqualTo(count));
            root.PauseMenu.Open();var panel=root.GetComponent<WeaponEquipmentPanel>();panel.Open();yield return null;
            Assert.That(panel.DisplayedWeapon,Is.SameAs(root.VisualEnvironment.Definition.UiSkin.WeaponThumbnail(5)));
            Assert.That(root.GetComponentsInChildren<Image>(true).Count(i=>i.name=="Production EXE Frame"),Is.GreaterThan(5));
            Capture(root,"09_equipment_inventory");root.PauseMenu.Resume();Assert.IsTrue(root.FlushNow());
            yield return _scene.Load();root=_scene.Root;weapon=root.PlayerObject.GetComponent<WeaponEquipmentPresenter>();
            Assert.That(root.Equipment.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(5));
            Assert.That(weapon.VisualRank,Is.EqualTo(5));Assert.IsTrue(weapon.IsEquipped);
            Assert.That(root.GetComponentInChildren<GravityLashVfxPool>().EquipmentOrigin,Is.SameAs(weapon.ActiveMuzzle));
        }
        private static void AdvanceCaptureParticles(Phase6BVfxInstance cue)
        {
            foreach(var particle in cue.GetComponentsInChildren<ParticleSystem>())
            {particle.Simulate(.045f,true,false,true);particle.Pause();}
        }
    }
}
