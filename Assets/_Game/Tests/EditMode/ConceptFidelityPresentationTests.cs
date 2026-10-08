using Gravivore.Gameplay.Encounters;
using Gravivore.Presentation.Player;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class ConceptFidelityPresentationTests
    {
        [TestCase(CustodianBossState.Dead,BossAttackType.LineCharge,true,true,true,CustodianPresentationPose.Death)]
        [TestCase(CustodianBossState.Dormant,BossAttackType.LineCharge,true,true,true,CustodianPresentationPose.Idle)]
        [TestCase(CustodianBossState.Resetting,BossAttackType.CirclePulse,true,true,true,CustodianPresentationPose.Idle)]
        [TestCase(CustodianBossState.Telegraphing,BossAttackType.ConeSweep,true,false,true,CustodianPresentationPose.Windup)]
        [TestCase(CustodianBossState.ExecutingAttack,BossAttackType.CirclePulse,true,false,true,CustodianPresentationPose.Special)]
        [TestCase(CustodianBossState.Recovery,BossAttackType.LineCharge,true,true,false,CustodianPresentationPose.Release)]
        [TestCase(CustodianBossState.Recovery,BossAttackType.ConeSweep,true,false,false,CustodianPresentationPose.Hit)]
        [TestCase(CustodianBossState.Engaging,BossAttackType.ConeSweep,false,false,true,CustodianPresentationPose.Run)]
        [TestCase(CustodianBossState.Recovery,BossAttackType.CirclePulse,false,false,false,CustodianPresentationPose.Idle)]
        public void AuthoritativeStateSelectsPoseWithoutInterruptingDangerWindup(CustodianBossState state,BossAttackType attack,bool hit,bool release,bool moving,CustodianPresentationPose expected)
        {Assert.That(CustodianPresentationSelector.Select(state,attack,hit,release,moving),Is.EqualTo(expected));}
    }
}
