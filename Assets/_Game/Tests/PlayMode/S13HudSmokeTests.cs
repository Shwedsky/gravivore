using System;
using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Input;
using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class S13HudSmokeTests
    {
        [TearDown]
        public void RestoreTimeScale()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator HealthAndStatsHud_ReflectAuthoritativeChangesAndRespawn()
        {
            var root = new GameObject("S13 Player HUD Root", typeof(RectTransform));
            var stats = CreateStats();
            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(PlayerHealthController));
            playerObject.transform.SetParent(root.transform, false);
            var health = playerObject.GetComponent<PlayerHealthController>();
            health.Initialize(playerObject.GetComponent<CharacterController>(), stats, Vector3.zero, 0f);
            var healthHud = new GameObject("Health HUD", typeof(PlayerHealthHudPresenter))
                .GetComponent<PlayerHealthHudPresenter>();
            healthHud.Initialize(health, stats, root.GetComponent<RectTransform>());
            var statsHud = new GameObject("Stats HUD", typeof(PlayerStatsHudPresenter))
                .GetComponent<PlayerStatsHudPresenter>();
            statsHud.Initialize(stats, root.GetComponent<RectTransform>(), false);

            var initialMaximum = health.MaximumHitPoints;
            health.ApplyDamage(new DamageRequest(25f, DamageType.Physical));
            Assert.That(healthHud.FillAmount, Is.LessThan(1f));
            StringAssert.Contains(RussianUiText.Durability, healthHud.DisplayText);

            stats.SetLevel(PlayerStatType.Hull, 2);
            Assert.That(health.MaximumHitPoints, Is.GreaterThan(initialMaximum));
            StringAssert.Contains("Корпус  ур. 2", statsHud.DisplayText);
            Assert.That(statsHud.RecentChangeText, Is.EqualTo("Корпус: уровень 2"));

            health.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));
            Assert.That(healthHud.FillAmount, Is.EqualTo(1f).Within(0.001f));
            Assert.That(health.CurrentHitPoints, Is.EqualTo(health.MaximumHitPoints));

            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(healthHud.gameObject);
            UnityEngine.Object.Destroy(statsHud.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossHud_ShowsUpdatesAndHidesOnResetAndCompletion()
        {
            var root = new GameObject("S13 Boss HUD Root", typeof(RectTransform));
            var player = CreateHealth(root.transform, CreateStats());
            var boss = new FakeBossHealthSource(500f);
            var completion = new BossCompletionState("custodian-m0");
            var presenter = new GameObject("Boss HUD", typeof(BossHealthHudPresenter))
                .GetComponent<BossHealthHudPresenter>();
            presenter.Initialize(boss, completion, player, root.GetComponent<RectTransform>());

            Assert.IsFalse(presenter.IsVisible);
            boss.Start();
            Assert.IsTrue(presenter.IsVisible);
            boss.Damage(125f);
            Assert.That(presenter.FillAmount, Is.EqualTo(0.75f).Within(0.001f));
            StringAssert.Contains("375 / 500", presenter.DisplayText);
            StringAssert.Contains(RussianUiText.BossName, presenter.DisplayText);
            boss.Reset();
            Assert.IsFalse(presenter.IsVisible);
            boss.Start();
            completion.TryRecordDefeat("custodian-m0", Vector3.zero);
            Assert.IsFalse(presenter.IsVisible);

            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(presenter.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OfflinePanel_ClaimsThroughServiceAndClosesIdempotently()
        {
            var root = CreateModalRoot(out var input, out var modal);
            var state = new OfflineRewardState(10, 25);
            var service = new OfflineRewardService(
                new OfflineRewardConfiguration(100d, 0.5d, TimeSpan.FromHours(2)),
                state);
            var summary = new OfflineReturnSummary(
                TimeSpan.FromMinutes(10),
                TimeSpan.FromMinutes(10),
                25,
                25,
                false,
                OfflineClockAnomaly.None);
            var panel = new GameObject("Offline Panel", typeof(OfflineRewardPanelPresenter))
                .GetComponent<OfflineRewardPanelPresenter>();
            panel.Initialize(service, summary, root.GetComponent<RectTransform>(), modal);

            Assert.IsTrue(panel.IsVisible);
            Assert.IsFalse(input.enabled);
            StringAssert.Contains("25 материала", panel.SummaryText);
            panel.ShowReturnSummary(new OfflineReturnSummary(
                TimeSpan.Zero,
                TimeSpan.Zero,
                0,
                25,
                false,
                OfflineClockAnomaly.NonPositiveElapsed));
            var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            service.Accrue(now.AddMinutes(-10), now);
            StringAssert.Contains("ВНЕ ИГРЫ 10 мин", panel.SummaryText);
            var expectedBalance = state.MaterialBalance + state.PendingReward;
            panel.Claim();
            Assert.That(state.PendingReward, Is.Zero);
            Assert.That(state.MaterialBalance, Is.EqualTo(expectedBalance));
            Assert.IsFalse(panel.IsVisible);
            Assert.IsTrue(input.enabled);
            panel.Claim();
            Assert.That(state.MaterialBalance, Is.EqualTo(expectedBalance));

            panel.Shutdown();
            modal.Dispose();
            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(panel.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RestoredCompletionStaysHiddenWhileLiveDefeatOpens()
        {
            var root = CreateModalRoot(out _, out var modal);
            var stats = CreateStats();
            var restoredState = BossCompletionState.Restore(
                "custodian-m0",
                new BossCompletionSnapshot(true));
            var restored = new GameObject("Restored Completion", typeof(ChapterCompletionPresenter))
                .GetComponent<ChapterCompletionPresenter>();
            restored.Initialize(restoredState, stats, root.GetComponent<RectTransform>(), modal);
            Assert.IsFalse(restored.IsVisible);

            var liveState = new BossCompletionState("custodian-m0");
            var live = new GameObject("Live Completion", typeof(ChapterCompletionPresenter))
                .GetComponent<ChapterCompletionPresenter>();
            live.Initialize(liveState, stats, root.GetComponent<RectTransform>(), modal);
            liveState.TryRecordDefeat("custodian-m0", Vector3.zero);
            Assert.IsTrue(live.IsVisible);

            restored.Shutdown();
            live.Shutdown();
            modal.Dispose();
            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(restored.gameObject);
            UnityEngine.Object.Destroy(live.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ModalFreezesScaledGameplayAndSafelyResumesAllListeners()
        {
            var root = CreateModalRoot(out _, out var modal);
            var player = new GameObject("Paused Player Probe", typeof(ScaledGameplayProbe));
            var enemy = new GameObject("Paused Enemy Probe", typeof(ScaledGameplayProbe));
            var playerProbe = player.GetComponent<ScaledGameplayProbe>();
            var enemyProbe = enemy.GetComponent<ScaledGameplayProbe>();
            yield return null;
            var playerPosition = player.transform.position;
            var enemyPosition = enemy.transform.position;
            var playerState = playerProbe.ScaledState;
            var enemyState = enemyProbe.ScaledState;
            var attackCadence = enemyProbe.AttackCadence;
            var owner = new object();
            Time.timeScale = 0.5f;
            Assert.IsTrue(modal.TryOpen(owner));

            yield return null;
            yield return null;
            yield return null;

            Assert.That(player.transform.position, Is.EqualTo(playerPosition));
            Assert.That(enemy.transform.position, Is.EqualTo(enemyPosition));
            Assert.That(playerProbe.ScaledState, Is.EqualTo(playerState));
            Assert.That(enemyProbe.ScaledState, Is.EqualTo(enemyState));
            Assert.That(enemyProbe.AttackCadence, Is.EqualTo(attackCadence));

            var laterListenerCalled = false;
            modal.Available += () => throw new InvalidOperationException("modal listener failed");
            modal.Available += () => laterListenerCalled = true;
            LogAssert.Expect(LogType.Exception, "InvalidOperationException: modal listener failed");
            modal.Close(owner);
            Assert.That(Time.timeScale, Is.EqualTo(0.5f));
            Assert.IsTrue(laterListenerCalled);
            yield return null;
            yield return null;
            Assert.That(playerProbe.ScaledState, Is.GreaterThan(playerState));
            Assert.That(enemyProbe.AttackCadence, Is.GreaterThan(attackCadence));

            modal.Dispose();
            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(player);
            UnityEngine.Object.Destroy(enemy);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CompletionAndPause_SerializeModalsAndRestoreGameplayInput()
        {
            var root = CreateModalRoot(out var input, out var modal);
            var stats = CreateStats();
            var statsHud = new GameObject("Stats HUD", typeof(PlayerStatsHudPresenter))
                .GetComponent<PlayerStatsHudPresenter>();
            statsHud.Initialize(stats, root.GetComponent<RectTransform>(), false);
            var pause = new GameObject("Pause", typeof(PauseMenuPresenter)).GetComponent<PauseMenuPresenter>();
            pause.Initialize(root.GetComponent<RectTransform>(), modal, statsHud);
            var completionState = new BossCompletionState("custodian-m0");
            var completion = new GameObject("Completion", typeof(ChapterCompletionPresenter))
                .GetComponent<ChapterCompletionPresenter>();
            completion.Initialize(completionState, stats, root.GetComponent<RectTransform>(), modal);

            pause.Open();
            Assert.IsTrue(pause.IsPaused);
            Assert.IsFalse(input.enabled);
            Assert.That(Time.timeScale, Is.Zero);
            completionState.TryRecordDefeat("custodian-m0", Vector3.zero);
            Assert.IsFalse(completion.IsVisible, "Completion must wait for the active modal.");
            pause.Resume();
            Assert.IsTrue(completion.IsVisible);
            Assert.IsFalse(input.enabled);
            StringAssert.Contains("Мощность  ур. 1", completion.StatsText);
            completion.ContinueExploring();
            Assert.IsTrue(input.enabled);
            Assert.That(Time.timeScale, Is.EqualTo(1f));

            completion.Shutdown();
            pause.Resume();
            statsHud.Shutdown();
            modal.Dispose();
            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(statsHud.gameObject);
            UnityEngine.Object.Destroy(pause.gameObject);
            UnityEngine.Object.Destroy(completion.gameObject);
            yield return null;
        }

        private static GameObject CreateModalRoot(out FloatingJoystickInput input, out HudModalController modal)
        {
            var root = new GameObject("S13 Modal Root", typeof(RectTransform));
            input = new GameObject("Input", typeof(FloatingJoystickInput)).GetComponent<FloatingJoystickInput>();
            input.transform.SetParent(root.transform, false);
            modal = new HudModalController(input);
            return root;
        }

        private static PlayerHealthController CreateHealth(Transform parent, PlayerStatsState stats)
        {
            var gameObject = new GameObject("Player", typeof(CharacterController), typeof(PlayerHealthController));
            gameObject.transform.SetParent(parent, false);
            var health = gameObject.GetComponent<PlayerHealthController>();
            health.Initialize(gameObject.GetComponent<CharacterController>(), stats, Vector3.zero, 0f);
            return health;
        }

        private static PlayerStatsState CreateStats()
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 100f, 1f, 0f, 10f, 1000f),
                new StatCurve(10, 0f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                new StatCurve(10, 4f, 0f, 0f, 0f, 20f),
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private sealed class FakeBossHealthSource : IBossHealthSource
        {
            public FakeBossHealthSource(float maximum)
            {
                MaximumHitPoints = maximum;
                CurrentHitPoints = maximum;
            }

            public event Action<BossEncounterStartedEvent> EncounterStarted;
            public event Action<DamageResult> Damaged;
            public event Action<BossEncounterResetEvent> EncounterReset;

            public float CurrentHitPoints { get; private set; }
            public float MaximumHitPoints { get; }

            public void Start() => EncounterStarted?.Invoke(new BossEncounterStartedEvent("custodian-m0"));

            public void Damage(float amount)
            {
                CurrentHitPoints = Mathf.Max(0f, CurrentHitPoints - amount);
                Damaged?.Invoke(new DamageResult(amount, CurrentHitPoints <= 0f));
            }

            public void Reset()
            {
                CurrentHitPoints = MaximumHitPoints;
                EncounterReset?.Invoke(new BossEncounterResetEvent("custodian-m0", Vector3.zero));
            }
        }
    }

    public sealed class ScaledGameplayProbe : MonoBehaviour
    {
        public float ScaledState { get; private set; }
        public float AttackCadence { get; private set; }

        private void Update()
        {
            ScaledState += Time.deltaTime;
            AttackCadence += Time.deltaTime;
            transform.position += Vector3.forward * Time.deltaTime;
        }
    }
}
