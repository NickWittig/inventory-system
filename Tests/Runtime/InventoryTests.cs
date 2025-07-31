// Assets/Tests/Runtime/InventoryTests.cs

using InventorySystem.Inventory;
using InventorySystem.Items;
using NUnit.Framework;

namespace Tests
{
    /// <summary>
    ///     Behaviour‑driven tests for <see cref="Inventory" /> that must satisfy the
    ///     <see cref="IInventory" /> contract and the new overflow rules.
    /// </summary>
    public class InventoryTests
    {
        private const int MaxStack = 5;
        private Inventory _inventory; // default: handlesOverflow = true
        private ItemData _potionSo;
        private ItemData _elixirSo;
        private IItem _potion;
        private IItem _elixir;

        [SetUp]
        public void SetUp()
        {
            _inventory = Inventory.Create();

            _potionSo = TestUtils.CreateItemSO("Potion", MaxStack);
            _elixirSo = TestUtils.CreateItemSO("Elixir", MaxStack);

            _potion = new MockItem(_potionSo);
            _elixir = new MockItem(_elixirSo);
        }

        #region Capacity / empty state -----------------------------------------

        [Test]
        public void NewInventory_IsEmpty_AtCorrectCapacity()
        {
            Assert.AreEqual(2, _inventory.Capacity);
            Assert.IsTrue(_inventory.IsEmpty);
        }

        [Test]
        public void TryGetItemAt_OutOfBounds_ReturnsNull()
        {
            Assert.IsNull(_inventory.TryGetItemAt(-1));
            Assert.IsNull(_inventory.TryGetItemAt(999));
        }

        [Test]
        public void TryGetSlotAt_OutOfBounds_ReturnsNull()
        {
            Assert.IsNull(_inventory.TryGetSlotAt(-1));
            Assert.IsNull(_inventory.TryGetSlotAt(999));
        }

        #endregion

        #region IsSlotAvailable -------------------------------------------------

        [Test]
        public void IsSlotAvailable_ReturnsFirstEmpty_WhenNoEquivalentFound()
        {
            var canAdd = _inventory.IsSlotAvailable(_potion, out var index);
            Assert.IsTrue(canAdd);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void IsSlotAvailable_PrefersEquivalentNonFullSlot()
        {
            // Fill slot‑0 with 3/5 potions
            _inventory.TryAddItem(_potion, 3);

            // Slot‑0 not full – should be chosen instead of empty 1
            var canAdd = _inventory.IsSlotAvailable(new MockItem(_potionSo), out var index);
            Assert.IsTrue(canAdd);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void IsSlotAvailable_ReturnsFalse_WhenNoRoomForDistinctItem()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir); // now full with two distinct stacks

            var canAdd = _inventory.IsSlotAvailable(
                new MockItem(TestUtils.CreateItemSO("Sword", 3)),
                out var _);

            Assert.IsFalse(canAdd);
        }

        #endregion

        #region Basic adds ------------------------------------------------------

        [Test]
        public void TryAddItem_WhenSlotFree_AddsToFirstFree()
        {
            var added = _inventory.TryAddItem(_potion);

            Assert.IsTrue(added);
            Assert.AreEqual(_potion, _inventory.TryGetItemAt(0));
            Assert.AreEqual(1, _inventory.TryGetSlotAt(0).Quantity);
            Assert.IsFalse(_inventory.IsEmpty);
        }

        [Test]
        public void TryAddItem_WhenEquivalentExists_IncreasesQuantity()
        {
            _inventory.TryAddItem(_potion, 2); // slot‑0 = 2
            _inventory.TryAddItem(new MockItem(_potionSo)); // +1

            Assert.AreEqual(3, _inventory.TryGetSlotAt(0).Quantity);
            Assert.AreEqual(1, _inventory.GetAllItems().FindAll(i => i == _potion).Count);
        }

        [Test]
        public void TryAddItem_WhenNoSpace_ReturnsFalse()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir);

            var added = _inventory.TryAddItem(
                new MockItem(TestUtils.CreateItemSO("Sword", 10)));

