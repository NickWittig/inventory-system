namespace InventorySystem.EquipmentInventory.Weapons
{
    
    /// <summary>
    /// Interface for any weapon.
    /// Always is an <see cref="IEquipmentItem"/>.
    /// </summary>
    public interface IWeapon : IEquipmentItem
    {
        public WeaponData WeaponData { get; }
        public void Use();
        
    }
}