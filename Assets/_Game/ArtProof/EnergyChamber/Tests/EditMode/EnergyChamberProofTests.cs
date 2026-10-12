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

    [Test]
    public void EnergyChamberProof_FbxAssemblyRetainsSourceUnitsAndElevatedConnections()
    {
        var instance=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EnergyChamberProofBuilder.PrefabPath));
        try
        {
            foreach(var route in instance.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Core-to-node route ")))
            {
                var bounds=EnergyChamberProofBuilder.BoundsOf(route.gameObject);
                Assert.That(bounds.min.y,Is.GreaterThan(.5f),"A lost FBX axis rotation puts the conduit below its carrier.");
                Assert.That(bounds.max.y,Is.GreaterThan(2f),"Feed must meet the raised central pressure seat.");
            }
            var stairs=instance.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Service stair ")).ToArray();
            Assert.That(stairs.Length,Is.EqualTo(2));
            foreach(var stair in stairs)
            {
                var bounds=EnergyChamberProofBuilder.BoundsOf(stair.gameObject);
                Assert.That(bounds.min.y,Is.EqualTo(.015f).Within(.02f));
                Assert.That(bounds.max.y,Is.EqualTo(1.325f).Within(.02f));
            }
            foreach(var rail in instance.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Service quadrant safety rail ")))
                Assert.That(EnergyChamberProofBuilder.BoundsOf(rail.gameObject).size.y,Is.GreaterThan(.5f),"The original FBX unit conversion must survive uniform placement scale.");
        }
        finally {Object.DestroyImmediate(instance);}
    }
}
