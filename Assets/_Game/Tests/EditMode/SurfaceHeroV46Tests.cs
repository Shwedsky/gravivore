using System.IO;
using System.Linq;
using Gravivore.Editor.VisualIntegration;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class SurfaceHeroV46Tests
    {
        [Test] public void SharedHeroMapsAndFrozenCollisionAuthorityPass()=>SurfaceHeroV46Audit.ValidateOrThrow();
        [TestCase("PressureVessel")][TestCase("EnergyCylinder")][TestCase("ArcMachine")][TestCase("GateModule")]
        [TestCase("LightDock")][TestCase("HeavyDock")][TestCase("SpecialDock")]
        public void ProductionMachineryRetainsEditableSourcesAndOneAtlas(string family)
        {
            Assert.IsTrue(File.Exists("art/surface-hero-v46/"+family+".blend"));
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(SurfaceHeroV46Builder.Root+"/Prefabs/V46_"+family+".prefab");Assert.NotNull(model);
            Assert.IsEmpty(model.GetComponentsInChildren<Collider>(true));Assert.IsEmpty(model.GetComponentsInChildren<MonoBehaviour>(true));
            var mesh=model.GetComponentInChildren<MeshFilter>().sharedMesh;Assert.That(mesh.subMeshCount,Is.EqualTo(1));Assert.That(mesh.vertexCount,Is.GreaterThan(500));
            Assert.That(model.GetComponentInChildren<Renderer>().sharedMaterial.GetTexture("_BumpMap"),Is.Not.Null);
            foreach(var t in model.GetComponentInChildren<Renderer>().sharedMaterial.GetTexturePropertyNames().Select(n=>model.GetComponentInChildren<Renderer>().sharedMaterial.GetTexture(n)).Where(t=>t!=null))Assert.That(Mathf.Max(t.width,t.height),Is.LessThanOrEqualTo(2048));
        }
    }
}
