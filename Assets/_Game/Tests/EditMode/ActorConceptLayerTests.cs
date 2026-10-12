using System.Linq;
using Gravivore.Editor.VisualIntegration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class ActorConceptLayerTests
    {
        [TestCase("G0")][TestCase("Magnetar")][TestCase("Custodian")]
        public void SupportMaskPreservesCoreWeaponsAndMainAttackStates(string actor)
        {
            var path=actor=="G0"?"Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab":Chapter01ActorIntegrationBuilder.Prefab(actor);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var controller=(AnimatorController)prefab.GetComponentInChildren<Animator>(true).runtimeAnimatorController;
            var layer=controller.layers.Single(l=>l.name=="Industrial moving supports");Assert.That(layer.defaultWeight,Is.EqualTo(0));
            Assert.That(layer.stateMachine.defaultState.motion.name,Is.EqualTo("Run"));Assert.IsTrue(layer.stateMachine.defaultState.timeParameterActive);
            var active=Enumerable.Range(0,layer.avatarMask.transformCount).Where(i=>layer.avatarMask.GetTransformActive(i)).Select(i=>layer.avatarMask.GetTransformPath(i)).ToArray();
            Assert.IsNotEmpty(active);Assert.IsFalse(active.Any(p=>p.Contains("HEAD")||p.Contains("TOOL")||p.Contains("Core")||p.Contains("AttackOrigin")),"A gait cannot overwrite the attack source or core");
            Assert.IsTrue(controller.layers[0].stateMachine.states.Any(s=>s.state.name=="Attack"));Assert.IsTrue(controller.layers[0].stateMachine.states.Any(s=>s.state.name=="Death"));
        }
        [Test]public void PresentationValidationKeepsOpaqueMapsAndPhysicalWallEnvelope(){Assert.DoesNotThrow(ActorConceptWorldBuildAudit.Validate);}
    }
}
