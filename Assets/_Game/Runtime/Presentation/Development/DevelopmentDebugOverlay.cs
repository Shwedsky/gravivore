#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Text;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Development
{
    [DisallowMultipleComponent]
    public sealed class DevelopmentDebugOverlay : MonoBehaviour
    {
        private const float RefreshInterval = 0.25f;
        private readonly StringBuilder _buffer = new StringBuilder(512);
        private DevelopmentCommandService _commands;
        private DevelopmentSessionSummary _summary;
        private EnemyPopulationController _population;
        private Transform _player;
        private PlayerHealthController _health;
        private PlayerStatsState _stats;
        private AssimilationProgressionService _progression;
        private WorldUnlockState _world;
        private CustodianBossController _boss;
        private BossCompletionState _completion;
        private RectTransform _panel;
        private Text _metrics;
        private Button _godModeButton;
        private float _refreshRemaining;
        private float _smoothedFrameSeconds = 1f / 60f;

        public RectTransform ToggleRect { get; private set; }
        public RectTransform ToggleVisualRect { get; private set; }
        public RectTransform PanelRect => _panel;
        public bool IsVisible => _panel != null && _panel.gameObject.activeSelf;

        public void Initialize(
            RectTransform hudRoot,
            DevelopmentCommandService commands,
            DevelopmentSessionSummary summary,
            EnemyPopulationController population,
            Transform player,
            PlayerHealthController health,
            PlayerStatsState stats,
            AssimilationProgressionService progression,
            WorldUnlockState world,
            CustodianBossController boss,
            BossCompletionState completion)
        {
            if (hudRoot == null) throw new ArgumentNullException(nameof(hudRoot));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _summary = summary ?? throw new ArgumentNullException(nameof(summary));
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _boss = boss ?? throw new ArgumentNullException(nameof(boss));
            _completion = completion ?? throw new ArgumentNullException(nameof(completion));

            var toggle = HudUiFactory.CreateCompactButton(
                hudRoot, "DEV Toggle", new Vector2(0.70f, 0.925f), new Vector2(0.82f, 0.985f),
                "DEV", Toggle, out var toggleVisual);
            ToggleRect = toggle.GetComponent<RectTransform>();
            ToggleVisualRect = toggleVisual;
            _panel = HudUiFactory.CreatePanel(hudRoot, "Development Tools", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.91f), HudUiFactory.ModalBackdropColor, true);
            _metrics = HudUiFactory.CreateText(_panel, "Metrics", new Vector2(0.04f, 0.61f), new Vector2(0.96f, 0.97f), string.Empty, 25, TextAnchor.UpperLeft, Color.white);

            CreateButton("Power", "+1 Мощность", 0, () => _commands.GrantStat(PlayerStatType.Power, 1));
            CreateButton("Hull", "+1 Корпус", 1, () => _commands.GrantStat(PlayerStatType.Hull, 1));
            CreateButton("Armor", "+1 Броня", 2, () => _commands.GrantStat(PlayerStatType.Armor, 1));
            CreateButton("Flux", "+1 Поток", 3, () => _commands.GrantStat(PlayerStatType.Flux, 1));
            CreateButton("Mobility", "+1 Манёвренность", 4, () => _commands.GrantStat(PlayerStatType.Mobility, 1));
            CreateButton("All", "+1 Все", 5, () => _commands.GrantAllStats(1));
            CreateButton("Elite", "Открыть элиту", 6, () => _commands.UnlockElite());
            CreateButton("Boss", "Открыть босса", 7, () => _commands.UnlockBoss());
            CreateButton("Reset Boss", "Сбросить босса", 8, () => _commands.ResetBoss());
            _godModeButton = CreateButton("God Mode", "Бессмертие: ВЫКЛ", 9, () => { _commands.ToggleGodMode(); UpdateGodModeLabel(); });
            CreateButton("Reset Profile", "Сброс профиля", 10, () => _commands.ResetProfile());
            _panel.gameObject.SetActive(false);
        }

        public void Toggle()
        {
            _panel.gameObject.SetActive(!IsVisible);
            if (IsVisible) RefreshMetrics();
        }

        private Button CreateButton(string name, string label, int index, Action action)
        {
            const int columns = 2;
            var row = index / columns;
            var column = index % columns;
            var x0 = column == 0 ? 0.04f : 0.52f;
            var x1 = column == 0 ? 0.48f : 0.96f;
            var y1 = 0.58f - row * 0.085f;
            var y0 = y1 - 0.07f;
            return HudUiFactory.CreateButton(_panel, name, new Vector2(x0, y0), new Vector2(x1, y1), label, () => RunCommand(action));
        }

        private static void RunCommand(Action action)
        {
            try { action(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private void Update()
        {
            var frameSeconds = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            _smoothedFrameSeconds = Mathf.Lerp(_smoothedFrameSeconds, frameSeconds, 0.08f);
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1) || UnityEngine.Input.GetKeyDown(KeyCode.BackQuote)) Toggle();
            if (!IsVisible) return;
            _refreshRemaining -= frameSeconds;
            if (_refreshRemaining > 0f) return;
            _refreshRemaining = RefreshInterval;
            RefreshMetrics();
        }

        private void RefreshMetrics()
        {
            var levels = _stats.BaseLevels;
            var position = _player.position;
            _buffer.Clear();
            _buffer.AppendFormat("FPS {0:0.0} / {1:0.0} ms\n", 1f / _smoothedFrameSeconds, _smoothedFrameSeconds * 1000f);
            _buffer.AppendFormat("Враги {0}/{1}  Позиция {2:0.0}, {3:0.0}, {4:0.0}\n", _population.LiveEnemyCount, _population.GlobalLiveEnemyCap, position.x, position.y, position.z);
            _buffer.AppendFormat("HP {0:0}/{1:0}  P/H/A/F/M {2}/{3}/{4}/{5}/{6}\n", _health.CurrentHitPoints, _health.MaximumHitPoints, levels.Power, levels.Hull, levels.Armor, levels.Flux, levels.Mobility);
            _buffer.AppendFormat("Ассимиляция {0}  Элита {1}\n", _progression.State.TotalAssimilationScore, _world.EliteDefeated ? "побеждена" : (_world.EliteGateUnlocked ? "открыта" : "закрыта"));
            _buffer.AppendFormat("Босс {0} / завершён {1}\n", _boss.State, _completion.IsDefeated ? "да" : "нет");
            _buffer.AppendFormat("Сессия {0:0}s  убийства {1}  смерти {2}  уровни +{3}\n", _summary.ElapsedSeconds, _summary.OrdinaryEnemiesDefeated, _summary.PlayerDeaths, _summary.StatLevelUps);
            _buffer.AppendFormat("Босс: попытки {0}  сбросы {1}", _summary.BossAttempts, _summary.BossResets);
            _metrics.text = _buffer.ToString();
        }

        private void UpdateGodModeLabel()
        {
            var label = _godModeButton != null ? _godModeButton.GetComponentInChildren<Text>() : null;
            if (label != null) label.text = _commands.GodModeEnabled ? "Бессмертие: ВКЛ" : "Бессмертие: ВЫКЛ";
        }
    }
}
#endif
