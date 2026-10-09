using Gravivore.Presentation.Assets;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    [CreateAssetMenu(fileName = "Chapter01_VisualIntegration", menuName = "Gravivore/Presentation/Chapter Visual Integration")]
    public sealed class ChapterVisualIntegrationDefinition : ScriptableObject
    {
        [SerializeField] private PresentationModelBinding _elite = new PresentationModelBinding();
        [SerializeField] private PresentationModelBinding _boss = new PresentationModelBinding();
        [SerializeField] private PresentationModelBinding _repairHub = new PresentationModelBinding();
        [SerializeField] private ConceptFidelityDefinition _fidelity;

        public PresentationModelBinding Elite => _elite;
        public PresentationModelBinding Boss => _boss;
        public PresentationModelBinding RepairHub => _repairHub;
        [SerializeField] private GameObject _weaponPrefab;
        public GameObject WeaponPrefab => _weaponPrefab;
        [SerializeField] private Gravivore.Presentation.UI.ProductionUiSkinDefinition _uiSkin;
        public Gravivore.Presentation.UI.ProductionUiSkinDefinition UiSkin => _uiSkin;
        [SerializeField] private Gravivore.Presentation.AudioVfx.Phase6BVfxInstance _hostileCharge, _hostileTravel, _hostileImpact;
        public Gravivore.Presentation.AudioVfx.Phase6BVfxInstance HostileCharge => _hostileCharge;
        public Gravivore.Presentation.AudioVfx.Phase6BVfxInstance HostileTravel => _hostileTravel;
        public Gravivore.Presentation.AudioVfx.Phase6BVfxInstance HostileImpact => _hostileImpact;
        public ConceptFidelityDefinition Fidelity => _fidelity;

        public void ValidateOrThrow()
        {
            _elite.ValidateOrThrow(); _boss.ValidateOrThrow(); _repairHub.ValidateOrThrow();
            _fidelity?.ValidateOrThrow();
            _uiSkin?.ValidateOrThrow();
        }
    }
}
