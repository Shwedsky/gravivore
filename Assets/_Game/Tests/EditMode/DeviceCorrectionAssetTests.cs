using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.AudioVfx;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class DeviceCorrectionAssetTests
    {
        [Test] public void ProductionMusicIsLongStreamingAndReachableFromTheActualScene()
        {
            var path = FirstVisualSliceBuilder.Root + "/DeviceCorrection.asset";
            var definition = AssetDatabase.LoadAssetAtPath<DeviceCorrectionDefinition>(path);
            Assert.IsNotNull(definition); Assert.DoesNotThrow(definition.ValidateOrThrow);
            var dependencies = AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
            Assert.Contains(path,dependencies);
            foreach (var clip in new[] { definition.Exploration,definition.CombatLayer })
            {
                Assert.That(clip.length,Is.EqualTo(72).Within(.1)); Assert.That(clip.channels,Is.EqualTo(2));
                var clipPath = AssetDatabase.GetAssetPath(clip); Assert.Contains(clipPath,dependencies);
                var importer = (AudioImporter)AssetImporter.GetAtPath(clipPath);
                Assert.That(importer.defaultSampleSettings.loadType,Is.EqualTo(AudioClipLoadType.Streaming));
            }
        }
        [Test] public void G0SilhouetteArmorAndScaleGrowAcrossAllTiers()
        {
            var lastSize = 0f; var lastTriangles = 0;
            for (var tier = 0; tier < 3; tier++)
            {
                var obj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab("G0_V3_Live_Tier"+tier)));
                try
                {
                    var lod = obj.GetComponentInChildren<LODGroup>(); var renderers = lod.GetLODs()[0].renderers;
                    var bounds = renderers[0].bounds; foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                    var triangles = renderers.Sum(r => ((SkinnedMeshRenderer)r).sharedMesh.triangles.Length/3);
                    Assert.That(bounds.size.magnitude,Is.GreaterThan(lastSize)); Assert.That(triangles,Is.GreaterThan(lastTriangles));
                    Assert.IsEmpty(obj.GetComponentsInChildren<Collider>(true)); lastSize = bounds.size.magnitude; lastTriangles = triangles;
                }
                finally { Object.DestroyImmediate(obj); }
            }
        }
    }
}
