using System;
using System.Collections;
using System.IO;
using Gravivore.Core.Time;
using Gravivore.Presentation.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravivore.Tests.PlayMode
{
    internal sealed class CanonicalSceneTestScope
    {
        private readonly string _directory = Path.Combine(
            Path.GetTempPath(),
            "gravivore-canonical-playmode-" + Guid.NewGuid().ToString("N"));
        private readonly FixedTimeProvider _time = new FixedTimeProvider(
            new DateTime(2031, 4, 5, 12, 0, 0, DateTimeKind.Utc));

        public S01SceneCompositionRoot Root { get; private set; }
        public void AdvanceTime(TimeSpan duration) => _time.UtcNow += duration;

        public IEnumerator Load(Action<S01SceneCompositionRoot> configure = null)
        {
            void Configure(Scene scene, LoadSceneMode mode)
            {
                var roots = scene.GetRootGameObjects();
                for (var i = 0; i < roots.Length; i++)
                {
                    if (!roots[i].TryGetComponent(out S01SceneCompositionRoot root)) continue;
                    Root = root;
                    Root.ConfigurePersistence(_directory, _time);
                    configure?.Invoke(Root);
                    return;
                }
            }

            SceneManager.sceneLoaded += Configure;
            var operation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(operation);
            yield return operation;
            SceneManager.sceneLoaded -= Configure;
            yield return null;
            Assert.IsNotNull(Root);
        }

        public IEnumerator Cleanup()
        {
            if (Root != null) UnityEngine.Object.Destroy(Root.gameObject);
            yield return null;
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
            Root = null;
        }

        private sealed class FixedTimeProvider : ITimeProvider
        {
            public FixedTimeProvider(DateTime utcNow) => UtcNow = utcNow;
            public DateTime UtcNow { get; set; }
        }
    }
}
