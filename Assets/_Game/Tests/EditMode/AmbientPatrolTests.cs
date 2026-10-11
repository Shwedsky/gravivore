using Gravivore.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;
namespace Gravivore.Tests.EditMode
{
    public sealed class AmbientPatrolTests
    {
        private static AmbientPatrolParameters Settings => new AmbientPatrolParameters(.9f,.5f,1.4f,3.6f,2.4f,95);
        [Test] public void PatrolIsDeterministicBoundedAndHasPauses()
        {
            var a=new AmbientPatrolState();var b=new AmbientPatrolState();var home=new Vector3(12,0,8);var p=home;var q=home;var moves=0;var pauses=0;
            a.Reset(home,7919,Settings);b.Reset(home,7919,Settings);
            for(var i=0;i<6000;i++)
            {
                var d=a.Step(p,.016f,Settings);p+=d;q+=b.Step(q,.016f,Settings);
                Assert.That(p,Is.EqualTo(q));Assert.That(Vector3.Distance(p,home),Is.LessThanOrEqualTo(.901f));Assert.That(d.magnitude,Is.LessThanOrEqualTo(.0081f));
                if(d.sqrMagnitude>0)moves++;else pauses++;
            }
            Assert.That(moves,Is.GreaterThan(100));Assert.That(pauses,Is.GreaterThan(100));
        }
        [Test] public void PoolResetChangesHomeAndDisabledSettingsDoNotMove()
        {
            var state=new AmbientPatrolState();state.Reset(Vector3.zero,1,Settings);state.Step(Vector3.zero,4,Settings);
            var home=new Vector3(18,0,42);state.Reset(home,2,Settings);var p=home;
            for(var i=0;i<600;i++){p+=state.Step(p,.016f,Settings);Assert.That(Vector3.Distance(p,home),Is.LessThanOrEqualTo(.901f));}
            Assert.That(state.Step(p,1,default),Is.EqualTo(Vector3.zero));
        }
        [Test] public void CombatDisplacementReturnsGraduallyWithoutTeleporting()
        {
            var state=new AmbientPatrolState();state.Reset(Vector3.zero,1,Settings);var p=Vector3.right*8;
            for(var i=0;i<1600;i++){var d=state.Step(p,.016f,Settings);Assert.That(d.magnitude,Is.LessThanOrEqualTo(.0081f));p+=d;}
            Assert.That(p.magnitude,Is.LessThanOrEqualTo(.901f));
        }
        [Test] public void InvalidTimeAndSettingsAreRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(()=>new AmbientPatrolParameters(float.NaN,.5f,1,2,2,90));
            var state=new AmbientPatrolState();Assert.Throws<System.ArgumentOutOfRangeException>(()=>state.Step(Vector3.zero,float.NaN,Settings));
        }
        [Test] public void SteadyStatePatrolDoesNotAllocateManagedMemory()
        {
            var settings=Settings;var state=new AmbientPatrolState();state.Reset(Vector3.zero,1,settings);var p=Vector3.zero;
            for(var i=0;i<64;i++)p+=state.Step(p,.016f,settings);
            var before=System.GC.GetAllocatedBytesForCurrentThread();
            for(var i=0;i<6000;i++)p+=state.Step(p,.016f,settings);
            var after=System.GC.GetAllocatedBytesForCurrentThread();Assert.That(after-before,Is.Zero);
        }
        [Test] public void CorrectiveSceneRendererMotionAndEcologyContractsPass()
        {Gravivore.Editor.VisualIntegration.ConceptCorrectiveV45Audit.ValidateOrThrow();}
    }
}
