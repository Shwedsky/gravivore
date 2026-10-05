#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using Gravivore.Presentation.AudioVfx;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Phase6BAudioVfxSmokeTests
    {
        private const string Root = "Assets/_Game/Content/Presentation/Phase6B";

        [UnityTest]
        public IEnumerator RuntimeRendering_PreservesPlayerHostileAndRepairCueTints()
        {
            var cameraObject = new GameObject("Isolated Cue Tint Camera", typeof(UnityEngine.Camera));
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 1.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = 1 << 31;
            var origin = new Vector3(1000f, 0f, 0f);
            var destination = origin + Vector3.forward * 2f;
            camera.transform.position = origin + new Vector3(0f, 4f, 1f);
            camera.transform.LookAt(origin + Vector3.forward);
            var target = new RenderTexture(128, 128, 24);
            var pixels = new Texture2D(128, 128, TextureFormat.RGB24, false);
            var priorTarget = RenderTexture.active;
            Phase6BVfxInstance instance = null;
            try
            {
                target.Create();
                camera.targetTexture = target;
                var files = new[] { "PFX_GravityLashTravel", "PFX_BossLineTelegraph", "PFX_RepairBeam" };
                for (var cue = 0; cue < files.Length; cue++)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/VFX/Prefabs/" + files[cue] + ".prefab");
                    instance = Object.Instantiate(prefab).GetComponent<Phase6BVfxInstance>();
                    instance.enabled = false; // Advance presentation time explicitly, independent of editor frame rate.
                    instance.Play(origin, destination);
                    instance.Tick(0.06f);
                    foreach (var child in instance.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
                    yield return null;
                    camera.Render();
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0f, 0f, 128f, 128f), 0, 0);
                    pixels.Apply(false);
                    var captured = pixels.GetPixels();
                    // Player and repair energy are authored cyan; hostile telegraphs are red.
                    var hasCueTint = cue == 1
                        ? captured.Any(c => c.r > c.g + 0.1f && c.r > c.b + 0.1f)
                        : captured.Any(c => c.g > c.r + 0.1f && c.b > c.r + 0.1f);
                    Assert.IsTrue(hasCueTint, files[cue] + " must render its cue tint, rather than white.");
                    Object.Destroy(instance.gameObject);
                    instance = null;
                    yield return null;
                }
            }
            finally
            {
                RenderTexture.active = priorTarget;
                camera.targetTexture = null;
                if (instance != null) Object.Destroy(instance.gameObject);
                Object.Destroy(cameraObject);
                target.Release();
                Object.Destroy(target);
                Object.Destroy(pixels);
            }
        }

        [UnityTest]
        public IEnumerator RuntimePools_PlayEveryCueRemainBoundedAndStop()
        {
            var host = new GameObject("Phase 6B Runtime Pool Test");
            var listener = new GameObject("Isolated Audio Listener", typeof(AudioListener));
            try
            {
                var bank = AssetDatabase.LoadAssetAtPath<Phase6BAudioBank>(Root + "/Audio/Phase6B_AudioBank.asset");
                var audio = host.AddComponent<Phase6BAudioPlayer>();
                audio.Initialize(bank, 6);
                foreach (Phase6BAudioCue cue in Enum.GetValues(typeof(Phase6BAudioCue)))
                {
                    Assert.IsTrue(audio.TryPlay(cue, Vector3.zero), cue.ToString());
                    Assert.IsTrue(host.GetComponentsInChildren<AudioSource>().Any(s => s.isPlaying), cue.ToString());
                    yield return null;
                }
                Assert.IsFalse(audio.TryPlay((Phase6BAudioCue)999, Vector3.zero));
                for (var i = 0; i < 200; i++) Assert.IsTrue(audio.TryPlay(Phase6BAudioCue.EnemyMechanicalHit, Vector3.zero, i));
                Assert.That(host.GetComponentsInChildren<AudioSource>().Length, Is.EqualTo(6));
                audio.StopAll();
                Assert.IsTrue(host.GetComponentsInChildren<AudioSource>().All(s => !s.isPlaying));

                var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { Root + "/VFX/Prefabs" })
                    .Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)).GetComponent<Phase6BVfxInstance>()).ToArray();
                Assert.That(prefabs.Length, Is.EqualTo(13));
                var vfx = host.AddComponent<Phase6BVfxPool>();
                vfx.Initialize(prefabs.Select(p => new Phase6BVfxPool.Binding(p.Cue, p, 2)).ToArray());
                foreach (var prefab in prefabs)
                    for (var i = 0; i < 20; i++) Assert.IsTrue(vfx.TryPlay(prefab.Cue, Vector3.zero, Vector3.forward * 4));
                Assert.That(vfx.CreatedInstanceCount, Is.EqualTo(26));
                Assert.That(vfx.ActiveCount, Is.LessThanOrEqualTo(26));
                yield return null;
                foreach (var particle in host.GetComponentsInChildren<ParticleSystem>(true))
                    Assert.That(particle.particleCount, Is.LessThanOrEqualTo(particle.main.maxParticles));
                vfx.StopAll();
                Assert.That(vfx.ActiveCount, Is.Zero);
            }
            finally
            {
                Object.Destroy(host);
                Object.Destroy(listener);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator IsolatedReviewScene_HasAudibleListenerAndCompletesReplay()
        {
            const string path = "Assets/_Game/ArtReview/Scenes/Phase6B_AudioVfxReview.unity";
            var prior = SceneManager.GetActiveScene();
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path,
                new LoadSceneParameters(LoadSceneMode.Additive));
            var scene = SceneManager.GetSceneByPath(path);
            try
            {
                var roots = scene.GetRootGameObjects();
                Assert.That(roots.SelectMany(r => r.GetComponentsInChildren<AudioListener>()).Count(), Is.EqualTo(1),
                    "The standalone review scene needs one AudioListener for audible preview.");
                var driver = roots.SelectMany(r => r.GetComponentsInChildren<Phase6BReviewDriver>()).Single();
                yield return null;
                var audio = driver.GetComponentInChildren<Phase6BAudioPlayer>();
                var vfx = driver.GetComponentInChildren<Phase6BVfxPool>();
                Assert.That(audio.PoolSize, Is.EqualTo(6));
                Assert.That(vfx.CreatedInstanceCount, Is.EqualTo(27));
                driver.Replay();
                yield return new WaitForSeconds(6.6f);
                Assert.That(vfx.ActiveCount, Is.Zero);
                Assert.IsTrue(audio.GetComponentsInChildren<AudioSource>().All(s => !s.isPlaying));
            }
            finally { SceneManager.SetActiveScene(prior); }
            yield return SceneManager.UnloadSceneAsync(scene);
        }
    }
}
#endif
