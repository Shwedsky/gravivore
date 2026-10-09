using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Map;
using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter1DeviceStabilizationSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup() { if (_scene != null) yield return _scene.Cleanup(); }
        private IEnumerator Load()
        {
            _scene = new CanonicalSceneTestScope();
            yield return _scene.Load();
            var root = _scene.Root;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            root.EnemyPopulation.enabled = false;
            root.MagnetarGuard.enabled = false;
            root.CustodianBoss.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
        }

        [UnityTest]
        public IEnumerator FreshProfile_PreEliteStrongPackSpawnsWithoutUnlockAndPhysicalGatesRemainIntact()
        {
            yield return Load();
            var root = _scene.Root;
            var player = root.PlayerObject.transform;
            var body = root.PlayerObject.GetComponent<CharacterController>();
            foreach(var other in root.GetComponentsInChildren<CharacterController>(true))if(other!=body)other.enabled=false;
            var spot = root.EnemyPopulation.GetSpot(5);
            // Reach the actual pre-elite pack with CharacterController collision enabled.
            WalkCapsulePath(root, body, spot.Position + Vector3.back*5f);
            root.EnemyPopulation.Tick(0f);
            Assert.That(root.WorldUnlocks.State.EliteGateUnlocked, Is.False);
            Assert.That(root.WorldUnlocks.State.BossGateUnlocked, Is.False);
            Assert.That(spot.LiveCount, Is.EqualTo(3), spot.Id);
            Assert.That(spot.GetLiveEnemy(0).CurrentHitPoints, Is.GreaterThan(root.EnemyPopulation.GetSpot(3).GetLiveEnemy(0).CurrentHitPoints));
            var marker = root.MapMarkers.GetMarker(6);
            Assert.That(marker.Availability, Is.EqualTo(MapAvailabilityState.Active));
            Assert.That(marker.LiveEnemyCount,Is.EqualTo(3));
            Assert.That(marker.ProgressionLocked, Is.False);
            Assert.That(marker.DisplayName, Is.EqualTo("Усиленная зона"));
            Assert.That(root.EnemyPopulation.LiveEnemyCount, Is.LessThanOrEqualTo(25));
            yield return null;
            Chapter1ConsolidatedRuntimeSmokeTests.Capture(root, "02_strong_spot_pre_elite.png");
            WalkCapsulePath(root, body, new Vector3(root.WorldPresenter.Bounds.MinX+2, player.position.y, player.position.z));
            for (var i = 0; i < 100; i++) body.Move(Vector3.forward * .5f);
            Assert.That(player.position.z, Is.LessThan(root.WorldPresenter.Configuration.EliteGate.Position.z), "Permanent gate side walls still enforce the genuine boundary.");
            Assert.That(root.WorldUnlocks.State.EliteGateUnlocked, Is.False);
            Assert.That(root.WorldUnlocks.State.BossGateUnlocked, Is.False);
        }

        [UnityTest]
        public IEnumerator ProductionUi_AllMarkersEquipmentStatesAndDeveloperSurfaceUseRussian()
        {
            yield return Load();
            var root = _scene.Root;
            Audit(root);
            var map = root.MapIntegration.MapPresenter;
            map.OpenExpanded(); map.RefreshNow();
            for (var i = 1; i < root.MapMarkers.Count; i++)
            {
                var marker = root.MapMarkers.GetMarker(i);
                Assert.That(Regex.IsMatch(marker.DisplayName, "[A-Za-z]"), Is.False, marker.DisplayName);
                Assert.That(map.SelectMarker(marker.Id), Is.True);
                Audit(root);
            }
            map.SelectMarker("repair-hub");
            yield return null;
            Chapter1ConsolidatedRuntimeSmokeTests.Capture(root, "01_russian_map.png");
            map.CloseExpanded();
            var menu = root.GetComponentInChildren<PauseMenuPresenter>(true);
            for (var i = 0; i < root.EquipmentCatalog.Count; i++)
            {
                var item = root.EquipmentCatalog.GetAt(i);
                root.Equipment.GrantEquipment(item.Id); root.Equipment.Equip(item.Id, item.Slot);
                menu.Open(); Audit(root); menu.Resume();
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            root.DevelopmentOverlay.Toggle(); Audit(root); root.DevelopmentOverlay.Toggle();
#endif
            // Exercise repeat/cooldown strings from the authoritative runtime, not a mock.
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment(); root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            map.OpenExpanded(); map.RefreshNow(); Assert.That(map.SelectMarker("magnetar"), Is.True); Audit(root);
            map.CloseExpanded();
            var body = root.PlayerObject.GetComponent<CharacterController>();
            body.enabled = false; root.PlayerObject.transform.position = root.MapMarkers.GetMarker(12).WorldPosition; body.enabled = true;
            Physics.SyncTransforms();
            root.PlayerHealth.enabled = false;
            root.PlayerHealth.ApplyDamage(new DamageRequest(15f, DamageType.Physical));
            root.PlayerHealth.Tick(3.1f);
            Assert.That(root.RepairHub.IsRepairing, Is.True);
            Audit(root);
            yield return null;
            Chapter1ConsolidatedRuntimeSmokeTests.Capture(root, "08_repair_hub_russian.png");
        }

        [UnityTest]
        public IEnumerator ActualNonlethalEnemyHit_UsesDirectionalHostileImpactWithoutLegacyRedDiscs()
        {
            yield return Load();
            var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            enemy.transform.position = root.PlayerObject.transform.position + Vector3.forward * 2.5f;
            Physics.SyncTransforms();
            var playerHp = root.PlayerHealth.CurrentHitPoints;
            enemy.ApplyDamage(new DamageRequest(1f, DamageType.Gravity));
            Assert.That(root.Phase6BCombat.EnemyHitCount, Is.EqualTo(1));
            Assert.That(root.Phase6BCombat.EnemyDeathCount, Is.Zero);
            Assert.That(root.PlayerHealth.CurrentHitPoints, Is.EqualTo(playerHp));
            var active = root.Phase6BCombat.Vfx.GetComponentsInChildren<Phase6BVfxInstance>(true).Where(x => x.IsPlaying).ToArray();
            Assert.That(active.Length, Is.EqualTo(1));
            Assert.That(active[0].Cue, Is.EqualTo(Phase6BVfxCue.HostileImpact));
            Assert.That(active[0].Shape, Is.EqualTo(Phase6BVfxShape.Sparks));
            Assert.That(active[0].ParticleCount, Is.EqualTo(10));
            var particles = active[0].GetComponentInChildren<ParticleSystem>();
            Assert.That(particles.GetComponent<ParticleSystemRenderer>().renderMode, Is.EqualTo(ParticleSystemRenderMode.Stretch));
            Assert.That(active[0].GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(root.CombatFeedback.EnemyHitPool.ActiveCount, Is.Zero);
            Assert.That(root.CombatFeedback.EnemyDeathPool.ActiveCount, Is.Zero);
            particles.Simulate(.12f, true, false, true);
            Chapter1ConsolidatedRuntimeSmokeTests.Capture(root, "07_enemy_impact_updated.png");
            enemy.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            Assert.That(root.Phase6BCombat.EnemyDeathCount, Is.EqualTo(1));
            Assert.That(root.Phase6BCombat.Vfx.GetComponentsInChildren<Phase6BVfxInstance>(true).Any(x => x.IsPlaying && x.Cue == Phase6BVfxCue.MechanicalKillBurst), Is.True);
            yield return null;
        }

        private static void Walk(CharacterController body, Vector3 target)
        {
            for (var i = 0; i < 400; i++)
            {
                var offset = target - body.transform.position; offset.y = 0f;
                if (offset.magnitude < .02f) return;
                body.Move(Vector3.ClampMagnitude(offset, .16f)); Physics.SyncTransforms();
            }
            Assert.Fail("Physical side route blocked before " + target + ": " + body.transform.position);
        }

        internal static void WalkCapsulePath(S01SceneCompositionRoot root, CharacterController body, Vector3 target)
        {
            // Test-only route search uses the real capsule, proxies and casts; compact
            // service clusters no longer promise a straight line across every row.
            Physics.SyncTransforms();
            var bounds=root.WorldPresenter.Bounds;const float spacing=.5f;
            var radius=body.radius+body.skinWidth+.08f;
            var lower=body.center-Vector3.up*(body.height/2-body.radius);
            var upper=body.center+Vector3.up*(body.height/2-body.radius);
            var width=Mathf.RoundToInt(bounds.Size.x/spacing)-1;
            var height=Mathf.RoundToInt(bounds.Size.y/spacing)-1;
            var origin=new Vector3(bounds.MinX+spacing,body.transform.position.y,bounds.MinZ+spacing);
            int Cell(Vector3 p)=>Mathf.RoundToInt((p.z-origin.z)/spacing)*width+Mathf.RoundToInt((p.x-origin.x)/spacing);
            Vector3 Point(int i)=>origin+new Vector3(i%width*spacing,0,i/width*spacing);
            var parents=Enumerable.Repeat(-1,width*height).ToArray();var queue=new Queue<int>();
            var start=Cell(body.transform.position);var goal=Cell(target);parents[start]=start;queue.Enqueue(start);
            var mask=LayerMask.GetMask("HardBlocker");
            while(queue.Count>0&&parents[goal]<0)
            {
                var current=queue.Dequeue();var x=current%width;var z=current/width;
                foreach(var offset in new[]{-1,1,-width,width})
                {
                    var next=current+offset;
                    if(next<0||next>=parents.Length||parents[next]>=0||offset==-1&&x==0||offset==1&&x==width-1||offset==-width&&z==0||offset==width&&z==height-1)continue;
                    var a=Point(current);var b=Point(next);
                    if(Physics.CheckCapsule(b+lower,b+upper,radius,mask,QueryTriggerInteraction.Ignore)||
                       Physics.CapsuleCast(a+lower,a+upper,radius,(b-a).normalized,spacing,mask,QueryTriggerInteraction.Ignore))continue;
                    parents[next]=current;queue.Enqueue(next);
                }
            }
            Assert.That(parents[goal],Is.GreaterThanOrEqualTo(0),"No real capsule route to "+target);
            var path=new List<int>();for(var at=goal;at!=start;at=parents[at])path.Add(at);
            path.Reverse();foreach(var at in path)Walk(body,Point(at));Walk(body,target);
        }

        private static void Audit(S01SceneCompositionRoot root)
        {
            foreach (var text in root.GetComponentsInChildren<Text>(true))
            {
                // Required legal attribution preserves the author's proper name,
                // source URL and license identifier; functional UI stays Russian.
                if(text.name=="UI Attribution")
                {
                    StringAssert.StartsWith("Оформление:",text.text);
                    StringAssert.Contains("Catherine Laserna",text.text);
                    StringAssert.Contains("CC BY 4.0",text.text);
                    StringAssert.Contains("cjlaserna.itch.io/exe",text.text);
                    continue;
                }
                Assert.That(Regex.IsMatch(text.text, "[A-Za-z]"), Is.False, text.name + ": " + text.text);
            }
        }
    }
}
