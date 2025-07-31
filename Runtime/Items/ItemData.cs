using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace InventorySystem.Items
{
    /// <summary>
    /// <see cref="ScriptableObject"/> containing data for a base <see cref="IItem"/>.
    /// Can be inherited by other <see cref="ScriptableObject"/>s
    /// to extend to more specific items.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Item", menuName = "Items/Item")]
    public class ItemData : ScriptableObject
    {
        [FormerlySerializedAs("id")] public int Uid;
        [FormerlySerializedAs("itemName")] public string ItemName;
        [FormerlySerializedAs("itemDescription")] public string ItemDescription;
        [FormerlySerializedAs("maxStackAmount")] public int MaxStackAmount = 1;
        [FormerlySerializedAs("prefab")] public GameObject Prefab;
        [FormerlySerializedAs("sprite")] public Sprite Sprite;

    }
}

