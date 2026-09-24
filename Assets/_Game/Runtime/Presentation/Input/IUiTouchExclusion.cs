using UnityEngine;

namespace Gravivore.Presentation.Input
{
    public interface IUiTouchExclusion
    {
        bool Blocks(Vector2 screenPosition);
    }
}
