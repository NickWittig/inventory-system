namespace InventorySystem.Items
{
    
    /// <summary>
    /// Base interface for anything regarded as an Item.
    /// </summary>
    public interface IItem : ICopyable<IItem>, IEquivalent<IItem>
    {
        public ItemData ItemData { get; }
    }
}