using System;
using InventorySystem.Items;
using InventorySystem.EquipmentInventory.Weapons;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Weapons
{
    [Serializable]
    [CreateAssetMenu(fileName = "Weapon", menuName = "Items/Weapon")]
    public class WeaponData : ItemData
    {
        internal const EquipmentType _equipmentType = EquipmentType.Weapon;
          
        public float Damage; 
        public float Range;
        
        public WeaponType WeaponType;
        [SerializeReference] public WeaponAttackStrategy WeaponAttackStrategy;
    }
}