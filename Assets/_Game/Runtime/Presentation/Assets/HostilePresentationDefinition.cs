using System;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    [Serializable]
    public sealed class HostileMotionProfile
    {
        public RuntimeAnimatorController Controller;
        public float RunCycleDistance = 1;
        public string Idle = "Idle", Move = "Run", Attack = "Attack", Hit = "Hit";
        public string Windup = "Windup", Release = "Attack", Block = "Hit";
        public string Line = "AttackLine", Circle = "AttackCircle", Cone = "AttackCone";
        public float AttackRecovery = .52f;
        public float VisualRadius;
        public float WallInset = .025f;
        public float MaximumWallOffset;
        public float TurnDegreesPerSecond = 360;
        public Color AttackColor = new Color(1, .25f, .07f, .8f);
        public bool ElectricalTravel = true;
        public bool PairedAttackSources;
        [Range(0, .5f)] public float AttackStartNormalizedTime;
        public int State(string state) => Animator.StringToHash(state);
    }

    /// <summary>Cosmetic playback and envelope data only. Never changes gameplay cadence or collision.</summary>
    [CreateAssetMenu(menuName = "Gravivore/Presentation/Hostile Motion")]
    public sealed class HostilePresentationDefinition : ScriptableObject
    {
        public HostileMotionProfile[] Profiles = Array.Empty<HostileMotionProfile>();
        public HostileMotionProfile Get(RuntimeAnimatorController controller)
        {
            foreach (var profile in Profiles) if (profile.Controller == controller) return profile;
            return null;
        }
    }
}
