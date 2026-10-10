#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gravivore.ArtReview.Tests
{
    public sealed class ActorProductionSmokeTests
    {
        [UnityTest] public IEnumerator ReviewSceneOffersEveryActorPhaseAndLod()
        {
            var scene = EditorSceneManager.LoadSceneInPlayMode("Assets/_Game/ArtReview/ActorProductionV2/ActorProductionV2_Review.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var review = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<ActorReviewPlayback>(true)).Single();
            Assert.AreEqual(7, review.ActorCount);
            for (var index = 0; index < 7; index++)
            {
                review.ShowActor(index); Assert.AreEqual(index, review.SelectedIndex);
                Assert.GreaterOrEqual(review.ReviewPoseCount, 5);
                if (index == 6) Assert.AreEqual(9, review.ReviewPoseCount, "All boss phase presentations must be available on device");
                for (var pose = 0; pose < review.ReviewPoseCount; pose++) review.PlayPose(pose);
                for (var lod = -1; lod < 3; lod++) review.SetLod(lod);
                yield return null;
            }
            review.ShowFamily(); yield return null;
            Assert.AreEqual(7, scene.GetRootGameObjects().Count(x => x.name.EndsWith("_V2_Review") && x.activeSelf));
        }
        [UnityTest] public IEnumerator CompleteFamilyAnimatesReactivatesAndKeepsGameplayRootStationary()
        {
            foreach (var actor in new[] { "Scout", "Cutter", "Warden", "ArcDrone", "Carrier", "Magnetar", "Custodian" })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/ArtReview/ActorProductionV2/Prefabs/" + actor + "_V2_Review.prefab");
                Assert.NotNull(prefab, actor); var go = Object.Instantiate(prefab);
                try
                {
                    var animator = go.GetComponent<Animator>(); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; animator.applyRootMotion = false;
                    animator.Rebind(); animator.Play("Idle", 0, 0); animator.Update(0);
                    var bones = go.GetComponentsInChildren<SkinnedMeshRenderer>().First().bones;
                    var initialRotations = bones.Select(x => x.localRotation).ToArray();
                    foreach (var state in new[] { "Idle", "Run", "Attack", "Hit", "Death" })
                    {
                        animator.Rebind(); animator.Play(state, 0, .25f); animator.Update(.02f); yield return null;
                        Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName(state), actor + "/" + state);
                        Assert.That(go.transform.position.sqrMagnitude, Is.LessThan(.000001f));
                        Assert.AreEqual(0, go.GetComponentsInChildren<Collider>().Length);
                        if (state == "Attack")
                            Assert.IsTrue(bones.Where((bone, i) => Quaternion.Angle(bone.localRotation, initialRotations[i]) > 2).Any(), actor + " attack must articulate the mechanism");
                    }
                    go.SetActive(false); yield return null; go.SetActive(true); animator.Rebind(); animator.Play("Idle", 0, 0); animator.Update(0); yield return null;
                    Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), actor + " reactivation");
                    Assert.AreEqual(3, go.GetComponent<LODGroup>().GetLODs().Length);
                    Assert.IsTrue(go.GetComponentsInChildren<SkinnedMeshRenderer>().All(x => x.bones.All(b => b != null)));
                }
                finally { Object.Destroy(go); }
                yield return null;
            }
        }
    }
}
#endif
