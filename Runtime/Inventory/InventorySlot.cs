using System;
using InventorySystem.Items;
using InventorySystem.Utils;
using UnityEngine;

namespace InventorySystem.Inventory
{
    /// <summary>
    /// The concrete implementation of an <see cref="IInventorySlot"/>.
    /// Manages an <see cref="IItem"/> and it's quantity.
    /// </summary>
    [Serializable]
    internal class InventorySlot : IInventorySlot, ICopyable<InventorySlot>
    {
        [SerializeReference] private IItem _item;
        [SerializeField] private int _quantity;

        internal InventorySlot()
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


        /// <summary>
        ///     Clear this slot.
        ///     Sets <see cref="Item" /> to null and <see cref="Quantity" /> to 0.
        /// </summary>
        internal void Clear()
        {
            _item = null;
            _quantity = 0;
        }

        /// <summary>
        ///     Try to add <see cref="Quantity" /> and return overflow amount.
        /// </summary>
        /// <param name="quantity"> The quantity that <see cref="Quantity" /> is trying to be added.</param>
        /// <param name="overflow">
        ///     The amount of <see cref="IItem" />
        ///     that exceeds the <see cref="ItemData.MaxStackAmount" /> for <see cref="InventorySlot.Item" />.
        ///     Set to 0, if there is no overflow.
        /// </param>
        /// <exception cref="NullReferenceException">
        ///     Thrown, if this <see cref="InventorySlot" /> has no <see cref="Item" /> assinged.
        /// </exception>
        private void TryAddQuantity(int quantity, out int overflow)
        {
            if (_item is null)
                throw new NullReferenceException("No Item in InventorySlot set. Cannot change quantity.");

            var (clampedQuant, leftover) =
                MathUtils.ClampWithLeftover(Quantity + quantity, _item.ItemData.MaxStackAmount);
            _quantity = clampedQuant;
            if (leftover <= 0) overflow = 0;
            overflow = leftover;
        }

        /// <summary>
        ///     Set the <see cref="IItem" /> and the <see cref="Quantity" /> for this <see cref="InventorySlot" />.
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> to be set.</param>
        /// <param name="quantity">
        ///     The <see cref="Quantity" /> to be set. Defaults to 1.
        ///     Must be equal or greater than 1.
        /// </param>
        /// <returns>The amount of overflow. Defaults to 0, if there is no overflow.</returns>
        /// <exception cref="ArgumentNullException">
        ///     If the item is null, we throw an exception.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     If the quantity is out of range (less than 1), we throw an exception.
        /// </exception>
        /// <exception cref="Exception">
        ///     If an <see cref="IItem" /> <see cref="_item" /> is already assigned to this <see cref="InventorySlot" />
        ///     and the <see cref="_quantity" /> is not zero, we throw an exception as
        ///     this means the caller is trying to override the <see cref="_item" />, which is disallowed.
        /// </exception>
        /// <remarks>
        ///     Does not allow items to be overridden.
        ///     Uses the <see cref="TryAddQuantity(int, out int)" /> method to set quantity and
        ///     thus applies it's rules for changing quantity.
        /// </remarks>
        internal int SetItemAndQuantity(IItem item, int quantity = 1)
        {
            _item ??= item ?? throw new ArgumentNullException(nameof(item));
            if (!_item.IsEquivalentTo(item) && _quantity != 0)
                throw new Exception("Cannot override existing Item with new item.");

            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
            TryAddQuantity(quantity, out var overflow);
            return overflow;
        }

        /// <inheritdoc cref="ICopyable{TSelf}.DeepCopy" />
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