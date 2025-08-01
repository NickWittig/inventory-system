using System;
using InventorySystem.Items;

namespace InventorySystem.Inventory
{
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
        ///     Quantity is limited by <see cref="ItemSO.maxStackAmount" />,
        ///     which is enforced in <see cref="TryAddQuantity" />.
        /// </remarks>
        int Quantity { get; }

        /// <summary>
        ///     Whether an <see cref="IItem" /> is assigned to this <see cref="IInventorySlot" />
        ///     with a <see cref="Quantity" /> not equal 0.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        ///     Check if the <see cref="Quantity" /> greater than or equal
        ///     to the assigned <see cref="IItem" />s <see cref="ItemSO.maxStackAmount" />.
        /// </summary>
        bool IsFull { get; }


    }
}