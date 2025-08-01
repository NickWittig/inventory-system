using InventorySystem.Items;

namespace InventorySystem.EquipmentInventory
{
    /// <summary>
    ///     Interface for all <see cref="IItem" />s that can be equipped.
    /// </summary>
    public interface IEquipmentItem : IItem
    {
        /// <summary>
        ///     The <see cref="InventorySystem.EquipmentInventory.EquipmentType" /> in which this <see cref="IEquipmentItem" /> can be equipped in.
        /// </summary>
        public EquipmentType EquipmentType { get; }
    }
}