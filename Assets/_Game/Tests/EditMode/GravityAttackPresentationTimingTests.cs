using System;
using System.Collections.Generic;
using System.Reflection;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.EditMode
{
    public sealed class GravityAttackPresentationTimingTests
    {
        private GameObject _player, _targetObject;
        private GravityAttackSettings _settings;
        private GravityAttackController _attack;
        private TimingTarget _target;
        private Sensor _sensor;
        private Presentation _presentation;
        private PlayerStatsState _stats;
        private readonly List<string> _order = new List<string>();

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Timing Player", typeof(GravityAttackController));
            _targetObject = new GameObject("Timing Target", typeof(TimingTarget));
            _targetObject.transform.position = Vector3.forward * 3;
            _target = _targetObject.GetComponent<TimingTarget>();
            _target.OnDamage = () => _order.Add("damage");
            _settings = ScriptableObject.CreateInstance<GravityAttackSettings>();
            typeof(GravityAttackSettings).GetField("_targetLayers", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_settings, (LayerMask)(1 << 9));
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 2f, 0f, 0f, 1000f),
                new StatCurve(10, 100f, 1f, 0f, 1f, 1000f),
                new StatCurve(10, 0f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, -.1f, 0f, .1f, 10f),
                new StatCurve(10, 4f, 0f, 0f, 0f, 20f),
                .1f, 20f, new PlayerStatLevels(1, 1, 1, 1, 1));
            _stats = new PlayerStatsState(configuration, configuration.StartingLevels);
            _sensor = new Sensor { Target = _target };
            _presentation = new Presentation(_order);
            _attack = _player.GetComponent<GravityAttackController>();
            _attack.enabled = false;
            _attack.Initialize(_player.transform, _stats, _settings, _sensor, new Resolver(), _presentation);
        }

        [TearDown]
        public void TearDown()
        {
            if (_attack != null) _attack.ResetTransientState();
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_targetObject);
            Object.DestroyImmediate(_settings);
            _order.Clear();
        }

        [Test]
        public void PrechargeUsesExistingFinalIntervalAndOnlyGameplayTickCommitsDamage()
        {
            _attack.Tick(0);
            _order.Clear();
            _attack.Tick(.86f);
            Assert.That(_order, Is.EqualTo(new[] { "charge" }));
            Assert.That(_presentation.LastChargeDuration, Is.EqualTo(.14f).Within(.0001f));
            Assert.That(_target.DamageCount, Is.EqualTo(1));
            _attack.Tick(.1f);
            Assert.That(_target.DamageCount, Is.EqualTo(1));
            _attack.Tick(.05f);
            Assert.That(_order, Is.EqualTo(new[] { "charge", "release", "damage" }));
            Assert.That(_target.DamageCount, Is.EqualTo(2));
        }

        [Test]
        public void FirstAttackAndReacquisitionReleaseImmediatelyWithoutGameplayDelay()
        {
            _attack.Tick(0);
            Assert.That(_order, Is.EqualTo(new[] { "release", "damage" }));
            _sensor.Target = null;
            _attack.Tick(.2f);
            _sensor.Target = _target;
            _attack.Tick(.1f);
            Assert.That(_target.DamageCount, Is.EqualTo(2));
            Assert.That(_presentation.Charges, Is.Zero);
        }

        [Test]
        public void LargeFrameCrossingChargeWindowCommitsDirectlyAtOriginalCadence()
        {
            _attack.Tick(0);
            _attack.Tick(1.1f);
            Assert.That(_target.DamageCount, Is.EqualTo(2));
            Assert.That(_presentation.Charges, Is.Zero);
        }

        [TestCase(0f)]
        [TestCase(.15f)]
        [TestCase(3f)]
        public void PresentationDurationDoesNotChangeCadenceDamageOrStatScaling(float chargeDuration)
        {
            _presentation.ChargeDuration = chargeDuration;
            var reference = new AttackCadenceTimer();
            var expectedCount = 0;
            var expectedDamage = 0f;
            for (var i = 0; i < 240; i++)
            {
                if (i == 100) { _stats.SetLevel(PlayerStatType.Flux, 3); _stats.SetLevel(PlayerStatType.Power, 2); }
                var delta = i % 7 == 0 ? .21f : .031f;
                if (reference.Advance(delta, true, _stats.DerivedStats.AttackInterval))
                {
                    expectedCount++;
                    expectedDamage += _stats.DerivedStats.BaseDamage;
                }
                _attack.Tick(delta);
                Assert.That(_target.DamageCount, Is.EqualTo(expectedCount), "Commit frame " + i);
                Assert.That(_target.TotalDamage, Is.EqualTo(expectedDamage));
            }
        }

        [TestCase("charge")]
        [TestCase("release")]
        [TestCase("cancel")]
        public void PresentationFailureCannotBlockCombat(string stage)
        {
            _attack.Tick(0);
            _presentation.ThrowOn = stage == "release" ? null : stage;
            if (stage == "charge") LogAssert.Expect(LogType.Exception, "InvalidOperationException: charge failed");
            _attack.Tick(.86f);
            if (stage == "cancel")
            {
                _target.IsAlive = false;
                LogAssert.Expect(LogType.Exception, "InvalidOperationException: cancel failed");
                _attack.Tick(.1f);
                _target.IsAlive = true;
                _presentation.ThrowOn = null;
                _attack.Tick(.1f);
            }
            else
            {
                _presentation.ThrowOn = stage == "release" ? stage : null;
                if (stage == "release") LogAssert.Expect(LogType.Exception, "InvalidOperationException: release failed");
                _attack.Tick(.15f);
            }
            Assert.That(_target.DamageCount, Is.EqualTo(2));
        }

        [TestCase("dead")]
        [TestCase("disabled")]
        [TestCase("range")]
        [TestCase("line-of-sight")]
        public void TargetLossCancelsChargeWithoutStaleReleaseOrDamage(string loss)
        {
            _attack.Tick(0); _attack.Tick(.86f); _order.Clear();
            if (loss == "dead") _target.IsAlive = false;
            if (loss == "disabled") _target.enabled = false;
            if (loss == "range") _target.transform.position = Vector3.forward * 20;
            if (loss == "line-of-sight") _sensor.Visible = false;
            _attack.Tick(.2f);
            Assert.That(_order, Is.EqualTo(new[] { "cancel" }));
            Assert.That(_target.DamageCount, Is.EqualTo(1));
            Assert.IsFalse(_attack.HasCurrentTarget);
        }

        [Test]
        public void TargetSwitchCancelsOldChargeAndShortensNewChargeWithoutMovingCommit()
        {
            _attack.Tick(0); _attack.Tick(.86f);
            var otherObject = new GameObject("Switched Timing Target", typeof(TimingTarget));
            try
            {
                var other = otherObject.GetComponent<TimingTarget>();
                other.transform.position = Vector3.forward;
                other.OnDamage = () => _order.Add("new damage");
                _sensor.Target = other; _order.Clear();
                _attack.Tick(.1f);
                Assert.That(_order, Is.EqualTo(new[] { "cancel", "charge" }));
                Assert.That(_presentation.LastChargeDuration, Is.EqualTo(.04f).Within(.0001f));
                _attack.Tick(.05f);
                Assert.That(other.DamageCount, Is.EqualTo(1));
                Assert.That(_target.DamageCount, Is.EqualTo(1));
                Assert.That(_order, Is.EqualTo(new[] { "cancel", "charge", "release", "new damage" }));
            }
            finally { Object.DestroyImmediate(otherObject); }
        }

        [Test]
        public void ResetCancelsChargeAndKeepsNextAttackImmediate()
        {
            _attack.Tick(0); _attack.Tick(.86f);
            _attack.ResetTransientState(); _order.Clear();
            _attack.Tick(0);
            Assert.That(_order, Is.EqualTo(new[] { "release", "damage" }));
        }

        private sealed class TimingTarget : MonoBehaviour, ITargetable, IDamageable
        {
            public Transform TargetPoint => transform;
            public bool CanBeTargeted => IsAlive;
            public bool IsAlive { get; set; } = true;
            public int DamageCount;
            public float TotalDamage;
            public Action OnDamage;
            public bool IsHostileTo(CombatFaction faction) => faction == CombatFaction.Player;
            public DamageResult ApplyDamage(in DamageRequest request)
            {
                DamageCount++; TotalDamage += request.RawDamage; OnDamage();
                return new DamageResult(request.RawDamage, false);
            }
        }

        private sealed class Sensor : ITargetSensor
        {
            public TimingTarget Target;
            public bool Visible = true;
            public bool HasLineOfSight(Vector3 origin, Vector3 destination) => Visible;
            public IReadOnlyList<TargetCandidate<CombatTarget>> Collect(Transform source, CombatFaction faction, TargetingParameters parameters)
            {
                if (Target == null) return Array.Empty<TargetCandidate<CombatTarget>>();
                var target = new CombatTarget(Target, Target, Target, null);
                return new[] { new TargetCandidate<CombatTarget>(target,
                    Vector3.Distance(source.position, Target.transform.position), 1f, Visible && target.IsValidFor(faction)) };
            }
        }

        private sealed class Resolver : IPullDestinationResolver
        {
            public Vector3 Resolve(Vector3 origin, Vector3 destination, float radius) => destination;
        }

        private sealed class Presentation : IPrechargedGravityLashVfx
        {
            private readonly List<string> _order;
            public float ChargeDuration { get; set; } = .15f;
            public float LastChargeDuration;
            public int Charges;
            public string ThrowOn;
            public Presentation(List<string> order) => _order = order;
            public void BeginCharge(Vector3 origin, Vector3 destination, ITargetable target, float remainingUntilCommit)
            { LastChargeDuration = remainingUntilCommit; Charges++; Emit("charge"); }
            public void CancelCharge() => Emit("cancel");
            public void Play(Vector3 origin, Vector3 destination) => Emit("release");
            public void Play(Vector3 origin, Vector3 destination, ITargetable target) => Emit("release");
            private void Emit(string stage)
            {
                _order.Add(stage);
                if (ThrowOn == stage) throw new InvalidOperationException(stage + " failed");
            }
        }
    }
}
