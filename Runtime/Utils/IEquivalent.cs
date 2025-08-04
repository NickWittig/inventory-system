namespace InventorySystem.Utils
{
    /// <summary>
    ///     Check if this is equivalent to another "other" object.
    ///     This is not the same as Equals,
    ///     as we do not compare the object instance
    ///     but rather the state of this to "other".
    /// </summary>
    /// <typeparam name="TType">
    ///     Type of the class implementing this that can be checked for equivalence.
    /// </typeparam>
    public interface IEquivalent<TType>
    {
        /// <summary>
        ///     Check if this is the same type <typeparamref name="TType"/> in the same internal state as other <typeparamref name="TType"/>.
        /// </summary>
        /// <param name="other">
        ///     The object <typeparamref name="TType"/> to be compared against.
        /// </param>
        /// <returns>
        ///     Whether this and other are equivalent in type and internal state.
        /// </returns>
        bool IsEquivalentTo(TType other);
    }
}