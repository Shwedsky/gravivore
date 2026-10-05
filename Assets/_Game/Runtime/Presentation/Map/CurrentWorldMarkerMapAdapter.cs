using System;
using Gravivore.Presentation.World;

namespace Gravivore.Presentation.Map
{
    public sealed class CurrentWorldMarkerMapAdapter : IMapMarkerSource
    {
        private readonly IWorldMarkerSource _source;
        private readonly Func<float> _playerHeadingDegrees;
        private readonly Func<WorldMarkerSnapshot, string> _displayNameResolver;

        public CurrentWorldMarkerMapAdapter(
            IWorldMarkerSource source,
            Func<float> playerHeadingDegrees = null,
            Func<WorldMarkerSnapshot, string> displayNameResolver = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _playerHeadingDegrees = playerHeadingDegrees;
            _displayNameResolver = displayNameResolver;
        }

        public int Count => _source.Count;

        public MapMarkerSnapshot GetMarker(int index)
        {
            var current = _source.GetMarker(index);
            var kind = MapKind(current.Kind);
            var availability = MapAvailability(current.Status);
            var remainingSeconds = current.Status == WorldMarkerStatus.Respawning
                ? current.NextRespawnSeconds
                : 0f;
            var heading = current.Kind == WorldMarkerKind.Player && _playerHeadingDegrees != null
                ? _playerHeadingDegrees()
                : 0f;
            var progressionLocked = current.Status == WorldMarkerStatus.Locked &&
                                    (current.Kind == WorldMarkerKind.Elite || current.Kind == WorldMarkerKind.Boss);
            var displayName = _displayNameResolver != null
                ? _displayNameResolver(current)
                : DefaultDisplayName(current);

            return new MapMarkerSnapshot(
                current.Id,
                kind,
                current.Position,
                availability,
                displayName,
                true,
                progressionLocked,
                heading,
                remainingSeconds,
                -1f,
                MapRewardState.Unknown);
        }

        private static MapMarkerKind MapKind(WorldMarkerKind kind)
        {
            switch (kind)
            {
                case WorldMarkerKind.Player: return MapMarkerKind.Player;
                case WorldMarkerKind.RegularSpot: return MapMarkerKind.Ordinary;
                case WorldMarkerKind.Elite: return MapMarkerKind.Elite;
                case WorldMarkerKind.Boss: return MapMarkerKind.Boss;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported current world marker kind.");
            }
        }

        private static MapAvailabilityState MapAvailability(WorldMarkerStatus status)
        {
            switch (status)
            {
                case WorldMarkerStatus.Available: return MapAvailabilityState.Available;
                case WorldMarkerStatus.Respawning: return MapAvailabilityState.Cooldown;
                case WorldMarkerStatus.Defeated: return MapAvailabilityState.Defeated;
                case WorldMarkerStatus.Locked: return MapAvailabilityState.Inactive;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported current world marker status.");
            }
        }

        private static string DefaultDisplayName(WorldMarkerSnapshot marker)
        {
            switch (marker.Kind)
            {
                case WorldMarkerKind.Player: return "Игрок";
                case WorldMarkerKind.Elite: return "Элита";
                case WorldMarkerKind.Boss: return "Босс";
                case WorldMarkerKind.RegularSpot: return Gravivore.Presentation.UI.RussianUiText.SpotName(marker.Id);
                default:
                    throw new ArgumentOutOfRangeException(nameof(marker), marker.Kind, "Unsupported current world marker kind.");
            }
        }
    }
}
