using UnityEngine;

namespace InventorySystem.Utils
{
    /// <summary>
    /// Static Utility for common math problems.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Clamps a value to a max limit and returns the clamped value and leftover.
        /// </summary>
        /// <param name="originalValue">The original value before clamping.</param>
        /// <param name="max">The maximum allowed value.</param>
        /// <returns>A tuple of (clampedValue, leftover)</returns>
        public static (int clampedValue, int leftover) ClampWithLeftover(int originalValue, int max)
        {
            int clamped = Mathf.Clamp(originalValue, 0, max);
            int leftover = originalValue - clamped;
            return (clamped, leftover);
        }
    }
}
