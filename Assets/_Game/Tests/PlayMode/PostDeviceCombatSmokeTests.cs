using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Player;
using Gravivore.Presentation.UI;
using Gravivore.Presentation.Development;
using Gravivore.Presentation.AudioVfx;
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ReadabilityTestTarget : MonoBehaviour, ITargetable, IDamageable
    {
        public bool Alive = true;
        public Transform TargetPoint => transform;
        public bool CanBeTargeted => Alive;
        public bool IsAlive => Alive;
        public bool IsHostileTo(CombatFaction faction) => faction == CombatFaction.Player;
        public DamageResult ApplyDamage(in DamageRequest request) => new DamageResult(request.RawDamage * .5f, false);
    }

    public sealed class PostDeviceCombatSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        private readonly List<GameObject> _owned = new List<GameObject>();
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var item in _owned) UnityEngine.Object.Destroy(item); _owned.Clear();
            if (_scene != null) yield return _scene.Cleanup(); _scene = null;
        }
        private IEnumerator Load()
        {
            _scene = new CanonicalSceneTestScope(); yield return _scene.Load();
            var root = _scene.Root; root.EnemyPopulation.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            root.PlayerObject.GetComponent<MechMotionPresenter>().enabled = false;
            root.MagnetarGuard.enabled = false; root.CustodianBoss.enabled = false;
        }
        [UnityTest] public IEnumerator MovingAttackReturnsLeft_StationaryTracksRightThenChangedTarget_CombatEndKeepsFacing()
        {
            yield return Load(); var root = _scene.Root; var player = root.PlayerObject.transform;
            var targetObject = new GameObject("Readability target", typeof(ReadabilityTestTarget)); _owned.Add(targetObject);
            var target = targetObject.GetComponent<ReadabilityTestTarget>();
            var basis = new GameObject("Input basis"); _owned.Add(basis);
            var input = new TestInput { Movement = Vector2.left };
            var locomotion = player.GetComponent<PlayerLocomotion>();
            locomotion.Initialize(input, basis.transform, root.PlayerStats, 720);
            var motion = player.GetComponent<MechMotionPresenter>();
            var form = player.GetComponent<PlayerEvolutionView>().GetTierForm(Gravivore.Gameplay.Progression.EvolutionTier.Tier0);
            locomotion.Step(.2f); motion.Tick(.2f);
            target.transform.position = player.position + Vector3.right * 2;
            var attack = player.GetComponent<GravityAttackController>();
            var settings = (GravityAttackSettings)typeof(GravityAttackController).GetField("_settings", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(attack);
            attack.Initialize(player, root.PlayerStats, settings, new TestSensor(target), new TestPull(), new TestLash());
            attack.Tick(.11f); Assert.That(attack.ValidCurrentTarget, Is.SameAs(target));
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            lash.Play(player.position, target.transform.position, target); motion.Tick(.02f);
            Assert.That(Vector3.Dot(form.forward, Vector3.right), Is.GreaterThan(.99f));
            motion.Tick(.5f); Assert.That(Vector3.Dot(form.forward, Vector3.left), Is.GreaterThan(.99f));
            input.Movement = Vector2.zero; motion.Tick(.5f);
            Assert.That(Vector3.Dot(form.forward, Vector3.right), Is.GreaterThan(.99f));
            for (var i = 0; i < 4; i++) { lash.Play(player.position, target.transform.position, target); motion.Tick(.3f); }
            Assert.That(Vector3.Dot(form.forward, Vector3.right), Is.GreaterThan(.99f));
            target.transform.position = player.position + Vector3.forward * 2; motion.Tick(.5f);
            Assert.That(Vector3.Dot(form.forward, Vector3.forward), Is.GreaterThan(.99f));
            target.Alive = false; attack.Tick(.11f); var last = form.rotation; motion.Tick(.5f);
            Assert.That(Quaternion.Angle(last, form.rotation), Is.LessThan(.001f));
            Assert.That(Vector3.Dot(player.forward, Vector3.left), Is.GreaterThan(.99f), "Presentation must not rotate gameplay authority.");
        }

        [UnityTest] public IEnumerator AllCurrentStrongSpotsHaveCapsulePathsAndControllerCanTraverseEastServicePortal()
        {
            yield return Load(); var root = _scene.Root;
            root.WorldPresenter.EliteGate.SetLocked(false); root.WorldPresenter.BossGate.SetLocked(false);
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.GetComponent<CharacterController>().enabled = false;
            root.MagnetarGuard.GetComponent<CharacterController>().enabled = false;
            root.CustodianBoss.GetComponent<CharacterController>().enabled = false;
            Physics.SyncTransforms();
            var body = root.PlayerObject.GetComponent<CharacterController>();
            var bounds = root.WorldPresenter.Bounds; const float spacing = .5f;
            var width = Mathf.RoundToInt(bounds.Size.x / spacing) - 1;
            var height = Mathf.RoundToInt(bounds.Size.y / spacing) - 1;
            var origin = new Vector3(bounds.MinX + spacing, 0, bounds.MinZ + spacing);
            var visited = new bool[width * height]; var queue = new Queue<int>();
            int Cell(Vector3 p) => Mathf.RoundToInt((p.z-origin.z)/spacing)*width + Mathf.RoundToInt((p.x-origin.x)/spacing);
            Vector3 Point(int index) => origin + new Vector3(index%width*spacing,0,index/width*spacing);
            bool Clear(Vector3 p) => !Physics.CheckCapsule(p + Vector3.up * .5f, p + Vector3.up * .9f,
                body.radius + .06f, LayerMask.GetMask("HardBlocker"), QueryTriggerInteraction.Ignore);
            var start = Cell(root.PlayerObject.transform.position); visited[start] = true; queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue(); var x = current%width; var z = current/width;
                foreach (var offset in new[] { -1,1,-width,width })
                {
                    var next = current+offset;
                    if (next < 0 || next >= visited.Length || offset == -1 && x == 0 || offset == 1 && x == width-1 ||
                        offset == -width && z == 0 || offset == width && z == height-1 || visited[next]) continue;
                    var a = Point(current); var b = Point(next);
                    if (!Clear(b) || Physics.CapsuleCast(a + Vector3.up*.5f,a + Vector3.up*.9f,body.radius+.06f,
                        (b-a).normalized,spacing,LayerMask.GetMask("HardBlocker"),QueryTriggerInteraction.Ignore)) continue;
                    visited[next] = true; queue.Enqueue(next);
                }
            }
            foreach (var spot in root.StrongSpots) Assert.IsTrue(visited[Cell(spot.Position)], spot.Id + " requires a complete capsule corridor.");
            body.enabled = false; body.transform.position = new Vector3(0,0,64); body.enabled = true;
            for (var i = 0; i < 72; i++) body.Move(Vector3.right * .25f);
            Assert.That(body.transform.position.x, Is.EqualTo(18).Within(.25f), "Visible service portal must be traversable with the real controller, including its ground/skin tolerance.");
            Assert.That(body.transform.position.z, Is.EqualTo(64).Within(.08f));
        }
        private sealed class TestInput : IMovementInput { public Vector2 Movement { get; set; } }
        [UnityTest] public IEnumerator HealthAndRewardPreviewReadAuthority_DamageUsesResolvedResult_DeathRewardsExactlyOnce()
        {
            yield return Load(); var root=_scene.Root; var ui=root.CombatReadability;
            Assert.IsNotNull(ui); ui.enabled=false;
            var enemy=root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            var body=enemy.GetComponent<CharacterController>(); body.enabled=false;
            enemy.transform.position=root.PlayerObject.transform.position+Vector3.forward*2; body.enabled=true;
            UnityEngine.Camera.main.GetComponent<Gravivore.Presentation.Camera.PortraitFollowCamera>().SnapToTarget();
            Canvas.ForceUpdateCanvases(); ui.Tick(.016f);
            Assert.IsTrue(ui.TryReadPlate(enemy,out var fraction,out var label),"Nearby enemy should have a compact HP/reward plate.");
            Assert.That(fraction,Is.EqualTo(1));
            var spot=root.EnemyPopulation.GetSpot(0);
            root.Progression.TryPreview(spot.EnemyId,spot.RewardMultiplier,out var preview);
            Assert.That(label,Is.EqualTo(EnemyCombatReadabilityPresenter.FormatReward(preview)));
            enemy.ApplyDamage(new DamageRequest(2.75f,DamageType.Gravity)); ui.Tick(.016f);
            Assert.IsTrue(ui.TryReadPlate(enemy,out fraction,out label));
            Assert.That(fraction,Is.EqualTo(enemy.CurrentHitPoints/enemy.MaximumHitPoints).Within(.0001f));
            var targetObject=new GameObject("Mitigated target",typeof(ReadabilityTestTarget)); _owned.Add(targetObject);
            var target=targetObject.GetComponent<ReadabilityTestTarget>(); target.transform.position=root.PlayerObject.transform.position+Vector3.right*2;
            var attack=root.PlayerObject.GetComponent<GravityAttackController>();
            var settings=(GravityAttackSettings)typeof(GravityAttackController).GetField("_settings",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(attack);
            attack.Initialize(root.PlayerObject.transform,root.PlayerStats,settings,new TestSensor(target),new TestPull(),new TestLash());
            attack.ResetTransientState(); attack.Tick(.01f); ui.Tick(.01f);
            Assert.That(ui.LastAppliedDamage,Is.EqualTo(root.PlayerStats.DerivedStats.BaseDamage*.5f));
            Assert.That(ui.LastDamageText,Is.EqualTo(EnemyCombatReadabilityPresenter.FormatDamage(ui.LastAppliedDamage)));
            var life=enemy.LifeId; var rewardCount=ui.RewardFeedbackCount;
            enemy.ApplyDamage(new DamageRequest(10000,DamageType.Gravity)); ui.Tick(.01f);
            Assert.That(ui.RewardFeedbackCount,Is.EqualTo(rewardCount+1));
            Assert.That(ui.LastGrantedReward.StatExperience,Is.EqualTo(preview.StatExperience));
            Assert.That(ui.LastGrantedReward.AssimilationScore,Is.EqualTo(preview.AssimilationScore));
            enemy.ApplyDamage(new DamageRequest(10000,DamageType.Gravity));
            Assert.IsFalse(root.Progression.TryGrant(new EnemyDeathEvent(life,spot.EnemyId,Vector3.zero)));
            Assert.That(ui.RewardFeedbackCount,Is.EqualTo(rewardCount+1));
        }
        [UnityTest] public IEnumerator CombatTextVfxAndAudioStayBoundedDuringRepeatedKillsAndExpiredTextReturns()
        {
            yield return Load(); var root=_scene.Root; var ui=root.CombatReadability; ui.enabled=false;
            var objects=root.GetComponentsInChildren<Transform>(true).Length;
            var audio=root.GetComponentsInChildren<AudioSource>(true).Length;
            var lash=root.GetComponentInChildren<GravityLashVfxPool>(); var vfx=lash.Phase6BCreatedVfxCount;
            var rewardCount=ui.RewardFeedbackCount;
            for(var i=0;i<64;i++)
            {
                var spot=root.EnemyPopulation.GetSpot(0); Assert.That(spot.LiveCount,Is.GreaterThan(0));
                var enemy=spot.GetLiveEnemy(0); enemy.ApplyDamage(new DamageRequest(10000,DamageType.Gravity));
                root.EnemyPopulation.Tick(50); ui.Tick(.01f);
                Assert.That(ui.ActiveCombatTextCount,Is.LessThanOrEqualTo(ui.TextCapacity));
            }
            Assert.That(ui.RewardFeedbackCount,Is.EqualTo(rewardCount+64)); ui.Tick(2);
            Assert.That(ui.ActiveCombatTextCount,Is.Zero);
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(objects));
            Assert.That(root.GetComponentsInChildren<AudioSource>(true).Length,Is.EqualTo(audio));
            Assert.That(lash.Phase6BCreatedVfxCount,Is.EqualTo(vfx));
        }
        [UnityTest] public IEnumerator VelocityMatchedAnimationAndFootContactsNeverMoveGameplayAuthority()
        {
            yield return Load(); var root=_scene.Root; var player=root.PlayerObject.transform;
            var basis=new GameObject("Locomotion input basis"); _owned.Add(basis);
            var input=new TestInput { Movement=Vector2.right };
            var locomotion=player.GetComponent<PlayerLocomotion>(); locomotion.Initialize(input,basis.transform,root.PlayerStats,720);
            var bridge=root.GetComponent<VisualSliceAnimationBridge>(); bridge.enabled=false;
            var motion=player.GetComponent<MechMotionPresenter>();
            var start=player.position;
            for(var i=0;i<20;i++)
            {
                locomotion.Step(.016f); var authority=player.position; var rotation=player.rotation;
                motion.Tick(.016f); bridge.Tick(.016f);
                Assert.That(player.position,Is.EqualTo(authority)); Assert.That(player.rotation,Is.EqualTo(rotation));
                Assert.That(bridge.MaximumFootContactOffset,Is.InRange(0,.18001f));
            }
            Assert.That(Vector3.Distance(start,player.position),Is.GreaterThan(1));
            Assert.That(bridge.PlayerRunPlaybackRate,Is.InRange(1.5f,2.5f));
            input.Movement=Vector2.zero; bridge.Tick(.016f);
            Assert.That(bridge.PlayerState,Is.EqualTo("Idle")); Assert.That(bridge.PlayerRunPlaybackRate,Is.EqualTo(1));
            foreach(var animator in player.GetComponentsInChildren<Animator>(true)) Assert.IsFalse(animator.applyRootMotion);
        }
        [UnityTest] public IEnumerator DevHitchSnapshotsHaveCooldownAndBoundedFilesWithPresentationCounts()
        {
            yield return Load(); var root=_scene.Root;
            var diagnostic=root.GetComponent<PresentationHitchDiagnostics>(); Assert.IsNotNull(diagnostic); diagnostic.enabled=false;
            var settings=(Gravivore.Presentation.Combat.PostDevicePresentationDefinition)typeof(Gravivore.Presentation.Composition.S01SceneCompositionRoot)
                .GetField("_postDevicePresentation",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(root);
            var directory=Path.Combine(Path.GetTempPath(),"gravivore-hitch-test-"+Guid.NewGuid().ToString("N"));
            try
            {
                diagnostic.Initialize(root,settings,directory); diagnostic.RecordFrame(.5f,1);
                Assert.That(diagnostic.WrittenSnapshotCount,Is.Zero,"Composition warmup is suppressed.");
                diagnostic.RecordFrame(.5f,6); diagnostic.RecordFrame(.7f,7);
                Assert.That(diagnostic.WrittenSnapshotCount,Is.EqualTo(1));
                for(var i=0;i<10;i++) diagnostic.RecordFrame(.4f,30+i*21);
                Assert.That(Directory.GetFiles(directory).Length,Is.EqualTo(settings.HitchSnapshots));
                var json=File.ReadAllText(diagnostic.LastSnapshotPath);
                foreach(var field in new[] { "sessionSeconds","recentFrameSeconds","activeEnemies","activeVfx","activeCombatText","audioSources","managedBytes","transforms","lineRenderers" })
                    StringAssert.Contains(field,json);
                Assert.That(new FileInfo(diagnostic.LastSnapshotPath).Length,Is.LessThan(12000));
            }
            finally { if(Directory.Exists(directory)) Directory.Delete(directory,true); }
        }
        private sealed class TestPull : IPullDestinationResolver { public Vector3 Resolve(Vector3 a,Vector3 b,float c) => a; }
        private sealed class TestLash : IGravityLashVfx { public void Play(Vector3 a,Vector3 b) {} }
        private sealed class TestSensor : ITargetSensor
        {
            private readonly ReadabilityTestTarget _target;
            public TestSensor(ReadabilityTestTarget target) => _target = target;
            public IReadOnlyList<TargetCandidate<CombatTarget>> Collect(Transform source,CombatFaction faction,TargetingParameters settings) =>
                new[] { new TargetCandidate<CombatTarget>(new CombatTarget(_target,_target,_target,null),2,1,_target.Alive) };
            public bool HasLineOfSight(Vector3 a,Vector3 b) => true;
        }
    }
}
