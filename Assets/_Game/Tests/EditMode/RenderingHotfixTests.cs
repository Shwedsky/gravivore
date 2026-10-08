using System;
using System.Linq;
using Gravivore.Editor;
using Gravivore.Editor.Rendering;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.EditMode
{
    public sealed class RenderingHotfixTests
    {
        private static Shader[] Included()
        {
            var list=RenderingBuildAudit.GraphicsObject().FindProperty("m_AlwaysIncludedShaders");
            return Enumerable.Range(0,list.arraySize).Select(i=>list.GetArrayElementAtIndex(i).objectReferenceValue as Shader).ToArray();
        }
        private static void SetIncluded(Shader[] shaders)
        {
            var settings=RenderingBuildAudit.GraphicsObject();var list=settings.FindProperty("m_AlwaysIncludedShaders");list.arraySize=shaders.Length;
            for(var i=0;i<shaders.Length;i++)list.GetArrayElementAtIndex(i).objectReferenceValue=shaders[i];
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
        [Test] public void EmptyFreshCheckoutDeterministicallyRetainsOnlyTwoRuntimeUiShadersAndIsIdempotent()
        {
            var prior=Included();
            try{SetIncluded(Array.Empty<Shader>());RenderingBuildAudit.EnsureRuntimeUiShaders();RenderingBuildAudit.EnsureRuntimeUiShaders();CollectionAssert.AreEquivalent(RenderingBuildAudit.UiShaders,Included().Select(s=>s.name));}
            finally{SetIncluded(prior);}
        }
        [Test] public void PreservationDoesNotRemoveOtherExplicitlyIncludedShaders()
        {
            var prior=Included();var existing=Shader.Find("Sprites/Default");
            try{SetIncluded(new[]{existing});RenderingBuildAudit.EnsureRuntimeUiShaders();Assert.Contains(existing,Included());Assert.That(Included().Length,Is.EqualTo(3));}
            finally{SetIncluded(prior);}
        }
        [Test] public void ValidatorRejectsStrippedUiDependencyWithoutRepairingIt()
        {
            var prior=Included();
            try{SetIncluded(Array.Empty<Shader>());StringAssert.Contains("MAGENTA_GUARD",Assert.Throws<InvalidOperationException>(RenderingBuildAudit.ValidateOrThrow).Message);Assert.That(Included(),Is.Empty);}
            finally{SetIncluded(prior);}
        }
        [TestCase("Custodian_V3")][TestCase("Emitter_M0")][TestCase("Broken_Edge_V3_0")][TestCase("Broken_Edge_V3_1")]
        [TestCase("Service_Trench_V3")][TestCase("Collapsed_Hull_V3")][TestCase("HostileChargeV3")][TestCase("HostileTravelV3")]
        public void EveryV3PrefabHasSupportedMaterialCoverageAndBoundedGeometry(string name)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Gravivore.Editor.VisualIntegration.Chapter01V3Builder.Prefab(name));Assert.NotNull(prefab);
            foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))Assert.DoesNotThrow(()=>RenderingBuildAudit.ValidateRenderer(renderer));
            foreach(var effect in prefab.GetComponentsInChildren<Gravivore.Presentation.AudioVfx.Phase6BVfxInstance>(true))Assert.DoesNotThrow(()=>RenderingBuildAudit.ValidateMaterial(effect.Material,name));
        }
        [Test] public void BoundsAndNullOrErrorMaterialRegressionsAreRejected()
        {
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);var renderer=cube.GetComponent<Renderer>();var error=new Material(Shader.Find("Hidden/InternalErrorShader"));
            try{
                renderer.sharedMaterial=null;Assert.Throws<InvalidOperationException>(()=>RenderingBuildAudit.ValidateRenderer(renderer));
                renderer.sharedMaterial=error;Assert.Throws<InvalidOperationException>(()=>RenderingBuildAudit.ValidateRenderer(renderer));
                renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(RenderingBuildAudit.Atlas);cube.transform.localScale=Vector3.one*100;
                Assert.Throws<InvalidOperationException>(()=>RenderingBuildAudit.ValidateRenderer(renderer));
            }finally{Object.DestroyImmediate(cube);Object.DestroyImmediate(error);}
        }
        [Test] public void IntendedPipelineAndMaterialProfilePreservesKnownGoodRenderingValues()
        {
            RenderingBuildAudit.ValidateOrThrow();RenderingBuildAudit.CaptureProfile();
            var pipeline=new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>(UrpConfigurator.UrpAssetPath));
            var renderer=new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>(UrpConfigurator.RendererDataPath));
            Assert.IsTrue(pipeline.FindProperty("m_SupportsHDR").boolValue);Assert.That(pipeline.FindProperty("m_MSAA").intValue,Is.EqualTo(1));Assert.That(pipeline.FindProperty("m_RenderScale").floatValue,Is.EqualTo(1));
            Assert.That(renderer.FindProperty("m_RendererFeatures").arraySize,Is.Zero);Assert.That(pipeline.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue,Is.SameAs(renderer.targetObject));
            Assert.That(PlayerSettings.colorSpace,Is.EqualTo(ColorSpace.Gamma));Assert.That(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android),Does.Contain(GraphicsDeviceType.OpenGLES3));
        }
        [Test] public void StaticBatchSubsetUsesEffectiveMaterialCoverageAndRejectsInvalidRanges()
        {
            var owner = new GameObject("static batch subset", typeof(MeshFilter), typeof(MeshRenderer));
            var mesh = new Mesh { vertices = new[] { Vector3.zero, Vector3.right, Vector3.up }, subMeshCount = 3 };
            for (var i = 0; i < 3; i++) mesh.SetTriangles(new[] { 0, 1, 2 }, i);
            owner.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = owner.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(RenderingBuildAudit.Atlas);
            void Subset(int first, int count)
            {
                var serialized = new SerializedObject(renderer);
                var batch = serialized.FindProperty("m_StaticBatchInfo");
                batch.FindPropertyRelative("firstSubMesh").intValue = first;
                batch.FindPropertyRelative("subMeshCount").intValue = count;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            try
            {
                Subset(1, 1); Assert.DoesNotThrow(() => RenderingBuildAudit.ValidateRenderer(renderer));
                Subset(3, 1); Assert.Throws<InvalidOperationException>(() => RenderingBuildAudit.ValidateRenderer(renderer));
                Subset(0, 0);
                renderer.sharedMaterials = new[] { AssetDatabase.LoadAssetAtPath<Material>(RenderingBuildAudit.Atlas) };
                Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(1));
                Assert.Throws<InvalidOperationException>(() => RenderingBuildAudit.ValidateRenderer(renderer));
            }
            finally { Object.DestroyImmediate(owner); Object.DestroyImmediate(mesh); }
        }
    }
}
