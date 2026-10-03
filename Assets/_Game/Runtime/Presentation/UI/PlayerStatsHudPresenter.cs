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
        private Text _feedbackText;
        private float _feedbackRemaining;

        public string DisplayText => string.Empty;
        public string RecentChangeText => _feedbackText != null ? _feedbackText.text : string.Empty;
        public bool IsExpanded => false;
        public bool HasPersistentStatsPanel => false;

        public void Initialize(PlayerStatsState stats, RectTransform hudRoot, bool expanded)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));

            _feedbackText = HudUiFactory.CreateText(
                hudRoot,
                "Stat Change Feedback",
                new Vector2(0.26f, 0.74f),
                new Vector2(0.74f, 0.8f),
                string.Empty,
                28,
                TextAnchor.MiddleCenter,
                HudUiFactory.AccentColor);
            _feedbackText.gameObject.SetActive(false);
            _stats.StatChanged += HandleStatChanged;
            ApplyState();
        }

        public void SetExpanded(bool expanded)
        {
            ApplyState();
        }

        public void ApplyState()
        {
            // Detailed values live in the paused game menu; gameplay shows only transient gains.
        }

        public void Shutdown()
        {
            if (_stats == null) return;
            _stats.StatChanged -= HandleStatChanged;
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
            if (change.CurrentLevel <= change.PreviousLevel) return;
            _feedbackText.text = RussianUiText.StatIncreased(change.Stat, change.CurrentLevel);
            _feedbackText.gameObject.SetActive(true);
            _feedbackRemaining = FeedbackDuration;
        }

        private void OnDestroy() => Shutdown();
    }
}
