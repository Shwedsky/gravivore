using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public interface IEnemyVisualState
    {
        void Apply(string enemyId);

        void Reset();
    }

    public interface IEnemyVisualFactory
    {
        IEnemyVisualState Create(Transform parent);
    }
}
