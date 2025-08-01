using System;
using System.Collections.Generic;
using InventorySystem.Items;

namespace InventorySystem.Inventory
{
    /// <summary>
    ///     Interface for an Inventory.
    /// </summary>
    public interface IInventory
    {

        /// <summary>
        /// Event invoked when <see cref="IItem"/>s were successfully added to this.
        /// Returns the newly added <see cref="IItem"/> and the added quantity.
        /// </summary>
        /// <remarks>
        /// Triggered per slot that receives items; each invocation returns the added <see cref="IItem"/> and
        /// the quantity placed into that slot. (i.e., if more <see cref="IItem"/>s are added than
        /// <see cref="ItemData.MaxStackAmount"/> for one slot, and items are added to a second 
        /// slot, the event will fire again for the second slot.)
        /// </remarks>
        public event Action<IItem, int> ItemsAdded;

        /// <summary>
        /// Event invoked when <see cref="IItem"/>s were successfully removed from this.
        /// Returns the newly removed <see cref="IItem"/> and the removed quantity.
        /// </summary>
        /// <remarks>
        /// Fired per slot that loses items; each invocation returns the removed <see cref="IItem"/> and
        /// the quantity taken from that slot.
        /// </remarks>
        public event Action<IItem, int> ItemsRemoved;

        /// <summary>
        ///     Maximum amount of items that can be inside the <see cref="IInventory" />.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        ///     Whether the entire <see cref="IInventory" /> is empty meaning all <see cref="IInventorySlot" />s are empty.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Get all <see cref="IInventorySlot"/>s in this.
        /// </summary>
        /// <returns><see cref="IReadOnlyList{IInventorySlot}"/> of all <see cref="IInventorySlot" />s in this <see cref="IInventory" />.
        /// </returns>
        /// <remarks>
        ///     WARNING: Also returns empty <see cref="IInventorySlot"/>s.
        ///     Thus, always returns a list of size equal to <see cref="Capacity" />.
        /// </remarks>
        public IReadOnlyList<IInventorySlot> InventorySlots { get; }

        /// <summary>
        ///     Get all <see cref="IItem" />s in <see cref="IInventory" />.
        /// </summary>
        /// <returns><see cref="IReadOnlyList{IItem}"/> of all <see cref="IItem" />s in this <see cref="IInventory" />.
        /// </returns>
        /// <remarks>
        ///     WARNING: Also returns empty <see cref="IItem"/>s as null.
        ///     Thus, always returns a list of size equal to <see cref="Capacity" />.
        /// </remarks>
        public IReadOnlyList<IItem> Items { get; }


        /// <summary>
        ///     Try to get the <see cref="IInventorySlot" /> at slotIndex.
        /// </summary>
        /// <param name="index">Index to look for the <see cref="IInventorySlot" /></param>
        /// <returns><see cref="IInventorySlot" /> or null, if the index is higher than the <see cref="IInventory.Capacity" />.</returns>
        public IInventorySlot TryGetSlotAt(int index);

        /// <summary>
        ///     Whether an <see cref="IItem" /> item can be added into an <see cref="IInventorySlot" />.
        ///     Does not add the <see cref="IItem" />.
        /// </summary>
        /// <param name="item"><see cref="IItem" /> to be checked.</param>
        /// <param name="addIndex">
        ///     First found index for the <see cref="IItem" /> item to be added into.
        ///     Looks for index with equivalent, non-full <see cref="IInventorySlot" /> and empty slots second.
        ///     Returns -1, if it cannot be added.
        /// </param>
        /// <returns>Whether the <see cref="IItem" /> can be added or not.</returns>
        /// <remarks>
        ///     Does not guarantee that the <see cref="IItem" /> can be added with a quantity greater than one.
        ///     Only guarantees that there is either an empty <see cref="IInventorySlot" />
        ///     or a non-full <see cref="IInventorySlot" /> that is already occupied by the item
        ///     that is to be added.
        ///     It is still possible that the quantity of the <see cref="IItem" /> being
        ///     added exceeds the <see cref="ItemSO.maxStackAmount" /> and thus needs to occupy
        ///     additional Slots to be fully added.
        /// </remarks>
        public bool IsSlotAvailable(IItem item, out int addIndex);

        /// <summary>
        ///     Try to <see cref="IInventorySlot.Clear" /> the <see cref="IInventorySlot" />, if it's valid.
        /// </summary>
        /// <param name="index">The index of the <see cref="IInventorySlot" /> to clear.</param>
        public void TryClearSlotAt(int index);

