using System;
using InventorySystem.EquipmentInventory;
using InventorySystem.Items;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Armors
{

    /// <summary>
    /// <see cref="ScriptableObject"/> defining data for an <see cref="IEquipmentItem"/>
    /// of type Armor.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Armor", menuName = "InventorySystem/Items/Armor")]
    public class ArmorData : ItemData
    {
        public float ArmorValue;
    }
}
