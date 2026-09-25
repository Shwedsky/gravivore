using System;
using UnityEngine;

namespace Gravivore.Presentation.Evolution
{
    [DisallowMultipleComponent]
    public sealed class EvolutionVfxRelay : MonoBehaviour, IEvolutionVfxHook
    {
        public event Action<EvolutionTierChangedEvent> Requested;

        public void Play(in EvolutionTierChangedEvent change)
        {
            Requested?.Invoke(change);
        }
    }
}
