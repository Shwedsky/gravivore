using System;
using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class ConnectedConceptSectorsTests
    {
        [Test] public void SharedCameraAndPhysicalPresentationContract()=>ConnectedConceptSectorsBuilder.ValidateOrThrow();
        [Test] public void EntireEnergyToFabricationAxisAndBothApproachesStayClear()
        {
            var world=AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            for(var x=-15f;x<=15;x+=.25f)Assert.IsTrue(world.Layout.IsClear(new Vector3(x,0,10),.65f),"Transfer clearance at "+x);
            foreach(var p in new[]{new Vector3(-15,0,12.9f),new Vector3(15,0,12.8f),new Vector3(18.2f,0,8)})Assert.IsTrue(world.Layout.IsClear(p,.65f),p.ToString());
            Assert.That(Enumerable.Range(0,world.Layout.BlockerCount).Count(i=>world.Layout.GetBlocker(i).Id.StartsWith("transfer-portal/",StringComparison.Ordinal)),Is.EqualTo(4));
        }
        [Test] public void ColdAndHotInstallationsShareCoreMaterialsAndRetainFunctionalExceptions()
        {
            var cutting=AssetDatabase.LoadAssetAtPath<GameObject>(ConnectedConceptSectorsBuilder.Prefab("CuttingFloor"));
            var cap=AssetDatabase.LoadAssetAtPath<GameObject>(CapacitorFieldProofBuilder.Prefab);
            var cm=cutting.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct().ToArray();
            var em=cap.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct().ToArray();
            foreach(var name in new[]{"CF_Graphite","CF_Steel","CF_Hot","CF_Cold"})Assert.IsTrue(cm.Any(m=>m.name==name&&em.Contains(m)),"Shared physical vocabulary "+name);
            foreach(var name in new[]{"CS_HeatSteel","CS_PressPaint","CS_FabricationDeck"})Assert.IsTrue(cm.Any(m=>m.name==name),name);
            foreach(var mat in cm)foreach(var map in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap"})Assert.NotNull(mat.GetTexture(map),mat.name+map);
        }
    }
}
