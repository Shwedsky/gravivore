using System;
using System.Collections.Generic;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Map;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using Gravivore.Persistence.Profile;
using Gravivore.Platform.Monetization;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Feedback;
using Gravivore.Presentation.Input;
using Gravivore.Presentation.Quests;
using Gravivore.Presentation.UI;
using Gravivore.Presentation.World;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Gravivore.Presentation.Development;
using UnityEngine.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gravivore.Presentation.Composition
{
    [DisallowMultipleComponent]
    public sealed class S01SceneCompositionRoot : MonoBehaviour
    {
        private static readonly Vector2 HudReferenceResolution = new Vector2(1080f, 1920f);

        [SerializeField] private PlayerMovementSettings _movementSettings;
        [SerializeField] private PlayerStatsDefinition _playerStatsDefinition;
        [SerializeField] private PlayerRecoverySettings _playerRecoverySettings;
        [SerializeField] private EquipmentCatalogDefinition _equipmentCatalogDefinition;
        [SerializeField] private GravityAttackSettings _gravityAttackSettings;
        [SerializeField] private SpawnSpotDefinition[] _spawnSpotDefinitions;
        [SerializeField] private PlayerProgressionDefinition _progressionDefinition;
        [SerializeField] private QuestDefinition _questDefinition;
        [SerializeField] private QuestOnboardingDefinition _questOnboardingDefinition;
        [SerializeField] private SaveOfflineDefinition _saveOfflineDefinition;
        [SerializeField] private EvolutionDefinition _evolutionDefinition;
        [SerializeField] private Chapter01WorldDefinition _worldDefinition;
        [SerializeField] private MagnetarGuardDefinition _magnetarGuardDefinition;
        [SerializeField] private CustodianBossDefinition _custodianBossDefinition;
        [SerializeField, Min(1)] private int _globalLiveEnemyCap = 25;
        [SerializeField] private FloatingJoystickSettings _joystickSettings;
        [SerializeField] private CameraFollowSettings _cameraSettings;
        [SerializeField] private PresentationMaterialPalette _materialPalette;
        [SerializeField] private S14PresentationDefinition _s14PresentationDefinition;
        [SerializeField] private Phase6BProductionDefinition _phase6BProductionDefinition;
        [SerializeField] private S15VisualCatalog _s15VisualCatalog;
        [SerializeField] private ChapterVisualEnvironment _visualEnvironment;
        [SerializeField] private Vector3 _playerSpawn = Vector3.zero;
        [SerializeField, Min(0f)] private float _postRespawnInvulnerabilitySeconds = 1.5f;

        private Material _eliteMaterial;
        private Material _bossMaterial;
        private Transform _playerVisualRoot;
        private float _repeatRetryRemaining;
        private QuestMovementSignal _questMovementSignal;
        private bool _isComposed;
        private RectTransform _hudRoot;
        private FloatingJoystickInput _movementInput;
        private string _profileDirectoryOverride;
        private ITimeProvider _timeProviderOverride;
        private ProfileSession _profileSession;
        private GravityLashVfxPool _gravityLashVfx;
        private PresentationHapticSettings _hapticSettings;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private DevelopmentTelemetryObserver _developmentTelemetry;
#endif

        public GameObject PlayerObject { get; private set; }

        public PlayerStatsState PlayerStats { get; private set; }

        public EquipmentCatalog EquipmentCatalog { get; private set; }

        public InventoryState Inventory { get; private set; }

        public EquipmentService Equipment { get; private set; }

        public PlayerHealthController PlayerHealth { get; private set; }

        public EnemyPopulationController EnemyPopulation { get; private set; }

        public AssimilationProgressionService Progression { get; private set; }
        public QuestService Quests { get; private set; }
        public QuestTrackerPresenter QuestTracker { get; private set; }

        public PlayerEvolutionPresenter EvolutionPresenter { get; private set; }

        public WorldUnlockService WorldUnlocks { get; private set; }

        public Chapter01WorldPresenter WorldPresenter { get; private set; }
        public ChapterVisualEnvironment VisualEnvironment => _visualEnvironment;

        public MagnetarGuardController MagnetarGuard { get; private set; }

        public CustodianBossController CustodianBoss { get; private set; }

        public BossCompletionState BossCompletion { get; private set; }

        public SaveCoordinator SaveCoordinator { get; private set; }

        public OfflineRewardService OfflineRewards => _profileSession?.OfflineRewards;

        public MonetizationFeatureConfiguration MonetizationFeatures { get; private set; }
        public IPurchaseService PurchaseService { get; private set; }
        public IRewardedAdService RewardedAdService { get; private set; }

        public OfflineReturnSummary OfflineReturnSummary =>
            _profileSession != null ? _profileSession.ReturnSummary : default;

        public Guid ProfileId => _profileSession != null ? _profileSession.State.ProfileId : Guid.Empty;

        public EncounterTelegraphPresenter EncounterTelegraphs { get; private set; }
        public IWorldMarkerSource WorldMarkers { get; private set; }
        public IMapMarkerSource MapMarkers { get; private set; }
        public Chapter1EncounterRuntime Chapter1Encounters { get; private set; }
        public Chapter1WorldMarkerAuthority MarkerAuthority { get; private set; }
        public IReadOnlyList<StrongOrdinarySpotDefinition> StrongSpots { get; private set; }
        public Chapter01MapProductionIntegration MapIntegration { get; private set; }
        public Phase6BCombatProductionBridge Phase6BCombat { get; private set; }
        public RepairHubProductionPresenter RepairHub { get; private set; }
        public int SaveSchemaVersion => SaveSchema.CurrentVersion;
        public MapWorldBounds MapBounds
        {
            get
            {
                var bounds = _worldDefinition.Configuration.Bounds;
                return new MapWorldBounds(bounds.MinX, bounds.MaxX, bounds.MinZ, bounds.MaxZ);
            }
        }
        public PlayerHealthHudPresenter PlayerHealthHud { get; private set; }
        public PlayerStatsHudPresenter PlayerStatsHud { get; private set; }
        public BossHealthHudPresenter BossHealthHud { get; private set; }
        public OfflineRewardPanelPresenter OfflineRewardPanel { get; private set; }
        public ChapterCompletionPresenter ChapterCompletion { get; private set; }
        public PauseMenuPresenter PauseMenu { get; private set; }
        public S14AudioPresenter AudioPresenter { get; private set; }
        public S14CombatFeedbackPresenter CombatFeedback { get; private set; }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public DevelopmentDebugOverlay DevelopmentOverlay { get; private set; }
#endif

        private HudModalController _hudModal;

        private void Start()
        {
            Compose();
        }

        public void Compose()
        {
            if (_isComposed)
            {
                return;
            }

            if (_movementSettings == null || _playerStatsDefinition == null || _playerRecoverySettings == null ||
                _equipmentCatalogDefinition == null ||
                _gravityAttackSettings == null ||
                _spawnSpotDefinitions == null || _spawnSpotDefinitions.Length != 5 || _globalLiveEnemyCap < 1 ||
                _progressionDefinition == null || _questDefinition == null || _questOnboardingDefinition == null ||
                _saveOfflineDefinition == null ||
                _evolutionDefinition == null || _worldDefinition == null ||
                _magnetarGuardDefinition == null || _custodianBossDefinition == null ||
                _joystickSettings == null || _cameraSettings == null || _materialPalette == null ||
                _s14PresentationDefinition == null || _s15VisualCatalog == null ||
                float.IsNaN(_postRespawnInvulnerabilitySeconds) ||
                float.IsInfinity(_postRespawnInvulnerabilitySeconds) ||
                _postRespawnInvulnerabilitySeconds < 0f)
            {
                throw new InvalidOperationException(
                    "Scene composition requires movement, stats, equipment, attack, progression, quests, evolution, world, elite, boss, five spawn spots, joystick, camera, and material settings.");
            }

            if (_phase6BProductionDefinition == null) throw new InvalidOperationException("Production Phase6B definition is required.");
            _phase6BProductionDefinition.ValidateOrThrow();
            _materialPalette.ValidateOrThrow();
            _playerRecoverySettings.ValidateOrThrow();
            _s14PresentationDefinition.ValidateOrThrow();
            _s15VisualCatalog.ValidateOrThrow();
            _visualEnvironment?.Initialize();
            InitializeMonetization();

            var statsConfiguration = _playerStatsDefinition.Configuration;
            var progressionConfiguration = _progressionDefinition.Configuration;
            var worldConfiguration = _worldDefinition.Configuration;
            ValidateS15Coverage(worldConfiguration);
            var questCatalog = _questDefinition.Catalog;
            EquipmentCatalog = _equipmentCatalogDefinition.Catalog;
            var bossConfiguration = _custodianBossDefinition.CreateConfiguration(worldConfiguration);
            InitializeProfile(
                statsConfiguration,
                progressionConfiguration,
                worldConfiguration,
                questCatalog,
                bossConfiguration.Id);
            PlayerStats = _profileSession.State.PlayerStats;
            Inventory = _profileSession.State.Inventory;
            Equipment = new EquipmentService(PlayerStats, EquipmentCatalog, Inventory);
            CreateHud(out var uiTouchExclusion, out var topTouchExclusion, out var joystickView);
            _movementInput = CreateMovementInput(uiTouchExclusion, joystickView);
            var locomotion = CreatePlayer();
            var cameraTransform = CreateCamera(PlayerObject.transform);

            locomotion.Initialize(
                _movementInput,
                cameraTransform,
                PlayerStats,
                _movementSettings.RotationDegreesPerSecond);
            InitializePlayerHealth();
            InitializeGravityAttack();
            CreateLight();
            InitializeEnemyPopulation();
            Progression = new AssimilationProgressionService(
                PlayerStats,
                _profileSession.State.Progression,
                progressionConfiguration,
                EnemyPopulation);
            InitializeEncounterActors(worldConfiguration, bossConfiguration);
            InitializeQuests(questCatalog, worldConfiguration);
            InitializeWorld(worldConfiguration);
            Chapter1Encounters.Attach(MagnetarGuard, CustodianBoss, Quests);
            MarkerAuthority = new Chapter1WorldMarkerAuthority(Chapter1Encounters.Encounters, StrongSpots);
            MapMarkers = new Chapter1WorldMarkerMapAdapter(PlayerObject.transform, EnemyPopulation,
                MagnetarGuard, CustodianBoss, WorldUnlocks.State, MarkerAuthority,
                _spawnSpotDefinitions.Length, _playerSpawn);
            WorldMarkers = new WorldMarkerReadModel(PlayerObject.transform, EnemyPopulation,
                MagnetarGuard, CustodianBoss, WorldUnlocks.State);
            InitializeEvolution();
            Phase6BCombat = gameObject.AddComponent<Phase6BCombatProductionBridge>();
            Phase6BCombat.Initialize(this, _gravityLashVfx);
            var repairObject = new GameObject("Repair Hub Presentation", typeof(RepairHubProductionPresenter));
            repairObject.transform.SetParent(transform, false);
            RepairHub = repairObject.GetComponent<RepairHubProductionPresenter>();
            RepairHub.Initialize(PlayerHealth, _playerSpawn, _playerRecoverySettings.Configuration, _phase6BProductionDefinition);
            InitializeS14Presentation();
            if (_evolutionDefinition.HasTierPrefabs)
            {
                var motion = PlayerObject.AddComponent<Gravivore.Presentation.Player.MechMotionPresenter>();
                motion.Initialize(PlayerObject.transform, PlayerObject.GetComponent<PlayerEvolutionView>(),
                    _playerVisualRoot.Find("Gravity Lash Presentation Origin"), _s14PresentationDefinition,
                    _gravityLashVfx, AudioPresenter);
            }
            SaveCoordinator = new SaveCoordinator(
                _profileSession,
                PlayerStats,
                Progression,
                Equipment,
                Quests,
                WorldUnlocks.State,
                BossCompletion,
                _profileSession.State.Offline,
                _saveOfflineDefinition.Configuration.AutosaveDelaySeconds);
            InitializeS13Hud(uiTouchExclusion, topTouchExclusion);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            InitializeDevelopmentTools(uiTouchExclusion, topTouchExclusion, worldConfiguration, bossConfiguration);
#endif
            MapIntegration = gameObject.AddComponent<Chapter01MapProductionIntegration>();
            MapIntegration.Initialize(this);
            _isComposed = true;
        }

        private void InitializeMonetization()
        {
            MonetizationFeatures = MonetizationFeatureConfiguration.Disabled();
            PurchaseService = new DisabledPurchaseService();
            RewardedAdService = new DisabledRewardedAdService();
        }

        public void ConfigurePersistence(string directory, ITimeProvider timeProvider)
        {
            if (_isComposed) throw new InvalidOperationException("Persistence must be configured before composition.");
            _profileDirectoryOverride = !string.IsNullOrWhiteSpace(directory)
                ? directory
                : throw new ArgumentException("Profile directory is required.", nameof(directory));
            _timeProviderOverride = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public bool FlushNow() => SaveCoordinator != null && SaveCoordinator.FlushNow();

        private void InitializeProfile(
            PlayerStatsConfiguration stats,
            ProgressionConfiguration progression,
            Chapter01WorldConfiguration world,
            QuestCatalog quests,
            string bossId)
        {
            _saveOfflineDefinition.ValidateOrThrow();
            var configuration = _saveOfflineDefinition.Configuration;
            var time = _timeProviderOverride ?? new SystemUtcTimeProvider();
            var serializer = new UnityJsonSaveSerializer();
            var migrations = new SaveMigrationPipeline(
                configuration.CurrentSchemaVersion,
                serializer,
                new ISaveMigration[] { new SaveMigrationV0ToV1() });
            var diagnostics = new UnitySaveDiagnostics();
            var repository = new JsonProfileRepository(
                _profileDirectoryOverride ?? Application.persistentDataPath,
                serializer,
                migrations,
                time,
                diagnostics);
            var context = new ProfileRestoreContext(
                stats,
                progression,
                EquipmentCatalog,
                quests,
                world.EliteGate.Id,
                world.BossGate.Id,
                world.EliteEnemyId,
                bossId);
            _profileSession = ProfileSession.Start(repository, context, configuration, time, diagnostics);
        }

        private void ValidateS15Coverage(Chapter01WorldConfiguration world)
        {
            var enemyIds = new string[_spawnSpotDefinitions.Length];
            for (var i = 0; i < _spawnSpotDefinitions.Length; i++)
            {
                if (_spawnSpotDefinitions[i] == null)
                    throw new InvalidOperationException($"Spawn spot definition {i} is not assigned.");
                enemyIds[i] = _spawnSpotDefinitions[i].CreateRuntimeConfiguration().Enemy.Id;
            }

            var landmarkIds = new string[world.ZoneCount];
            for (var i = 0; i < world.ZoneCount; i++) landmarkIds[i] = world.GetZone(i).Id;
            _s15VisualCatalog.ValidateCoverageOrThrow(enemyIds, landmarkIds);
        }

        private void CreateHud(
            out UiTouchExclusion uiTouchExclusion,
            out RectTransform topTouchExclusion,
            out FloatingJoystickView joystickView)
        {
            var canvasObject = new GameObject(
                "HUD Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = HudReferenceResolution;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasScaler.referencePixelsPerUnit = 100f;

            var safeAreaObject = new GameObject("Safe Area", typeof(RectTransform));
            var safeAreaTransform = safeAreaObject.GetComponent<RectTransform>();
            safeAreaTransform.SetParent(canvasObject.transform, false);
            safeAreaTransform.anchorMin = Vector2.zero;
            safeAreaTransform.anchorMax = Vector2.one;
            safeAreaTransform.offsetMin = Vector2.zero;
            safeAreaTransform.offsetMax = Vector2.zero;
            safeAreaObject.AddComponent<SafeAreaHudRoot>();
            _hudRoot = safeAreaTransform;

            var exclusionObject = new GameObject("HUD Touch Exclusion", typeof(RectTransform));
            var exclusionTransform = exclusionObject.GetComponent<RectTransform>();
            exclusionTransform.SetParent(safeAreaTransform, false);
            exclusionTransform.anchorMin = new Vector2(0f, 0.82f);
            exclusionTransform.anchorMax = Vector2.one;
            exclusionTransform.offsetMin = Vector2.zero;
            exclusionTransform.offsetMax = Vector2.zero;
            topTouchExclusion = exclusionTransform;

            if (EventSystem.current == null)
            {
                var eventSystemObject = new GameObject(
                    "HUD Event System",
                    typeof(EventSystem),
                    typeof(StandaloneInputModule));
                eventSystemObject.transform.SetParent(transform, false);
            }

            var joystickViewObject = new GameObject(
                "Floating Joystick View",
                typeof(RectTransform),
                typeof(FloatingJoystickView));
            var joystickViewTransform = joystickViewObject.GetComponent<RectTransform>();
            joystickViewTransform.SetParent(safeAreaTransform, false);
            joystickViewTransform.anchorMin = Vector2.zero;
            joystickViewTransform.anchorMax = Vector2.one;
            joystickViewTransform.offsetMin = Vector2.zero;
            joystickViewTransform.offsetMax = Vector2.zero;
            joystickView = joystickViewObject.GetComponent<FloatingJoystickView>();
            joystickView.Initialize(safeAreaTransform, _joystickSettings);

            uiTouchExclusion = gameObject.AddComponent<UiTouchExclusion>();
            uiTouchExclusion.Initialize(new[] { exclusionTransform });
        }

        private void InitializeS13Hud(UiTouchExclusion uiTouchExclusion, RectTransform topTouchExclusion)
        {
            var healthObject = new GameObject("Player Health HUD", typeof(PlayerHealthHudPresenter));
            healthObject.transform.SetParent(transform, false);
            PlayerHealthHud = healthObject.GetComponent<PlayerHealthHudPresenter>();
            PlayerHealthHud.Initialize(PlayerHealth, PlayerStats, _hudRoot);

            var statsObject = new GameObject("Player Stats HUD", typeof(PlayerStatsHudPresenter));
            statsObject.transform.SetParent(transform, false);
            PlayerStatsHud = statsObject.GetComponent<PlayerStatsHudPresenter>();
            PlayerStatsHud.Initialize(PlayerStats, _hudRoot, false);

            var bossHealthObject = new GameObject("Boss Health HUD", typeof(BossHealthHudPresenter));
            bossHealthObject.transform.SetParent(transform, false);
            BossHealthHud = bossHealthObject.GetComponent<BossHealthHudPresenter>();
            BossHealthHud.Initialize(CustodianBoss, BossCompletion, PlayerHealth, _hudRoot);

            _hudModal = new HudModalController(_movementInput);

            var pauseObject = new GameObject("Pause Menu", typeof(PauseMenuPresenter));
            pauseObject.transform.SetParent(transform, false);
            PauseMenu = pauseObject.GetComponent<PauseMenuPresenter>();
            PauseMenu.Initialize(
                _hudRoot,
                _hudModal,
                PlayerStats,
                Inventory,
                EquipmentCatalog,
                AudioPresenter,
                _hapticSettings);

            var offlineObject = new GameObject("Offline Reward Panel", typeof(OfflineRewardPanelPresenter));
            offlineObject.transform.SetParent(transform, false);
            OfflineRewardPanel = offlineObject.GetComponent<OfflineRewardPanelPresenter>();
            OfflineRewardPanel.Initialize(OfflineRewards, OfflineReturnSummary, _hudRoot, _hudModal);

            var completionObject = new GameObject("Chapter Completion", typeof(ChapterCompletionPresenter));
            completionObject.transform.SetParent(transform, false);
            ChapterCompletion = completionObject.GetComponent<ChapterCompletionPresenter>();
            ChapterCompletion.Initialize(BossCompletion, PlayerStats, _hudRoot, _hudModal);

            uiTouchExclusion.Initialize(new[]
            {
                topTouchExclusion,
                PauseMenu.PauseButtonRect,
                PauseMenu.ModalRect,
                OfflineRewardPanel.ModalRect,
                ChapterCompletion.ModalRect
            });
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void InitializeDevelopmentTools(
            UiTouchExclusion uiTouchExclusion,
            RectTransform topTouchExclusion,
            Chapter01WorldConfiguration worldConfiguration,
            CustodianBossConfiguration bossConfiguration)
        {
            var sessionId = Guid.NewGuid();
            var startedUtc = DateTime.UtcNow;
            var telemetryPath = DevelopmentTelemetryRecorder.CreateSessionPath(
                _profileDirectoryOverride ?? Application.persistentDataPath,
                startedUtc,
                sessionId);
            var recorder = new DevelopmentTelemetryRecorder(
                sessionId,
                ProfileId,
                new DevelopmentJsonLinesSink(telemetryPath));
            var summary = new DevelopmentSessionSummary(() => Time.realtimeSinceStartupAsDouble);
            _developmentTelemetry = new DevelopmentTelemetryObserver(
                recorder,
                summary,
                PlayerStats,
                Progression,
                Quests,
                EnemyPopulation,
                MagnetarGuard,
                CustodianBoss,
                BossCompletion,
                PlayerHealth,
                _profileSession.State.Offline);
            _developmentTelemetry.RecordStartupOfflineReward(OfflineReturnSummary);

            var commands = new DevelopmentCommandService(
                PlayerStats,
                Quests,
                WorldUnlocks,
                MagnetarGuard,
                BossCompletion,
                CustodianBoss,
                PlayerHealth,
                worldConfiguration.EliteEnemyId,
                bossConfiguration.Id,
                SaveCoordinator.MarkDirty,
                _profileSession.ResetProfileForDevelopment,
                ReloadCurrentSceneForDevelopment);
            var overlayObject = new GameObject("S16 Development Overlay", typeof(DevelopmentDebugOverlay));
            overlayObject.transform.SetParent(transform, false);
            DevelopmentOverlay = overlayObject.GetComponent<DevelopmentDebugOverlay>();
            DevelopmentOverlay.Initialize(
                _hudRoot,
                commands,
                summary,
                EnemyPopulation,
                PlayerObject.transform,
                PlayerHealth,
                PlayerStats,
                Progression,
                WorldUnlocks.State,
                CustodianBoss,
                BossCompletion);

            uiTouchExclusion.Initialize(new[]
            {
                topTouchExclusion,
                PauseMenu.PauseButtonRect,
                PauseMenu.ModalRect,
                OfflineRewardPanel.ModalRect,
                ChapterCompletion.ModalRect,
                DevelopmentOverlay.ToggleRect,
                DevelopmentOverlay.PanelRect
            });
        }

        private static void ReloadCurrentSceneForDevelopment()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }
#endif

        private FloatingJoystickInput CreateMovementInput(
            IUiTouchExclusion uiTouchExclusion,
            FloatingJoystickView joystickView)
        {
            var inputObject = new GameObject("Floating Joystick Input");
            inputObject.transform.SetParent(transform, false);
            var movementInput = inputObject.AddComponent<FloatingJoystickInput>();
            movementInput.Initialize(_joystickSettings, uiTouchExclusion, joystickView);
            return movementInput;
        }

        private PlayerLocomotion CreatePlayer()
        {
            PlayerObject = new GameObject(
                "Player",
                typeof(CharacterController),
                typeof(PlayerLocomotion),
                typeof(PlayerHealthController),
                typeof(GravityAttackController),
                typeof(PlayerEvolutionView),
                typeof(PlayerEvolutionPresenter),
                typeof(EvolutionVfxRelay));
            PlayerObject.transform.SetParent(transform, false);
            PlayerObject.transform.position = _playerSpawn;

            var characterController = PlayerObject.GetComponent<CharacterController>();
            characterController.height = 1.4f;
            characterController.radius = 0.42f;
            characterController.center = new Vector3(0f, 0.7f, 0f);
            characterController.stepOffset = 0.25f;

            var visualRoot = new GameObject("Player Visual Root");
            visualRoot.transform.SetParent(PlayerObject.transform, false);
            _playerVisualRoot = visualRoot.transform;

            if (!_evolutionDefinition.HasTierPrefabs)
                S15VisualFactory.Build(_playerVisualRoot, _s15VisualCatalog.Player, _s15VisualCatalog);

            return PlayerObject.GetComponent<PlayerLocomotion>();
        }

        private void InitializeEvolution()
        {
            var catalog = _evolutionDefinition.Catalog;
            var view = PlayerObject.GetComponent<PlayerEvolutionView>();
            view.Initialize(_playerVisualRoot, catalog, _materialPalette.LitMaterial);
            EvolutionPresenter = PlayerObject.GetComponent<PlayerEvolutionPresenter>();
            EvolutionPresenter.Initialize(
                Progression,
                PlayerStats,
                catalog.Selection,
                view,
                PlayerObject.GetComponent<EvolutionVfxRelay>());
        }

        private void InitializeWorld(Chapter01WorldConfiguration configuration)
        {
            var state = _profileSession.State.World;
            WorldUnlocks = new WorldUnlockService(Progression, Quests, configuration.EliteRequirement, state);

            var worldObject = new GameObject("Chapter 01 World", typeof(Chapter01WorldPresenter));
            worldObject.transform.SetParent(transform, false);
            WorldPresenter = worldObject.GetComponent<Chapter01WorldPresenter>();
            WorldPresenter.Initialize(configuration, state, _materialPalette.LitMaterial, _s15VisualCatalog, _visualEnvironment);
        }

        private void InitializeQuests(QuestCatalog catalog, Chapter01WorldConfiguration world)
        {
            var state = _profileSession.State.Quests;
            Quests = new QuestService(catalog, state, Progression, null, BossCompletion);

            var movementObject = new GameObject("Quest Movement Signal", typeof(QuestMovementSignal));
            movementObject.transform.SetParent(transform, false);
            _questMovementSignal = movementObject.GetComponent<QuestMovementSignal>();
            _questMovementSignal.Initialize(
                _movementInput,
                Quests,
                _questOnboardingDefinition.MovementInputDeadZone,
                _questOnboardingDefinition.MovementInputSeconds);

            var trackerObject = new GameObject("Quest Tracker Presentation", typeof(QuestTrackerPresenter));
            trackerObject.transform.SetParent(transform, false);
            QuestTracker = trackerObject.GetComponent<QuestTrackerPresenter>();
            QuestTracker.Initialize(
                Quests,
                Progression,
                world.EliteRequirement,
                world,
                _magnetarGuardDefinition.Configuration.SpawnPosition,
                _hudRoot,
                CreateMaterial);
        }

        private void InitializeEncounterActors(
            Chapter01WorldConfiguration world,
            CustodianBossConfiguration bossConfiguration)
        {
            var targetLayer = LayerMask.NameToLayer("CombatTarget");
            if (targetLayer < 0) throw new InvalidOperationException("CombatTarget layer is required for encounters.");

            var eliteObject = CreateEncounterObject(
                "Magnetar Guard",
                typeof(MagnetarGuardController),
                targetLayer,
                new Color(0.95f, 0.55f, 0.12f, 1f),
                new Vector3(1.05f, 1f, 1.05f),
                out var eliteBody,
                out var eliteTargetPoint,
                out var eliteSensor,
                out _eliteMaterial);
            MagnetarGuard = eliteObject.GetComponent<MagnetarGuardController>();
            MagnetarGuard.Initialize(
                eliteBody,
                eliteTargetPoint,
                eliteSensor,
                targetLayer,
                _magnetarGuardDefinition.Configuration,
                PlayerObject.transform,
                PlayerHealth);
            if (!ProgressionUnit(progressionId: _spawnSpotDefinitions[3].CreateRuntimeConfiguration().Enemy.Id, out var eliteUnit) ||
                !ProgressionUnit(progressionId: _spawnSpotDefinitions[4].CreateRuntimeConfiguration().Enemy.Id, out var bossUnit))
                throw new InvalidOperationException("Encounter ordinary reward comparison units are required.");
            Chapter1Encounters = new Chapter1EncounterRuntime(_profileSession, _progressionDefinition.Configuration, eliteUnit, bossUnit);
            var bossObject = CreateEncounterObject(
                "Custodian M-0",
                typeof(CustodianBossController),
                targetLayer,
                new Color(0.72f, 0.16f, 0.2f, 1f),
                new Vector3(1.6f, 1.2f, 1.6f),
                out var bossBody,
                out var bossTargetPoint,
                out var bossSensor,
                out _bossMaterial);
            BossCompletion = _profileSession.State.Boss;
            CustodianBoss = bossObject.GetComponent<CustodianBossController>();
            CustodianBoss.Initialize(
                bossBody,
                bossTargetPoint,
                bossSensor,
                targetLayer,
                bossConfiguration,
                PlayerObject.transform,
                PlayerHealth,
                new PhysicsPullDestinationResolver(
                    _gravityAttackSettings.HardBlockerLayers,
                    _gravityAttackSettings.BlockerClearance),
                Chapter1Encounters,
                BossCompletion, externalDefeatAuthority: true);
            ApplyEncounterBinding(eliteObject.transform, _visualEnvironment?.Definition.Elite);
            ApplyEncounterBinding(bossObject.transform, _visualEnvironment?.Definition.Boss);

            var presentationObject = new GameObject("Encounter Telegraph Presentation", typeof(EncounterTelegraphPresenter));
            presentationObject.transform.SetParent(transform, false);
            EncounterTelegraphs = presentationObject.GetComponent<EncounterTelegraphPresenter>();
            EncounterTelegraphs.Initialize(MagnetarGuard, CustodianBoss, BossCompletion, _materialPalette.LitMaterial, phase6BOwned: true);
        }

        private bool ProgressionUnit(string progressionId, out CoreReward reward) =>
            _progressionDefinition.Configuration.TryGetReward(progressionId, out reward);

        private static void ApplyEncounterBinding(Transform authority, PresentationModelBinding binding)
        {
            var visualRoot = new GameObject("Encounter Visual Root").transform;
            visualRoot.SetParent(authority, false);
            var fallback = authority.Find(authority.name + " Visual");
            if (fallback != null) fallback.SetParent(visualRoot, true);
            var active = fallback;
            if (binding != null && binding.HasPrefab)
            {
                active = binding.InstantiateUnder(visualRoot).transform;
                if (fallback != null) fallback.gameObject.SetActive(false);
            }
            var sockets = active != null ? new PresentationSocketSet(active) : null;
            authority.gameObject.AddComponent<CharacterVisualBinding>().Bind(visualRoot, active, sockets);
        }

        private GameObject CreateEncounterObject(
            string name,
            Type controllerType,
            int targetLayer,
            Color color,
            Vector3 visualScale,
            out CharacterController body,
            out Transform targetPoint,
            out Collider sensingCollider,
            out Material material)
        {
            var encounterObject = new GameObject(name, typeof(CharacterController), controllerType);
            encounterObject.transform.SetParent(transform, false);
            body = encounterObject.GetComponent<CharacterController>();
            body.stepOffset = 0.2f;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = $"{name} Visual";
            visual.transform.SetParent(encounterObject.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = visualScale;
            var visualCollider = visual.GetComponent<Collider>();
            visualCollider.enabled = false;
            Destroy(visualCollider);
            material = CreateMaterial(color);
            visual.GetComponent<Renderer>().sharedMaterial = material;

            var targetObject = new GameObject("Combat Target Sensor", typeof(SphereCollider));
            targetObject.layer = targetLayer;
            targetObject.transform.SetParent(encounterObject.transform, false);
            targetPoint = targetObject.transform;
            sensingCollider = targetObject.GetComponent<SphereCollider>();
            sensingCollider.isTrigger = true;
            return encounterObject;
        }

        private void InitializeGravityAttack()
        {
            var vfxObject = new GameObject("Gravity Lash VFX Pool", typeof(GravityLashVfxPool));
            vfxObject.transform.SetParent(transform, false);
            _gravityLashVfx = vfxObject.GetComponent<GravityLashVfxPool>();
            Transform presentationOrigin = null;
            if (_evolutionDefinition.HasTierPrefabs)
            {
                presentationOrigin = new GameObject("Gravity Lash Presentation Origin").transform;
                presentationOrigin.SetParent(_playerVisualRoot, false);
                presentationOrigin.localPosition = _evolutionDefinition.AttackPresentationOffset;
            }
            _gravityLashVfx.Initialize(_gravityAttackSettings, _materialPalette.UnlitMaterial, _s14PresentationDefinition, presentationOrigin, _phase6BProductionDefinition);

            var targetSensor = new PhysicsTargetSensor(
                _gravityAttackSettings.TargetColliderCapacity,
                _gravityAttackSettings.TargetLayers,
                _gravityAttackSettings.HardBlockerLayers);
            var pullResolver = new PhysicsPullDestinationResolver(
                _gravityAttackSettings.HardBlockerLayers,
                _gravityAttackSettings.BlockerClearance);
            PlayerObject.GetComponent<GravityAttackController>().Initialize(
                PlayerObject.transform,
                PlayerStats,
                _gravityAttackSettings,
                targetSensor,
                pullResolver,
                _gravityLashVfx,
                PlayerHealth);
        }

        private void InitializeS14Presentation()
        {
            var audioObject = new GameObject("S14 Audio Presenter", typeof(S14AudioPresenter));
            audioObject.transform.SetParent(transform, false);
            AudioPresenter = audioObject.GetComponent<S14AudioPresenter>();
            AudioPresenter.Initialize(_s14PresentationDefinition);
            _hapticSettings = new PresentationHapticSettings();

            var feedbackObject = new GameObject("S14 Combat Feedback", typeof(S14CombatFeedbackPresenter));
            feedbackObject.transform.SetParent(transform, false);
            CombatFeedback = feedbackObject.GetComponent<S14CombatFeedbackPresenter>();
            CombatFeedback.Initialize(
                EnemyPopulation,
                Progression,
                PlayerHealth,
                PlayerObject.transform,
                PlayerObject.GetComponent<EvolutionVfxRelay>(),
                _gravityLashVfx,
                MagnetarGuard,
                CustodianBoss,
                _s14PresentationDefinition,
                _materialPalette.UnlitMaterial,
                AudioPresenter,
                new ThrottledHapticFeedback(
                    new PlatformHapticFeedback(),
                    _hapticSettings,
                    new UnityUnscaledTimeSource()));
        }

        private void InitializePlayerHealth()
        {
            PlayerHealth = PlayerObject.GetComponent<PlayerHealthController>();
            PlayerHealth.Initialize(
                PlayerObject.GetComponent<CharacterController>(),
                PlayerStats,
                _playerSpawn,
                _postRespawnInvulnerabilitySeconds,
                _playerRecoverySettings.Configuration);
            PlayerHealth.Respawned += HandlePlayerRespawned;
        }

        private void HandlePlayerRespawned(PlayerRespawnEvent respawn)
        {
            PlayerObject.GetComponent<GravityAttackController>().ResetTransientState();
        }

        private void InitializeEnemyPopulation()
        {
            var targetLayer = LayerMask.NameToLayer("CombatTarget");
            if (targetLayer < 0)
            {
                throw new InvalidOperationException("The CombatTarget layer is required for enemy sensing colliders.");
            }

            var ordinary = new SpawnSpotRuntimeConfiguration[_spawnSpotDefinitions.Length];
            var positions = new Vector3[ordinary.Length];
            for (var i = 0; i < ordinary.Length; i++)
            {
                ordinary[i] = _spawnSpotDefinitions[i].CreateRuntimeConfiguration();
                positions[i] = ordinary[i].WorldOrigin;
            }
            StrongSpots = Chapter1StrongOrdinarySpotCatalog.Create(_worldDefinition.Configuration, positions);
            var configurations = new SpawnSpotRuntimeConfiguration[ordinary.Length + StrongSpots.Count];
            for (var i = 0; i < StrongSpots.Count; i++)
            {
                var source = ordinary[StrongSpots[i].Region == StrongOrdinaryRegion.Elite ? 3 : 4];
                configurations[ordinary.Length + i] = StrongSpots[i].CreateSpawnConfiguration(source);
            }
            for (var i = 0; i < _spawnSpotDefinitions.Length; i++)
            {
                if (_spawnSpotDefinitions[i] == null)
                {
                    throw new InvalidOperationException($"Spawn spot definition {i} is not assigned.");
                }

                configurations[i] = _spawnSpotDefinitions[i].CreateRuntimeConfiguration();
            }

            var populationObject = new GameObject("Enemy Population", typeof(EnemyPopulationController));
            populationObject.transform.SetParent(transform, false);
            EnemyPopulation = populationObject.GetComponent<EnemyPopulationController>();
            EnemyPopulation.Initialize(
                configurations,
                PlayerObject.transform,
                PlayerHealth,
                _globalLiveEnemyCap,
                targetLayer,
                _materialPalette.LitMaterial,
                new S15EnemyVisualFactory(_s15VisualCatalog),
                PlayerStats, shareCapacityAcrossSpots: true);
        }

        private Transform CreateCamera(Transform target)
        {
            var cameraObject = new GameObject(
                "Portrait Follow Camera",
                typeof(UnityEngine.Camera),
                typeof(AudioListener),
                typeof(PortraitFollowCamera));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.tag = "MainCamera";

            var cameraComponent = cameraObject.GetComponent<UnityEngine.Camera>();
            cameraComponent.nearClipPlane = 0.1f;
            cameraComponent.farClipPlane = 100f;
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.035f, 0.047f, 0.06f, 1f);

            cameraObject.GetComponent<PortraitFollowCamera>().Initialize(target, _cameraSettings);
            return cameraObject.transform;
        }

        private void CreateLight()
        {
            if (_visualEnvironment != null && _visualEnvironment.KeyLight != null) return;
            var lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var lightComponent = lightObject.GetComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.1f;
            lightComponent.shadows = LightShadows.None;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _developmentTelemetry?.Dispose();
#endif
            SaveCoordinator?.FlushNow();
            SaveCoordinator?.Dispose();
            ChapterCompletion?.Shutdown();
            OfflineRewardPanel?.Shutdown();
            PauseMenu?.Resume();
            BossHealthHud?.Shutdown();
            PlayerStatsHud?.Shutdown();
            PlayerHealthHud?.Shutdown();
            _hudModal?.Dispose();
            EvolutionPresenter?.Shutdown();
            CombatFeedback?.Shutdown();
            EncounterTelegraphs?.Shutdown();
            QuestTracker?.Shutdown();
            CustodianBoss?.Shutdown();
            Quests?.Dispose();
            Chapter1Encounters?.Dispose();
            WorldPresenter?.Shutdown();
            WorldUnlocks?.Dispose();
            Progression?.Dispose();

            if (PlayerHealth != null)
            {
                PlayerHealth.Respawned -= HandlePlayerRespawned;
            }

            if (_eliteMaterial != null) Destroy(_eliteMaterial);
            if (_bossMaterial != null) Destroy(_bossMaterial);

        }

        private void Update()
        {
            _repeatRetryRemaining -= Time.unscaledDeltaTime;
            if (_isComposed && _repeatRetryRemaining <= 0f)
            {
                Chapter1Encounters.Tick();
                EvolutionPresenter.ApplyCurrentState();
                _repeatRetryRemaining = 1f;
            }
            SaveCoordinator?.Tick(Time.unscaledDeltaTime);
        }

        private void OnApplicationPause(bool paused)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _developmentTelemetry?.RecordPause(paused);
#endif
            if (paused)
            {
                SaveCoordinator?.FlushNow();
            }
            else
            {
                if (SaveCoordinator != null)
                {
                    var summary = SaveCoordinator.ProcessResume();
                    OfflineRewardPanel?.ShowReturnSummary(summary);
                }
            }
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _developmentTelemetry?.EndSession();
#endif
            SaveCoordinator?.FlushNow();
        }

        private Material CreateMaterial(Color color)
        {
            return _materialPalette.CreateLitInstance(color);
        }
    }
}
