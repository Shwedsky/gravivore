using System;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Input;
using Gravivore.Presentation.UI;
using Gravivore.Presentation.World;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Composition
{
    [DisallowMultipleComponent]
    public sealed class S01SceneCompositionRoot : MonoBehaviour
    {
        private static readonly Vector2 HudReferenceResolution = new Vector2(1080f, 1920f);

        [SerializeField] private PlayerMovementSettings _movementSettings;
        [SerializeField] private PlayerStatsDefinition _playerStatsDefinition;
        [SerializeField] private GravityAttackSettings _gravityAttackSettings;
        [SerializeField] private SpawnSpotDefinition[] _spawnSpotDefinitions;
        [SerializeField] private PlayerProgressionDefinition _progressionDefinition;
        [SerializeField] private EvolutionDefinition _evolutionDefinition;
        [SerializeField] private Chapter01WorldDefinition _worldDefinition;
        [SerializeField, Min(1)] private int _globalLiveEnemyCap = 25;
        [SerializeField] private FloatingJoystickSettings _joystickSettings;
        [SerializeField] private CameraFollowSettings _cameraSettings;
        [SerializeField] private Vector3 _playerSpawn = Vector3.zero;
        [SerializeField, Min(0f)] private float _postRespawnInvulnerabilitySeconds = 1.5f;

        private Material _playerMaterial;
        private Transform _playerVisualRoot;
        private bool _isComposed;

        public GameObject PlayerObject { get; private set; }

        public PlayerStatsState PlayerStats { get; private set; }

        public PlayerHealthController PlayerHealth { get; private set; }

        public EnemyPopulationController EnemyPopulation { get; private set; }

        public AssimilationProgressionService Progression { get; private set; }

        public PlayerEvolutionPresenter EvolutionPresenter { get; private set; }

        public WorldUnlockService WorldUnlocks { get; private set; }

        public Chapter01WorldPresenter WorldPresenter { get; private set; }

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

            if (_movementSettings == null || _playerStatsDefinition == null || _gravityAttackSettings == null ||
                _spawnSpotDefinitions == null || _spawnSpotDefinitions.Length != 5 || _globalLiveEnemyCap < 1 ||
                _progressionDefinition == null || _evolutionDefinition == null || _worldDefinition == null ||
                _joystickSettings == null || _cameraSettings == null ||
                float.IsNaN(_postRespawnInvulnerabilitySeconds) ||
                float.IsInfinity(_postRespawnInvulnerabilitySeconds) ||
                _postRespawnInvulnerabilitySeconds < 0f)
            {
                throw new InvalidOperationException(
                    "Scene composition requires movement, stats, attack, progression, evolution, world, five spawn spots, joystick, and camera settings.");
            }

            PlayerStats = _playerStatsDefinition.CreateState();
            CreateHud(out var uiTouchExclusion, out var joystickView);
            var movementInput = CreateMovementInput(uiTouchExclusion, joystickView);
            var locomotion = CreatePlayer();
            var cameraTransform = CreateCamera(PlayerObject.transform);

            locomotion.Initialize(
                movementInput,
                cameraTransform,
                PlayerStats,
                _movementSettings.RotationDegreesPerSecond);
            InitializePlayerHealth();
            InitializeGravityAttack();
            CreateLight();
            InitializeEnemyPopulation();
            Progression = new AssimilationProgressionService(
                PlayerStats,
                new ProgressionState(),
                _progressionDefinition.Configuration,
                EnemyPopulation);
            InitializeWorld();
            InitializeEvolution();
            _isComposed = true;
        }

        private void CreateHud(
            out UiTouchExclusion uiTouchExclusion,
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

            var exclusionObject = new GameObject("HUD Touch Exclusion", typeof(RectTransform));
            var exclusionTransform = exclusionObject.GetComponent<RectTransform>();
            exclusionTransform.SetParent(safeAreaTransform, false);
            exclusionTransform.anchorMin = new Vector2(0f, 0.82f);
            exclusionTransform.anchorMax = Vector2.one;
            exclusionTransform.offsetMin = Vector2.zero;
            exclusionTransform.offsetMax = Vector2.zero;

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

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Player Visual";
            visual.transform.SetParent(_playerVisualRoot, false);
            visual.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            visual.transform.localScale = new Vector3(0.65f, 0.7f, 0.65f);
            var visualCollider = visual.GetComponent<Collider>();
            visualCollider.enabled = false;
            Destroy(visualCollider);

            _playerMaterial = CreateMaterial(new Color(0.12f, 0.82f, 0.68f, 1f));
            if (_playerMaterial != null)
            {
                visual.GetComponent<Renderer>().sharedMaterial = _playerMaterial;
            }

            return PlayerObject.GetComponent<PlayerLocomotion>();
        }

        private void InitializeEvolution()
        {
            var catalog = _evolutionDefinition.Catalog;
            var view = PlayerObject.GetComponent<PlayerEvolutionView>();
            view.Initialize(_playerVisualRoot, catalog);
            EvolutionPresenter = PlayerObject.GetComponent<PlayerEvolutionPresenter>();
            EvolutionPresenter.Initialize(
                Progression,
                PlayerStats,
                catalog.Selection,
                view,
                PlayerObject.GetComponent<EvolutionVfxRelay>());
        }

        private void InitializeWorld()
        {
            var configuration = _worldDefinition.Configuration;
            var state = new WorldUnlockState(
                configuration.EliteGate.Id,
                configuration.BossGate.Id,
                configuration.EliteEnemyId);
            WorldUnlocks = new WorldUnlockService(Progression, configuration.EliteRequirement, state);

            var worldObject = new GameObject("Chapter 01 World", typeof(Chapter01WorldPresenter));
            worldObject.transform.SetParent(transform, false);
            WorldPresenter = worldObject.GetComponent<Chapter01WorldPresenter>();
            WorldPresenter.Initialize(configuration, state);
        }

        private void InitializeGravityAttack()
        {
            var vfxObject = new GameObject("Gravity Lash VFX Pool", typeof(GravityLashVfxPool));
            vfxObject.transform.SetParent(transform, false);
            var vfxPool = vfxObject.GetComponent<GravityLashVfxPool>();
            vfxPool.Initialize(_gravityAttackSettings);

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
                vfxPool);
        }

        private void InitializePlayerHealth()
        {
            PlayerHealth = PlayerObject.GetComponent<PlayerHealthController>();
            PlayerHealth.Initialize(
                PlayerObject.GetComponent<CharacterController>(),
                PlayerStats,
                _playerSpawn,
                _postRespawnInvulnerabilitySeconds);
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

            var configurations = new SpawnSpotRuntimeConfiguration[_spawnSpotDefinitions.Length];
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
                targetLayer);
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
            EvolutionPresenter?.Shutdown();
            WorldPresenter?.Shutdown();
            WorldUnlocks?.Dispose();
            Progression?.Dispose();

            if (PlayerHealth != null)
            {
                PlayerHealth.Respawned -= HandlePlayerRespawned;
            }

            if (_playerMaterial != null)
            {
                Destroy(_playerMaterial);
            }

        }

        private static Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                return null;
            }

            return new Material(shader)
            {
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
    }
}
