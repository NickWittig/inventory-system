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
        [SerializeField] private InventorySlot[] _slots;
        [field: SerializeField] public int Capacity { get; private set; }
        public bool IsEmpty => _slots.All(slot => slot.IsEmpty);

        /// <summary>
        ///     Whether any Overflow (i.e., adding an <see cref="IItem" />
        ///     with a quantity higher than <see cref="ItemSO.maxStackAmount" />)
        ///     is being handled further or is being discarded.
        /// </summary>
        [SerializeField] private bool _handlesOverflow;

        /// <inheritdoc/>
        public event Action<IItem, int> ItemsAdded;

        /// <inheritdoc/>
        public event Action<IItem, int> ItemsRemoved;

        /// <inheritdoc cref="IInventory.Items" />
        public IReadOnlyList<IItem> Items => _slots.Select(slot => slot.Item).ToList();

        /// <inheritdoc cref="IInventory.InventorySlots" />
        public IReadOnlyList<IInventorySlot> InventorySlots => _slots;

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
                _slots = new InventorySlot[capacity]
            };

            for (var i = 0; i < capacity; i++) inventory._slots[i] = new InventorySlot();

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
            _slots[index].Clear();
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

            InventorySlot slot = _slots[index];

            if (!slot.IsEmpty && !slot.Item.IsEquivalentTo(item))
            {
                Debug.LogWarning(
                    "Tried to add item to a non-matching occupied slot and no other slots are empty. Ignored.");
                return false;
            }

            // here, the slot now contains the item to be added
            var overflow = slot.SetItemAndQuantity(item, quantity);
            var addedItemAmount = quantity - overflow;
            OnItemsAdded(item, addedItemAmount);

            int remaining = overflow;
            // if there is any overflow, but don't handle it, we return true (item was added until the slot was filled)
            if (remaining > 0 && !_handlesOverflow) return true;
            if (remaining <= 0) return true;
            // otherwise, if the overflow is equal or less than 0, we added the entire quantity that was requested

            // otherwise, there is an overflow, and we do handle it
            // we look for the next available slot
            // and try to add it there with the remaining quantity
            return TryAddItem(item, remaining);
        }

        /// <inheritdoc cref="IInventory.TryGetItemAt" />
        public IItem TryGetItemAt(int index)
        {
            return IsValidSlotIndex(index) ? _slots[index].Item : null;
        }

        /// <inheritdoc cref="IInventory.RemoveItem" />
        public void RemoveItem(IItem item, bool onlyFirstOccurence = false)
        {
            foreach (InventorySlot slot in _slots)
            {
                if (slot.Item != item) continue;
                ClearSlotWithEvent(slot);
                if (onlyFirstOccurence) return;
            }
        }


  

        /// <inheritdoc cref="IInventory.Clear" />
        public void Clear()
        {
            foreach (InventorySlot slot in _slots)
            {
                ClearSlotWithEvent(slot);
            }
        }

        /// <inheritdoc cref="IInventory.TryInsertItemAtFront" />
        public bool TryInsertItemAtFront(IItem item, int quantity = 1)
        {
            if (!IsSlotAvailable(item, out _)) return false;
            var existingSlots = _slots.Select(slot => slot.DeepCopy()).ToList();
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
            return IsValidSlotIndex(index) ? _slots[index] : null;
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
            for (var i = 0; i < _slots.Length; i++)
                if (!_slots[i].IsEmpty && !_slots[i].IsFull && _slots[i].Item.IsEquivalentTo(item))
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
            for (var i = 0; i < _slots.Length; i++)
                if (_slots[i].IsEmpty)
                    return i;

            return -1;
        }

        /// <summary>
        ///     Whether the index is valid (i.e., would not throw a <see cref="ArgumentOutOfRangeException" />) for
        ///     <see cref="_slots" />.
        /// </summary>
        /// <param name="index">The index to be checked.</param>
        private bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < _slots.Length;
        }

        /// <summary>
        /// Clear the <see cref="InventorySlot"/> slot and call the <see cref="OnItemsRemoved(IItem, int)"/> method.
        /// </summary>
        /// <param name="slot">The slot to be cleared.</param>
        /// <remarks>
        /// Wrapper for <see cref="InventorySlot.Clear"/>
        /// </remarks>
        private void ClearSlotWithEvent(InventorySlot slot)
        {
            var (slotItem, slotQuantity) = (slot.Item, slot.Quantity);
            slot.Clear();
            OnItemsRemoved(slotItem, slotQuantity);
        }

        /// <summary>
        /// Trigger the <see cref="ItemsAdded"/> event.
        /// </summary>
        /// <param name="item">The <see cref="IItem"/> that was successfully added to this.</param>
        /// <param name="quantity">The quantity that was successfully added.</param>
        private void OnItemsAdded(IItem item, int quantity)
        {
            if (item == null || quantity < 1) return;
            ItemsAdded?.Invoke(item, quantity);
        }

        /// <summary>
        /// Trigger the <see cref="ItemsRemoved"/> event.
        /// </summary>
        /// <param name="item">The <see cref="IItem"/> that was successfully removed from this.</param>
        /// <param name="quantity">The quantity that was successfully removed.</param>
        private void OnItemsRemoved(IItem item, int quantity)
        {
            if (item == null || quantity < 1) return;
            ItemsRemoved?.Invoke(item, quantity);
        }



    }
}