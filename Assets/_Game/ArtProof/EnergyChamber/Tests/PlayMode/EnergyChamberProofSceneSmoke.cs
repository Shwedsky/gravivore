#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class EnergyChamberProofSceneSmoke
{
    [UnityTest]
    public IEnumerator EnergyChamberProof_LoadsAndRendersWithoutGameplayInitialization()
    {
        const string path="Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_HybridProof.unity";
        return LoadAndCheck(path);
    }
    [UnityTest]
    public IEnumerator EnergyChamberProof_ChapterContextLoadsWithoutGameplayInitialization()
    {
        return LoadAndCheck("Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_Chapter01Context.unity");
    }
    private static IEnumerator LoadAndCheck(string path)
    {
        EditorSceneManager.LoadSceneInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;yield return null;
        var roots=SceneManager.GetActiveScene().GetRootGameObjects();
        var assembly=roots.Single(g=>g.name.StartsWith("Energy Chamber /"));
        Assert.That(assembly.GetComponentsInChildren<MonoBehaviour>(true),Is.Empty);
        Assert.That(assembly.GetComponentsInChildren<Renderer>().All(r=>r.enabled && r.sharedMaterials.All(m=>m!=null)),Is.True);
        var camera=roots.SelectMany(g=>g.GetComponentsInChildren<Camera>()).Single();
        Assert.That(camera.enabled,Is.True);
        LogAssert.NoUnexpectedReceived();
    }
}
#endif
