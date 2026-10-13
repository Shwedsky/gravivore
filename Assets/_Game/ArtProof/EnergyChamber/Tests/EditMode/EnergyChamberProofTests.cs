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
    public void EnergyChamberProof_VerticalStabilizersKeepReservoirsAndWarmCoolMaterialHierarchy()
    {
        var instance=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EnergyChamberProofBuilder.PrefabPath));
        try
        {
            var nodes=instance.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Distribution node ")).ToArray();
            foreach(var node in nodes)
            {
                var module=node.Find("EC_VerticalDistribution");Assert.That(module,Is.Not.Null);
                Assert.That(EnergyChamberProofBuilder.BoundsOf(module.gameObject).max.y,Is.GreaterThan(4f));
                Assert.That(node.Find("VR3_generator"),Is.Not.Null,"The horizontal reservoir remains supporting machinery.");
                var cool=module.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Single(m=>m.name=="EC_NodeCool");
                Assert.That(cool.IsKeywordEnabled("_EMISSION"),Is.True);
                Assert.That(cool.GetColor("_EmissionColor").b,Is.GreaterThan(cool.GetColor("_EmissionColor").r*3));
            }
            var core=instance.transform.Find("EC_CentralChamber");
            var warm=core.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Single(m=>m.name=="EC_CoreWarm");
            Assert.That(warm.GetColor("_EmissionColor").r,Is.GreaterThan(warm.GetColor("_EmissionColor").b*3));
            Assert.That(EnergyChamberProofBuilder.BoundsOf(core.gameObject).max.y,Is.GreaterThan(6f));
        }
        finally {Object.DestroyImmediate(instance);}
    }

    [Test]
    public void EnergyChamberProof_PolishPreservesAcceptedFootprintAndCardinalNodeTransforms()
    {
        var instance=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(EnergyChamberProofBuilder.PrefabPath));
        try
        {
            var nodes=instance.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Distribution node ")).OrderBy(t=>t.name).ToArray();
            for(var i=0;i<4;i++)
            {
                var rotation=Quaternion.Euler(0,i*90,0);
                Assert.That(Vector3.Distance(nodes[i].localPosition,rotation*new Vector3(4.40f,0,0)),Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(nodes[i].localRotation,rotation),Is.LessThan(.001f));
            }
            Assert.That(instance.transform.Find("EC_CentralChamber").localPosition,Is.EqualTo(Vector3.zero));
            var bounds=EnergyChamberProofBuilder.BoundsOf(instance);
            Assert.That(bounds.size.x,Is.EqualTo(11.48f).Within(.02f));
            Assert.That(bounds.size.z,Is.EqualTo(11.48f).Within(.02f));
            Assert.That(bounds.min.y,Is.EqualTo(.015f).Within(.02f));
            Assert.That(bounds.max.y,Is.EqualTo(6.11f).Within(.02f));
        }
        finally {Object.DestroyImmediate(instance);}
    }

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
    public void EnergyChamberProof_ChapterContextRetainsChapterPresentationAndExactPlayerFollowAnchor()
    {
        var scene=EditorSceneManager.OpenScene(EnergyChamberChapterContext.ScenePath,OpenSceneMode.Additive);
        try
        {
            var roots=scene.GetRootGameObjects();
            var assembly=roots.Single(g=>g.name.StartsWith("Energy Chamber /"));
            Assert.That(assembly.transform.position,Is.EqualTo(EnergyChamberChapterContext.InstallationPosition));
            var camera=roots.SelectMany(g=>g.GetComponentsInChildren<Camera>()).Single();
            var settings=AssetDatabase.LoadAssetAtPath<CameraFollowSettings>(EnergyChamberProofBuilder.CameraSettingsPath);
            Assert.That(Vector3.Distance(camera.transform.position,EnergyChamberChapterContext.PlayerPosition+settings.Offset),Is.LessThan(.001f));
            Assert.That(camera.fieldOfView,Is.EqualTo(settings.FieldOfView));
            var look=EnergyChamberChapterContext.PlayerPosition+Vector3.up*settings.LookAtHeight-camera.transform.position;
            Assert.That(Vector3.Angle(camera.transform.forward,look),Is.LessThan(.01f));
            var context=roots.Single(g=>g.name=="S01 Composition Root");
            Assert.That(context.GetComponentsInChildren<Renderer>(true).Length,Is.EqualTo(476));
            Assert.That(roots.SelectMany(g=>g.GetComponentsInChildren<Light>(true)).Count(),Is.EqualTo(17));
            Assert.That(context.GetComponentsInChildren<Collider>(true),Is.Empty);
            // Package components keep the authored lighting/volume settings;
            // application behaviours cannot enter the static review scene.
            Assert.That(context.GetComponentsInChildren<MonoBehaviour>(true).All(m=>m.GetType().Namespace?.StartsWith("UnityEngine.Rendering")==true),Is.True);
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
