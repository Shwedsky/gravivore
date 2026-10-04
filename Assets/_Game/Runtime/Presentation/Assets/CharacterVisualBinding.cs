using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    /// <summary>Presentation references on the authority root. Contains no combat/health/AI state or Update.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterVisualBinding : MonoBehaviour
    {
        public Transform VisualRoot { get; private set; }
        public Transform ActiveModel { get; private set; }
        public PresentationSocketSet Sockets { get; private set; }
        public void Bind(Transform visualRoot, Transform activeModel, PresentationSocketSet sockets)
        {
            VisualRoot = visualRoot; ActiveModel = activeModel; Sockets = sockets;
        }
        public Transform GetSocketOr(PresentationSocket role, Transform fallback) => Sockets?.GetOr(role, fallback) ?? fallback;
    }
}
