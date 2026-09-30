using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class PauseMenuPresenter : MonoBehaviour
    {
        private const string StatsDetailsKey = "gravivore.ui.statsExpanded";
        private HudModalController _modal;
        private PlayerStatsHudPresenter _statsPresenter;
        private RectTransform _root;
        private Button _detailsButton;

        public bool IsPaused => _root != null && _root.gameObject.activeSelf;
        public bool StatsDetailsExpanded { get; private set; }
        public RectTransform PauseButtonRect { get; private set; }
        public RectTransform ModalRect => _root;

        public void Initialize(
            RectTransform hudRoot,
            HudModalController modal,
            PlayerStatsHudPresenter statsPresenter)
        {
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));
            _modal = modal ?? throw new ArgumentNullException(nameof(modal));
            _statsPresenter = statsPresenter != null
                ? statsPresenter
                : throw new ArgumentNullException(nameof(statsPresenter));

            var pauseButton = HudUiFactory.CreateButton(
                hudRoot,
                "Pause Button",
                new Vector2(0.84f, 0.925f),
                new Vector2(0.97f, 0.985f),
                "II",
                Open);
            PauseButtonRect = pauseButton.GetComponent<RectTransform>();

            _root = HudUiFactory.CreatePanel(
                hudRoot,
                "Pause Modal",
                Vector2.zero,
                Vector2.one,
                HudUiFactory.ModalBackdropColor,
                true);
            var panel = HudUiFactory.CreatePanel(
                _root,
                "Pause Panel",
                new Vector2(0.12f, 0.27f),
                new Vector2(0.88f, 0.73f),
                HudUiFactory.PanelColor,
                true);
            HudUiFactory.CreateText(
                panel,
                "Title",
                new Vector2(0.1f, 0.72f),
                new Vector2(0.9f, 0.94f),
                "PAUSED",
                40,
                TextAnchor.MiddleCenter,
                Color.white);
            _detailsButton = HudUiFactory.CreateButton(
                panel,
                "Stats Details Button",
                new Vector2(0.12f, 0.4f),
                new Vector2(0.88f, 0.59f),
                string.Empty,
                ToggleStatsDetails);
            HudUiFactory.CreateButton(
                panel,
                "Resume Button",
                new Vector2(0.12f, 0.12f),
                new Vector2(0.88f, 0.31f),
                "RESUME",
                Resume);
            _root.gameObject.SetActive(false);

            StatsDetailsExpanded = PlayerPrefs.GetInt(StatsDetailsKey, 0) != 0;
            _statsPresenter.SetExpanded(StatsDetailsExpanded);
            ApplyDetailsLabel();
        }

        public void Open()
        {
            if (!_modal.TryOpen(this)) return;
            _root.SetAsLastSibling();
            _root.gameObject.SetActive(true);
        }

        public void Resume()
        {
            if (_root != null) _root.gameObject.SetActive(false);
            _modal?.Close(this);
        }

        public void ToggleStatsDetails()
        {
            StatsDetailsExpanded = !StatsDetailsExpanded;
            PlayerPrefs.SetInt(StatsDetailsKey, StatsDetailsExpanded ? 1 : 0);
            PlayerPrefs.Save();
            _statsPresenter.SetExpanded(StatsDetailsExpanded);
            ApplyDetailsLabel();
        }

        private void ApplyDetailsLabel()
        {
            var label = _detailsButton != null ? _detailsButton.GetComponentInChildren<Text>() : null;
            if (label != null)
            {
                label.text = $"STAT DETAILS: {(StatsDetailsExpanded ? "EXPANDED" : "COMPACT")}";
            }
        }

        private void OnDestroy() => Resume();
    }
}
