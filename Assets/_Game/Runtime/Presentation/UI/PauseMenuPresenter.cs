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
        private Button _hapticsButton;
        private S14AudioPresenter _audio;
        private PresentationHapticSettings _hapticSettings;

        public bool IsPaused => _root != null && _root.gameObject.activeSelf;
        public bool StatsDetailsExpanded { get; private set; }
        public RectTransform PauseButtonRect { get; private set; }
        public RectTransform ModalRect => _root;
        public bool AudioMuted => _audio != null ? _audio.IsMuted : PresentationAudioSettings.IsMuted;
        public float AudioVolume => _audio != null ? _audio.Volume : PresentationAudioSettings.Volume;
        public bool HapticsEnabled => _hapticSettings == null || _hapticSettings.Enabled;

        public void Initialize(
            RectTransform hudRoot,
            HudModalController modal,
            PlayerStatsHudPresenter statsPresenter,
            S14AudioPresenter audio = null,
            PresentationHapticSettings hapticSettings = null)
        {
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));
            _modal = modal ?? throw new ArgumentNullException(nameof(modal));
            _statsPresenter = statsPresenter != null
                ? statsPresenter
                : throw new ArgumentNullException(nameof(statsPresenter));
            _audio = audio;
            _hapticSettings = hapticSettings ?? new PresentationHapticSettings();

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
                new Vector2(0.12f, 0.56f),
                new Vector2(0.88f, 0.68f),
                string.Empty,
                ToggleStatsDetails);
            _audioButton = HudUiFactory.CreateButton(
                panel,
                "Audio Toggle",
                new Vector2(0.12f, 0.4f),
                new Vector2(0.58f, 0.52f),
                string.Empty,
                ToggleAudio);
            HudUiFactory.CreateButton(
                panel,
                "Volume Down",
                new Vector2(0.62f, 0.4f),
                new Vector2(0.74f, 0.52f),
                "-",
                DecreaseVolume);
            HudUiFactory.CreateButton(
                panel,
                "Volume Up",
                new Vector2(0.76f, 0.4f),
                new Vector2(0.88f, 0.52f),
                "+",
                IncreaseVolume);
            _hapticsButton = HudUiFactory.CreateButton(
                panel,
                "Haptics Toggle",
                new Vector2(0.12f, 0.24f),
                new Vector2(0.88f, 0.36f),
                string.Empty,
                ToggleHaptics);
            HudUiFactory.CreateButton(
                panel,
                "Resume Button",
                new Vector2(0.12f, 0.07f),
                new Vector2(0.88f, 0.19f),
                "RESUME",
                Resume);
            _root.gameObject.SetActive(false);

            StatsDetailsExpanded = PlayerPrefs.GetInt(StatsDetailsKey, 0) != 0;
            _statsPresenter.SetExpanded(StatsDetailsExpanded);
            ApplyDetailsLabel();
            ApplyAudioLabel();
            ApplyHapticsLabel();
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

        public void ToggleHaptics()
        {
            _hapticSettings?.SetEnabled(!HapticsEnabled);
            ApplyHapticsLabel();
        }

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

        private void ApplyHapticsLabel()
        {
            var label = _hapticsButton != null ? _hapticsButton.GetComponentInChildren<Text>() : null;
            if (label != null) label.text = HapticsEnabled ? "HAPTICS: ON" : "HAPTICS: OFF";
        }

        private void OnDestroy() => Resume();
    }
}
