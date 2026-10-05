using System.Linq;
using Gravivore.Editor;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Phase3DVisualQualityTests
    {
        private const string PackRoot = "Assets/_Game/Phase3D/Prefabs";

        [Test]
        public void Phase3DPackIsPurePresentationAndUsesRestrainedMaterialFamily()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { PackRoot });
            Assert.That(guids, Has.Length.EqualTo(16));

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.IsNotNull(prefab, path);
                Assert.DoesNotThrow(() => PresentationPrefabValidation.ValidateOrThrow(prefab), path);
                Assert.That(prefab.GetComponentsInChildren<Collider>(true), Is.Empty, path);
                Assert.That(prefab.GetComponentsInChildren<Rigidbody>(true), Is.Empty, path);
                Assert.That(prefab.GetComponentsInChildren<Camera>(true), Is.Empty, path);
                Assert.That(prefab.GetComponentsInChildren<Light>(true), Is.Empty, path);
                Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty, path);

                var intake = ArtIntakeTool.Inspect(prefab, ArtCandidateRole.Environment);
                Assert.That(intake.runtimeSafe, Is.True, path);
                Assert.That(intake.missingMeshes + intake.missingMaterials + intake.brokenTextureReferences,
                    Is.Zero, path);

                foreach (var transform in prefab.GetComponentsInChildren<Transform>(true))
                    AssertValidScale(transform.localScale, path + " / " + transform.name);

                foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
                    Assert.IsNotNull(filter.sharedMesh, path + " / " + filter.name + " missing mesh");
                foreach (var skinned in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    Assert.IsNotNull(skinned.sharedMesh, path + " / " + skinned.name + " missing skinned mesh");

                var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                Assert.That(renderers, Is.Not.Empty, path);
                foreach (var material in renderers.SelectMany(r => r.sharedMaterials))
                {
                    Assert.IsNotNull(material, path);
                    Assert.IsNotNull(material.shader, path + " / " + material.name + " missing shader");
                    Assert.That(AssetDatabase.GetAssetPath(material),
                        Does.StartWith("Assets/_Game/Phase3D/Materials/"), path);
                }
            }
        }

        [Test]
        public void MandatoryCGradeReplacementsExposeIndustrialSemanticForms()
        {
            AssertNode("Assets/_Game/Phase3D/Prefabs/Enemies/Carrier_Phase3D.prefab", "ProtectedCargo_L");
            AssertNode("Assets/_Game/Phase3D/Prefabs/Enemies/Carrier_Phase3D.prefab", "LoaderBoom");
            AssertNode("Assets/_Game/Phase3D/Prefabs/Environment/ShieldDump_Phase3D.prefab", "EmitterCarcass_L");
            AssertNode("Assets/_Game/Phase3D/Prefabs/Environment/ShieldDump_Phase3D.prefab", "BrokenShieldPlate");
            AssertNode("Assets/_Game/Phase3D/Prefabs/Environment/HaulerGraveyard_Phase3D.prefab", "BrokenHaulerChassis_L");
            AssertNode("Assets/_Game/Phase3D/Prefabs/Environment/EliteArena_Phase3D.prefab", "DamagedNorthContainment");
            AssertNode("Assets/_Game/Phase3D/Prefabs/Environment/CapacitorField_Phase3D.prefab", "CapacitorBank_L0");
            AssertNode("Assets/_Game/Phase3D/Prefabs/Environment/CapacitorField_Phase3D.prefab", "PowerTrunk");
        }

        [Test]
        public void RepairHubReadsAsServiceBayInAuthoredHierarchy()
        {
            const string path = "Assets/_Game/Phase3D/Prefabs/Environment/RepairHub_Phase3D.prefab";
            var hub = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var name in new[]
            {
                "ServiceArm_L_Upper", "ServiceArm_L_Forearm", "ServiceTool_L",
                "ServiceArm_R_Upper", "ServiceArm_R_Forearm", "ServiceTool_R",
                "RearServiceArm_A", "RearServiceArm_B"
            })
                Assert.That(hub.transform.Find(name), Is.Not.Null, name);
        }

        [Test]
        public void CameraAndGameplayAnchorsStayOnPhase3CBaseline()
        {
            var camera = AssetDatabase.LoadAssetAtPath<CameraFollowSettings>(
                "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            Assert.That(camera.Offset, Is.EqualTo(new Vector3(0f, 14.8f, -11.2f)));
            Assert.That(camera.FieldOfView, Is.EqualTo(46f));
            Assert.DoesNotThrow(VisualIntegrationValidator.ValidateOrThrow);
        }

        private static void AssertValidScale(Vector3 scale, string context)
        {
            Assert.That(IsFinitePositive(scale.x), Is.True, context + " invalid X scale: " + scale.x);
            Assert.That(IsFinitePositive(scale.y), Is.True, context + " invalid Y scale: " + scale.y);
            Assert.That(IsFinitePositive(scale.z), Is.True, context + " invalid Z scale: " + scale.z);
        }

        private static bool IsFinitePositive(float value) =>
            value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);

        private static void AssertNode(string path, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, path);
            Assert.That(prefab.transform.Find(name), Is.Not.Null, path + " / " + name);
        }
    }
}
