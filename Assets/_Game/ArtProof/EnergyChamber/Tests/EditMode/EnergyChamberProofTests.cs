using System.Linq;
using Gravivore.ArtProof.EnergyChamber;
using Gravivore.Presentation.Camera;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class EnergyChamberProofTests
{
    [Test]
    public void EnergyChamberProof_HasFourInstancesOfTheSameMasterNodeAndNoGameplayAuthority()
    {
        EnergyChamberProofBuilder.Validate();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(EnergyChamberProofBuilder.PrefabPath);
        var nodes=prefab.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Distribution node ")).ToArray();
        Assert.That(nodes.Length,Is.EqualTo(4));
        foreach(var node in nodes)
            Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(node.gameObject),Is.EqualTo(EnergyChamberProofBuilder.NodePath));
        Assert.That(prefab.GetComponentsInChildren<Collider>(true),Is.Empty);
        Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true),Is.Empty);
    }

    [Test]
    public void EnergyChamberProof_SceneUsesCurrentGameplayLensAndUnscaledG0()
    {
        var scene=EditorSceneManager.OpenScene(EnergyChamberProofBuilder.ScenePath,OpenSceneMode.Additive);
        try
        {
            var roots=scene.GetRootGameObjects();
            var camera=roots.SelectMany(g=>g.GetComponentsInChildren<Camera>()).Single();
            var settings=AssetDatabase.LoadAssetAtPath<CameraFollowSettings>(EnergyChamberProofBuilder.CameraSettingsPath);
            Assert.That(camera.orthographic,Is.False);
            Assert.That(camera.fieldOfView,Is.EqualTo(settings.FieldOfView).Within(.001));
            var g0=roots.Single(g=>g.name.StartsWith("G-0 /"));
            Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(g0),Is.EqualTo(EnergyChamberProofBuilder.G0Path));
            Assert.That(g0.transform.localScale,Is.EqualTo(Vector3.one));
        }
        finally {EditorSceneManager.CloseScene(scene,true);}
    }
}
