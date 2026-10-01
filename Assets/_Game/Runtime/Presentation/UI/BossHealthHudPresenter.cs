using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class BossHealthHudPresenter : MonoBehaviour
    {
        private IBossHealthSource _boss;
        private BossCompletionState _completion;
        private PlayerHealthController _playerHealth;
        private RectTransform _panel;
        private HealthBarView _healthBar;
        private Text _label;

        public bool IsVisible => _panel != null && _panel.gameObject.activeSelf;
        public string DisplayText => _label != null ? _label.text : string.Empty;
        public float FillAmount => _healthBar != null ? _healthBar.NormalizedValue : 0f;
        public RectTransform FillRect => _healthBar?.FillRect;

        public void Initialize(
            IBossHealthSource boss,
            BossCompletionState completion,
            PlayerHealthController playerHealth,
            RectTransform hudRoot)
        {
            _boss = boss ?? throw new ArgumentNullException(nameof(boss));
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));
            _playerHealth = playerHealth != null ? playerHealth : throw new ArgumentNullException(nameof(playerHealth));
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));

            _panel = HudUiFactory.CreatePanel(
                hudRoot,
                "Boss Health",
                new Vector2(0.18f, 0.79f),
                new Vector2(0.82f, 0.85f),
                HudUiFactory.PanelColor);
            _healthBar = HealthBarView.Create(
                _panel,
                "Boss Health Track",
                "Boss Health Fill",
                new Vector2(0.03f, 0.15f),
                new Vector2(0.97f, 0.62f),
                new Color(0.12f, 0.14f, 0.15f, 1f),
                HudUiFactory.WarningColor);
            _label = HudUiFactory.CreateText(
                _panel,
                "Boss Health Label",
                new Vector2(0f, 0.55f),
                Vector2.one,
                RussianUiText.BossName,
                24,
                TextAnchor.MiddleCenter,
                Color.white);
            _panel.gameObject.SetActive(false);

            _boss.EncounterStarted += HandleEncounterStarted;
            _boss.Damaged += HandleDamaged;
            _boss.EncounterReset += HandleEncounterReset;
            _completion.Defeated += HandleDefeated;
            _playerHealth.Died += HandlePlayerDied;
        }

        public void ApplyState()
        {
            if (_boss == null) return;
            var maximum = _boss.MaximumHitPoints;
            _label.text = $"{RussianUiText.BossName}  {Mathf.Max(0f, _boss.CurrentHitPoints):0} / {Mathf.Max(0f, maximum):0}";
            _healthBar.SetNormalizedValue(
                maximum > 0f ? _boss.CurrentHitPoints / maximum : 0f);
        }

        public void Shutdown()
        {
            if (_boss == null) return;
            _boss.EncounterStarted -= HandleEncounterStarted;
            _boss.Damaged -= HandleDamaged;
            _boss.EncounterReset -= HandleEncounterReset;
            _completion.Defeated -= HandleDefeated;
            _playerHealth.Died -= HandlePlayerDied;
            _boss = null;
            _completion = null;
            _playerHealth = null;
        }

        private void HandleEncounterStarted(BossEncounterStartedEvent _)
        {
            if (_completion.IsDefeated) return;
            ApplyState();
            _panel.gameObject.SetActive(true);
        }

        private void HandleDamaged(DamageResult _) => ApplyState();
        private void HandleEncounterReset(BossEncounterResetEvent _) => _panel.gameObject.SetActive(false);
        private void HandleDefeated(BossDefeatedEvent _) => _panel.gameObject.SetActive(false);
        private void HandlePlayerDied(PlayerDeathEvent _) => _panel.gameObject.SetActive(false);
        private void OnDestroy() => Shutdown();
    }
}
