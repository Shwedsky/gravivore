#if UNITY_EDITOR
using Gravivore.Presentation.AudioVfx;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter1WarningReadabilityTests
    {
        [TestCase("HostileTelegraphBase", 3.2f, 0f, 0f, 32)]
        [TestCase("BossCircleTelegraph", 4f, 0f, 0f, 32)]
        [TestCase("BossConeTelegraph", 6f, 0f, 35f, 22)]
        [TestCase("BossLineTelegraph", 7f, 1.8f, 0f, 4)]
        public void WarningFill_MatchesContourPersistsAndReusesItsMesh(string cue, float range, float width, float halfAngle, int count)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/Presentation/Phase6B/VFX/Prefabs/PFX_" + cue + ".prefab");
            var host = Object.Instantiate(prefab);
            try
            {
                var instance = host.GetComponent<Phase6BVfxInstance>();
                var origin = new Vector3(5f, .35f, 8f);
                var direction = Quaternion.Euler(0f, 37f, 0f) * Vector3.forward;
                instance.ConfigureTelegraph(direction, range, width, halfAngle);
                instance.Play(origin, origin, 1f);
                var contour = host.GetComponentInChildren<LineRenderer>();
                var filter = host.GetComponentInChildren<MeshFilter>();
                var fill = filter.GetComponent<MeshRenderer>();
                var mesh = filter.sharedMesh;
                Assert.That(contour.positionCount, Is.EqualTo(count));
                Assert.That(mesh.vertexCount, Is.EqualTo(count + 1));
                var vertices = mesh.vertices;
                for (var i = 0; i < count; i++)
                    Assert.That(Vector3.Distance(filter.transform.TransformPoint(vertices[i + 1]), contour.GetPosition(i)), Is.LessThan(.0001f));
                Assert.That(instance.PresentedRange, Is.EqualTo(range));
                if (width > 0f)
                {
                    Assert.That(Vector3.Distance(contour.GetPosition(0), contour.GetPosition(1)), Is.EqualTo(width).Within(.0001f));
                    Assert.That(Vector3.Distance(contour.GetPosition(1), contour.GetPosition(2)), Is.EqualTo(range).Within(.0001f));
                }
                else if (halfAngle > 0f)
                {
                    var edge = contour.GetPosition(1) - origin; edge.y = 0f;
                    Assert.That(Vector3.Angle(direction, edge), Is.EqualTo(halfAngle).Within(.001f));
                    Assert.That(edge.magnitude, Is.EqualTo(range).Within(.0001f));
                }
                else
                {
                    var edge = contour.GetPosition(0) - origin; edge.y = 0f;
                    Assert.That(edge.magnitude, Is.EqualTo(range).Within(.0001f));
                }
                Assert.That(fill.sharedMaterial.GetFloat("_Surface"), Is.EqualTo(1f));
                Assert.That(fill.sharedMaterial.GetFloat("_ZWrite"), Is.Zero);
                var properties = new MaterialPropertyBlock();
                fill.GetPropertyBlock(properties);
                Assert.That(properties.GetColor("_BaseColor").a, Is.InRange(.05f, .3f));
                instance.Tick(.9f);
                Assert.That(instance.IsPlaying, Is.True);
                Assert.That(contour.startColor.a, Is.GreaterThan(.65f), "The warning must remain legible until damage time.");
                instance.Tick(.101f);
                Assert.That(instance.IsPlaying, Is.False);
                instance.StopImmediate();
                Assert.That(fill.enabled, Is.False);
                instance.ConfigureTelegraph(direction, range, width, halfAngle);
                instance.Play(origin, origin, 1f);
                Assert.That(filter.sharedMesh, Is.SameAs(mesh));
                Assert.That(host.GetComponentsInChildren<MeshFilter>(true).Length, Is.EqualTo(1));
                instance.StopImmediate();
                Assert.That(fill.enabled, Is.False);
            }
            finally { Object.DestroyImmediate(host); }
        }
    }
}
#endif
