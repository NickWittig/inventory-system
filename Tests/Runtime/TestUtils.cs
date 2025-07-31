using InventorySystem.Items;
using UnityEngine;
 
namespace Tests
{
    public class TestUtils
    {
        public static ItemData CreateItemSO(string name, int maxStack, int id = default)
        {
            if (id == default)
            {
                id = Random.Range(1, 100000) + 100000;
            }   
            var itemSO = ScriptableObject.CreateInstance<ItemData>();
            itemSO.Uid = id;
            itemSO.ItemName = name;
            itemSO.MaxStackAmount = maxStack;
            return itemSO;
        }
        
        public static IItem CreateMockItem(string name, int maxStack, int id = default)
        {
            var itemSO = CreateItemSO(name, maxStack, id);
            return new MockItem(itemSO);
        }
        
    }
}