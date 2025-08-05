// Assets/Tests/Runtime/InventorySlotInterfaceTests.cs

using InventorySystem.Inventory;
using InventorySystem.Items;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace InventorySystemTests
{
    /// <summary>
    /// Behaviour-level tests of the observable contract of <see cref="IInventorySlot"/>,
    /// driven exclusively through <see cref="IInventory"/> operations. Does not assume
    /// any internal mutators on the slot itself.
    /// </summary>
    [TestFixture]
    public class InventorySlotInterfaceTests
    {
        private const int MAX_STACK = 5;
        private IInventory _inventory;
        private ItemData _potionSo;
        private ItemData _elixirSo;
        private IItem _potion;
        private IItem _elixir;

        [SetUp]
        public void SetUp()
        {
            _inventory = InventoryFactory.Create(); // default handlesOverflow = true

            _potionSo = TestUtils.CreateItemSO("Potion", MAX_STACK);
            _elixirSo = TestUtils.CreateItemSO("Elixir", MAX_STACK);

            _potion = new MockItem(_potionSo);
            _elixir = new MockItem(_elixirSo);
        }

        #region ����� empty / full / item / quantity via inventory ops �����

        [Test]
        public void NewSlot_IsEmpty_QuantityZero_ItemNull()
        {
            var slot = _inventory.TryGetSlotAt(0);
            Assert.IsNotNull(slot);
            Assert.IsNull(slot.Item);
            Assert.AreEqual(0, slot.Quantity);
            Assert.IsTrue(slot.IsEmpty);
            Assert.IsFalse(slot.IsFull);
        }

        [Test]
        public void AddingSingleItem_UpdatesSlotProperties()
        {
            _inventory.TryAddItem(_potion, 1);

            var slot = _inventory.TryGetSlotAt(0);
            Assert.IsNotNull(slot);
            Assert.IsFalse(slot.IsEmpty);
            Assert.IsFalse(slot.IsFull);
            Assert.AreEqual(1, slot.Quantity);
            Assert.IsTrue(slot.Item.IsEquivalentTo(_potion));
        }

        [Test]
        public void FillingSlot_ToMaxMarksFull()
        {
            _inventory.TryAddItem(_potion, MAX_STACK); // fills first slot

            var slot = _inventory.TryGetSlotAt(0);
            Assert.IsTrue(slot.IsFull);
            Assert.IsFalse(slot.IsEmpty);
            Assert.AreEqual(MAX_STACK, slot.Quantity);
        }

        [Test]
        public void OverflowDistribution_PopulatesSecondSlotAndFirstIsFull()
        {
            _inventory.TryAddItem(_potion, MAX_STACK + 2); // 7 => first full, second has 2

            var first = _inventory.TryGetSlotAt(0);
            var second = _inventory.TryGetSlotAt(1);

            Assert.IsTrue(first.IsFull);
            Assert.AreEqual(MAX_STACK, first.Quantity);
            Assert.IsFalse(first.IsEmpty);

            Assert.IsFalse(second.IsFull);
            Assert.AreEqual(2, second.Quantity);
            Assert.IsFalse(second.IsEmpty);

            Assert.IsTrue(first.Item.IsEquivalentTo(_potion));
            Assert.IsTrue(second.Item.IsEquivalentTo(_potion));
        }

        [Test]
        public void ClearingSlotViaInventory_MakesSlotEmpty()
        {
            _inventory.TryAddItem(_potion, 3);
            Assert.IsFalse(_inventory.TryGetSlotAt(0).IsEmpty);

            _inventory.TryClearSlotAt(0);
            var slot = _inventory.TryGetSlotAt(0);
            Assert.IsTrue(slot.IsEmpty);
            Assert.AreEqual(0, slot.Quantity);
            Assert.IsNull(slot.Item);
            Assert.IsFalse(slot.IsFull);
        }

        [Test]
        public void RemovingItem_RemovesFromSlotAndReflectsEmpty()
        {
            _inventory.TryAddItem(_potion, 2);
            Assert.IsFalse(_inventory.TryGetSlotAt(0).IsEmpty);

            _inventory.RemoveItem(_potion, true); // remove that stack

            var slot = _inventory.TryGetSlotAt(0);
            Assert.IsTrue(slot.IsEmpty);
            Assert.AreEqual(0, slot.Quantity);
            Assert.IsNull(slot.Item);
            Assert.IsFalse(slot.IsFull);
        }

        [Test]
        public void InsertAtFront_ShiftsItems_SlotPropertiesFollow()
        {
            // Put potion in slot 0
            _inventory.TryAddItem(_potion, 1);

            // Insert new front item
            var front = new MockItem(TestUtils.CreateItemSO("Scroll", MAX_STACK));
            var success = _inventory.TryInsertItemAtFront(front, 1);
            Assert.IsTrue(success);

            // After insert, slot 0 should contain 'front', slot 1 has previous potion
            var first = _inventory.TryGetSlotAt(0);
            var second = _inventory.TryGetSlotAt(1);

            Assert.IsFalse(first.IsEmpty);
            Assert.AreEqual(1, first.Quantity);
            Assert.IsTrue(first.Item.IsEquivalentTo(front));

            Assert.IsFalse(second.IsEmpty);
            Assert.AreEqual(1, second.Quantity);
            Assert.IsTrue(second.Item.IsEquivalentTo(_potion));
        }

        #endregion

        #region ����� edge / invariants �����

        [Test]
        public void FullSlot_RemainsFull_WhenAttemptingToAddSameItemWithoutOverflowHandlingDisabled()
        {
            // default inventory handles overflow: to test "full stays full" we create no-overflow inventory
            var inv = InventoryFactory.Create(2, false);
            inv.TryAddItem(_potion, MAX_STACK - 1);

            // adding more should not spill into second slot because handlesOverflow=false
            inv.TryAddItem(_potion, 3);
            var first = inv.TryGetSlotAt(0);
            var second = inv.TryGetSlotAt(1);
            Assert.IsTrue(first.IsFull);
            Assert.AreEqual(MAX_STACK, first.Quantity);
            Assert.IsTrue(second.IsEmpty);
        }

        [Test]
        public void IsEmpty_FalseOnlyWhenQuantityPositiveAndItemAssigned()
        {
            _inventory.TryAddItem(_potion, 1);
            var slot = _inventory.TryGetSlotAt(0);
            Assert.IsFalse(slot.IsEmpty);
            Assert.AreEqual(1, slot.Quantity);
            Assert.IsTrue(slot.Item.IsEquivalentTo(_potion));
        }

        #endregion
    }
}
