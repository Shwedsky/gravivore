using System;
using Gravivore.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerStatsHudPresenter : MonoBehaviour
    {
        private const float FeedbackDuration = 2f;
        private PlayerStatsState _stats;
        private RectTransform _panel;
        private Text _statsText;
        private Text _feedbackText;
        private float _feedbackRemaining;
        private bool _expanded;

        public string DisplayText => _statsText != null ? _statsText.text : string.Empty;
        public string RecentChangeText => _feedbackText != null ? _feedbackText.text : string.Empty;
        public bool IsExpanded => _expanded;

        public void Initialize(PlayerStatsState stats, RectTransform hudRoot, bool expanded)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));

            _panel = HudUiFactory.CreatePanel(
                hudRoot,
                "Player Stats",
                new Vector2(0.03f, 0.58f),
                new Vector2(0.3f, 0.91f),
                HudUiFactory.PanelColor);
            _statsText = HudUiFactory.CreateText(
                _panel,
                "Stats Label",
                new Vector2(0.08f, 0.04f),
                new Vector2(0.92f, 0.96f),
                string.Empty,
                27,
                TextAnchor.UpperLeft,
                Color.white);
            _feedbackText = HudUiFactory.CreateText(
                hudRoot,
                "Stat Change Feedback",
                new Vector2(0.29f, 0.73f),
                new Vector2(0.78f, 0.79f),
                string.Empty,
                28,
                TextAnchor.MiddleCenter,
                HudUiFactory.AccentColor);
            _feedbackText.gameObject.SetActive(false);
            _stats.StatChanged += HandleStatChanged;
            _stats.DerivedStatsChanged += HandleDerivedStatsChanged;
            SetExpanded(expanded);
        }

        public void SetExpanded(bool expanded)
        {
            _expanded = expanded;
            ApplyState();
        }

        public void ApplyState()
        {
            if (_stats == null) return;
            _statsText.text = HudTextFormatter.Stats(_stats.BaseLevels, _stats.DerivedStats, _expanded);
            _panel.anchorMin = new Vector2(0.03f, _expanded ? 0.49f : 0.58f);
        }

        public void Shutdown()
        {
            if (_stats == null) return;
            _stats.StatChanged -= HandleStatChanged;
            _stats.DerivedStatsChanged -= HandleDerivedStatsChanged;
            _stats = null;
        }

        private void Update()
        {
            if (_feedbackText == null || _feedbackRemaining <= 0f) return;
            _feedbackRemaining -= Time.unscaledDeltaTime;
            if (_feedbackRemaining <= 0f) _feedbackText.gameObject.SetActive(false);
        }

        private void HandleStatChanged(PlayerStatChange change)
        {
            ApplyState();
            if (change.CurrentLevel <= change.PreviousLevel) return;
            _feedbackText.text = $"{change.Stat} increased to L{change.CurrentLevel}";
            _feedbackText.gameObject.SetActive(true);
            _feedbackRemaining = FeedbackDuration;
        }

        private void HandleDerivedStatsChanged(PlayerDerivedStatsChange _) => ApplyState();
        private void OnDestroy() => Shutdown();
    }
}
