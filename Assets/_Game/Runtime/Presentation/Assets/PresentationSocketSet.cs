using System;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    public enum PresentationSocket
    {
        Root, Core, Sensor, WeaponLeft, WeaponRight, AttackOrigin, HitCenter,
        GroundContact, VfxTop, VfxRear, TelegraphOrigin
    }

    /// <summary>Optional exact-name transforms, cached once inside one visual hierarchy. No gameplay references.</summary>
    public sealed class PresentationSocketSet
    {
        private readonly Transform[] _sockets;

        public PresentationSocketSet(Transform visualRoot)
        {
            if (visualRoot == null) throw new ArgumentNullException(nameof(visualRoot));
            var names = Enum.GetNames(typeof(PresentationSocket));
            _sockets = new Transform[names.Length];
            // A dedicated container avoids treating arbitrary imported bone names as sockets.
            // Without it, only direct children are opt-in socket candidates.
            var container = visualRoot.Find("Presentation Sockets");
            var nodes = container != null ? container.GetComponentsInChildren<Transform>(true) : DirectChildren(visualRoot);
            for (var n = 0; n < nodes.Length; n++)
                for (var i = 0; i < names.Length; i++)
                    if (string.Equals(nodes[n].name, names[i], StringComparison.Ordinal))
                    {
                        if (_sockets[i] != null)
                            throw new InvalidOperationException("Ambiguous presentation socket: " + names[i]);
                        _sockets[i] = nodes[n];
                    }
            if (_sockets[(int)PresentationSocket.Root] == null) _sockets[(int)PresentationSocket.Root] = visualRoot;
        }

        private static Transform[] DirectChildren(Transform root)
        {
            var children = new Transform[root.childCount];
            for (var i = 0; i < children.Length; i++) children[i] = root.GetChild(i);
            return children;
        }

        public bool TryGet(PresentationSocket id, out Transform socket)
        {
            var index = (int)id;
            if (index < 0 || index >= _sockets.Length) throw new ArgumentOutOfRangeException(nameof(id));
            socket = _sockets[index];
            return socket != null;
        }

        public Transform GetOr(PresentationSocket id, Transform fallback) => TryGet(id, out var socket) ? socket : fallback;
    }
}
