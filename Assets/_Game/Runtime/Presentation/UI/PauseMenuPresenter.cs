using System;
using Gravivore.Presentation.Composition;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class PauseMenuPresenter : MonoBehaviour
    {
        private HudModalController _modal;
        private PlayerStatsState _stats;
        private InventoryState _inventory;
        private EquipmentCatalog _equipmentCatalog;
        private RectTransform _root;
        private Text _statsText;
        private Text _equipmentText;
        private Button _audioButton;
        private Button _hapticsButton;
        private S14AudioPresenter _audio;
        private PresentationHapticSettings _hapticSettings;
        public CharacteristicsPresenter Characteristics { get; private set; }

        public void ConfigureCharacteristics(Gravivore.Gameplay.Progression.AssimilationProgressionService progression,
            Gravivore.Gameplay.World.EliteGateRequirement requirement, Gravivore.Gameplay.Quests.QuestService quests,
            Gravivore.Gameplay.World.WorldUnlockState world)
        {
            Characteristics = gameObject.AddComponent<CharacteristicsPresenter>();
            Characteristics.Initialize(_root, _stats, progression, requirement, quests, world, null);
            var title = _root.Find("Game Menu/Characteristics Title");
            if (title != null) title.gameObject.SetActive(false);
            var panel = (RectTransform)_root.Find("Game Menu");
            HudUiFactory.CreateButton(panel, "Open Characteristics", new Vector2(.08f,.82f),new Vector2(.92f,.89f),
                RussianUiText.Characteristics, Characteristics.Open);
        }

        public bool IsPaused => _root != null && _root.gameObject.activeSelf;
        public RectTransform PauseButtonRect { get; private set; }
        public RectTransform PauseButtonVisualRect { get; private set; }
        public RectTransform ModalRect => _root;
        public string StatsText => _statsText != null ? _statsText.text : string.Empty;
        public string EquipmentText => _equipmentText != null ? _equipmentText.text : string.Empty;
        public bool HasSettingsControls => _audioButton != null && _hapticsButton != null;
        public bool AudioMuted => _audio != null ? _audio.IsMuted : PresentationAudioSettings.IsMuted;
        public float AudioVolume => _audio != null ? _audio.Volume : PresentationAudioSettings.Volume;
        public bool HapticsEnabled => _hapticSettings == null || _hapticSettings.Enabled;

        public void Initialize(
            RectTransform hudRoot,
            HudModalController modal,
            PlayerStatsState stats,
            InventoryState inventory,
            EquipmentCatalog equipmentCatalog,
            S14AudioPresenter audio = null,
            PresentationHapticSettings hapticSettings = null)
        {
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));
            _modal = modal ?? throw new ArgumentNullException(nameof(modal));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _equipmentCatalog = equipmentCatalog ?? throw new ArgumentNullException(nameof(equipmentCatalog));
            _audio = audio;
            _hapticSettings = hapticSettings ?? new PresentationHapticSettings();

            var pauseButton = HudUiFactory.CreateCompactButton(
                hudRoot, "Menu Button", new Vector2(0.84f, 0.925f), new Vector2(0.97f, 0.985f),
                "МЕНЮ", Open, out var pauseVisual);
            PauseButtonRect = pauseButton.GetComponent<RectTransform>();
            PauseButtonVisualRect = pauseVisual;

            _root = HudUiFactory.CreatePanel(hudRoot, "Game Menu Modal", Vector2.zero, Vector2.one, HudUiFactory.ModalBackdropColor, true);
            var panel = HudUiFactory.CreatePanel(_root, "Game Menu", new Vector2(0.07f, 0.08f), new Vector2(0.93f, 0.92f), HudUiFactory.PanelColor, true);
            HudUiFactory.CreateText(panel, "Title", new Vector2(0.08f, 0.9f), new Vector2(0.92f, 0.98f), RussianUiText.GameMenu, 36, TextAnchor.MiddleCenter, Color.white);
            HudUiFactory.CreateText(panel, "Characteristics Title", new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.89f), RussianUiText.Characteristics, 25, TextAnchor.MiddleLeft, HudUiFactory.AccentColor);
            _statsText = HudUiFactory.CreateText(panel, "Characteristics", new Vector2(0.08f, 0.5f), new Vector2(0.92f, 0.82f), string.Empty, 23, TextAnchor.UpperLeft, Color.white);
            HudUiFactory.CreateText(panel, "Equipment Title", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.49f), RussianUiText.Equipment, 25, TextAnchor.MiddleLeft, HudUiFactory.AccentColor);
            _equipmentText = HudUiFactory.CreateText(panel, "Equipment", new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.42f), string.Empty, 23, TextAnchor.UpperLeft, Color.white);
            HudUiFactory.CreateText(panel, "Settings Title", new Vector2(0.08f, 0.19f), new Vector2(0.92f, 0.26f), RussianUiText.Settings, 25, TextAnchor.MiddleLeft, HudUiFactory.AccentColor);
            _audioButton = HudUiFactory.CreateButton(panel, "Audio Toggle", new Vector2(0.08f, 0.11f), new Vector2(0.47f, 0.19f), string.Empty, ToggleAudio);
            HudUiFactory.CreateButton(panel, "Volume Down", new Vector2(0.49f, 0.11f), new Vector2(0.61f, 0.19f), "-", DecreaseVolume);
            HudUiFactory.CreateButton(panel, "Volume Up", new Vector2(0.63f, 0.11f), new Vector2(0.75f, 0.19f), "+", IncreaseVolume);
            _hapticsButton = HudUiFactory.CreateButton(panel, "Haptics Toggle", new Vector2(0.08f, 0.02f), new Vector2(0.55f, 0.1f), string.Empty, ToggleHaptics);
            HudUiFactory.CreateButton(panel, "Resume Button", new Vector2(0.58f, 0.02f), new Vector2(0.92f, 0.1f), RussianUiText.Resume, Resume);
            _root.gameObject.SetActive(false);
            RefreshContent();
        }

        public void Open()
        {
            if (!_modal.TryOpen(this)) return;
            RefreshContent();
            _root.SetAsLastSibling();
            _root.gameObject.SetActive(true);
        }

        public void Resume()
        {
            GetComponentInParent<S01SceneCompositionRoot>()?.GetComponent<WeaponEquipmentPanel>()?.Close();
            if (Characteristics != null && Characteristics.IsVisible) Characteristics.Close();
            if (_root != null) _root.gameObject.SetActive(false);
            _modal?.Close(this);
        }

        public void ToggleAudio()
        {
            var muted = !AudioMuted;
            if (_audio != null) _audio.SetMuted(muted); else PresentationAudioSettings.SetMuted(muted);
            ApplySettingsLabels();
        }

        public void IncreaseVolume() => ChangeVolume(0.1f);
        public void DecreaseVolume() => ChangeVolume(-0.1f);

        public void ToggleHaptics()
        {
            _hapticSettings?.SetEnabled(!HapticsEnabled);
            ApplySettingsLabels();
        }

        private void ChangeVolume(float delta)
        {
            var volume = Mathf.Clamp01(AudioVolume + delta);
            if (_audio != null) _audio.SetVolume(volume); else PresentationAudioSettings.SetVolume(volume);
            ApplySettingsLabels();
        }

        private void RefreshContent()
        {
            _statsText.text = HudTextFormatter.Stats(_stats.BaseLevels, _stats.DerivedStats, true);
            _equipmentText.text = HudTextFormatter.Equipment(_inventory, _equipmentCatalog);
            ApplySettingsLabels();
        }

        private void ApplySettingsLabels()
        {
            var audioLabel = _audioButton != null ? _audioButton.GetComponentInChildren<Text>() : null;
            if (audioLabel != null) audioLabel.text = RussianUiText.Audio(AudioMuted, Mathf.RoundToInt(AudioVolume * 100f));
            var hapticsLabel = _hapticsButton != null ? _hapticsButton.GetComponentInChildren<Text>() : null;
            if (hapticsLabel != null) hapticsLabel.text = RussianUiText.Haptics(HapticsEnabled);
        }

        private void OnDestroy() => Resume();
    }
}
