using System;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using UnityEngine;

namespace Gravivore.Presentation.Player
{
    [DefaultExecutionOrder(300)]
    public sealed class WeaponEquipmentPresenter : MonoBehaviour
    {
        private EquipmentService _equipment;
        private PlayerEvolutionView _view;
        private GravityLashVfxPool _lash;
        private GameObject[] _weapons;
        private Transform[] _muzzles;
        private Transform[,] _rankModels;
        private Vector3[,] _rankOrigins;
        public int VisualRank { get; private set; }
        public bool IsEquipped { get; private set; }
        public Transform ActiveMuzzle => IsEquipped ? _muzzles[(int)_view.CurrentTier] : null;
        public int AttachmentCount => _weapons?.Length ?? 0;
        public void Initialize(EquipmentService equipment, PlayerEvolutionView view, GravityLashVfxPool lash, GameObject prefab)
        {
            _equipment = equipment; _view = view; _lash = lash;
            _weapons = new GameObject[3]; _muzzles = new Transform[3];
            _rankModels = new Transform[3,5]; _rankOrigins = new Vector3[3,5];
            for (var i = 0; i < 3; i++)
            {
                var form = view.GetTierForm((EvolutionTier)i);
                Transform hardpoint = null;
                foreach (var node in form.GetComponentsInChildren<Transform>(true))
                    if (node.name == "R_TOOL") { hardpoint = node; break; }
                if (hardpoint == null) throw new InvalidOperationException("Approved bipedal G-0 needs its R_TOOL hardpoint.");
                var weapon = Instantiate(prefab, hardpoint, false); weapon.name = "M-0 Equipped Weapon";
                _weapons[i] = weapon; _muzzles[i] = weapon.transform.Find("Muzzle");
                if (_muzzles[i] == null) throw new InvalidOperationException("M-0 muzzle is required.");
                for(var rank=0;rank<5;rank++)
                {
                    var model=weapon.transform.Find("Rank"+(rank+1)); _rankModels[i,rank]=model;
                    if(model!=null)
                    {
                        var origin=model.Find("RankMuzzle");
                        if(origin==null)throw new InvalidOperationException("M-0 rank muzzle is required.");
                        _rankOrigins[i,rank]=origin.localPosition;
                    }
                }
            }
            equipment.EquipmentChanged += Changed;
            equipment.InventoryChanged += InventoryChanged;
            Apply();
        }
        private void Changed(EquipmentChangedEvent value) { if (value.Slot == EquipmentSlot.Weapon) Apply(); }
        private void InventoryChanged(InventoryChangedEvent value) => Apply();
        private void Apply()
        {
            IsEquipped = _equipment.Inventory.TryGetEquipped(EquipmentSlot.Weapon, out var id) && id == Chapter01Weapon.ItemId;
            VisualRank=Mathf.Clamp(_equipment.Inventory.GetRank(Chapter01Weapon.ItemId),1,5);
            for(var tier=0;tier<3;tier++)for(var rank=0;rank<5;rank++)
            {
                var model=_rankModels[tier,rank];if(model==null)continue;
                model.gameObject.SetActive(rank==VisualRank-1);
                if(rank==VisualRank-1)_muzzles[tier].localPosition=_rankOrigins[tier,rank];
            }
            foreach (var weapon in _weapons) weapon.SetActive(IsEquipped);
            _lash.SetEquipmentOrigin(ActiveMuzzle);
        }
        private void LateUpdate() { if (_equipment != null) _lash.SetEquipmentOrigin(ActiveMuzzle); }
        private void OnDestroy()
        {if(_equipment!=null){_equipment.EquipmentChanged-=Changed;_equipment.InventoryChanged-=InventoryChanged;}}
    }
}
