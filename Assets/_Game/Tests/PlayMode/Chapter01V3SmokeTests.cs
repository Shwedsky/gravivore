using System;
using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Player;
using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter01V3SmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;root.PlayerHealth.enabled=false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        private static void Move(S01SceneCompositionRoot root,Vector3 point)
        {var body=root.PlayerObject.GetComponent<CharacterController>();body.enabled=false;body.transform.position=point;body.enabled=true;Physics.SyncTransforms();}
        private static void UnlockBoss(S01SceneCompositionRoot root)
        {root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));}
        [UnityTest] public IEnumerator RepairRightExitAndAllStylizedEdgesRemainTraversableWithRealCapsule()
        {
            yield return Load();var root=_scene.Root;var body=root.PlayerObject.GetComponent<CharacterController>();
            foreach(var collider in root.GetComponentsInChildren<CharacterController>(true))if(collider!=body)collider.enabled=false;
            Assert.IsFalse(root.WorldPresenter.GameplayRoot.GetComponentsInChildren<Transform>().Any(t=>t.name=="Repair Hub Right Service Frame"));
            Move(root,new Vector3(0,0,-27));for(var i=0;i<24;i++)body.Move(Vector3.right*.25f);
            var near=Physics.OverlapCapsule(body.transform.position+Vector3.up*.5f,body.transform.position+Vector3.up*.9f,body.radius+.25f,LayerMask.GetMask("HardBlocker"));
            Assert.That(body.transform.position.x,Is.EqualTo(6).Within(.1f),string.Join("; ",near.Select(c=>c.name+" center="+c.bounds.center+" size="+c.bounds.size)));Assert.That(body.transform.position.z,Is.EqualTo(-27).Within(.1f));
            var richness=root.VisualEnvironment.Floor.Find("Chapter 01 V3 richness");Assert.NotNull(richness);
            Assert.That(richness.GetComponentsInChildren<Collider>(true).Length,Is.Zero);
            Assert.That(richness.GetComponentsInChildren<Renderer>().Length,Is.GreaterThan(75));
            root.WorldPresenter.EliteGate.SetLocked(false);root.WorldPresenter.BossGate.SetLocked(false);
            var bounds=root.WorldPresenter.Bounds;
            foreach(var pair in new[]{new Vector3(bounds.MinX+2,0,-32),new Vector3(bounds.MaxX-2,0,-32),new Vector3(bounds.MinX+2,0,32),new Vector3(bounds.MaxX-2,0,32),new Vector3(bounds.MinX+2,0,bounds.MaxZ-10),new Vector3(bounds.MaxX-2,0,bounds.MaxZ-10)})
            {
                Move(root,pair);for(var i=0;i<12;i++)body.Move(Vector3.forward*.25f);
                Assert.That(body.transform.position.z,Is.EqualTo(pair.z+3).Within(.15f));Capture(root,"edge_"+pair.x+"_"+pair.z);
            }
        }
        [UnityTest] public IEnumerator SurvivingWaveShowsCountUntilEmptyThenFirstKillDeadlineAndReady()
        {
            yield return Load();var root=_scene.Root;var spot=root.EnemyPopulation.GetSpot(0);var map=root.MapIntegration.MapPresenter;Move(root,spot.Position);
            for(var remaining=3;remaining>=0;remaining--)
            {
                spot.GetLiveEnemy(0).ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
                if(remaining==3)spot.Tick(18);
                map.RefreshNow();var text=map.CompactSurface.Find("Marker_"+spot.Id+"/Timer").GetComponent<Text>().text;
                Assert.That(text,Is.EqualTo(remaining>0?"×"+remaining:"01:42"));
                Assert.That(spot.SecondsUntilNextRespawn,Is.EqualTo(102));
            }
            Move(root,spot.Position);spot.Tick(102);map.RefreshNow();
            Assert.That(map.CompactSurface.Find("Marker_"+spot.Id+"/Timer").GetComponent<Text>().text,Is.EqualTo("ГОТОВО"));
        }
        [UnityTest] public IEnumerator EliteAndBossBasicAttacksEmitBoundedSourceTravelImpactBetweenSpecials()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            var presentation=root.GetComponentInChildren<AttackCausalityPresenter>();Assert.NotNull(presentation);Assert.That(presentation.Capacity,Is.EqualTo(16));
            var eliteCount=0;root.MagnetarGuard.BasicAttackResolved+=value=>{if(value.Hit)eliteCount++;};
            Move(root,root.MagnetarGuard.transform.position+Vector3.back*6);
            for(var i=0;i<100;i++){root.MagnetarGuard.Tick(.1f);presentation.Tick(.1f);}
            Assert.That(eliteCount,Is.GreaterThanOrEqualTo(2));UnlockBoss(root);
            Move(root,root.CustodianBoss.transform.position+Vector3.back*4.5f);
            var bossCount=0;root.CustodianBoss.BasicAttackResolved+=value=>{if(value.Hit)bossCount++;};
            for(var i=0;i<120;i++){root.CustodianBoss.Tick(.1f);presentation.Tick(.1f);}
            Assert.That(bossCount,Is.GreaterThanOrEqualTo(2));Assert.That(presentation.SourceCount,Is.GreaterThan(3));
            Assert.That(presentation.TravelCount,Is.GreaterThan(3));Assert.That(presentation.ImpactCount,Is.GreaterThan(3));
            Assert.That(presentation.Capacity,Is.EqualTo(16));Assert.That(Vector3.Distance(presentation.LastSource,presentation.LastTarget),Is.GreaterThan(.5f));
            StringAssert.StartsWith("Custodian_V3",root.CustodianBoss.GetComponent<CharacterVisualBinding>().ActiveModel.name);
            Capture(root,"custodian_v3_pressure");
        }
        [UnityTest] public IEnumerator BossUsesSmallAdmissionThenWideCombatLeashAndOnlyResetsBeyondGrace()
        {
            yield return Load();var root=_scene.Root;UnlockBoss(root);root.PlayerHealth.SetDevelopmentGodMode(true);var boss=root.CustodianBoss;
            var config=(CustodianBossConfiguration)typeof(CustodianBossController).GetField("_configuration",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(boss);
            Move(root,config.ArenaCenter+Vector3.back*(boss.InitialAggroRadius+1));boss.Tick(0);Assert.That(boss.State,Is.EqualTo(CustodianBossState.Dormant));
            Move(root,config.ArenaCenter+Vector3.back*4);boss.Tick(0);boss.ApplyDamage(new DamageRequest(100,DamageType.Gravity));var hp=boss.CurrentHitPoints;var resets=0;boss.EncounterReset+=value=>resets++;
            Move(root,config.ArenaCenter+Vector3.right*(boss.CombatLeashRadius-1));
            for(var i=0;i<80;i++)boss.Tick(.1f);
            Assert.That(resets,Is.Zero);Assert.That(boss.OutsideCombatLeashSeconds,Is.Zero);Assert.That(boss.CurrentHitPoints,Is.EqualTo(hp));
            Move(root,config.ArenaCenter+Vector3.right*(boss.CombatLeashRadius+1));boss.Tick(config.ArenaExitResetGraceSeconds*.5f);
            Assert.That(resets,Is.Zero);Move(root,config.ArenaCenter);boss.Tick(0);Assert.That(boss.OutsideCombatLeashSeconds,Is.Zero);
            Move(root,config.ArenaCenter+Vector3.right*(boss.CombatLeashRadius+1));boss.Tick(config.ArenaExitResetGraceSeconds+.01f);
            Assert.That(resets,Is.EqualTo(1));Assert.That(boss.State,Is.EqualTo(CustodianBossState.Dormant));Assert.That(boss.CurrentHitPoints,Is.EqualTo(boss.MaximumHitPoints));
        }
        [UnityTest] public IEnumerator LegitimateBossLootUnlocksEquipsAttachesSavesAndRanksExactlyOncePerKill()
        {
            yield return Load();var root=_scene.Root;UnlockBoss(root);var presenter=root.PlayerObject.GetComponent<WeaponEquipmentPresenter>();
            Assert.NotNull(presenter);Assert.That(presenter.AttachmentCount,Is.EqualTo(3));Assert.IsFalse(presenter.IsEquipped);
            Move(root,root.CustodianBoss.transform.position+Vector3.back*4.5f);root.CustodianBoss.Tick(0);
            root.CustodianBoss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Assert.That(root.Equipment.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(1));Assert.IsTrue(presenter.IsEquipped);
            Assert.That(presenter.ActiveMuzzle.parent.parent.name,Is.EqualTo("R_TOOL"));
            Assert.That(root.GetComponentInChildren<GravityLashVfxPool>().EquipmentOrigin,Is.SameAs(presenter.ActiveMuzzle));
            root.CustodianBoss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));root.Chapter1Encounters.Transactions.RecoverPending();
            Assert.That(root.Equipment.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(1));root.ChapterCompletion.ContinueExploring();
            _scene.AdvanceTime(TimeSpan.FromMinutes(30));root.Chapter1Encounters.Tick();root.CustodianBoss.Tick(0);root.CustodianBoss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Assert.That(root.Equipment.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(2));root.ChapterCompletion.ContinueExploring();Assert.IsTrue(root.FlushNow());
            yield return _scene.Load();root=_scene.Root;presenter=root.PlayerObject.GetComponent<WeaponEquipmentPresenter>();
            Assert.That(root.Equipment.Inventory.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(2));Assert.IsTrue(presenter.IsEquipped);
            var panel=root.GetComponent<WeaponEquipmentPanel>();root.PauseMenu.Open();panel.Open();StringAssert.Contains("ранг 2",panel.Description);Capture(root,"weapon_equipment_rank2");
            panel.ToggleWeapon();Assert.IsFalse(presenter.IsEquipped);Assert.IsNull(root.GetComponentInChildren<GravityLashVfxPool>().EquipmentOrigin);
            panel.ToggleWeapon();Assert.IsTrue(presenter.IsEquipped);root.PauseMenu.Resume();
            Capture(root,"g0_bipedal_with_m0");
        }
        [UnityTest] public IEnumerator IncomingDamageUsesHostileOutlinedPooledNumbersBesideG0()
        {
            yield return Load();var root=_scene.Root;var ui=root.CombatReadability;var transforms=root.GetComponentsInChildren<Transform>(true).Length;
            var result=root.PlayerHealth.ApplyDamage(new DamageRequest(12,DamageType.Physical));ui.Tick(.01f);
            Assert.That(ui.IncomingFeedbackCount,Is.EqualTo(1));Assert.That(ui.LastIncomingText,Is.EqualTo("−"+EnemyCombatReadabilityPresenter.FormatDamage(result.AppliedDamage)));
            var text=root.GetComponentsInChildren<Text>().Single(t=>t.text==ui.LastIncomingText);
            Assert.That(text.color.r,Is.GreaterThan(text.color.g*2));Assert.That(text.fontSize,Is.GreaterThanOrEqualTo(40));Assert.NotNull(text.GetComponent<Outline>());
            Assert.That(text.transform.localScale.x,Is.GreaterThan(1));Capture(root,"incoming_damage");
            for(var i=0;i<100;i++){root.PlayerHealth.ApplyDamage(new DamageRequest(.1f,DamageType.Physical));ui.Tick(.01f);}
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(transforms));Assert.That(ui.ActiveCombatTextCount,Is.LessThanOrEqualTo(ui.TextCapacity));
        }
        private static void Capture(S01SceneCompositionRoot root,string name)
        {
            var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();var canvas=root.GetComponentInChildren<Canvas>();
            var mode=canvas.renderMode;var priorCamera=canvas.worldCamera;var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;
            var render=new RenderTexture(540,960,24,RenderTextureFormat.ARGBHalf);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try{canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory("docs/chapter01-v3/internal");File.WriteAllBytes("docs/chapter01-v3/internal/"+name+".png",texture.EncodeToPNG());}
            finally{canvas.renderMode=mode;canvas.worldCamera=priorCamera;camera.targetTexture=priorTarget;RenderTexture.active=priorActive;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
    }
}
