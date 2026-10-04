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

        public PresentationModelBinding Elite => _elite;
        public PresentationModelBinding Boss => _boss;
        public PresentationModelBinding RepairHub => _repairHub;

        public void ValidateOrThrow()
        {
            _elite.ValidateOrThrow(); _boss.ValidateOrThrow(); _repairHub.ValidateOrThrow();
        }
    }
}
