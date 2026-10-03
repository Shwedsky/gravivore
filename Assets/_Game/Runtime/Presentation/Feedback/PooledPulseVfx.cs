using System;
using UnityEngine;

namespace Gravivore.Presentation.Feedback
{
    [DisallowMultipleComponent]
    public sealed class PooledPulseVfx : MonoBehaviour
    {
        private sealed class Item
        {
            public GameObject Object;
            public Transform Transform;
            public float Age;
            public float Duration;
            public float StartScale;
            public float EndScale;
            public Vector3 Start;
            public Transform Target;
        }

        private Item[] _items;
        private int _cursor;
        private bool _initialized;
        private Mesh _industrialMesh;

        public int Capacity => _items != null ? _items.Length : 0;
        public int ActiveCount { get; private set; }
        public GameObject LastPlayedObject { get; private set; }

        public void Initialize(int capacity, Material material, bool industrial = false)
        {
            if (_initialized) throw new InvalidOperationException("Pulse VFX pool is already initialized.");
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (material == null) throw new ArgumentNullException(nameof(material));
            _items = new Item[capacity];
            if (industrial) _industrialMesh = IndustrialPulseMesh.Create();
            for (var i = 0; i < capacity; i++)
            {
                var effect = industrial ? new GameObject("Industrial Pulse", typeof(MeshFilter), typeof(MeshRenderer)) :
                    GameObject.CreatePrimitive(PrimitiveType.Sphere);
                effect.name = $"Pooled Pulse {i}";
                effect.transform.SetParent(transform, false);
                var collider = effect.GetComponent<Collider>();
                if (collider != null) { collider.enabled = false; Destroy(collider); }
                if (industrial) effect.GetComponent<MeshFilter>().sharedMesh = _industrialMesh;
                effect.GetComponent<Renderer>().sharedMaterial = material;
                effect.SetActive(false);
                _items[i] = new Item { Object = effect, Transform = effect.transform };
            }

            _initialized = true;
        }

        public void Play(Vector3 position, float duration, float startScale, float endScale, Transform target = null)
        {
            if (!_initialized) throw new InvalidOperationException("Pulse VFX pool must be initialized before use.");
            if (duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
                throw new ArgumentOutOfRangeException(nameof(duration));
            var item = FindAvailable();
            if (!item.Object.activeSelf) ActiveCount++;
            item.Age = 0f;
            item.Duration = duration;
            item.StartScale = startScale;
            item.EndScale = endScale;
            item.Start = position;
            item.Target = target;
            item.Transform.position = position;
            item.Transform.localScale = Vector3.one * startScale;
            item.Object.SetActive(true);
            LastPlayedObject = item.Object;
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized) return;
            for (var i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                if (!item.Object.activeSelf) continue;
                item.Age += deltaTime;
                var progress = Mathf.Clamp01(item.Age / item.Duration);
                item.Transform.localScale = Vector3.one * Mathf.Lerp(item.StartScale, item.EndScale, progress);
                if (item.Target != null) item.Transform.position = Vector3.Lerp(item.Start, item.Target.position, progress);
                if (progress < 1f) continue;
                Reset(item);
                ActiveCount--;
            }
        }

        private void Update() => Tick(Time.deltaTime);

        private void OnDestroy() { if (_industrialMesh != null) Destroy(_industrialMesh); }

        private Item FindAvailable()
        {
            for (var i = 0; i < _items.Length; i++)
            {
                var index = (_cursor + i) % _items.Length;
                if (_items[index].Object.activeSelf) continue;
                _cursor = (index + 1) % _items.Length;
                return _items[index];
            }

            var item = _items[_cursor];
            _cursor = (_cursor + 1) % _items.Length;
            Reset(item);
            ActiveCount--;
            return item;
        }

        private static void Reset(Item item)
        {
            item.Age = 0f;
            item.Duration = 0f;
            item.Target = null;
            item.Transform.localScale = Vector3.one;
            item.Object.SetActive(false);
        }
    }
}
