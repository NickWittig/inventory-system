namespace InventorySystem.Utils
{
    /// <summary>
    /// Make this able to be copied.
    /// </summary>
    /// <typeparam name="TType">Type of class implementing this that can be copied.</typeparam>
    public interface ICopyable<TType>
    {
        /// <summary>
        /// Create a deep copy of this <typeparamref name="TType"/>.
        /// </summary>
        /// <returns>
        ///     A deep copy of this <typeparamref name="TType"/>.
        /// </returns>
        /// <remarks>
        ///     Deep Copy: A deep copy of an object is a copy whose properties do not share
        ///     the same references (point to the same underlying values)
        ///     as those of the source object from which the copy was made.
        ///     https://developer.mozilla.org/en-US/docs/Glossary/Deep_copy
        /// </remarks>
        public TType DeepCopy();
    }
}