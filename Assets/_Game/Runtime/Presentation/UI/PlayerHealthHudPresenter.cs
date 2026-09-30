using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHudPresenter : MonoBehaviour
    {
        private PlayerHealthController _health;
        private PlayerStatsState _stats;
        private Image _fill;
        private Text _label;

        public string DisplayText => _label != null ? _label.text : string.Empty;
        public float FillAmount => _fill != null ? _fill.fillAmount : 0f;

        public void Initialize(PlayerHealthController health, PlayerStatsState stats, RectTransform hudRoot)
        {
            _health = health != null ? health : throw new ArgumentNullException(nameof(health));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));

            var panel = HudUiFactory.CreatePanel(
                hudRoot,
                "Player Health",
                new Vector2(0.03f, 0.925f),
                new Vector2(0.43f, 0.975f),
                HudUiFactory.PanelColor);
            var track = HudUiFactory.CreatePanel(
                panel,
                "Health Track",
                new Vector2(0.04f, 0.2f),
                new Vector2(0.96f, 0.8f),
                new Color(0.12f, 0.14f, 0.15f, 1f));
            var fillRect = HudUiFactory.CreatePanel(
                track,
                "Health Fill",
                Vector2.zero,
                Vector2.one,
                HudUiFactory.WarningColor);
            _fill = fillRect.GetComponent<Image>();
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _label = HudUiFactory.CreateText(
                panel,
                "Health Label",
                Vector2.zero,
                Vector2.one,
                string.Empty,
                26,
                TextAnchor.MiddleCenter,
                Color.white);

            _health.Damaged += HandleDamaged;
            _health.Died += HandleDied;
            _health.Respawned += HandleRespawned;
            _stats.DerivedStatsChanged += HandleDerivedStatsChanged;
            ApplyState();
        }

        public void ApplyState()
        {
            if (_health == null) return;
            var maximum = _health.MaximumHitPoints;
            _label.text = HudTextFormatter.Health(_health.CurrentHitPoints, maximum);
            _fill.fillAmount = maximum > 0f ? Mathf.Clamp01(_health.CurrentHitPoints / maximum) : 0f;
        }

        public void Shutdown()
        {
            if (_health == null) return;
            _health.Damaged -= HandleDamaged;
            _health.Died -= HandleDied;
            _health.Respawned -= HandleRespawned;
            _stats.DerivedStatsChanged -= HandleDerivedStatsChanged;
            _health = null;
            _stats = null;
        }

        private void HandleDamaged(DamageResult _) => ApplyState();
        private void HandleDied(PlayerDeathEvent _) => ApplyState();
        private void HandleRespawned(PlayerRespawnEvent _) => ApplyState();
        private void HandleDerivedStatsChanged(PlayerDerivedStatsChange _) => ApplyState();
        private void OnDestroy() => Shutdown();
    }
}
