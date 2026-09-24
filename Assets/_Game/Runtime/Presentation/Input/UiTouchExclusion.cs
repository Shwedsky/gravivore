using System;
using UnityEngine;

namespace Gravivore.Presentation.Input
{
    [DisallowMultipleComponent]
    public sealed class UiTouchExclusion : MonoBehaviour, IUiTouchExclusion
    {
        [SerializeField] private RectTransform[] _excludedRegions = Array.Empty<RectTransform>();
        [SerializeField] private Camera _uiCamera;

        public void Initialize(RectTransform[] excludedRegions, Camera uiCamera = null)
        {
            _excludedRegions = excludedRegions ?? Array.Empty<RectTransform>();
            _uiCamera = uiCamera;
        }

        public bool Blocks(Vector2 screenPosition)
        {
            for (var i = 0; i < _excludedRegions.Length; i++)
            {
                var region = _excludedRegions[i];
                if (region != null && region.gameObject.activeInHierarchy &&
                    RectTransformUtility.RectangleContainsScreenPoint(region, screenPosition, _uiCamera))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
