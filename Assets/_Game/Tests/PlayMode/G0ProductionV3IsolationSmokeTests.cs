#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class G0ProductionV3IsolationSmokeTests
    {
        [UnityTest]
        public IEnumerator ImportedVisualAnimationsNeverMoveGameplayAuthority()
        {
            var scope=new CanonicalSceneTestScope();yield return scope.Load();
            var player=scope.Root.PlayerObject;var body=player.GetComponent<CharacterController>();
            var position=player.transform.position;var camera=Camera.main;
            var cameraPosition=camera.transform.position;var cameraRotation=camera.transform.rotation;
            var gate=scope.Root.WorldPresenter.EliteGate.BlockingCollider.bounds;
            var isolated=SceneManager.CreateScene("G0 V3 isolated animation smoke");
            var review=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/ArtReview/G0ProductionV3/G0_Production_V3_ArtReview.prefab"));
            SceneManager.MoveGameObjectToScene(review,isolated);review.transform.position=new Vector3(1000,0,0);review.transform.localScale=Vector3.one*.4585482776f;
            var animator=review.GetComponent<Animator>();Assert.IsFalse(animator.applyRootMotion);
            Assert.IsEmpty(review.GetComponentsInChildren<Collider>(true));
            foreach(var name in new[]{"Idle","Run","Attack","Hit","Death"})
            {
                var joint=review.GetComponentsInChildren<Transform>().Single(t=>t.name==(name=="Idle"?"TORSO":"R_SHOULDER"));
                animator.Play(name,0,0);animator.Update(0);var before=joint.localRotation;
                animator.Play(name,0,.45f);animator.Update(0);
                Assert.That(Quaternion.Angle(before,joint.localRotation),Is.GreaterThan(.01f),name+" imported pose must animate");
                Assert.That(review.transform.position,Is.EqualTo(new Vector3(1000,0,0)),name+" root motion");
                Assert.That(body.radius,Is.EqualTo(.42f));Assert.That(body.height,Is.EqualTo(1.4f));Assert.That(body.center,Is.EqualTo(new Vector3(0,.7f,0)));
                Assert.That(player.transform.position,Is.EqualTo(position));Assert.That(player.transform.localScale,Is.EqualTo(Vector3.one));
                Assert.That(camera.fieldOfView,Is.EqualTo(46));Assert.That(camera.transform.position,Is.EqualTo(cameraPosition));Assert.That(camera.transform.rotation,Is.EqualTo(cameraRotation));
                Assert.That(scope.Root.WorldPresenter.EliteGate.BlockingCollider.bounds,Is.EqualTo(gate));
            }
            yield return SceneManager.UnloadSceneAsync(isolated);yield return scope.Cleanup();
        }
    }
}
#endif
