using System;
using UnityEngine;

namespace Gravivore.Presentation.UI
{
    [CreateAssetMenu(menuName="Gravivore/Presentation/Production UI Skin")]
    public sealed class ProductionUiSkinDefinition : ScriptableObject
    {
        [SerializeField] private Sprite _frame, _selected, _utility, _button, _divider;
        [SerializeField] private Sprite[] _weaponThumbnails;
        public Sprite Frame => _frame;
        public Sprite Selected => _selected;
        public Sprite Utility => _utility;
        public Sprite Button => _button;
        public Sprite Divider => _divider;
        public Sprite WeaponThumbnail(int rank)
        {
            if (_weaponThumbnails == null || _weaponThumbnails.Length != 5) return null;
            return _weaponThumbnails[Mathf.Clamp(rank,1,5)-1];
        }
        public void ValidateOrThrow()
        {
            if(_frame==null||_selected==null||_utility==null||_weaponThumbnails==null||_weaponThumbnails.Length!=5)
                throw new InvalidOperationException("Production skin requires frames and five actual-model weapon thumbnails.");
            foreach(var sprite in _weaponThumbnails)if(sprite==null)throw new InvalidOperationException("Missing weapon thumbnail.");
        }
    }
}
