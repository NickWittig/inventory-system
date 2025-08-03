using System;
using UnityEngine;

namespace InventorySystem.Items
{
    /// <summary>
    /// <see cref="ScriptableObject"/> containing data for a base <see cref="IItem"/>.
    /// Can be inherited by other <see cref="ScriptableObject"/>s
    /// to extend to more specific items.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Item", menuName = "InventorySystem/Items/Item")]
    public class ItemData : ScriptableObject
    {
        public int Uid;
        public string ItemName;
        public string ItemDescription;
        public int MaxStackAmount = 1;
        public Sprite Icon;
        public GameObject Prefab;
    }
}

