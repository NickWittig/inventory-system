using InventorySystem.Items;
using UnityEngine;

public class MockItem : IItem
{
    [field: SerializeField] public ItemData ItemData { get; private set; }

    public MockItem(ItemData data)
    {
        ItemData = data;
    }

    public bool IsEquivalentTo(IItem other)
    {
        return ItemData.ItemName == other.ItemData.ItemName;
    }

    public IItem DeepCopy()
    {
        return new MockItem(ItemData);
    }
}
