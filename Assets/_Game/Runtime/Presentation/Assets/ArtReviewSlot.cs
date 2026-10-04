using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    /// <summary>Isolated review-scene helper. Never installed in the gameplay scene or an art prefab.</summary>
    public sealed class ArtReviewSlot : MonoBehaviour
    {
        [SerializeField] private PresentationModelBinding _candidate = new PresentationModelBinding();
        [SerializeField] private GameObject _fallback;
        [SerializeField] private GameObject _candidateInstance;
        public PresentationModelBinding Candidate => _candidate;
        public GameObject Fallback => _fallback;

        public void RefreshPreview()
        {
            _candidate.ValidateOrThrow();
            if (_candidateInstance != null)
            {
                _candidateInstance.SetActive(false);
                if (Application.isPlaying) Destroy(_candidateInstance); else DestroyImmediate(_candidateInstance);
            }
            _candidateInstance = _candidate.InstantiateUnder(transform);
            if (_fallback != null) _fallback.SetActive(_candidateInstance == null);
        }
    }
}
