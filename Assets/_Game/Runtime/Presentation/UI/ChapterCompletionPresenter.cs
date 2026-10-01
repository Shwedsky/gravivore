using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class ChapterCompletionPresenter : MonoBehaviour
    {
        private BossCompletionState _completion;
        private PlayerStatsState _stats;
        private HudModalController _modal;
        private RectTransform _root;
        private Text _statsText;
        private bool _wantsVisible;

        public bool IsVisible => _root != null && _root.gameObject.activeSelf;
        public string StatsText => _statsText != null ? _statsText.text : string.Empty;
        public RectTransform ModalRect => _root;

        public void Initialize(
            BossCompletionState completion,
            PlayerStatsState stats,
            RectTransform hudRoot,
            HudModalController modal)
        {
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _modal = modal ?? throw new ArgumentNullException(nameof(modal));
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));

            _root = HudUiFactory.CreatePanel(
                hudRoot,
                "Chapter Completion Modal",
                Vector2.zero,
                Vector2.one,
                HudUiFactory.ModalBackdropColor,
                true);
            var panel = HudUiFactory.CreatePanel(
                _root,
                "Chapter Completion Panel",
                new Vector2(0.1f, 0.2f),
                new Vector2(0.9f, 0.8f),
                HudUiFactory.PanelColor,
                true);
            HudUiFactory.CreateText(
                panel,
                "Title",
                new Vector2(0.06f, 0.76f),
                new Vector2(0.94f, 0.94f),
                RussianUiText.ChapterComplete,
                38,
                TextAnchor.MiddleCenter,
                HudUiFactory.AccentColor);
            HudUiFactory.CreateText(
                panel,
                "Message",
                new Vector2(0.08f, 0.62f),
                new Vector2(0.92f, 0.76f),
                RussianUiText.BossDefeated,
                29,
                TextAnchor.MiddleCenter,
                Color.white);
            _statsText = HudUiFactory.CreateText(
                panel,
                "Final Stats",
                new Vector2(0.18f, 0.25f),
                new Vector2(0.82f, 0.62f),
                string.Empty,
                27,
                TextAnchor.UpperLeft,
                Color.white);
            HudUiFactory.CreateButton(
                panel,
                "Continue Button",
                new Vector2(0.15f, 0.07f),
                new Vector2(0.85f, 0.2f),
                RussianUiText.ContinueExploring,
                ContinueExploring);
            _root.gameObject.SetActive(false);

            _completion.Defeated += HandleDefeated;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _completion.DevelopmentReset += HandleDevelopmentReset;
#endif
            _stats.StatChanged += HandleStatChanged;
            _modal.Available += HandleModalAvailable;
        }

        public void ContinueExploring()
        {
            _wantsVisible = false;
            if (_root != null) _root.gameObject.SetActive(false);
            _modal?.Close(this);
        }

        public void Shutdown()
        {
            if (_completion == null) return;
            _completion.Defeated -= HandleDefeated;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _completion.DevelopmentReset -= HandleDevelopmentReset;
#endif
            _stats.StatChanged -= HandleStatChanged;
            _modal.Available -= HandleModalAvailable;
            ContinueExploring();
            _completion = null;
            _stats = null;
            _modal = null;
        }

        private void RequestShow()
        {
            _wantsVisible = true;
            TryShow();
        }

        private void TryShow()
        {
            if (!_wantsVisible || !_modal.TryOpen(this)) return;
            ApplyState();
            _root.SetAsLastSibling();
            _root.gameObject.SetActive(true);
        }

        private void ApplyState()
        {
            _statsText.text = HudTextFormatter.Stats(_stats.BaseLevels, _stats.DerivedStats, false);
        }

        private void HandleDefeated(BossDefeatedEvent _) => RequestShow();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void HandleDevelopmentReset() => ContinueExploring();
#endif
        private void HandleStatChanged(PlayerStatChange _)
        {
            if (IsVisible) ApplyState();
        }

        private void HandleModalAvailable() => TryShow();
        private void OnDestroy() => Shutdown();
    }
}
