using System;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Input;
using Gravivore.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Composition
{
    [DisallowMultipleComponent]
    public sealed class S01SceneCompositionRoot : MonoBehaviour
    {
        private static readonly Vector2 HudReferenceResolution = new Vector2(1080f, 1920f);

        [SerializeField] private PlayerMovementSettings _movementSettings;
        [SerializeField] private FloatingJoystickSettings _joystickSettings;
        [SerializeField] private CameraFollowSettings _cameraSettings;
        [SerializeField] private Vector3 _playerSpawn = Vector3.zero;

        private Material _playerMaterial;
        private Material _groundMaterial;
        private bool _isComposed;

        public GameObject PlayerObject { get; private set; }

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

            if (_movementSettings == null || _joystickSettings == null || _cameraSettings == null)
            {
                throw new InvalidOperationException("S01 composition requires movement, joystick, and camera settings.");
            }

            CreateHud(out var uiTouchExclusion, out var joystickView);
            var movementInput = CreateMovementInput(uiTouchExclusion, joystickView);
            var locomotion = CreatePlayer();
            var cameraTransform = CreateCamera(PlayerObject.transform);

            locomotion.Initialize(movementInput, cameraTransform, _movementSettings.Parameters);
            CreateGround();
            CreateLight();
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
            PlayerObject = new GameObject("Player", typeof(CharacterController), typeof(PlayerLocomotion));
            PlayerObject.transform.SetParent(transform, false);
            PlayerObject.transform.position = _playerSpawn;

            var characterController = PlayerObject.GetComponent<CharacterController>();
            characterController.height = 1.4f;
            characterController.radius = 0.42f;
            characterController.center = new Vector3(0f, 0.7f, 0f);
            characterController.stepOffset = 0.25f;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Player Visual";
            visual.transform.SetParent(PlayerObject.transform, false);
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

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Movement Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            ground.transform.localScale = new Vector3(3f, 1f, 3f);

            _groundMaterial = CreateMaterial(new Color(0.09f, 0.12f, 0.14f, 1f));
            if (_groundMaterial != null)
            {
                ground.GetComponent<Renderer>().sharedMaterial = _groundMaterial;
            }
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
            if (_playerMaterial != null)
            {
                Destroy(_playerMaterial);
            }

            if (_groundMaterial != null)
            {
                Destroy(_groundMaterial);
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
