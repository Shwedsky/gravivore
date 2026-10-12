using Gravivore.Gameplay.World;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class ActorWallEnvelopeTests
    {
        [Test] public void UnconfiguredEnvelopePreservesRequestedMotion()
        {var envelope=new ActorWallEnvelope();var step=new Vector3(3,0,2);Assert.That(envelope.Resolve(Vector3.zero,step),Is.EqualTo(step));}
        [Test] public void SweptWallHullStopsAndSlidesWithoutChangingTargetSensor()
        {
            var wall=new GameObject("Envelope test wall",typeof(BoxCollider));wall.layer=LayerMask.NameToLayer("HardBlocker");
            wall.transform.position=new Vector3(2,1,0);wall.GetComponent<BoxCollider>().size=new Vector3(.2f,2,8);
            try
            {
                Physics.SyncTransforms();var envelope=new ActorWallEnvelope();envelope.Configure(1,1<<wall.layer);
                var motion=envelope.Resolve(Vector3.zero,new Vector3(4,0,1));
                Assert.That(motion.x,Is.LessThan(.92f));Assert.That(motion.z,Is.GreaterThan(.2f));Assert.That(motion.y,Is.EqualTo(0));
            }
            finally{Object.DestroyImmediate(wall);}
        }
        [Test] public void EnlargedEnvelopeIgnoresNonWallCombatCollider()
        {
            var sensor=new GameObject("Unchanged combat sensor",typeof(SphereCollider));sensor.GetComponent<SphereCollider>().isTrigger=true;sensor.transform.position=Vector3.right;
            try{Physics.SyncTransforms();var envelope=new ActorWallEnvelope();envelope.Configure(2,LayerMask.GetMask("HardBlocker"));Assert.That(envelope.Resolve(Vector3.zero,Vector3.right*3),Is.EqualTo(Vector3.right*3));Assert.That(sensor.GetComponent<SphereCollider>().radius,Is.EqualTo(.5f));}
            finally{Object.DestroyImmediate(sensor);}
        }
        [Test] public void SteadyWallQueriesDoNotAllocateManagedMemory()
        {
            var wall=new GameObject("Allocation wall",typeof(BoxCollider));wall.layer=LayerMask.NameToLayer("HardBlocker");wall.transform.position=new Vector3(2,1,0);
            try
            {
                Physics.SyncTransforms();var envelope=new ActorWallEnvelope();envelope.Configure(1,1<<wall.layer);
                for(var i=0;i<20;i++)envelope.Resolve(Vector3.zero,new Vector3(3,0,1));
                var before=System.GC.GetAllocatedBytesForCurrentThread();
                for(var i=0;i<250;i++)envelope.Resolve(Vector3.zero,new Vector3(3,0,1));
                var allocation=System.GC.GetAllocatedBytesForCurrentThread()-before;Assert.That(allocation,Is.EqualTo(0));
            }
            finally{Object.DestroyImmediate(wall);}
        }
    }
}
