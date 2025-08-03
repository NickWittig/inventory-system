namespace InventorySystem.EquipmentInventory
{
    public static class EquipmentInventoryFactory
    {
        public static IEquipmentInventory Create()
        {
            return EquipmentInventory.Create();
        }
    }
}