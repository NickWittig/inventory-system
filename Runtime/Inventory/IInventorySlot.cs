using InventorySystem.Items;

namespace InventorySystem.Inventory
{
    /// <summary>
    /// An inventory slot in an <see cref="IInventory"/>, which
    /// manages one <see cref="IItem"/> with a certain <see cref="Quantity"/>.
    /// </summary>
    public interface IInventorySlot
    {
        /// <summary>
        ///     Get <see cref="IItem" /> assigned to this <see cref="IInventorySlot" />.
        /// </summary>
        IItem Item { get; }

        /// <summary>
        ///     Get quantity of <see cref="IInventorySlot.Item" /> in this <see cref="IInventorySlot" />.
        /// </summary>
        /// <remarks>
        ///     Quantity is limited by <see cref="ItemData.MaxStackAmount" />.
        /// </remarks>
        int Quantity { get; }

        /// <summary>
        ///     Whether an <see cref="IItem" /> is assigned to this <see cref="IInventorySlot" />
        ///     with a <see cref="Quantity" /> not equal 0.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        ///     Check if the <see cref="Quantity" /> greater than or equal
        ///     to the assigned <see cref="IItem" />s <see cref="ItemData.MaxStackAmount" />.
        /// </summary>
        bool IsFull { get; }
    }
}