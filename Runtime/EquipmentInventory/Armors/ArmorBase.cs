using System;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Armors
{
    public class ArmorBase : IEquipmentItem
    {
        [SerializeField] private ArmorData _armorData;

        public ArmorBase(ArmorData armorData)
        {
            _armorData = armorData;
        }

        public float GetArmorValue()
        {
            return _armorData.ArmorValue;
        }

        public IItem DeepCopy()
        {
            throw new NotImplementedException();
        }

        public bool IsEquivalentTo(IItem other)
        {
            throw new NotImplementedException();
        }

        public ItemData ItemData => _armorData;
        public EquipmentType equipmentType =>  EquipmentType.Armor;
    }
}
