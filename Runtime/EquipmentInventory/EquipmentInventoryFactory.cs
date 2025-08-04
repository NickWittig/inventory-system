namespace InventorySystem.EquipmentInventory
{
    /// <summary>
    ///     Static factory to create <see cref="IEquipmentInventory"/>s.
    /// </summary>
    public static class EquipmentInventoryFactory
    {
        /// <summary>
        ///     Create a new <see cref="IEquipmentInventory"/>.
        /// </summary>
        /// <returns>
        ///     A new <see cref="IEquipmentInventory"/>.
        /// </returns>
        public static IEquipmentInventory Create()
        {
            return EquipmentInventory.Create();
        }
    }
}