        /// <summary>
        ///     Try to add an <see cref="IItem" /> into the first possible <see cref="IInventorySlot" />.
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> to be added.</param>
        /// <param name="quantity">The number of <see cref="IItem" /> item to be added. </param>
        /// <returns>Whether the <see cref="IItem" /> item was added.</returns>
        /// <remarks>
        ///     Wrapper for <see cref="TryAddItemAt" />.
        ///     If the quantity exceeds the first found <see cref="IInventorySlot" /> capacity,
        ///     it tries to find the next empty <see cref="IInventorySlot" /> and adds the leftover amount there.
        ///     Loops recursively until all items are stored in the <see cref="IInventory" /> or until there is
        ///     no more space available. If no more space is available, discards/ignores the remaining leftover.
        ///     Returns true, even if any leftover is discarded.
        /// </remarks>
        public bool TryAddItem(IItem item, int quantity = 1);

        /// <summary>
        ///     Try to add an <see cref="IItem" /> item into the <see cref="IInventorySlot" /> at index.
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> to be added.</param>
        /// <param name="index">The index of the <see cref="IInventorySlot" /> that the item is trying to be added into.</param>
        /// <param name="quantity">The number of <see cref="IItem" /> item to be added.</param>
        /// <returns>Whether the <see cref="IItem" /> item was added.</returns>
        /// <remarks>
        ///     If the quantity exceeds the <see cref="IInventorySlot.Quantity" /> of the <see cref="IInventorySlot" /> capacity,
        ///     it tries to find the next non-full occupied <see cref="IInventorySlot" /> by the same <see cref="IItem" /> item
        ///     or the next empty <see cref="IInventorySlot" /> and adds the leftover amount there.
        ///     Loops recursively until all items are stored in the <see cref="IInventory" /> or until there is
        ///     no more space available. If no more space is available, discards/ignores the remaining leftover.
        ///     Returns true, even if any leftover is discarded.
        ///     WARNING: If <see cref="Inventory._handlesOverflow" /> is true, it only tries to add the item to the first found
        ///     <see cref="IInventorySlot" />
        ///     and returns true if any amount was added.
        /// </remarks>
        public bool TryAddItemAt(IItem item, int index, int quantity = 1);

        /// <summary>
        ///     Try to insert an <see cref="IItem" /> at the front of the <see cref="IInventory" />, i.e., in
        ///     <see cref="IInventorySlot" /> 0.
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> to be added to the <see cref="IInventory" /> at the front.</param>
        /// <param name="quantity">The quantity of <see cref="IItem" /> to be added.</param>
        /// <returns>Whether the <see cref="IItem" /> was added to the front.</returns>
        /// <remarks>
        ///     Pushed all other <see cref="IItem" />s already in the <see cref="IInventory" /> one slot up.
        ///     FIXME: If the <see cref="quantity" /> exceeds <see cref="ItemSO.maxStackAmount" />, it is
        ///     possible that <see cref="IItem" />s that were previously in the <see cref="IInventory" />
        ///     get removed.
        /// </remarks>
        public bool TryInsertItemAtFront(IItem item, int quantity = 1);

        /// <summary>
        ///     Try to get <see cref="IItem" /> as index.
        /// </summary>
        /// <param name="index">The index to look for <see cref="IItem" /> item.</param>
        /// <returns>
        ///     Returns <see cref="IItem" />, if the index is inside the bounds
        ///     of the <see cref="Capacity" />. Otherwise, or if <see cref="IInventorySlot" />
        ///     <see cref="IsEmpty" />, returns null.
        /// </returns>
        public IItem TryGetItemAt(int index);

        /// <summary>
        ///     Remove <see cref="IItem" /> item from the <see cref="IInventory" />.
        ///     If the <see cref="IItem" /> is in the <see cref="IInventory" />,
        ///     we call <see cref="IInventorySlot.Clear" /> on its <see cref="IInventorySlot" />.
        /// </summary>
        /// <param name="item"><see cref="IItem" /> to be removed.</param>
        /// <param name="onlyFirstOccurence">
        ///     Whether only the first occurence of the <see cref="IItem" /> in the
        ///     <see cref="IInventory" /> gets removed (true) or all of its occurrences (i.e., we have the same
        ///     <see cref="IItem" /> in multiple <see cref="IInventorySlot" />s).
        /// </param>
        /// <seealso cref="IInventorySlot.Clear" />
        public void RemoveItem(IItem item, bool onlyFirstOccurence = false);

        /// <summary>
        ///     Clear the entire <see cref="IInventory"/> by clearing all <see cref="IInventorySlot"/>s.
        /// </summary>
        public void Clear();
    }
}