using System;
using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class CapacitorFieldProofTests
    {
        [Test] public void RenderAndPresentationContractHasRealResourcesAndNoGameplayInArt() => CapacitorFieldProofBuilder.ValidateOrThrow();
        [Test] public void MissingPostProcessingResourcesAreRejectedAndRestored()
        {
            var renderer=new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Gravivore.Editor.UrpConfigurator.RendererDataPath));
            var resources=renderer.FindProperty("postProcessData");var original=resources.objectReferenceValue;
            Assert.NotNull(original);
            try
            {
                resources.objectReferenceValue=null;renderer.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<InvalidOperationException>(()=>CapacitorFieldProofBuilder.ValidateOrThrow());
            }
            finally
            {
                renderer.Update();renderer.FindProperty("postProcessData").objectReferenceValue=original;
                renderer.ApplyModifiedPropertiesWithoutUndo();
            }
            CapacitorFieldProofBuilder.ValidateOrThrow();
        }
        [Test] public void NewBankFootprintsAndNormalCameraPositionsAreClear()
        {
            var world=AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            foreach(var point in new[]{new Vector3(-8,0,10),new Vector3(-15,0,10),new Vector3(-15,0,12.9f),new Vector3(-18.5f,0,8),new Vector3(-9,0,10)})
                Assert.IsTrue(world.Layout.IsClear(point,.65f),point.ToString());
            Assert.That(Enumerable.Range(0,world.Layout.BlockerCount).Count(i=>world.Layout.GetBlocker(i).Id.StartsWith("capacitor-",StringComparison.Ordinal)),Is.EqualTo(4));
        }
        [Test] public void PhysicalSurfaceUVsRepeatAndDonorTrimsRemainBound()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(CapacitorFieldProofBuilder.Prefab);
            Assert.That(prefab.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Any(m=>m.name.StartsWith("R1_Trim",StringComparison.Ordinal)),Is.True);
            foreach(var material in prefab.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct())
            {
                foreach(var map in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap"})Assert.NotNull(material.GetTexture(map),material.name+map);
                if(material.name.StartsWith("CF_",StringComparison.Ordinal)&&material.name!="CF_DonorPalette")Assert.That(material.GetTexture("_BaseMap").wrapMode,Is.EqualTo(TextureWrapMode.Repeat));
            }
        }
    }
}
