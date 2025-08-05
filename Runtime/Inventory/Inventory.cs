using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.Inventory
{
    /// <summary>
    /// The concrete implementation of the <see cref="IInventory"/>.
    /// Manages several <see cref="IInventorySlot"/>s and the <see cref="IItem"/>s inside.
    /// </summary>
    [Serializable]
    internal class Inventory : IInventory
    {
        [SerializeField] private InventorySlot[] _slots;
        [field: SerializeField] public int Capacity { get; private set; }
        [SerializeField] private int _maxCapacity;
        public int MaxCapacity => _maxCapacity;
        public bool IsEmpty => _slots.All(slot => slot.IsEmpty);

        /// <summary>
        ///     Whether any Overflow (i.e., adding an <see cref="IItem" />
        ///     with a quantity higher than <see cref="ItemData.MaxStackAmount" />)
        ///     is being handled further or is being discarded.
        /// </summary>
        [SerializeField] private bool _handlesOverflow;

        /// <inheritdoc cref="IInventory.ItemsAdded" />
        public event Action<IInventorySlot, int> ItemsAdded;

        /// <inheritdoc cref="IInventory.ItemsRemoved" />
        public event Action<IInventorySlot, int> ItemsRemoved;
        
        /// <inheritdoc cref="IInventory.CapacityChanged" />
        public event Action<int> CapacityChanged ;

        /// <inheritdoc cref="IInventory.Items" />
        public IReadOnlyList<IItem> Items => _slots.Select(slot => slot.Item).ToList();

        /// <inheritdoc cref="IInventory.InventorySlots" />
        public IReadOnlyList<IInventorySlot> InventorySlots => _slots;

        private Inventory(int capacity = 2, int maxCapacity = 2, bool handlesOverflow = true)
        {
            Capacity = capacity;
            _maxCapacity = maxCapacity;
            _handlesOverflow = handlesOverflow;
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
            OnItemsAdded(slot, index);

            var remaining = overflow;
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
        public void RemoveItem(IItem item, bool isRemovingFirstOccurenceOnly = false, bool isReversed = false)
        {
            if (item == null) return;

            var slots = isReversed
                ? _slots.Reverse()
                : _slots;

            foreach (InventorySlot slot in slots)
            {
                if (slot.IsEmpty || !slot.Item.IsEquivalentTo(item)) continue;

                ClearSlotWithEvent(slot);

                if (isRemovingFirstOccurenceOnly)
                    return;
            }
        }


        /// <inheritdoc cref="IInventory.Clear" />
        public void Clear()
        {
            foreach (InventorySlot slot in _slots) ClearSlotWithEvent(slot);
        }

        /// <inheritdoc cref="IInventory.TryIncreaseCapacity"/>
        public bool TryIncreaseCapacity(int addedCapacity)
        {
            if (addedCapacity <= 0) return false;
            if (Capacity >= _maxCapacity) return false;
            
            if (Capacity + addedCapacity >= _maxCapacity)
            {
                Capacity = _maxCapacity;
            }
            else
            {
                Capacity += addedCapacity;
            }
            
            var existingSlots = _slots.Select(slot => slot.DeepCopy()).ToList();
            var newSlots = new InventorySlot[Capacity];
            for (var i = 0; i < Capacity; i++)
            {
                if (i <  existingSlots.Count)
                {
                    newSlots[i] = existingSlots[i];
                }
                else
                {
                    newSlots[i] = new InventorySlot();
                }
            }
            _slots = newSlots;
            OnCapacityIncreased(Capacity);
            return true;
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
        ///     Clear the <see cref="InventorySlot" /> slot and call the <see cref="OnItemsRemoved(IInventorySlot,int)" /> method.
        /// </summary>
        /// <param name="slot">The slot to be cleared.</param>
        /// <remarks>
        ///     Wrapper for <see cref="InventorySlot.Clear" />
        /// </remarks>
        private void ClearSlotWithEvent(InventorySlot slot)
        {
            slot.Clear();
            OnItemsRemoved(slot, GetSlotIndex(slot));
        }

        /// <summary>
        ///     Get the index of the <see cref="IInventorySlot" /> in this.
        /// </summary>
        /// <param name="slot">The <see cref="IInventorySlot" /> for which the index is being looked for.</param>
        /// <returns></returns>
        private int GetSlotIndex(InventorySlot slot)
        {
            return Array.IndexOf(_slots, slot);
        }

        /// <summary>
        ///     Trigger the <see cref="ItemsAdded" /> event.
        /// </summary>
        /// <param name="slot">The <see cref="IInventorySlot" /> the <see cref="IItem" /> was added to.</param>
        /// <param name="slotIndex">The index of the slot the <see cref="IItem" />s were added to.</param>
        private void OnItemsAdded(IInventorySlot slot, int slotIndex)
        {
            if (slot.IsEmpty) return;
            ItemsAdded?.Invoke(slot, slotIndex);
        }

        /// <summary>
        ///     Trigger the <see cref="ItemsRemoved" /> event.
        /// </summary>
        /// <param name="slot">The <see cref="InventorySlots" /> the items were removed from.</param>
        /// <param name="index">The index of the <see cref="IInventorySlot" />.</param>
        private void OnItemsRemoved(IInventorySlot slot, int index)
        {
            if (slot is null) return;
            ItemsRemoved?.Invoke(slot, index);
        }

        /// <summary>
        ///     Trigger the <see cref="CapacityChanged"/> event.
        /// </summary>
        /// <param name="newCapacity">
        ///     The new <see cref="Capacity"/> of this.
        /// </param>
        private void OnCapacityIncreased(int newCapacity)
        {
            CapacityChanged?.Invoke(newCapacity);
        }

        
        #region Builder
        /// <summary>
        ///     A builder for creating a new <see cref="Inventory"/>.
        ///     Also, creates and assigns capacity amount of <see cref="InventorySlot" />s.
        ///     Can set maximum capacity.
        /// </summary>
        /// <returns>
        ///     A new <see cref="Inventory" />.
        /// </returns>
        internal class Builder
        {
            int _startCapacity = 2;
            int _maxCapacity = 2;
            bool _handlesOverflow = true;

            /// <summary>
            ///     Set the starting <see cref="Inventory.Capacity"/> of the <see cref="Inventory"/>. 
            /// </summary>
            /// <param name="startCapacity">
            ///     Capacity of the new <see cref="Inventory"/>.
            /// </param>
            /// <returns>
            ///     This <see cref="Builder"/>.
            /// </returns>
            public Builder WithStartCapacity(int startCapacity)
            {
                _startCapacity = startCapacity;
                return this;
            }

            public Builder WithMaxCapacity(int maxCapacity)
            {
                _maxCapacity = maxCapacity;
                return this;
            }
            
            public Builder WithOverflow(bool  handlesOverflow)
            {
                _handlesOverflow = handlesOverflow;
                return this;
            }

            /// <summary>
            ///     Builds the new <see cref="Inventory"/> with
            ///     the values assigned in this.
            /// </summary>
            /// <returns>
            ///     A new <see cref="Inventory"/>.
            /// </returns>
            public Inventory Build()
            {
                var inventory = new Inventory(_startCapacity, _maxCapacity, _handlesOverflow)
                {
                    _slots = new InventorySlot[_startCapacity]
                };

                for (var i = 0; i < _startCapacity; i++) inventory._slots[i] = new InventorySlot();
                return inventory;
            }
        }
        #endregion
    }
}