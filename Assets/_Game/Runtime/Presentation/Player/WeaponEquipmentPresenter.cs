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
        public bool IsEquipped { get; private set; }
        public Transform ActiveMuzzle => IsEquipped ? _muzzles[(int)_view.CurrentTier] : null;
        public int AttachmentCount => _weapons?.Length ?? 0;
        public void Initialize(EquipmentService equipment, PlayerEvolutionView view, GravityLashVfxPool lash, GameObject prefab)
        {
            _equipment = equipment; _view = view; _lash = lash;
            _weapons = new GameObject[3]; _muzzles = new Transform[3];
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
            }
            equipment.EquipmentChanged += Changed;
            Apply();
        }
        private void Changed(EquipmentChangedEvent value) { if (value.Slot == EquipmentSlot.Weapon) Apply(); }
        private void Apply()
        {
            IsEquipped = _equipment.Inventory.TryGetEquipped(EquipmentSlot.Weapon, out var id) && id == Chapter01Weapon.ItemId;
            foreach (var weapon in _weapons) weapon.SetActive(IsEquipped);
            _lash.SetEquipmentOrigin(ActiveMuzzle);
        }
        private void LateUpdate() { if (_equipment != null) _lash.SetEquipmentOrigin(ActiveMuzzle); }
        private void OnDestroy() { if (_equipment != null) _equipment.EquipmentChanged -= Changed; }
    }
}
