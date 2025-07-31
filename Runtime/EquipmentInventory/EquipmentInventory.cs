using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.EquipmentInventory
{
    [Serializable]
    public class EquipmentInventory : IEquipmentInventory, ISerializationCallbackReceiver
    {
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
        private EquipmentInventory()
        {
            _equippedItems = new Dictionary<EquipmentType, IEquipmentItem>();
            foreach (EquipmentType equipmentType in Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>())
                _equippedItems.Add(equipmentType, null);
        }

        /// <summary>
        ///     Create a new <see cref="EquipmentInventory" />.
        /// </summary>
        public static EquipmentInventory Create()
        {
            return new EquipmentInventory();
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
                _equippedItems.Add(equipmentItem.equipmentType, equipmentItem);
            foreach (EquipmentType equipmentType in Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>())
                _equippedItems.TryAdd(equipmentType, null);
        }


        /// <inheritdoc cref="IEquipmentInventory.TryEquip" />
        public bool TryEquip(IItem item)
        {
            if (item is not IEquipmentItem equipmentItem) return false;
            // if equipmentItem.equipmentType is not in _equippedItems,
            // it is not an allowed EquipmentType for this EquipmentInventory 
            if (!_equippedItems.TryGetValue(equipmentItem.equipmentType, out IEquipmentItem equippedItem)) return false;
            if (equippedItem != null) return false;
            _equippedItems[equipmentItem.equipmentType] = equipmentItem;
            Debug.Log($"Equipping {equipmentItem.equipmentType} with {equipmentItem.ItemData.name}");
            return true;
        }

        /// <inheritdoc cref="IEquipmentInventory.Unequip" />
        public IEquipmentItem Unequip(EquipmentType equipmentType)
        {
            _equippedItems.TryGetValue(equipmentType, out IEquipmentItem equipmentItem);
            if (equipmentItem is not null) _equippedItems[equipmentType] = null;
            return equipmentItem;
        }

        /// <inheritdoc cref="IEquipmentInventory.GetItem" />
        public IEquipmentItem GetItem(EquipmentType equipmentType)
        {
            _equippedItems.TryGetValue(equipmentType, out IEquipmentItem equipmentItem);
            return equipmentItem;
        }
    }
}