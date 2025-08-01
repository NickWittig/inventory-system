using System;
using System.Collections.Generic;
using InventorySystem.Items;

namespace InventorySystem.EquipmentInventory
{
    public interface IEquipmentInventory
    {

        /// <summary>
        /// Event invoked when an <see cref="IItem"/> was successfully equipped.
        /// Returns the newly equipped <see cref="IItem"/>.
        /// </summary>
        event Action<IItem> ItemEquipped;


        /// <summary>
        /// Event invoked when an <see cref="IItem"/> was successfully unequipped.
        /// Returns the newly unequipped <see cref="IItem"/>.
        /// </summary>
        event Action<IItem> ItemUnequipped;

        /// <summary>
        ///     List of all currently equipped <see cref="IEquipmentItem" />s.
        /// </summary>
        /// <remarks>
        /// Does not make a contract about the amount of items inside.
        /// Does not contain null values, but can be empty.
        /// </remarks>
        public IReadOnlyList<IEquipmentItem> EquippedItemList { get; }

        /// <summary>
        ///     Try to equip an <see cref="IItem" />.
        ///     Returns false if the <see cref="IItem" /> is not an <see cref="IEquipmentItem" />.
        ///     Returns false if the <see cref="IEquipmentItem.EquipmentType" /> is not allowed in this
        ///     <see cref="IEquipmentInventory" />.
        ///     Returns false if an <see cref="IItem" /> of the same <see cref="EquipmentType" /> is already equipped.
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> trying to be equipped.</param>
        /// <returns>Whether the <see cref="IItem" /> item was equipped.</returns>
        public bool TryEquip(IItem item);

        /// <summary>
        ///     Try to unequip an <see cref="IEquipmentItem" />.
        ///     Returns null if there was no <see cref="IEquipmentItem" /> equipped in
        ///     the slot for <see cref="EquipmentType" /> equipmentType in <see cref="EquippedItemList" />.
        /// </summary>
        /// <param name="equipmentType">The slot that the <see cref="IEquipmentItem" /> is being removed from.</param>
        /// <returns>
        ///     Returns the unequipped <see cref="IEquipmentItem" />.
        ///     Returns null, if there was no item in the <see cref="EquipmentType" /> in <see cref="EquippedItemList" />.
        /// </returns>
        public IEquipmentItem Unequip(EquipmentType equipmentType);

        /// <summary>
        ///     Get the  <see cref="IEquipmentItem" /> for slot <see cref="EquipmentType" /> equipmentType.
        /// </summary>
        /// <param name="equipmentType">The <see cref="EquipmentType" /> to check for.</param>
        /// <returns>
        ///     Returns the equipped <see cref="IEquipmentItem" />.
        ///     Returns null, if no <see cref="IEquipmentItem" /> is equipped in this slot.
        /// </returns>
        public IEquipmentItem GetItem(EquipmentType equipmentType);
    }
}