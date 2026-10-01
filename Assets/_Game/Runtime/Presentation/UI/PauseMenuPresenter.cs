using System;
using Gravivore.Presentation.Feedback;
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
        private Button _audioButton;
        private S14AudioPresenter _audio;

        public bool IsPaused => _root != null && _root.gameObject.activeSelf;
        public bool StatsDetailsExpanded { get; private set; }
        public RectTransform PauseButtonRect { get; private set; }
        public RectTransform ModalRect => _root;
        public bool AudioMuted => _audio != null ? _audio.IsMuted : PresentationAudioSettings.IsMuted;
        public float AudioVolume => _audio != null ? _audio.Volume : PresentationAudioSettings.Volume;

        public void Initialize(
            RectTransform hudRoot,
            HudModalController modal,
            PlayerStatsHudPresenter statsPresenter,
            S14AudioPresenter audio = null)
        {
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));
            _modal = modal ?? throw new ArgumentNullException(nameof(modal));
            _statsPresenter = statsPresenter != null
                ? statsPresenter
                : throw new ArgumentNullException(nameof(statsPresenter));
            _audio = audio;

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
                new Vector2(0.12f, 0.48f),
                new Vector2(0.88f, 0.63f),
                string.Empty,
                ToggleStatsDetails);
            _audioButton = HudUiFactory.CreateButton(
                panel,
                "Audio Toggle",
                new Vector2(0.12f, 0.3f),
                new Vector2(0.58f, 0.44f),
                string.Empty,
                ToggleAudio);
            HudUiFactory.CreateButton(
                panel,
                "Volume Down",
                new Vector2(0.62f, 0.3f),
                new Vector2(0.74f, 0.44f),
                "-",
                DecreaseVolume);
            HudUiFactory.CreateButton(
                panel,
                "Volume Up",
                new Vector2(0.76f, 0.3f),
                new Vector2(0.88f, 0.44f),
                "+",
                IncreaseVolume);
            HudUiFactory.CreateButton(
                panel,
                "Resume Button",
                new Vector2(0.12f, 0.1f),
                new Vector2(0.88f, 0.25f),
                "RESUME",
                Resume);
            _root.gameObject.SetActive(false);

            StatsDetailsExpanded = PlayerPrefs.GetInt(StatsDetailsKey, 0) != 0;
            _statsPresenter.SetExpanded(StatsDetailsExpanded);
            ApplyDetailsLabel();
            ApplyAudioLabel();
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

        public void ToggleAudio()
        {
            var muted = !AudioMuted;
            if (_audio != null) _audio.SetMuted(muted);
            else PresentationAudioSettings.SetMuted(muted);
            ApplyAudioLabel();
        }

        public void IncreaseVolume() => ChangeVolume(0.1f);

        public void DecreaseVolume() => ChangeVolume(-0.1f);

        private void ChangeVolume(float delta)
        {
            var volume = Mathf.Clamp01(AudioVolume + delta);
            if (_audio != null) _audio.SetVolume(volume);
            else PresentationAudioSettings.SetVolume(volume);
            ApplyAudioLabel();
        }

        private void ApplyDetailsLabel()
        {
            var label = _detailsButton != null ? _detailsButton.GetComponentInChildren<Text>() : null;
            if (label != null)
            {
                label.text = $"STAT DETAILS: {(StatsDetailsExpanded ? "EXPANDED" : "COMPACT")}";
            }
        }

        private void ApplyAudioLabel()
        {
            var label = _audioButton != null ? _audioButton.GetComponentInChildren<Text>() : null;
            if (label != null) label.text = AudioMuted ? "AUDIO: OFF" : $"AUDIO: {Mathf.RoundToInt(AudioVolume * 100f)}%";
        }

        private void OnDestroy() => Resume();
    }
}
