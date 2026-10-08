using Gravivore.Gameplay.Encounters;

namespace Gravivore.Presentation.Player
{
    public enum CustodianPresentationPose { Idle,Run,Windup,Release,Special,Hit,Death }
    /// <summary>Read-only presentation priority. Damage, cadence and charge stay with the boss.</summary>
    public static class CustodianPresentationSelector
    {
        public static CustodianPresentationPose Select(CustodianBossState state,BossAttackType attack,bool hit,bool release,bool moving)
        {
            if(state==CustodianBossState.Dead)return CustodianPresentationPose.Death;
            if(state==CustodianBossState.Dormant||state==CustodianBossState.Resetting)return CustodianPresentationPose.Idle;
            if(state==CustodianBossState.Telegraphing)return CustodianPresentationPose.Windup;
            if(release||state==CustodianBossState.ExecutingAttack)
                return attack==BossAttackType.CirclePulse?CustodianPresentationPose.Special:CustodianPresentationPose.Release;
            if(hit)return CustodianPresentationPose.Hit;
            return moving?CustodianPresentationPose.Run:CustodianPresentationPose.Idle;
        }
    }
}
