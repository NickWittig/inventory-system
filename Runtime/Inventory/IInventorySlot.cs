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

        /// <summary>
        ///     Try to add <see cref="Quantity" /> and return overflow amount.
        /// </summary>
        /// <param name="quantity"> The quantity that <see cref="Quantity" /> is trying to be added.</param>
        /// <param name="overflow">
        ///     The amount of <see cref="IItem" />
        ///     that exceeds the <see cref="ItemSO.maxStackAmount" /> for <see cref="IInventorySlot.Item" />.
        ///     Set to 0, if there is no overflow.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     Thrown, if this <see cref="IInventorySlot" /> has no <see cref="Item" /> assinged.
        /// </exception>
        public void TryAddQuantity(int quantity, out int overflow);

        /// <summary>
        ///     Clear this slot.
        ///     Sets <see cref="Item" /> to null and <see cref="Quantity" /> to 0.
        /// </summary>
        public void Clear();
    }
}