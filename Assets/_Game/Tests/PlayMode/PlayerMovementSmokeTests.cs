using System.Collections;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Input;
using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gravivore.Tests.PlayMode
{
    public sealed class PlayerMovementSmokeTests
    {
        [UnityTest]
        public IEnumerator ChapterScene_ComposesPlayerCameraAndSafeAreaHud()
        {
            var loadOperation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation);
            yield return loadOperation;
            yield return null;

            S01SceneCompositionRoot compositionRoot = null;
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].TryGetComponent(out compositionRoot))
                {
                    break;
                }
            }

            Assert.IsNotNull(compositionRoot);
            Assert.IsNotNull(compositionRoot.PlayerObject);
            Assert.IsNotNull(compositionRoot.PlayerStats);
            Assert.That(compositionRoot.PlayerStats.MoveSpeed, Is.EqualTo(4.5f).Within(0.0001f));
            Assert.IsNotNull(UnityEngine.Camera.main);
            Assert.IsNotNull(compositionRoot.GetComponentInChildren<SafeAreaHudRoot>());

            var canvasScaler = compositionRoot.GetComponentInChildren<CanvasScaler>();
            Assert.IsNotNull(canvasScaler);
            Assert.That(canvasScaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(canvasScaler.referenceResolution, Is.EqualTo(new Vector2(1080f, 1920f)));

            var joystickView = compositionRoot.GetComponentInChildren<FloatingJoystickView>(true);
            Assert.IsNotNull(joystickView);
            var joystickImages = joystickView.GetComponentsInChildren<Image>(true);
            Assert.That(joystickImages, Has.Length.EqualTo(2));
            for (var i = 0; i < joystickImages.Length; i++)
            {
                Assert.IsFalse(joystickImages[i].raycastTarget);
            }

            Assert.IsNotNull(compositionRoot.GetComponent<UiTouchExclusion>());
        }

        [UnityTest]
        public IEnumerator PlayerLocomotion_MovesSpawnedPlayerFromInjectedInput()
        {
            var player = new GameObject("Movement Smoke Player", typeof(CharacterController), typeof(PlayerLocomotion));
            var cameraBasis = new GameObject("Movement Smoke Camera Basis");
            player.transform.position = new Vector3(10f, 0f, 10f);
            var input = new StubMovementInput();
            var locomotion = player.GetComponent<PlayerLocomotion>();
            locomotion.Initialize(input, cameraBasis.transform, new PlayerMovementParameters(4f, 720f));
            var initialPosition = player.transform.position;

            yield return null;

            Assert.That(Vector3.Distance(player.transform.position, initialPosition), Is.LessThan(0.001f));

            input.Movement = Vector2.up;
            yield return null;

            Assert.That(player.transform.position.z, Is.GreaterThan(initialPosition.z));
            Assert.That(player.transform.position.x, Is.EqualTo(initialPosition.x).Within(0.001f));

            Object.Destroy(player);
            Object.Destroy(cameraBasis);
        }

        private sealed class StubMovementInput : IMovementInput
        {
            public Vector2 Movement { get; set; }
        }
    }
}
