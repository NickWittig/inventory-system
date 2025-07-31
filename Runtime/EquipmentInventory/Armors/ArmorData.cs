using System;
using InventorySystem.EquipmentInventory;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Armors
{
    
    /// <summary>
    /// <see cref="ScriptableObject"/> defining data for an <see cref="IEquipmentItem"/>
    /// of type Armor.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Armor", menuName = "Items/Armor")]
    public class ArmorData : ItemData
    {
        public float ArmorValue;
    }

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
