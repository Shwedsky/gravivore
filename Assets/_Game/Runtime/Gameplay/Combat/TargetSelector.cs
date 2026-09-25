using System;
using System.Collections.Generic;

namespace Gravivore.Gameplay.Combat
{
    public static class TargetSelector
    {
        public static float CalculateScore(
            float distance,
            float frontAlignment,
            TargetingParameters parameters)
        {
            if (float.IsNaN(distance) || float.IsInfinity(distance) || distance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            var clampedAlignment = Math.Max(-1f, Math.Min(1f, frontAlignment));
            var normalizedDistance = distance / parameters.AcquisitionRadius;
            var frontPenalty = (1f - clampedAlignment) * 0.5f;
            return (normalizedDistance * parameters.DistanceWeight) +
                   (frontPenalty * parameters.FrontBiasWeight);
        }

        public static bool TrySelect<TTarget>(
            IReadOnlyList<TargetCandidate<TTarget>> candidates,
            bool hasCurrentTarget,
            TTarget currentTarget,
            TargetingParameters parameters,
            out TTarget selectedTarget)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            var comparer = EqualityComparer<TTarget>.Default;
            var hasValidCurrent = false;
            var currentScore = float.PositiveInfinity;
            var hasBestCandidate = false;
            var bestCandidate = default(TTarget);
            var bestScore = float.PositiveInfinity;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!candidate.IsValid)
                {
                    continue;
                }

                var isCurrent = hasCurrentTarget && comparer.Equals(candidate.Target, currentTarget);
                if (isCurrent && candidate.Distance <= parameters.ReleaseRadius)
                {
                    hasValidCurrent = true;
                    currentScore = CalculateScore(candidate.Distance, candidate.FrontAlignment, parameters);
                }

                if (candidate.Distance > parameters.AcquisitionRadius)
                {
                    continue;
                }

                var score = CalculateScore(candidate.Distance, candidate.FrontAlignment, parameters);
                if (!hasBestCandidate || score < bestScore)
                {
                    hasBestCandidate = true;
                    bestCandidate = candidate.Target;
                    bestScore = score;
                }
            }

            if (hasValidCurrent)
            {
                if (!hasBestCandidate || comparer.Equals(bestCandidate, currentTarget) ||
                    bestScore + parameters.SwitchScoreAdvantage >= currentScore)
                {
                    selectedTarget = currentTarget;
                    return true;
                }
            }

            if (hasBestCandidate)
            {
                selectedTarget = bestCandidate;
                return true;
            }

            selectedTarget = default(TTarget);
            return false;
        }
    }
}
