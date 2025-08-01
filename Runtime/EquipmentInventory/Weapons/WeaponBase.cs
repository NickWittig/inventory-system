using System;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Weapons
{
    [Serializable]
    public class WeaponBase : IWeapon 
    {
        [SerializeField] private WeaponData _weaponData; 
        public WeaponData WeaponData => _weaponData;
        public ItemData ItemData => _weaponData;
        public EquipmentType EquipmentType { get; }

        /// <summary>
        /// FIXME: This is a shallow copy regarding <see cref="WeaponData"/>.
        /// </summary>
        /// <returns></returns>
        public IItem DeepCopy()
        {
            return new WeaponBase(_weaponData);
        }

        /// <summary>
        /// FIXME: Not fully implemented (should check individual values in weapon data
        /// or make weapon data <see cref="IsEquivalentTo"/>).
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEquivalentTo(IItem other)
        {
            if (other is not WeaponBase otherWeaponBase) return false;
            if (ItemData != otherWeaponBase.ItemData) return false;
            if (otherWeaponBase.WeaponData != _weaponData) return false;
            return true;
        }
        
        public WeaponBase(WeaponData weaponData)
        {
            _weaponData = weaponData;
            EquipmentType = WeaponData.EQUIPMENT_TYPE;
        }

        public virtual void Use()
        {
            Debug.Log($"Weapon {_weaponData.name} used.");
            if (_weaponData.WeaponAttackStrategy)
            {
                _weaponData.WeaponAttackStrategy.Execute();
            }
        }

    }
}