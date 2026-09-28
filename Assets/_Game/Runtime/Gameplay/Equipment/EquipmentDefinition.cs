using UnityEngine;

namespace Gravivore.Gameplay.Equipment
{
    [CreateAssetMenu(fileName = "EquipmentDefinition", menuName = "Gravivore/Equipment/Item Definition")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private EquipmentSlot _slot;
        [SerializeField] private float _baseDamage;
        [SerializeField] private float _maxHp;
        [SerializeField] private float _armorValue;
        [SerializeField] private float _attackInterval;
        [SerializeField] private float _moveSpeed;

        public string Id => _id;
        public EquipmentSlot Slot => _slot;

        public EquipmentItem Item
        {
            get
            {
                var modifier = new EquipmentFlatModifier(
                    _baseDamage,
                    _maxHp,
                    _armorValue,
                    _attackInterval,
                    _moveSpeed);
                return new EquipmentItem(_id, _displayName, _slot, modifier);
            }
        }

        public void ValidateOrThrow() => _ = Item;
    }
}
