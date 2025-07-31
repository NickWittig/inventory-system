namespace InventorySystem.EquipmentInventory.Weapons
{
    public interface IWeaponFactory
    {
        public IWeapon Create(WeaponData weaponData);
    }
}