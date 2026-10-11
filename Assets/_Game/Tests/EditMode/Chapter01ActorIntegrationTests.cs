using System;
using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.Assets;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter01ActorIntegrationTests
    {
        [Test] public void AllSevenAreProductionDependenciesWithAuthoredSockets()
        { Assert.DoesNotThrow(Chapter01ActorIntegrationBuilder.Validate); }

        [TestCase("Scout")] [TestCase("Cutter")] [TestCase("Warden")]
        [TestCase("ArcDrone")] [TestCase("Carrier")] [TestCase("Magnetar")] [TestCase("Custodian")]
        public void ProductionPrefabRetainsRigMaterialsLodsAndStationaryAuthority(string actor)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01ActorIntegrationBuilder.Prefab(actor));
            Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true));
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));
            Assert.IsEmpty(prefab.GetComponentsInChildren<Camera>(true));
            var animator=prefab.GetComponentInChildren<Animator>(true);
            Assert.IsFalse(animator.applyRootMotion);
            var definition=AssetDatabase.LoadAssetAtPath<HostilePresentationDefinition>(Chapter01ActorIntegrationBuilder.Root+"/HostileMotion.asset");
            var profile=definition.Get(animator.runtimeAnimatorController);Assert.NotNull(profile);
            var states=((AnimatorController)animator.runtimeAnimatorController).layers[0].stateMachine.states.Select(s=>s.state.name).ToArray();
            foreach(var state in new[]{"Idle","Run","Attack","Hit","Death",profile.Idle,profile.Move,profile.Attack,profile.Hit,profile.Block})
                Assert.Contains(state,states,actor+" "+state);
            if(actor=="Magnetar"||actor=="Custodian")Assert.Contains(profile.Windup,states);
            if(actor=="Custodian")foreach(var state in new[]{profile.Line,profile.Circle,profile.Cone})Assert.Contains(state,states);
            var lods=prefab.GetComponentInChildren<LODGroup>().GetLODs();Assert.That(lods.Length,Is.EqualTo(3));
            var counts=lods.Select(l=>((SkinnedMeshRenderer)l.renderers[0]).sharedMesh.GetIndexCount(0)).ToArray();
            Assert.That(counts[1],Is.LessThan(counts[0]));Assert.That(counts[2],Is.LessThan(counts[1]));
            foreach(var material in prefab.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct())
            {
                Assert.IsTrue(material.IsKeywordEnabled("_EMISSION"),"Authored energy map must be active under R2 lighting");
                foreach(var map in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap","_EmissionMap"})Assert.NotNull(material.GetTexture(map));
            }
            foreach(var clip in animator.runtimeAnimatorController.animationClips)Assert.IsEmpty(clip.events);
            var instance=UnityEngine.Object.Instantiate(prefab);
            try
            {
                var live=instance.GetComponentInChildren<Animator>();live.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                var sockets=new PresentationSocketSet(instance.transform);
                var core=sockets.GetOr(PresentationSocket.Core,instance.transform);
                foreach(var state in animator.runtimeAnimatorController.animationClips.Select(c=>c.name).Distinct())
                {
                    live.Play(state,0,.5f);live.Update(0);
                    Assert.That(instance.transform.position,Is.EqualTo(Vector3.zero));
                    Assert.IsFalse(float.IsNaN(core.position.y));
                }
                live.Play(profile.Idle,0,0);live.Update(0);
                Assert.That(sockets.GetOr(PresentationSocket.AttackOrigin,instance.transform).position.z,Is.GreaterThan(-.05f),"Front socket must face gameplay +Z.");
                if(actor=="Carrier")Assert.IsTrue(live.runtimeAnimatorController.animationClips.Single(c=>c.name=="Bank").isLooping);
            }
            finally{UnityEngine.Object.DestroyImmediate(instance);}
        }
    }
}
