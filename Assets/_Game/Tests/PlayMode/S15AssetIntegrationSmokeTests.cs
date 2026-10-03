using System.Collections;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class S15AssetIntegrationSmokeTests
    {
        [UnityTest]
        public IEnumerator CanonicalScene_BuildsPlayerAndEnvironmentFromAuthoredMeshes()
        {
            var scene = new CanonicalSceneTestScope();
            yield return scene.Load();

            var playerVisual = scene.Root.PlayerObject.transform.Find("Player Visual Root/S15 Visual [player-techno-organism]");
            if (playerVisual == null)
                playerVisual = scene.Root.PlayerObject.transform.Find("Player Visual Root/G0_Tier0_ArtSpike");
            Assert.IsNotNull(playerVisual);
            Assert.That(playerVisual.GetComponentsInChildren<MeshRenderer>(true), Has.Length.GreaterThanOrEqualTo(3));
            Assert.IsNotNull(GameObject.Find("Landmark relay-yard/S15 Visual [relay-yard]"));
            Assert.IsNotNull(GameObject.Find("Landmark hauler-graveyard/S15 Visual [hauler-graveyard]"));

            yield return scene.Cleanup();
        }

        [UnityTest]
        public IEnumerator EnemyPool_ResetsAndReappliesVisualStateAcrossArchetypes()
        {
            var root = new GameObject("S15 Pool Visual Test");
            var targetObject = new GameObject("S15 Pool Target");
            var factory = new RecordingVisualFactory();
            var pool = new OrdinaryEnemyPool(root.transform, 1, 9, TestMaterialFactory.Lit, factory);
            var target = new RecordingDamageable();

            var first = pool.Acquire(Configuration("scout-drone"), targetObject.transform, target, Vector3.zero, pool.Return);
            Assert.That(factory.State.LastAppliedId, Is.EqualTo("scout-drone"));
            pool.Return(first);
            Assert.That(factory.State.ResetCount, Is.EqualTo(2));

            var second = pool.Acquire(Configuration("carrier"), targetObject.transform, target, Vector3.one, pool.Return);
            Assert.AreSame(first, second);
            Assert.That(factory.State.LastAppliedId, Is.EqualTo("carrier"));
            pool.Return(second);
            Assert.That(factory.State.ResetCount, Is.EqualTo(3));

            Object.Destroy(root);
            Object.Destroy(targetObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyPool_ActivationFailureRestoresAccounting()
        {
            var root = new GameObject("S15 Pool Failure Test");
            var targetObject = new GameObject("S15 Pool Failure Target");
            var pool = new OrdinaryEnemyPool(
                root.transform,
                1,
                9,
                TestMaterialFactory.Lit,
                new ThrowingVisualFactory());

            Assert.Throws<System.InvalidOperationException>(() => pool.Acquire(
                Configuration("missing-visual"),
                targetObject.transform,
                new RecordingDamageable(),
                Vector3.zero,
                pool.Return));
            Assert.That(pool.AvailableCount, Is.EqualTo(1));
            Assert.That(pool.LeasedCount, Is.Zero);

            Object.Destroy(root);
            Object.Destroy(targetObject);
            yield return null;
        }

        private static EnemyRuntimeConfiguration Configuration(string id)
        {
            return new EnemyRuntimeConfiguration(id, 10f, 1f, 1f, 0.4f, 0.8f,
                new EnemyBehaviorParameters(2f, 3f, 1f, 1f));
        }

        private sealed class RecordingVisualFactory : IEnemyVisualFactory
        {
            public readonly RecordingVisualState State = new RecordingVisualState();
            public IEnemyVisualState Create(Transform parent) => State;
        }

        private sealed class RecordingVisualState : IEnemyVisualState
        {
            public string LastAppliedId { get; private set; }
            public int ResetCount { get; private set; }
            public void Apply(string enemyId) => LastAppliedId = enemyId;
            public void Reset() { LastAppliedId = null; ResetCount++; }
        }

        private sealed class RecordingDamageable : IDamageable
        {
            public bool IsAlive => true;
            public DamageResult ApplyDamage(in DamageRequest request) => new DamageResult(request.RawDamage, false);
        }

        private sealed class ThrowingVisualFactory : IEnemyVisualFactory
        {
            public IEnemyVisualState Create(Transform parent) => new ThrowingVisualState();
        }

        private sealed class ThrowingVisualState : IEnemyVisualState
        {
            public void Apply(string enemyId) => throw new System.InvalidOperationException("visual failed");
            public void Reset() { }
        }
    }
}
