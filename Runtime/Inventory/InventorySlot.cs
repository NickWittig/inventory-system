using System;
using InventorySystem.Items;
using InventorySystem.Utils;
using UnityEngine;

namespace InventorySystem.Inventory
{
    [Serializable]
    internal class InventorySlot : IInventorySlot, ICopyable<InventorySlot>
    {
        [SerializeReference] private IItem _item;
        [SerializeField] private int _quantity;

        public InventorySlot()
        {
            _item = null;
            _quantity = 0;
        }

        /// <inheritdoc cref="IInventorySlot.IsEmpty" />
        public bool IsEmpty => _item == null && _quantity == 0;
        /// <inheritdoc cref="IInventorySlot.IsFull" />
        public bool IsFull => _item != null && _quantity == _item.ItemData.MaxStackAmount;
        /// <inheritdoc cref="IInventorySlot.Item" />
        public IItem Item => _item;
        /// <inheritdoc cref="IInventorySlot.Quantity" />
        public int Quantity => _quantity;
        
        /// <inheritdoc cref="IInventorySlot.Clear" />
        public void Clear()
        {
            _item = null;
            _quantity = 0;
        }
        

        /// <inheritdoc cref="IInventorySlot.TryAddQuantity" />
        public void TryAddQuantity(int quantity, out int overflow)
        {
            if (_item is null)
                throw new NullReferenceException("No Item in InventorySlot set. Cannot change quantity.");

            var (clampedQuant, leftover) =
                MathUtils.ClampWithLeftover(Quantity + quantity, _item.ItemData.MaxStackAmount);
            _quantity = clampedQuant;
            if (leftover <= 0) overflow = 0;
            overflow = leftover;
        }
        
        internal void SetItem(IItem item)
        {
            _item = item;
        }

        /// <inheritdoc cref="ICopyable{TSelf}.DeepCopy"/>
        public InventorySlot DeepCopy()
        {
            return new InventorySlot
            {
                _item = _item?.DeepCopy(),
                _quantity = _quantity
            };
        }
    }
}