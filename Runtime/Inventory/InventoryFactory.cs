namespace InventorySystem.Inventory
{
    /// <summary>
    /// Static factory to create <see cref="IInventory"/>s.
    /// </summary>
    public static class InventoryFactory
    {
        public static IInventory Create(int capacity = 2, bool handlesOverflow = true, int maxCapacity = 2)
        {
            return new Inventory.Builder()
                .WithOverflow(handlesOverflow)
                .WithStartCapacity(capacity)
                .WithMaxCapacity(maxCapacity)
                .Build();
        }
    }
}