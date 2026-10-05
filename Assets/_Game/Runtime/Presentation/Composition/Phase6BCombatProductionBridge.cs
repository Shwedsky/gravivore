using System.Collections;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravivore.Presentation.Composition
{
    /// <summary>
    /// Production Chapter01 observer for Phase6B ordinary-enemy feedback. It only consumes
    /// authoritative EnemyPopulation events and never writes combat/gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Phase6BCombatProductionBridge : MonoBehaviour
    {
        private S01SceneCompositionRoot _root;
        private EnemyPopulationController _enemies;
        private GravityLashVfxPool _phase6BPresentation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachInAlreadyLoadedScene()
        {
            AttachToProductionRoots();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachToProductionRoots();
        }

        private static void AttachToProductionRoots()
        {
            var roots = UnityEngine.Object.FindObjectsByType<S01SceneCompositionRoot>(FindObjectsSortMode.None);
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null || root.GetComponent<Phase6BCombatProductionBridge>() != null) continue;
                root.gameObject.AddComponent<Phase6BCombatProductionBridge>();
            }
        }

        private IEnumerator Start()
        {
            _root = GetComponent<S01SceneCompositionRoot>();
            if (_root == null) yield break;

            while (_root.EnemyPopulation == null)
                yield return null;

            _phase6BPresentation = _root.GetComponentInChildren<GravityLashVfxPool>(true);
            if (_phase6BPresentation == null || !_phase6BPresentation.UsesPhase6BProductionPack)
            {
                Debug.LogError("Phase6B combat bridge requires the production Gravity Lash Phase6B presentation facade.", this);
                yield break;
            }

            _enemies = _root.EnemyPopulation;
            _enemies.EnemyDamaged += HandleEnemyDamaged;
            _enemies.EnemyDied += HandleEnemyDied;
        }

        private void HandleEnemyDamaged(EnemyDamageEvent damage)
        {
            if (damage.Result.WasLethal) return;
            _phase6BPresentation.PlayEnemyHit(damage.Position, damage.LifeId.GetHashCode());
        }

        private void HandleEnemyDied(EnemyDeathEvent death)
        {
            _phase6BPresentation.PlayEnemyShutdown(death.Position, death.LifeId.GetHashCode());
        }

        private void OnDestroy()
        {
            if (_enemies == null) return;
            _enemies.EnemyDamaged -= HandleEnemyDamaged;
            _enemies.EnemyDied -= HandleEnemyDied;
            _enemies = null;
        }
    }
}
