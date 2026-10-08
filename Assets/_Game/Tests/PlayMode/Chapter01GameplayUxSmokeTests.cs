using System;
using System.Collections;
using System.IO;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Development;
using Gravivore.Presentation.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter01GameplayUxSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            root.PlayerHealth.enabled=false;root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        private static void Move(S01SceneCompositionRoot root,Vector3 position)
        {var body=root.PlayerObject.GetComponent<CharacterController>();body.enabled=false;body.transform.position=position;body.enabled=true;Physics.SyncTransforms();}
        [UnityTest] public IEnumerator FirstKillOwnsOneTimerCampingWaitsThenWholeMissingWaveReturnsWithUniqueRewards()
        {
            yield return Load();var root=_scene.Root;var spot=root.EnemyPopulation.GetSpot(0);
            var first=spot.GetLiveEnemy(0);var initial=root.Progression.State.TotalAssimilationScore;
            first.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));var granted=root.Progression.State.TotalAssimilationScore;
            Assert.That(granted,Is.GreaterThan(initial));first.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Assert.That(root.Progression.State.TotalAssimilationScore,Is.EqualTo(granted));
            spot.Tick(18);spot.GetLiveEnemy(0).ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Assert.That(spot.SecondsUntilNextRespawn,Is.EqualTo(102));
            var marker=root.MapMarkers.GetMarker(1);Assert.That(marker.Availability,Is.EqualTo(MapAvailabilityState.Active));
            Assert.That(marker.LiveEnemyCount,Is.EqualTo(2));
            Assert.That(marker.RemainingSeconds,Is.EqualTo(spot.SecondsUntilNextRespawn));
            var map=root.MapIntegration.MapPresenter;map.RefreshNow();
            Assert.That(map.ExpandedSurface.Find("Marker_"+spot.Id+"/Timer").GetComponent<Text>().text,Is.EqualTo("×2"));
            map.OpenExpanded();Capture(root,"map-cooldown");map.CloseExpanded();
            Move(root,spot.Position);spot.Tick(101f);Assert.That(spot.LiveCount,Is.EqualTo(2));spot.Tick(1f);
            Assert.That(spot.Availability,Is.EqualTo(SpawnSpotAvailability.Ready));Assert.That(spot.LiveCount,Is.EqualTo(2));
            // Other combat within this wave blocks return even after the player leaves its origin.
            var survivor=spot.GetLiveEnemy(0);Move(root,spot.Position+Vector3.right*20);survivor.transform.position=root.PlayerObject.transform.position;
            survivor.enabled=true;yield return null;survivor.enabled=false;
            Assert.IsTrue(spot.IsInCombat);spot.Tick(0);Assert.That(spot.LiveCount,Is.EqualTo(2));
            Move(root,new Vector3(30,0,-30));survivor.enabled=true;yield return null;survivor.enabled=false;
            Assert.IsFalse(spot.IsInCombat);spot.Tick(0);Assert.That(spot.LiveCount,Is.EqualTo(4));
            Assert.That(root.EnemyPopulation.LiveEnemyCount,Is.LessThanOrEqualTo(25));
            Assert.That(root.Progression.State.TotalAssimilationScore,Is.GreaterThan(granted));
            var score=root.Progression.State.TotalAssimilationScore;spot.Tick(120);Assert.That(root.Progression.State.TotalAssimilationScore,Is.EqualTo(score));
            Assert.IsTrue(root.FlushNow());var levels=root.PlayerStats.BaseLevels;
            yield return _scene.Load();Assert.That(_scene.Root.Progression.State.TotalAssimilationScore,Is.EqualTo(score));
            Assert.That(_scene.Root.PlayerStats.BaseLevels,Is.EqualTo(levels));
        }
        [UnityTest] public IEnumerator RelevanceRecyclePreservesFirstKillDeadlineAndDoesNotGrowPool()
        {
            yield return Load();var root=_scene.Root;var spot=root.EnemyPopulation.GetSpot(0);
            spot.GetLiveEnemy(0).ApplyDamage(new DamageRequest(100000,DamageType.Gravity));spot.Tick(10);
            spot.SetActive(false);spot.Tick(90);spot.SetActive(true);
            Assert.That(spot.SecondsUntilNextRespawn,Is.EqualTo(20));spot.Tick(19);Assert.That(spot.LiveCount,Is.Zero);
            Move(root,new Vector3(30,0,-30));spot.Tick(1);Assert.That(spot.LiveCount,Is.EqualTo(4));
            var objects=root.GetComponentsInChildren<Transform>(true).Length;root.Compose();root.VisualEnvironment.Initialize();
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(objects));
            Assert.That(root.GetComponentsInChildren<RepairHubProductionPresenter>(true).Length,Is.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<MapMinimapPresenter>(true).Length,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator MapFootprintsMatchEveryTraversalBlockerAndOpenGatesKeepTheirGeometry()
        {
            yield return Load();var root=_scene.Root;var map=root.MapIntegration.MapPresenter;var topology=map.Topology;
            Physics.SyncTransforms();
            Assert.NotNull(topology);Assert.That(topology.BlockerCount,Is.EqualTo(root.WorldPresenter.GameplayRoot.GetComponentsInChildren<BoxCollider>(true).Length-1));
            for(var i=0;i<topology.BlockerCount;i++)
            {
                var f=topology.GetBlocker(i);var box=(BoxCollider)f.Authority;
                var min=Vector3.Min(Vector3.Min(f.A,f.B),Vector3.Min(f.C,f.D));var max=Vector3.Max(Vector3.Max(f.A,f.B),Vector3.Max(f.C,f.D));
                Assert.That(min.x,Is.EqualTo(box.bounds.min.x).Within(.001));Assert.That(max.z,Is.EqualTo(box.bounds.max.z).Within(.001));
            }
            Assert.IsTrue(topology.EliteGate.IsLocked);Assert.IsTrue(topology.BossGate.IsLocked);
            map.OpenExpanded();Capture(root,"map-locked");map.CloseExpanded();
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();map.RefreshNow();
            Assert.IsFalse(topology.EliteGate.IsLocked);Assert.IsTrue(topology.BossGate.IsLocked);
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));map.RefreshNow();
            Assert.IsFalse(topology.BossGate.IsLocked);
            map.OpenExpanded();Capture(root,"map-open");map.CloseExpanded();
            var elite=root.MapMarkers.GetMarker(root.EnemyPopulation.SpotCount+1);
            Assert.That(elite.Availability,Is.EqualTo(MapAvailabilityState.Cooldown));Assert.That(elite.RemainingSeconds,Is.GreaterThan(120));
            Assert.NotNull(map.CompactSurface.GetComponentInChildren<TacticalMapGraphic>());
            Assert.NotNull(map.ExpandedSurface.GetComponentInChildren<TacticalMapGraphic>(true));
        }
        [UnityTest] public IEnumerator RepairArmsObserveHealingAndKeepObjectsVfxAudioBounded()
        {
            yield return Load();var root=_scene.Root;var hub=root.RepairHub;Move(root,hub.RepairPosition);
            var count=hub.GetComponentsInChildren<Transform>(true).Length;var capacity=hub.Vfx.CreatedInstanceCount;
            root.PlayerHealth.ApplyDamage(new DamageRequest(root.PlayerHealth.MaximumHitPoints*.6f,DamageType.Physical));
            root.PlayerHealth.Tick(3.1f);root.PlayerHealth.Tick(.1f);Assert.IsTrue(hub.IsRepairing);
            var hp=root.PlayerHealth.CurrentHitPoints;hub.Manipulators.Tick(.4f);
            Assert.That(hub.Manipulators.Engagement,Is.EqualTo(1));Assert.That(root.PlayerHealth.CurrentHitPoints,Is.EqualTo(hp));
            Capture(root,"repair-active");
            for(var i=0;i<100;i++)hub.Manipulators.Tick(.02f);
            Assert.That(hub.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(count));
            Assert.That(hub.Vfx.CreatedInstanceCount,Is.EqualTo(capacity));Assert.That(hub.GetComponentsInChildren<AudioSource>(true).Length,Is.EqualTo(1));
            Assert.That(hub.GetComponentsInChildren<Collider>(true).Length,Is.Zero);
            Move(root,hub.RepairPosition+Vector3.right*10);yield return null;hub.Manipulators.Tick(.5f);
            Assert.IsFalse(hub.IsRepairing);Assert.That(hub.Manipulators.Engagement,Is.Zero);
        }
        [UnityTest] public IEnumerator ColdStartRecorderCapturesFirstFramesAndRemainsBounded()
        {
            yield return Load();var diag=_scene.Root.GetComponentInChildren<ColdStartDiagnostics>();Assert.NotNull(diag);
            for(var i=0;i<150;i++)diag.RecordFrame(100, .01*i);
            Assert.That(diag.Evidence.stallCount,Is.EqualTo(ColdStartDiagnostics.MaximumStalls));
            Assert.That(diag.Evidence.droppedStalls,Is.GreaterThan(0));
            Assert.That(diag.Evidence.phases[0].at,Is.LessThan(5));
            diag.Finish("deterministic bounded regression");Assert.IsFalse(diag.enabled);Assert.IsTrue(File.Exists(diag.OutputPath));
        }
        [UnityTest] public IEnumerator ColdStartNinetySecondObservationAndReloadEvidence()
        {
            _scene=new CanonicalSceneTestScope();yield return _scene.Load();var root=_scene.Root;
            root.PlayerHealth.SetDevelopmentGodMode(true);
            var diag=root.GetComponentInChildren<ColdStartDiagnostics>();
            var directory="docs/chapter01-gameplay-ux/verification";Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(5);
            diag.MarkPhase("first-combat/farming traversal");
            for(var i=0;i<5;i++)
            {
                Move(root,root.EnemyPopulation.GetSpot(i).Position);
                yield return new WaitForSecondsRealtime(5);
            }
            Move(root,root.RepairHub.RepairPosition);diag.MarkPhase("post-combat/repair idle");
            yield return new WaitForSecondsRealtime(61);
            Assert.IsFalse(diag.enabled);Assert.That(diag.Evidence.countCount,Is.LessThanOrEqualTo(ColdStartDiagnostics.MaximumCounts));
            File.Copy(diag.OutputPath,Path.Combine(directory,"cold-start-editor.json"),true);
            var count=root.GetComponentsInChildren<Transform>(true).Length;root.Compose();
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(count));
            yield return _scene.Load();diag=_scene.Root.GetComponentInChildren<ColdStartDiagnostics>();
            yield return new WaitForSecondsRealtime(2);diag.Finish("reload regression");
            File.Copy(diag.OutputPath,Path.Combine(directory,"reload-editor.json"),true);
        }
        private static void Capture(S01SceneCompositionRoot root,string name)
        {
            root.MapIntegration.MapPresenter.RefreshNow();
            var camera=UnityEngine.Camera.main;camera.GetComponent<Gravivore.Presentation.Camera.PortraitFollowCamera>().SnapToTarget();
            var canvas=root.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var world=canvas.worldCamera;var prior=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(540,960,24);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory("docs/chapter01-gameplay-ux/internal");
                File.WriteAllBytes("docs/chapter01-gameplay-ux/internal/"+name+".png",texture.EncodeToPNG());
            }
            finally{canvas.renderMode=mode;canvas.worldCamera=world;camera.targetTexture=prior;RenderTexture.active=active;render.Release();UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(texture);}
        }
    }
}
