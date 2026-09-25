namespace Gravivore.Gameplay.Combat
{
    public readonly struct TargetCandidate<TTarget>
    {
        public TargetCandidate(TTarget target, float distance, float frontAlignment, bool isValid)
        {
            Target = target;
            Distance = distance;
            FrontAlignment = frontAlignment;
            IsValid = isValid;
        }

        public TTarget Target { get; }

        public float Distance { get; }

        public float FrontAlignment { get; }

        public bool IsValid { get; }
    }
}
