#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class G0V21GameplayIsolationSmokeTests
    {
        [UnityTest]
        public IEnumerator ChangingReviewGeometryNeverChangesCanonicalPlayerOrCamera()
        {
            var scope=new CanonicalSceneTestScope();yield return scope.Load();
            var player=scope.Root.PlayerObject;var body=player.GetComponent<CharacterController>();
            var position=player.transform.position;var camera=Camera.main;
            var cameraPosition=camera.transform.position;var cameraRotation=camera.transform.rotation;
            var gate=scope.Root.WorldPresenter.EliteGate.BlockingCollider.bounds;
            Assert.IsNull(player.transform.Find("G-0 V2.1 PRESENTATION ONLY"));
            var scene=SceneManager.CreateScene("G0 V21 isolated geometry smoke");
            var review=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/ArtReview/G0V21/Models/G0_Bipedal_V21_Review.fbx"));
            SceneManager.MoveGameObjectToScene(review,scene);
            Assert.IsEmpty(review.GetComponentsInChildren<Collider>(true));
            Assert.IsEmpty(review.GetComponentsInChildren<MonoBehaviour>(true));
            foreach(var scale in new[]{.37517586f,.41686207f,.45854828f})
            {
                review.transform.localScale=Vector3.one*scale;
                Assert.That(body.radius,Is.EqualTo(.42f));Assert.That(body.height,Is.EqualTo(1.4f));
                Assert.That(body.center,Is.EqualTo(new Vector3(0,.7f,0)));
                Assert.That(player.transform.position,Is.EqualTo(position));Assert.That(player.transform.localScale,Is.EqualTo(Vector3.one));
                Assert.That(camera.fieldOfView,Is.EqualTo(46));Assert.That(camera.transform.position,Is.EqualTo(cameraPosition));
                Assert.That(camera.transform.rotation,Is.EqualTo(cameraRotation));
                Assert.That(scope.Root.WorldPresenter.EliteGate.BlockingCollider.bounds,Is.EqualTo(gate));
            }
            yield return SceneManager.UnloadSceneAsync(scene);yield return scope.Cleanup();
        }
    }
}
#endif
