using System;
using Gravivore.Gameplay.Offline;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class OfflineRewardPanelPresenter : MonoBehaviour
    {
        private OfflineRewardService _service;
        private HudModalController _modal;
        private RectTransform _root;
        private Text _summaryText;
        private Text _balanceText;
        private OfflineReturnSummary _summary;
        private bool _wantsVisible;

        public bool IsVisible => _root != null && _root.gameObject.activeSelf;
        public string SummaryText => _summaryText != null ? _summaryText.text : string.Empty;
        public RectTransform ModalRect => _root;

        public void Initialize(
            OfflineRewardService service,
            OfflineReturnSummary startupSummary,
            RectTransform hudRoot,
            HudModalController modal)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _modal = modal ?? throw new ArgumentNullException(nameof(modal));
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));

            _root = HudUiFactory.CreatePanel(
                hudRoot,
                "Offline Reward Modal",
                Vector2.zero,
                Vector2.one,
                HudUiFactory.ModalBackdropColor,
                true);
            var panel = HudUiFactory.CreatePanel(
                _root,
                "Offline Reward Panel",
                new Vector2(0.12f, 0.3f),
                new Vector2(0.88f, 0.7f),
                HudUiFactory.PanelColor,
                true);
            HudUiFactory.CreateText(
                panel,
                "Title",
                new Vector2(0.08f, 0.72f),
                new Vector2(0.92f, 0.94f),
                RussianUiText.WelcomeBack,
                38,
                TextAnchor.MiddleCenter,
                Color.white);
            _summaryText = HudUiFactory.CreateText(
                panel,
                "Summary",
                new Vector2(0.08f, 0.43f),
                new Vector2(0.92f, 0.72f),
                string.Empty,
                30,
                TextAnchor.MiddleCenter,
                HudUiFactory.AccentColor);
            _balanceText = HudUiFactory.CreateText(
                panel,
                "Balance",
                new Vector2(0.08f, 0.28f),
                new Vector2(0.92f, 0.43f),
                string.Empty,
                25,
                TextAnchor.MiddleCenter,
                Color.white);
            HudUiFactory.CreateButton(
                panel,
                "Claim Button",
                new Vector2(0.2f, 0.07f),
                new Vector2(0.8f, 0.25f),
                RussianUiText.Claim,
                Claim);
            _root.gameObject.SetActive(false);

            _service.State.Changed += HandleStateChanged;
            _modal.Available += HandleModalAvailable;
            ShowReturnSummary(startupSummary);
        }

        public void ShowReturnSummary(OfflineReturnSummary summary)
        {
            if (summary.EligibleDuration <= TimeSpan.Zero && summary.EarnedAmount <= 0) return;
            _summary = summary;
            if (summary.TotalPendingAmount <= 0 || _service.State.PendingReward <= 0) return;
            _wantsVisible = true;
            TryShow();
        }

        public void Claim()
        {
            if (_service == null || !_service.ClaimPendingReward()) return;
            Close();
        }

        public void Shutdown()
        {
            if (_service == null) return;
            _service.State.Changed -= HandleStateChanged;
            _modal.Available -= HandleModalAvailable;
            Close();
            _service = null;
            _modal = null;
        }

        private void TryShow()
        {
            if (!_wantsVisible || !_modal.TryOpen(this)) return;
            _root.SetAsLastSibling();
            ApplyState();
            _root.gameObject.SetActive(true);
        }

        private void ApplyState()
        {
            _summaryText.text =
                RussianUiText.Away(HudTextFormatter.OfflineDuration(_summary.EligibleDuration)) + "\n" +
                RussianUiText.Recovered(_service.State.PendingReward);
            _balanceText.text = RussianUiText.StoredMaterial(_service.State.MaterialBalance);
        }

        private void Close()
        {
            _wantsVisible = false;
            if (_root != null) _root.gameObject.SetActive(false);
            _modal?.Close(this);
        }

        private void HandleStateChanged()
        {
            if (_service.State.PendingReward <= 0)
            {
                Close();
                return;
            }

            if (IsVisible) ApplyState();
        }

        private void HandleModalAvailable() => TryShow();
        private void OnDestroy() => Shutdown();
    }
}
