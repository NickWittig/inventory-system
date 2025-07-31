using System;

namespace InventorySystem.EquipmentInventory.Weapons
{
    public class WeaponFactory : IWeaponFactory
    {
        public IWeapon Create(WeaponData weaponData) => weaponData is null ? null : new WeaponBase(weaponData);
    }
}