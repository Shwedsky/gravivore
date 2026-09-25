using System.Collections;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Input;
using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gravivore.Tests.PlayMode
{
    public sealed class PlayerMovementSmokeTests
    {
        [UnityTest]
        public IEnumerator ChapterScene_ComposesPlayerCameraAndSafeAreaHud()
        {
            var loadOperation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation);
            yield return loadOperation;
            yield return null;

            S01SceneCompositionRoot compositionRoot = null;
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].TryGetComponent(out compositionRoot))
                {
                    break;
                }
            }

            Assert.IsNotNull(compositionRoot);
            Assert.IsNotNull(compositionRoot.PlayerObject);
            Assert.IsNotNull(compositionRoot.PlayerStats);
            Assert.That(compositionRoot.PlayerStats.MoveSpeed, Is.EqualTo(4.5f).Within(0.0001f));
            Assert.IsNotNull(compositionRoot.PlayerHealth);
            Assert.That(
                compositionRoot.PlayerHealth.MaximumHitPoints,
                Is.EqualTo(compositionRoot.PlayerStats.DerivedStats.MaxHp));
            Assert.That(
                compositionRoot.PlayerHealth.Armor,
                Is.EqualTo(compositionRoot.PlayerStats.DerivedStats.ArmorValue));
            Assert.IsNotNull(compositionRoot.EnemyPopulation);
            Assert.That(compositionRoot.EnemyPopulation.SpotCount, Is.EqualTo(5));
            Assert.IsNotNull(compositionRoot.PlayerObject.GetComponent<GravityAttackController>());
            Assert.IsNotNull(UnityEngine.Camera.main);
            Assert.IsNotNull(compositionRoot.GetComponentInChildren<SafeAreaHudRoot>());

            var canvasScaler = compositionRoot.GetComponentInChildren<CanvasScaler>();
            Assert.IsNotNull(canvasScaler);
            Assert.That(canvasScaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(canvasScaler.referenceResolution, Is.EqualTo(new Vector2(1080f, 1920f)));

            var joystickView = compositionRoot.GetComponentInChildren<FloatingJoystickView>(true);
            Assert.IsNotNull(joystickView);
            var joystickImages = joystickView.GetComponentsInChildren<Image>(true);
            Assert.That(joystickImages, Has.Length.EqualTo(2));
            for (var i = 0; i < joystickImages.Length; i++)
            {
                Assert.IsFalse(joystickImages[i].raycastTarget);
            }

            Assert.IsNotNull(compositionRoot.GetComponent<UiTouchExclusion>());

            compositionRoot.PlayerStats.SetLevel(PlayerStatType.Hull, 2);
            compositionRoot.PlayerStats.SetLevel(PlayerStatType.Armor, 2);
            Assert.That(
                compositionRoot.PlayerHealth.MaximumHitPoints,
                Is.EqualTo(compositionRoot.PlayerStats.DerivedStats.MaxHp));
            Assert.That(
                compositionRoot.PlayerHealth.Armor,
                Is.EqualTo(compositionRoot.PlayerStats.DerivedStats.ArmorValue));
        }

        [UnityTest]
        public IEnumerator GravityAttack_AcquiresDamagesPullsAndReleasesDeadTarget()
        {
            var loadOperation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation);
            yield return loadOperation;
            yield return null;

            S01SceneCompositionRoot compositionRoot = null;
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].TryGetComponent(out compositionRoot))
                {
                    break;
                }
            }

            Assert.IsNotNull(compositionRoot);
            var attackController = compositionRoot.PlayerObject.GetComponent<GravityAttackController>();
            Assert.IsNotNull(attackController);

            var targetObject = new GameObject(
                "Gravity Attack Smoke Target",
                typeof(SphereCollider),
                typeof(FakeCombatTarget));
            targetObject.layer = 9;
            targetObject.transform.position = new Vector3(0f, 0f, 3f);
            var target = targetObject.GetComponent<FakeCombatTarget>();
            var aimPoint = new GameObject("Offset Aim Point").transform;
            aimPoint.SetParent(targetObject.transform, false);
            aimPoint.localPosition = new Vector3(0f, 1.25f, 0f);
            target.AimPoint = aimPoint;
            Physics.SyncTransforms();

            var timeout = Time.realtimeSinceStartup + 2f;
            while (target.DamageCount == 0 && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(target.DamageCount, Is.GreaterThan(0));
            Assert.That(target.LastDamage.RawDamage, Is.EqualTo(compositionRoot.PlayerStats.DerivedStats.BaseDamage));
            Assert.That(targetObject.transform.position.z, Is.EqualTo(1.2f).Within(0.0001f));
            Assert.That(targetObject.transform.position.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(target.TargetPoint.position.y, Is.EqualTo(1.25f).Within(0.0001f));
            Assert.IsTrue(attackController.HasCurrentTarget);

            target.IsAlive = false;
            timeout = Time.realtimeSinceStartup + 0.5f;
            while (attackController.HasCurrentTarget && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.IsFalse(attackController.HasCurrentTarget);

            Object.Destroy(targetObject);
        }

        [UnityTest]
        public IEnumerator GravityAttack_LethalResultClearsPooledIdentityBeforeDisplacement()
        {
            var loadOperation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation);
            yield return loadOperation;
            yield return null;

            S01SceneCompositionRoot compositionRoot = null;
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].TryGetComponent(out compositionRoot))
                {
                    break;
                }
            }

            Assert.IsNotNull(compositionRoot);
            var attackController = compositionRoot.PlayerObject.GetComponent<GravityAttackController>();
            var targetObject = new GameObject(
                "Lethal Pool Identity Target",
                typeof(SphereCollider),
                typeof(FakeCombatTarget));
            targetObject.layer = 9;
            targetObject.transform.position = new Vector3(0f, 0f, 3f);
            var target = targetObject.GetComponent<FakeCombatTarget>();
            target.ReturnLethalWhileRemainingAlive = true;
            Physics.SyncTransforms();

            var timeout = Time.realtimeSinceStartup + 2f;
            while (target.DamageCount == 0 && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(target.DamageCount, Is.EqualTo(1));
            Assert.That(target.DisplacementCount, Is.Zero);
            Assert.IsFalse(attackController.HasCurrentTarget);

            Object.Destroy(targetObject);
        }

        [UnityTest]
        public IEnumerator PullDestinationResolver_StopsTargetBeforeHardBlocker()
        {
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "Hard Blocker Smoke Wall";
            blocker.layer = 8;
            blocker.transform.position = new Vector3(0f, 0f, 1.5f);
            blocker.transform.localScale = new Vector3(2f, 2f, 0.2f);
            Physics.SyncTransforms();

            var resolver = new PhysicsPullDestinationResolver(1 << 8, 0.05f);
            var resolved = resolver.Resolve(
                new Vector3(0f, 0f, 3f),
                Vector3.zero,
                0.4f);

            Assert.That(resolved.z, Is.GreaterThanOrEqualTo(2.04f));
            Assert.That(resolved.z, Is.LessThan(3f));

            Object.Destroy(blocker);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerLocomotion_MovesSpawnedPlayerFromInjectedInput()
        {
            var player = new GameObject("Movement Smoke Player", typeof(CharacterController), typeof(PlayerLocomotion));
            var cameraBasis = new GameObject("Movement Smoke Camera Basis");
            player.transform.position = new Vector3(10f, 0f, 10f);
            var input = new StubMovementInput();
            var locomotion = player.GetComponent<PlayerLocomotion>();
            locomotion.Initialize(input, cameraBasis.transform, new PlayerMovementParameters(4f, 720f));
            var initialPosition = player.transform.position;

            yield return null;

            Assert.That(Vector3.Distance(player.transform.position, initialPosition), Is.LessThan(0.001f));

            input.Movement = Vector2.up;
            yield return null;

            Assert.That(player.transform.position.z, Is.GreaterThan(initialPosition.z));
            Assert.That(player.transform.position.x, Is.EqualTo(initialPosition.x).Within(0.001f));

            Object.Destroy(player);
            Object.Destroy(cameraBasis);
        }

        private sealed class StubMovementInput : IMovementInput
        {
            public Vector2 Movement { get; set; }
        }

        private sealed class FakeCombatTarget : MonoBehaviour, ITargetable, IDamageable, IDisplaceable
        {
            public Transform AimPoint { get; set; }

            public Transform TargetPoint => AimPoint != null ? AimPoint : transform;

            public Transform DisplacementRoot => transform;

            public bool CanBeTargeted => true;

            public bool IsAlive { get; set; } = true;

            public DisplacementClass DisplacementClass => DisplacementClass.Standard;

            public float CollisionRadius => 0.4f;

            public int DamageCount { get; private set; }

            public int DisplacementCount { get; private set; }

            public bool ReturnLethalWhileRemainingAlive { get; set; }

            public DamageRequest LastDamage { get; private set; }

            public bool IsHostileTo(CombatFaction faction)
            {
                return faction == CombatFaction.Player;
            }

            public DamageResult ApplyDamage(in DamageRequest request)
            {
                DamageCount++;
                LastDamage = request;
                return new DamageResult(request.RawDamage, ReturnLethalWhileRemainingAlive);
            }

            public bool TryDisplace(Vector3 destination, in DisplacementContext context)
            {
                DisplacementCount++;
                transform.position = destination;
                return true;
            }
        }
    }
}
