using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.EquipmentInventory
{
    /// <summary>
    ///     The concrete implementation of the <see cref="IEquipmentInventory" />,
    ///     that manages equipped <see cref="IEquipmentItem" />s.
    /// </summary>
    [Serializable]
    internal class EquipmentInventory : IEquipmentInventory, ISerializationCallbackReceiver
    {
        /// <inheritdoc />
        public event Action<IItem> ItemEquipped;

        /// <inheritdoc />
        public event Action<IItem> ItemUnequipped;

        /// <inheritdoc cref="IEquipmentInventory.EquippedItemList" />
        public IReadOnlyList<IEquipmentItem> EquippedItemList
        {
            get
            {
                return _equippedItems?.Values
                    .Where(_equippedItem => _equippedItem != null).ToList() ?? new List<IEquipmentItem>();
            }
        }

        /// <summary>
        ///     Dictionary for managing <see cref="IEquipmentItem" />s.
        ///     Can equip exactly one <see cref="IItem" /> per <see cref="EquipmentType" />.
        /// </summary>
        /// <remarks>
        ///     Get initialized with every <see cref="EquipmentType" /> as null.
        ///     Thus, overwrite <see cref="IEquipmentItem" /> instead of adding them.
        /// </remarks>
        private Dictionary<EquipmentType, IEquipmentItem> _equippedItems;

        /// <summary>
        ///     List of currently equipped <see cref="IEquipmentItem" />s.
        /// </summary>
        [SerializeReference] private List<IEquipmentItem> _equippedItemList;


        /// <summary>
        ///     FIXME: Currently always creates one slot for each equipment type in <see cref="EquipmentType" />.
        ///     We might want to limit this to certain types (i.e., if used on enemies or player loses equipment slots etc.)
        /// </summary>
        private EquipmentInventory(Dictionary<EquipmentType, IEquipmentItem> equippedItems)
        {
            _equippedItems = equippedItems;
        }

        /// <summary>
        ///     Create a new <see cref="EquipmentInventory" />.
        /// </summary>
        /// <returns>
        ///     A new empty <see cref="EquipmentInventory" />.
        /// </returns>
        internal static EquipmentInventory Create()
        {
            var equippedItems = new Dictionary<EquipmentType, IEquipmentItem>();
            foreach (EquipmentType equipmentType in Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>())
                equippedItems.Add(equipmentType, null);
            return new EquipmentInventory(equippedItems);
        }

        /// <summary>
        ///     We cannot serialize a <see cref="Dictionary{TKey,TValue}" />, therefore we save our <see cref="_equippedItems" />
        ///     in the <see cref="_equippedItemList" />.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _equippedItemList = EquippedItemList.ToList();
        }

        /// <summary>
        ///     Populate <see cref="_equippedItems" /> with all saved <see cref="IEquipmentItem" />s after deserialization.
        /// </summary>
        public void OnAfterDeserialize()
        {
            _equippedItems = new Dictionary<EquipmentType, IEquipmentItem>();
            foreach (IEquipmentItem equipmentItem in _equippedItemList)
                _equippedItems.Add(equipmentItem.EquipmentType, equipmentItem);
            foreach (EquipmentType equipmentType in Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>())
                _equippedItems.TryAdd(equipmentType, null);
        }


        /// <inheritdoc cref="IEquipmentInventory.TryEquip" />
        public bool TryEquip(IItem item)
        {
            if (item is not IEquipmentItem itemToEquip) return false;
            // if equipmentItem.equipmentType is not in _equippedItems,
            // it is not an allowed EquipmentType for this EquipmentInventory 
            if (!_equippedItems.TryGetValue(itemToEquip.EquipmentType, out IEquipmentItem equippedItem)) return false;
            if (equippedItem != null) return false;
            _equippedItems[itemToEquip.EquipmentType] = itemToEquip;
            OnItemEquipped(itemToEquip);
            Debug.Log($"Equipped {itemToEquip.ItemData.name} in Slot {itemToEquip.EquipmentType}");
            return true;
        }

        /// <inheritdoc cref="IEquipmentInventory.Unequip" />
        public IEquipmentItem Unequip(EquipmentType equipmentType)
        {
            _equippedItems.TryGetValue(equipmentType, out IEquipmentItem equipmentItem);
            if (equipmentItem is not null)
            {
                _equippedItems[equipmentType] = null;
                OnItemUnequipped(equipmentItem);
                Debug.Log($"Unequipped {equipmentItem.ItemData.name} in Slot {equipmentItem.EquipmentType}");
            }

            return equipmentItem;
        }

        /// <inheritdoc cref="IEquipmentInventory.GetItem" />
        public IEquipmentItem GetItem(EquipmentType equipmentType)
        {
            _equippedItems.TryGetValue(equipmentType, out IEquipmentItem equipmentItem);
            return equipmentItem;
        }

        /// <summary>
        ///     Trigger the <see cref="ItemEquipped" /> evennt.
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> that was successfully equipped.</param>
        private void OnItemEquipped(IItem item)
        {
            ItemEquipped?.Invoke(item);
        }

        /// <summary>
        ///     Trigger the <see cref="ItemUnequipped" /> event.
        /// </summary>
        /// <param name="item">The <see cref="IItem" /> that was successfully unequipped.</param>
        private void OnItemUnequipped(IItem item)
        {
            ItemUnequipped?.Invoke(item);
        }
    }
}