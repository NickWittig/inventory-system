using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.Inventory
{
    [Serializable]
    public class Inventory : IInventory
    {
        [SerializeField] private InventorySlot[] slots;
        [field: SerializeField] public int Capacity { get; private set; }
        public bool IsEmpty => slots.All(slot => slot.IsEmpty);

        /// <summary>
        ///     Whether any Overflow (i.e., adding an <see cref="IItem" />
        ///     with a quantity higher than <see cref="ItemSO.maxStackAmount" />)
        ///     is being handled further or is being discarded.
        /// </summary>
        [SerializeField] private bool _handlesOverflow;

        private Inventory(int capacity = 2, bool handlesOverflow = true)
        {
            Capacity = capacity;
            _handlesOverflow = handlesOverflow;
        }

        /// <summary>
        ///     Create a new <see cref="Inventory" />.
        ///     Also, creates and assigns capacity amount of <see cref="InventorySlot" />s.
        /// </summary>
        /// <param name="capacity">Amount of <see cref="IInventorySlot" /> in this.</param>
        /// <param name="handlesOverflow">Whether overflow is handled or discarded.</param>
        /// <returns>A new <see cref="Inventory" />.</returns>
        public static Inventory Create(int capacity = 2, bool handlesOverflow = true)
        {
            var inventory = new Inventory(capacity, handlesOverflow)
            {
                slots = new InventorySlot[capacity]
            };

            for (var i = 0; i < capacity; i++) inventory.slots[i] = new InventorySlot();

            return inventory;
        }

        /// <inheritdoc cref="IInventory.IsSlotAvailable" />
        public bool IsSlotAvailable(IItem item, out int addIndex)
        {
            addIndex = GetFirstNonFullEquivalentSlotIndex(item);
            if (addIndex == -1) addIndex = GetFirstEmptySlotIndex();

            // if we don't have an equivalent item or free item slot, we cannot add the item
            return addIndex != -1;
        }

        /// <inheritdoc cref="IInventory.TryClearSlotAt" />
        public void TryClearSlotAt(int index)
        {
            if (!IsValidSlotIndex(index)) return;
            slots[index].Clear();
        }

        /// <inheritdoc cref="IInventory.TryAddItem" />
        public bool TryAddItem(IItem item, int quantity = 1)
        {
            return IsSlotAvailable(item, out var index) && TryAddItemAt(item, index, quantity);
        }

        /// <inheritdoc cref="IInventory.TryAddItemAt" />
        public bool TryAddItemAt(IItem item, int index, int quantity = 1)
        {
            if (!IsValidSlotIndex(index))
            {
                Debug.LogWarning($"Invalid slot index {index}");
                return false;
            }

            InventorySlot slot = slots[index];

            if (!slot.IsEmpty && !slot.Item.IsEquivalentTo(item))
            {
                Debug.LogWarning(
                    "Tried to add item to a non-matching occupied slot and no other slots are empty. Ignored.");
                return false;
            }

            var remaining = 0;
            // if the slot is empty,
            // we add the item to the slot
            if (slot.IsEmpty) slot.SetItem(item);

            // the slot now contains the item to be added, so we add the quantity
            slot.TryAddQuantity(quantity, out var overflow);

            remaining = overflow;
            // if there is any overflow, but don't handle it, we return true (item was added until the slot was filled)
            if (remaining > 0 && !_handlesOverflow) return true;
            // otherwise, if the overflow is equal or less than 0, we added the entire quantity that was requested
            if (remaining <= 0) return true;

            // otherwise, there is an overflow, and we do handle it
            // we look for the next available slot
            // and try to add it there with the remaining quantity
            return TryAddItem(item, remaining);
        }

        /// <inheritdoc cref="IInventory.TryGetItemAt" />
        public IItem TryGetItemAt(int index)
        {
            return IsValidSlotIndex(index) ? slots[index].Item : null;
        }

        /// <inheritdoc cref="IInventory.RemoveItem" />
        public void RemoveItem(IItem item, bool onlyFirstOccurence = false)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.Item != item) continue;
                slot.Clear();
                if (onlyFirstOccurence) return;
            }
        }


        /// <inheritdoc cref="IInventory.GetAllItems" />
        public List<IItem> GetAllItems()
        {
            return slots.Select(slot => slot.Item).ToList();
        }

        /// <inheritdoc cref="IInventory.Clear" />
        public void Clear()
        {
            foreach (InventorySlot slot in slots) slot.Clear();
        }

        /// <inheritdoc cref="IInventory.TryInsertItemAtFront" />
        public bool TryInsertItemAtFront(IItem item, int quantity = 1)
        {
            if (!IsSlotAvailable(item, out _)) return false;
            var existingSlots = slots.Select(slot => slot.DeepCopy()).ToList();
            Clear();
            TryAddItem(item, quantity);
            foreach (InventorySlot existingSlot in existingSlots
                         .Where(existingSlot => !existingSlot.IsEmpty))
                TryAddItem(existingSlot.Item, existingSlot.Quantity);

            return true;
        }

        /// <inheritdoc cref="IInventory.TryGetSlotAt" />
        public IInventorySlot TryGetSlotAt(int index)
        {
            return IsValidSlotIndex(index) ? slots[index] : null;
        }

        /// <summary>
        ///     Get the index of the first <see cref="IInventorySlot" /> that is:
        ///     <list type="number">
        ///         <item>
        ///             Not Empty.
        ///         </item>
        ///         <item>
        ///             Not full.
        ///         </item>
        ///         <item>
        ///             Has an <see cref="IItem" /> assinged that is equivalent to item.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> that is checked for equality.</param>
        /// <returns>
        ///     The index of the <see cref="IInventorySlot" /> matching the conditions.
        ///     Returns -1, if no matching <see cref="IInventorySlot" /> was found.
        /// </returns>
        private int GetFirstNonFullEquivalentSlotIndex(IItem item)
        {
            for (var i = 0; i < slots.Length; i++)
                if (!slots[i].IsEmpty && !slots[i].IsFull && slots[i].Item.IsEquivalentTo(item))
                    return i;

            return -1;
        }

        /// <summary>
        ///     Get the index of the first empty <see cref="IInventorySlot" />.
        /// </summary>
        /// <returns>
        ///     Index of the first empty <see cref="IInventorySlot" /> or -1, if there are no empty
        ///     <see cref="IInventorySlot" />s.
        /// </returns>
        private int GetFirstEmptySlotIndex()
        {
            for (var i = 0; i < slots.Length; i++)
                if (slots[i].IsEmpty)
                    return i;

            return -1;
        }

        /// <summary>
        ///     Whether the index is valid (i.e., would not throw a <see cref="ArgumentOutOfRangeException" />) for
        ///     <see cref="slots" />.
        /// </summary>
        /// <param name="index">The index to be checked.</param>
        private bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < slots.Length;
        }
    }
}