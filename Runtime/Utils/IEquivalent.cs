namespace InventorySystem.Items
{
    /// <summary>
    ///     Check if this is equivalent to another "other" object.
    ///     This is not the same as Equals,
    ///     as we do not compare the object instance
    ///     but rather the state of this to "other".
    /// </summary>
    /// <typeparam name="T">Type of the class implementing this that can be checked for equivalence.</typeparam>
    public interface IEquivalent<T>
    {
        /// <summary>
        ///     Check if this is the same type <see cref="T" /> in the same internal state as other <see cref="T" />.
        /// </summary>
        /// <param name="other">The object <see cref="T" /> to be compared against.</param>
        /// <returns>Whether this and other are equivalent in type and internal state.</returns>
        bool IsEquivalentTo(T other);
    }
}