            Assert.IsFalse(added);
        }

        #endregion

        #region Overflow behaviour (handlesOverflow = true) ---------------------

        [Test]
        public void TryAddItem_DistributesOverflow_WhenHandlesOverflowTrue()
        {
            // 10 > 1 slot; should spread across two slots (5 + 5) and discard none#
            
            var added = _inventory.TryAddItem(_potion, 10);

            Assert.IsTrue(added);
            Assert.AreEqual(MaxStack, _inventory.TryGetSlotAt(0).Quantity);
            Assert.AreEqual(MaxStack, _inventory.TryGetSlotAt(1).Quantity);
        }

        [Test]
        public void TryAddItem_DiscardsRemainderBeyondCapacity_WhenHandlesOverflowTrue()
        {
            // capacity = 2*5 = 10 ; we add 12 => 2 items discarded
            _inventory.TryAddItem(_potion, 12);

            Assert.AreEqual(MaxStack, _inventory.TryGetSlotAt(0).Quantity);
            Assert.AreEqual(MaxStack, _inventory.TryGetSlotAt(1).Quantity);
        }

        #endregion

        #region Overflow behaviour (handlesOverflow = false) --------------------

        [Test]
        public void TryAddItem_StopsAfterFirstSlot_WhenHandlesOverflowFalse()
        {
            var inv = Inventory.Create(2, false);
            var added = inv.TryAddItem(_potion, 10);

            Assert.IsTrue(added); // still returns true
            Assert.AreEqual(MaxStack, inv.TryGetSlotAt(0).Quantity);
            Assert.IsTrue(inv.TryGetSlotAt(1).IsEmpty); // never touched
        }

        #endregion

        #region TryAddItemAt ----------------------------------------------------

        [Test]
        public void TryAddItemAt_InvalidIndex_ReturnsFalse()
        {
            Assert.IsFalse(_inventory.TryAddItemAt(_potion, -1));
            Assert.IsFalse(_inventory.TryAddItemAt(_potion, 99));
        }

        [Test]
        public void TryAddItemAt_NonEquivalentInOccupiedSlot_ReturnsFalse()
        {
            _inventory.TryAddItem(_potion);
            Assert.IsFalse(_inventory.TryAddItemAt(_elixir, 0));
        }

        #endregion

        #region Remove / Clear --------------------------------------------------

        [Test]
        public void RemoveItem_OnlyFirstOccurence_RemovesSingleStack()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_potion); // same item, same stack
            _inventory.TryAddItem(_elixir); // second slot

            _inventory.RemoveItem(_potion, true);

            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.AreEqual(_elixir, _inventory.TryGetItemAt(1));
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir);

            _inventory.Clear();

            Assert.IsTrue(_inventory.IsEmpty);
            foreach (var slotIdx in new[] { 0, 1 })
                Assert.IsTrue(_inventory.TryGetSlotAt(slotIdx).IsEmpty);
        }

        #endregion

        #region Insert at front -------------------------------------------------

        [Test]
        public void TryInsertItemAtFront_WhenInventoryFull_Fails()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir);

            var front = new MockItem(TestUtils.CreateItemSO("Scroll", MaxStack));
            var success = _inventory.TryInsertItemAtFront(front);

            Assert.IsFalse(success);
            CollectionAssert.Contains(_inventory.GetAllItems(), _potion);
            CollectionAssert.Contains(_inventory.GetAllItems(), _elixir);
        }

        [Test]
        public void TryInsertItemAtFront_PushesExistingItemsForward()
        {
            // goes to slot‑0
            _inventory.TryAddItem(_potion);

            var front = new MockItem(TestUtils.CreateItemSO("Scroll", MaxStack));
            var success = _inventory.TryInsertItemAtFront(front);

            Assert.IsTrue(success);
            Assert.IsTrue(_inventory.TryGetItemAt(0).IsEquivalentTo(front));
            Assert.IsTrue(_inventory.TryGetItemAt(1).IsEquivalentTo(_potion));
        }

        #endregion
    }
}