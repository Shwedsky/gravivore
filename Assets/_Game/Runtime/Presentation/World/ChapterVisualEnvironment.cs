using System;
using Gravivore.Presentation.Assets;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    [Serializable]
    public sealed class VisualRegionBinding
    {
        [SerializeField] private string _id;
        [SerializeField] private string _enemyId;
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _landmark;
        public string Id => _id;
        public string EnemyId => _enemyId;
        public Transform Root => _root;
        public Transform Landmark => _landmark;
    }

    [Serializable]
    public sealed class EnvironmentDressingBinding
    {
        [SerializeField] private Transform _anchor;
        [SerializeField] private PresentationModelBinding _model = new PresentationModelBinding();
        public Transform Anchor => _anchor;
        public PresentationModelBinding Model => _model;
    }

    /// <summary>Authored visual anchors and optional art only; owns no bounds, spawns, timers or unlock state.</summary>
    [DisallowMultipleComponent]
    public sealed class ChapterVisualEnvironment : MonoBehaviour
    {
        [SerializeField] private ChapterVisualIntegrationDefinition _definition;
        [SerializeField] private Transform _fallbackRoot;
        [SerializeField] private Transform _floor, _structures, _props, _pipes, _machinery, _lighting, _debris;
        [SerializeField] private Light _keyLight;
        [SerializeField] private bool _showFallbackEnvironment = true;
        [SerializeField] private VisualRegionBinding[] _regions = Array.Empty<VisualRegionBinding>();
        [SerializeField] private EnvironmentDressingBinding[] _dressing = Array.Empty<EnvironmentDressingBinding>();
        private Phase3DMechanicalMotionPresenter _phase3DMotion;
        private bool _initialized;

        public ChapterVisualIntegrationDefinition Definition => _definition;
        public Transform FallbackRoot => _fallbackRoot;
        public Transform Floor => _floor;
        public Transform Structures => _structures;
        public Transform Lighting => _lighting;
        public Light KeyLight => _keyLight;
        public int RegionCount => _regions.Length;
        public VisualRegionBinding GetRegion(int index) => _regions[index];
        public VisualRegionBinding GetRegion(string id)
        {
            for (var i = 0; i < _regions.Length; i++)
                if (string.Equals(_regions[i].Id, id, StringComparison.Ordinal)) return _regions[i];
            throw new InvalidOperationException("No authored visual region: " + id);
        }

        public void Initialize()
        {
            if (_initialized) return;
            ValidateOrThrow();
            for (var i = 0; i < _dressing.Length; i++) _dressing[i].Model.InstantiateUnder(_dressing[i].Anchor);
            _definition.RepairHub.InstantiateUnder(GetRegion("repair-hub").Root.Find("MainPlatform"));

            if (_phase3DMotion == null)
            {
                var motionObject = new GameObject("Phase 3D Mechanical Motion", typeof(Phase3DMechanicalMotionPresenter));
                motionObject.transform.SetParent(transform.parent, false);
                _phase3DMotion = motionObject.GetComponent<Phase3DMechanicalMotionPresenter>();
                _phase3DMotion.Initialize(transform);
            }

            _fallbackRoot.gameObject.SetActive(_showFallbackEnvironment);
            _initialized = true;
        }

        public void ValidateOrThrow()
        {
            if (_definition == null) throw new InvalidOperationException("Chapter visual definition is required.");
            _definition.ValidateOrThrow();
            foreach (var root in new[] { _fallbackRoot, _floor, _structures, _props, _pipes, _machinery, _lighting, _debris })
                if (root == null || !root.IsChildOf(transform))
                    throw new InvalidOperationException("Environment categories must stay inside the visual layer.");
            var ids = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            foreach (var region in _regions)
            {
                if (region == null || string.IsNullOrWhiteSpace(region.Id) || !ids.Add(region.Id) ||
                    region.Root == null || !region.Root.IsChildOf(transform) ||
                    region.Landmark != null && !region.Landmark.IsChildOf(region.Root))
                    throw new InvalidOperationException("Visual regions need unique ids and local visual anchors.");
            }
            foreach (var entry in _dressing)
            {
                if (entry == null || entry.Anchor == null || !entry.Anchor.IsChildOf(transform))
                    throw new InvalidOperationException("Dressing may attach only inside the visual layer.");
                entry.Model.ValidateOrThrow();
            }
            if (GetComponentsInChildren<Collider>(true).Length != 0 ||
                GetComponentsInChildren<Rigidbody>(true).Length != 0)
                throw new InvalidOperationException("Visual environment must not own gameplay collision.");
            foreach (var component in GetComponentsInChildren<MonoBehaviour>(true))
                if (component != this) throw new InvalidOperationException("Visual environment must not own gameplay scripts.");
            if (_keyLight == null || !_keyLight.transform.IsChildOf(_lighting))
                throw new InvalidOperationException("The baseline key light must belong to Lighting.");
            if (GetRegion("repair-hub").Root.Find("MainPlatform") == null ||
                GetRegion("repair-hub").Root.Find("PlayerDockPoint") == null)
                throw new InvalidOperationException("Repair hub integration anchors are required.");
        }

        private void OnDestroy()
        {
            if (_phase3DMotion == null) return;
            var motionObject = _phase3DMotion.gameObject;
            _phase3DMotion = null;
            if (Application.isPlaying) Destroy(motionObject);
            else DestroyImmediate(motionObject);
        }
    }
}
