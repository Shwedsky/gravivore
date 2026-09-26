using System;
using System.Collections.Generic;
using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    [DisallowMultipleComponent]
    public sealed class Chapter01WorldPresenter : MonoBehaviour
    {
        private readonly List<Material> _materials = new List<Material>();
        private Chapter01WorldConfiguration _configuration;
        private WorldUnlockState _state;
        private Collider[] _perimeterColliders;
        private bool _initialized;

        public WorldGateView EliteGate { get; private set; }
        public WorldGateView BossGate { get; private set; }
        public int ZoneCount => _configuration?.ZoneCount ?? 0;

        public void Initialize(Chapter01WorldConfiguration configuration, WorldUnlockState state)
        {
            if (_initialized) throw new InvalidOperationException("World presenter is already initialized.");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _state = state ?? throw new ArgumentNullException(nameof(state));

            BuildGround();
            BuildPerimeterBoundaries();
            BuildBasin();
            BuildZonesAndPaths();
            BuildBossArena();
            EliteGate = BuildGate(_configuration.EliteGate, new Color(0.9f, 0.55f, 0.12f, 1f));
            BossGate = BuildGate(_configuration.BossGate, new Color(0.84f, 0.18f, 0.22f, 1f));
            _state.GateUnlocked += HandleGateUnlocked;
            ApplyState();
            _initialized = true;
        }

        public Vector3 GetZoneCenter(int index) => _configuration.GetZone(index).Center;

        public WorldBounds Bounds => _configuration.Bounds;

        public int PerimeterColliderCount => _perimeterColliders?.Length ?? 0;

        public Collider GetPerimeterCollider(int index) => _perimeterColliders[index];

        public void ApplyState()
        {
            EliteGate?.SetLocked(!_state.EliteGateUnlocked);
            BossGate?.SetLocked(!_state.BossGateUnlocked);
        }

        public void Shutdown()
        {
            if (_state != null)
            {
                _state.GateUnlocked -= HandleGateUnlocked;
            }

            _state = null;
        }

        private void HandleGateUnlocked(WorldGateUnlockedEvent unlocked) => ApplyState();

        private void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Chapter 01 Movement Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.position = _configuration.GroundCenter + Vector3.down * 0.15f;
            ground.transform.localScale = new Vector3(_configuration.GroundSize.x, 0.25f, _configuration.GroundSize.y);
            SetMaterial(ground, new Color(0.09f, 0.12f, 0.14f, 1f));
        }

        private void BuildBasin()
        {
            var basin = CreateVisualPrimitive("Central Basin", PrimitiveType.Cylinder, _configuration.BasinCenter);
            basin.transform.localScale = new Vector3(3.4f, 0.08f, 3.4f);
            SetMaterial(basin, new Color(0.12f, 0.32f, 0.34f, 1f));
        }

        private void BuildPerimeterBoundaries()
        {
            var hardBlockerLayer = LayerMask.NameToLayer("HardBlocker");
            if (hardBlockerLayer < 0) throw new InvalidOperationException("HardBlocker layer is required for world boundaries.");

            var bounds = _configuration.Bounds;
            var boundary = _configuration.Boundary;
            var thickness = boundary.Thickness;
            var height = boundary.Height;
            var y = _configuration.GroundCenter.y + height * 0.5f;
            var material = CreateMaterial(new Color(0.12f, 0.16f, 0.17f, 1f));
            _perimeterColliders = new[]
            {
                BuildPermanentBlocker(
                    "West World Boundary",
                    new Vector3(bounds.MinX - thickness * 0.5f, y, bounds.Center.z),
                    new Vector3(thickness, height, bounds.Size.y + thickness * 2f),
                    hardBlockerLayer,
                    material),
                BuildPermanentBlocker(
                    "East World Boundary",
                    new Vector3(bounds.MaxX + thickness * 0.5f, y, bounds.Center.z),
                    new Vector3(thickness, height, bounds.Size.y + thickness * 2f),
                    hardBlockerLayer,
                    material),
                BuildPermanentBlocker(
                    "South World Boundary",
                    new Vector3(bounds.Center.x, y, bounds.MinZ - thickness * 0.5f),
                    new Vector3(bounds.Size.x + thickness * 2f, height, thickness),
                    hardBlockerLayer,
                    material),
                BuildPermanentBlocker(
                    "North World Boundary",
                    new Vector3(bounds.Center.x, y, bounds.MaxZ + thickness * 0.5f),
                    new Vector3(bounds.Size.x + thickness * 2f, height, thickness),
                    hardBlockerLayer,
                    material)
            };
        }

        private void BuildZonesAndPaths()
        {
            for (var i = 0; i < _configuration.ZoneCount; i++)
            {
                var zone = _configuration.GetZone(i);
                CreatePath($"Radial Path {zone.Id}", _configuration.BasinCenter, zone.Center);

                var pad = CreateVisualPrimitive($"Zone {zone.Id}", PrimitiveType.Cylinder, zone.Center);
                pad.transform.localScale = new Vector3(3.25f, 0.04f, 3.25f);
                SetMaterial(pad, zone.Color);

                var landmark = CreateVisualPrimitive($"Landmark {zone.Id}", PrimitiveType.Cube, zone.LandmarkPosition);
                landmark.transform.localScale = new Vector3(0.8f, 2.8f + i * 0.25f, 0.8f);
                landmark.transform.position += Vector3.up * (landmark.transform.localScale.y * 0.5f);
                landmark.transform.rotation = Quaternion.Euler(0f, i * 28f, 0f);
                SetMaterial(landmark, zone.Color * 1.25f);

                var next = _configuration.GetZone((i + 1) % _configuration.ZoneCount);
                CreatePath($"Outer Loop {i + 1}", zone.Center, next.Center);
            }
        }

        private void BuildBossArena()
        {
            CreatePath("Elite And Boss Approach", _configuration.EliteGate.Position, _configuration.BossArenaCenter);
            var arena = CreateVisualPrimitive("Boss Arena", PrimitiveType.Cylinder, _configuration.BossArenaCenter);
            arena.transform.localScale = new Vector3(
                _configuration.BossArenaRadius,
                0.05f,
                _configuration.BossArenaRadius);
            SetMaterial(arena, new Color(0.22f, 0.13f, 0.19f, 1f));
        }

        private WorldGateView BuildGate(WorldGateConfiguration configuration, Color color)
        {
            var gateObject = new GameObject(configuration.Id, typeof(WorldGateView));
            gateObject.transform.SetParent(transform, false);
            gateObject.transform.position = configuration.Position;
            var view = gateObject.GetComponent<WorldGateView>();
            view.Build(configuration.Size, _configuration.Bounds, color, CreateMaterial);
            return view;
        }

        private Collider BuildPermanentBlocker(
            string name,
            Vector3 position,
            Vector3 size,
            int layer,
            Material material)
        {
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = name;
            blocker.layer = layer;
            blocker.transform.SetParent(transform, false);
            blocker.transform.position = position;
            blocker.transform.localScale = size;
            if (material != null) blocker.GetComponent<Renderer>().sharedMaterial = material;
            return blocker.GetComponent<Collider>();
        }

        private void CreatePath(string name, Vector3 from, Vector3 to)
        {
            var path = CreateVisualPrimitive(name, PrimitiveType.Cube, (from + to) * 0.5f + Vector3.up * 0.01f);
            var delta = to - from;
            path.transform.localScale = new Vector3(1.15f, 0.035f, delta.magnitude);
            path.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            SetMaterial(path, new Color(0.22f, 0.25f, 0.25f, 1f));
        }

        private GameObject CreateVisualPrimitive(string name, PrimitiveType type, Vector3 position)
        {
            var visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            visual.transform.SetParent(transform, false);
            visual.transform.position = position;
            var collider = visual.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            return visual;
        }

        private void SetMaterial(GameObject target, Color color)
        {
            var material = CreateMaterial(color);
            if (material != null) target.GetComponent<Renderer>().sharedMaterial = material;
        }

        private Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return null;
            var material = new Material(shader) { color = color, hideFlags = HideFlags.HideAndDontSave };
            _materials.Add(material);
            return material;
        }

        private void OnDestroy()
        {
            Shutdown();
            for (var i = 0; i < _materials.Count; i++)
            {
                if (_materials[i] != null) Destroy(_materials[i]);
            }
        }
    }

    public sealed class WorldGateView : MonoBehaviour
    {
        private GameObject _barrier;
        public Collider BlockingCollider { get; private set; }
        public bool IsLocked => _barrier != null && _barrier.activeSelf;

        public void Build(Vector3 size, WorldBounds bounds, Color color, Func<Color, Material> materialFactory)
        {
            var hardBlockerLayer = LayerMask.NameToLayer("HardBlocker");
            if (hardBlockerLayer < 0) throw new InvalidOperationException("HardBlocker layer is required for world gates.");

            _barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _barrier.name = "Physical Gate Barrier";
            _barrier.layer = hardBlockerLayer;
            _barrier.transform.SetParent(transform, false);
            _barrier.transform.localPosition = Vector3.up * (size.y * 0.5f);
            _barrier.transform.localScale = size;
            BlockingCollider = _barrier.GetComponent<Collider>();
            var material = materialFactory(color);
            if (material != null) _barrier.GetComponent<Renderer>().sharedMaterial = material;

            var openingMinX = transform.position.x - size.x * 0.5f;
            var openingMaxX = transform.position.x + size.x * 0.5f;
            var leftWidth = openingMinX - bounds.MinX;
            var rightWidth = bounds.MaxX - openingMaxX;
            if (leftWidth <= 0f || rightWidth <= 0f)
            {
                throw new InvalidOperationException("Gate opening must fit strictly inside the world boundaries.");
            }

            BuildFlank(
                "Left Gate Wall",
                (bounds.MinX + openingMinX) * 0.5f - transform.position.x,
                leftWidth,
                size,
                material);
            BuildFlank(
                "Right Gate Wall",
                (openingMaxX + bounds.MaxX) * 0.5f - transform.position.x,
                rightWidth,
                size,
                material);
        }

        public void SetLocked(bool locked)
        {
            if (_barrier == null) return;
            _barrier.SetActive(locked);
            BlockingCollider.enabled = locked;
        }

        private void BuildFlank(string name, float centerX, float width, Vector3 gateSize, Material material)
        {
            var flank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flank.name = name;
            flank.layer = _barrier.layer;
            flank.transform.SetParent(transform, false);
            flank.transform.localPosition = new Vector3(centerX, gateSize.y * 0.5f, 0f);
            flank.transform.localScale = new Vector3(width, gateSize.y, gateSize.z);
            if (material != null) flank.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
