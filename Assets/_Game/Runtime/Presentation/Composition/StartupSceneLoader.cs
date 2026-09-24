using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravivore.Presentation.Composition
{
    [DisallowMultipleComponent]
    public sealed class StartupSceneLoader : MonoBehaviour
    {
        [SerializeField, Min(0)] private int _targetSceneBuildIndex = 1;

        private void Start()
        {
            if (_targetSceneBuildIndex < 0 || _targetSceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                throw new InvalidOperationException(
                    $"Startup scene build index {_targetSceneBuildIndex} is not configured in build settings.");
            }

            SceneManager.LoadSceneAsync(_targetSceneBuildIndex, LoadSceneMode.Single);
        }
    }
}
