// Assets/Tests/Runtime/InventoryTests.cs

using System.Collections.Generic;
using System.Linq;
using InventorySystem.Inventory;
using InventorySystem.Items;
using NUnit.Framework;

namespace InventorySystemTests
{
    /// <summary>
    ///     Behaviour-driven tests for <see cref="Inventory" /> that must satisfy the
    ///     <see cref="IInventory" /> contract and the documented overflow / slot rules.
    /// </summary>
    [TestFixture]
    public class InventoryTests
    {
        private const int MAX_STACK = 5;
        private IInventory _inventory; // default: handlesOverflow = true
        private ItemData _potionSo;
        private ItemData _elixirSo;
        private IItem _potion;
        private IItem _elixir;

        [SetUp]
        public void SetUp()
        {

            _inventory = InventoryFactory.Create(2, true, 5);

            _potionSo = TestUtils.CreateItemSO("Potion", MAX_STACK);
            _elixirSo = TestUtils.CreateItemSO("Elixir", MAX_STACK);

            _potion = new MockItem(_potionSo);
            _elixir = new MockItem(_elixirSo);
        }

        #region ––––– helpers –––––

        private static List<IItem> NonNullEquivalentItems(IReadOnlyList<IItem> list, IItem compareTo)
        {
            return list.Where(i => i is not null && i.IsEquivalentTo(compareTo)).ToList();
        }

        #endregion

        #region Capacity / empty state -----------------------------------------

        [Test]
        public void NewInventory_IsEmpty_AtCorrectCapacity()
        {
            Assert.AreEqual(2, _inventory.Capacity);
            Assert.IsTrue(_inventory.IsEmpty);
            Assert.AreEqual(_inventory.Capacity, _inventory.InventorySlots.Count);
            Assert.AreEqual(_inventory.Capacity, _inventory.Items.Count);
            Assert.That(_inventory.Items, Has.All.Null, "Initially all item entries must be null.");
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

        [Test]
        public void TryGetItemAt_EmptySlot_ReturnsNull()
        {
            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.IsNull(_inventory.TryGetItemAt(1));
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
            _inventory.TryAddItem(_potion, 3);
            var canAdd = _inventory.IsSlotAvailable(new MockItem(_potionSo), out var index);
            Assert.IsTrue(canAdd);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void IsSlotAvailable_ReturnsEmptyIfEquivalentIsFullButOtherSlotFree()
        {
            _inventory.TryAddItem(_potion, MAX_STACK);
            var canAdd = _inventory.IsSlotAvailable(new MockItem(_potionSo), out var index);
            Assert.IsTrue(canAdd);
            Assert.AreEqual(1, index);
        }

        [Test]
        public void IsSlotAvailable_ReturnsFalse_WhenNoRoomForDistinctItem()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir);
            var canAdd = _inventory.IsSlotAvailable(
                new MockItem(TestUtils.CreateItemSO("Sword", 3)),
                out _);
            Assert.IsFalse(canAdd);
        }

        #endregion

        #region Basic adds ------------------------------------------------------

        [Test]
        public void TryAddItem_WhenSlotFree_AddsToFirstFree()
        {
            var added = _inventory.TryAddItem(_potion);
            Assert.IsTrue(added);
            Assert.AreSame(_potion, _inventory.TryGetItemAt(0));
            Assert.AreEqual(1, _inventory.TryGetSlotAt(0).Quantity);
            Assert.IsFalse(_inventory.IsEmpty);
        }

        [Test]
        public void TryAddItem_WhenEquivalentExists_IncreasesQuantity()
        {
            _inventory.TryAddItem(_potion, 2);
            _inventory.TryAddItem(new MockItem(_potionSo));
            Assert.AreEqual(3, _inventory.TryGetSlotAt(0).Quantity);
            Assert.AreEqual(1, NonNullEquivalentItems(_inventory.Items, _potion).Count);
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
            var added = _inventory.TryAddItem(_potion, 10);
            Assert.IsTrue(added);
            Assert.AreEqual(MAX_STACK, _inventory.TryGetSlotAt(0).Quantity);
            Assert.AreEqual(MAX_STACK, _inventory.TryGetSlotAt(1).Quantity);
        }

        [Test]
        public void TryAddItem_DiscardsRemainderBeyondCapacity_WhenHandlesOverflowTrue()
        {
            _inventory.TryAddItem(_potion, 12);
            Assert.AreEqual(MAX_STACK, _inventory.TryGetSlotAt(0).Quantity);
            Assert.AreEqual(MAX_STACK, _inventory.TryGetSlotAt(1).Quantity);
        }

        #endregion

        #region Overflow behaviour (handlesOverflow = false) --------------------

        [Test]
        public void TryAddItem_StopsAfterFirstSlot_WhenHandlesOverflowFalse()
        {
            var inv = InventoryFactory.Create(2, false);
            var added = inv.TryAddItem(_potion, 10);
            Assert.IsTrue(added);
            Assert.AreEqual(MAX_STACK, inv.TryGetSlotAt(0).Quantity);
            Assert.IsTrue(inv.TryGetSlotAt(1).IsEmpty);
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

        [Test]
        public void TryAddItemAt_PrefersTargetSlot_WhenEquivalentExistsElsewhere()
        {
            _inventory.TryAddItem(_potion, 2);
            var added = _inventory.TryAddItemAt(new MockItem(_potionSo), 1, 3);
            Assert.IsTrue(added);
            Assert.AreEqual(2, _inventory.TryGetSlotAt(0).Quantity);
            Assert.AreEqual(3, _inventory.TryGetSlotAt(1).Quantity);
        }

        #endregion

        #region TryClearSlotAt --------------------------------------------------

        [Test]
        public void TryClearSlotAt_ValidIndex_ClearsSlot()
        {
            _inventory.TryAddItem(_potion);
            Assert.IsFalse(_inventory.TryGetSlotAt(0).IsEmpty);

            _inventory.TryClearSlotAt(0);
            Assert.IsTrue(_inventory.TryGetSlotAt(0).IsEmpty);
            Assert.IsNull(_inventory.TryGetItemAt(0));
        }

        [Test]
        public void TryClearSlotAt_InvalidIndex_NoThrowOrEffect()
        {
            Assert.DoesNotThrow(() => _inventory.TryClearSlotAt(999));
            Assert.IsTrue(_inventory.IsEmpty);
        }

        #endregion

        #region Remove / Clear --------------------------------------------------

        [Test]
        public void RemoveItem_OnlyFirstOccurence_RemovesSingleStack()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir);
            _inventory.RemoveItem(_potion, true);
            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.AreSame(_elixir, _inventory.TryGetItemAt(1));
        }

        [Test]
        public void RemoveItem_AllOccurrences_RemovesEverything()
        {
            _inventory.TryAddItem(_potion, MAX_STACK * 2);
            _inventory.RemoveItem(_potion);
            Assert.IsTrue(_inventory.IsEmpty);
            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.IsNull(_inventory.TryGetItemAt(1));
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

            var front = new MockItem(TestUtils.CreateItemSO("Scroll", MAX_STACK));
            var success = _inventory.TryInsertItemAtFront(front);

            Assert.IsFalse(success);
            CollectionAssert.Contains(_inventory.Items, _potion);
            CollectionAssert.Contains(_inventory.Items, _elixir);
        }

        [Test]
        public void TryInsertItemAtFront_PushesExistingItemsForward()
        {
            _inventory.TryAddItem(_potion);

            var front = new MockItem(TestUtils.CreateItemSO("Scroll", MAX_STACK));
            var success = _inventory.TryInsertItemAtFront(front);

            Assert.IsTrue(success);
            Assert.IsTrue(_inventory.TryGetItemAt(0).IsEquivalentTo(front));
            Assert.IsTrue(_inventory.TryGetItemAt(1).IsEquivalentTo(_potion));
        }

        #endregion

        #region Events ---------------------------------------------------------

        [Test]
        public void ItemsAdded_Event_FiresWithCorrectPayload_OnSimpleAdd()
        {
            IInventorySlot slot = null;
            _inventory.ItemsAdded += (eventSlot, eventIndex) =>
            {
                slot = eventSlot;
                _ = eventIndex;
            };

            _inventory.TryAddItem(_potion, 3);

            Assert.IsNotNull(slot.Item);
            Assert.IsTrue(slot.Item.IsEquivalentTo(_potion));
            Assert.AreEqual(3, slot.Quantity);
        }

        [Test]
        public void ItemsAdded_Event_FiresPerSlot_WhenOverflowDistributed()
        {
            var captured = new List<(IItem Item, int Quantity)>();
            _inventory.ItemsAdded += (slot, qty) => captured.Add((slot.Item, slot.Quantity));

            _inventory.TryAddItem(_potion, 8); // should split across two slots (5 + 3)

            Assert.That(captured, Is.Not.Empty, "Event must fire at least once.");
            Assert.That(captured.All(c => c.Item.IsEquivalentTo(_potion)), Is.True);
            var totalAdded = captured.Sum(c => c.Quantity);
            Assert.AreEqual(8, totalAdded, "Total quantity reported across events should equal requested addition.");
            Assert.That(captured.All(c => c.Quantity <= MAX_STACK), Is.True);
        }

        [Test]
        public void ItemsRemoved_Event_Fires_OnRemoveItem()
        {
            _inventory.TryAddItem(_potion, 3);
            IInventorySlot removedSlot = null;
            _inventory.ItemsRemoved += (slot, index) => { removedSlot = slot; };

            _inventory.RemoveItem(_potion, true);

            Assert.IsNull(removedSlot.Item);
            Assert.AreEqual(0, removedSlot.Quantity);
        }

        [Test]
        public void ItemsRemoved_Event_FiresPerSlot_WhenRemovingAllOccurrences()
        {
            _inventory.TryAddItem(_potion, MAX_STACK * 2); // fills both slots
            var captured = new List<(IItem Item, int Quantity)>();
            _inventory.ItemsRemoved += (slot, index) => captured.Add((slot.Item, slot.Quantity));

            _inventory.RemoveItem(_potion); // remove all

            Assert.That(captured, Is.Not.Empty, "Should fire removal event for each slot touched.");
            Assert.That(captured.All(c => c.Quantity == 0), Is.True);
        }

        [Test]
        public void ItemsRemoved_Event_Fires_OnClearEntireInventory()
        {
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir);
            IInventorySlot removedSlot = null;
            _inventory.ItemsRemoved += (slot, index) => { removedSlot = slot; };

            _inventory.Clear();

            Assert.IsNull(removedSlot.Item);
            Assert.AreEqual(removedSlot.Quantity, 0);
        }

        [Test]
        public void CapacityChanged_Event_Fires_WhenCapacityIsIncreased()
        {
            var inventory = InventoryFactory.Create(2, maxCapacity: 4);
            var fired = false;
            int newCap = -1;

            inventory.CapacityChanged += (cap) =>
            {
                fired = true;
                newCap = cap;
            };

            inventory.TryIncreaseCapacity(2);

            Assert.IsTrue(fired);
            Assert.AreEqual(4, newCap);
        }

        [Test]
        public void CapacityChanged_Event_DoesNotFire_WhenIncreaseFails()
        {
            var inventory = InventoryFactory.Create(5, maxCapacity: 5);
            var fired = false;

            inventory.CapacityChanged += (_) => fired = true;

            inventory.TryIncreaseCapacity(1);

            Assert.IsFalse(fired);
        }

        
        #endregion
        
        #region Capacity Increase ----------------------------------------------

        [Test]
        public void TryIncreaseCapacity_WhenNegativeValue_ReturnsFalseAndDoesNothing()
        {
            var result = _inventory.TryIncreaseCapacity(-5);

            Assert.IsFalse(result);
            Assert.AreEqual(2, _inventory.Capacity);
        }

        [Test]
        public void TryIncreaseCapacity_WhenZero_ReturnsFalseAndDoesNothing()
        {
            var result = _inventory.TryIncreaseCapacity(0);

            Assert.IsFalse(result);
            Assert.AreEqual(2, _inventory.Capacity);
        }

        [Test]
        public void TryIncreaseCapacity_WhenBelowMax_IncreasesCapacityAndReturnsTrue()
        {
            
            var result = _inventory.TryIncreaseCapacity(2);

            Assert.IsTrue(result);
            Assert.AreEqual(4, _inventory.Capacity);
            Assert.AreEqual(4, _inventory.InventorySlots.Count);
            Assert.AreEqual(4, _inventory.Items.Count);
        }

        [Test]
        public void TryIncreaseCapacity_WhenAddingPastMax_ClampsToMaxAndReturnsTrue()
        {
            // Assuming your MaxCapacity is 5
            _inventory.TryIncreaseCapacity(10);

            Assert.AreEqual(5, _inventory.Capacity);
        }

        [Test]
        public void TryIncreaseCapacity_WhenAtMax_DoesNothingAndReturnsFalse()
        {
            _inventory.TryIncreaseCapacity(3); // Bring to 5
            var result = _inventory.TryIncreaseCapacity(1); // Already at max

            Assert.IsFalse(result);
            Assert.AreEqual(5, _inventory.Capacity);
        }

        [Test]
        public void TryIncreaseCapacity_WhenNearMax_IncreasesToMaxAndReturnsTrue()
        {
            var result = _inventory.TryIncreaseCapacity(10); // More than needed

            Assert.IsTrue(result);
            Assert.AreEqual(5, _inventory.Capacity);
        }
        [Test]
        public void TryIncreaseCapacity_PreservesExistingSlotsAndAddsEmptyOnes()
        {
            // Arrange: Add two items (initial capacity is 2)
            _inventory.TryAddItem(_potion);
            _inventory.TryAddItem(_elixir);

            // Act: Increase capacity by 2 (new capacity should be 4)
            var success = _inventory.TryIncreaseCapacity(2);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(4, _inventory.Capacity);
            Assert.AreEqual(4, _inventory.InventorySlots.Count);

            // Existing items should be preserved
            Assert.IsTrue(_potion.IsEquivalentTo(_inventory.TryGetItemAt(0)));
            Assert.IsTrue(_elixir.IsEquivalentTo(_inventory.TryGetItemAt(1)));

            // New slots should be empty
            Assert.IsTrue(_inventory.TryGetSlotAt(2).IsEmpty);
            Assert.IsTrue(_inventory.TryGetSlotAt(3).IsEmpty);
        }

        #endregion

        #region Swap

        [Test]
        public void Swap_WhenCalled_SwapsItemsBetweenSlots()
        {
            _inventory.TryAddItem(_potion, 1);
            _inventory.TryAddItem(_elixir, 1);

            Assert.IsTrue(_potion.IsEquivalentTo(_inventory.TryGetItemAt(0)));
            Assert.IsTrue(_elixir.IsEquivalentTo(_inventory.TryGetItemAt(1)));

            _inventory.Swap(0, 1);

            Assert.IsTrue(_elixir.IsEquivalentTo(_inventory.TryGetItemAt(0)));
            Assert.IsTrue(_potion.IsEquivalentTo(_inventory.TryGetItemAt(1)));
        }

        [Test]
        public void Swap_WhenFirstSlotIsEmpty_SecondSlotMovesToFirst()
        {
            _inventory.TryAddItemAt(_elixir,1, 1);

            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.IsTrue(_elixir.IsEquivalentTo(_inventory.TryGetItemAt(1)));

            _inventory.Swap(0, 1);

            Assert.IsTrue(_elixir.IsEquivalentTo(_inventory.TryGetItemAt(0)));
            Assert.IsNull(_inventory.TryGetItemAt(1));
        }

        [Test]
        public void Swap_WhenSecondSlotIsEmpty_FirstSlotMovesToSecond()
        {
            _inventory.TryAddItem(_potion, 1);

            Assert.IsTrue(_potion.IsEquivalentTo(_inventory.TryGetItemAt(0)));
            Assert.IsNull(_inventory.TryGetItemAt(1));

            _inventory.Swap(0, 1);

            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.IsTrue(_potion.IsEquivalentTo(_inventory.TryGetItemAt(1)));
        }

        [Test]
        public void Swap_WhenBothSlotsAreEmpty_NoChangeOccurs()
        {
            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.IsNull(_inventory.TryGetItemAt(1));

            _inventory.Swap(0, 1);

            Assert.IsNull(_inventory.TryGetItemAt(0));
            Assert.IsNull(_inventory.TryGetItemAt(1));
        }

        #endregion
        
        #region ––––– Compact –––––

        [Test]
        public void Compact_WhenInventoryIsAlreadyCompact_DoesNothing()
        {
            _inventory.TryAddItem(_potion); // Index 0
            _inventory.TryAddItem(_elixir); // Index 1

            var before = _inventory.Items.ToList();

            _inventory.Compact();

            var after = _inventory.Items.ToList();
            CollectionAssert.AreEqual(before, after);
        }

        [Test]
        public void Compact_WhenOneGapExists_MovesItemLeft()
        {
            _inventory.TryAddItem(_potion); // Index 0
            _inventory.TryAddItem(_elixir); // Index 1

            ((InventorySlot)_inventory.TryGetSlotAt(0)).Clear(); // Now: [null, elixir]

            _inventory.Compact();

            Assert.That(_inventory.TryGetItemAt(0)?.IsEquivalentTo(_elixir), Is.True);
            Assert.That(_inventory.TryGetItemAt(1), Is.Null);
        }

        [Test]
        public void Compact_WhenMultipleGapsExist_MovesAllItemsLeftInOrder()
        {
            _inventory = InventoryFactory.Create(4, true, MAX_STACK);

            _inventory.TryAddItem(_potion, 5);  // Index 0, Full
            _inventory.TryAddItem(_elixir, 5);  // Index 1, Full
            _inventory.TryAddItem(_potion, 5);  // Index 2, Full
            _inventory.TryAddItem(_elixir, 5);  // Index 3, Full

            ((InventorySlot)_inventory.TryGetSlotAt(0)).Clear(); // Gap at 0
            ((InventorySlot)_inventory.TryGetSlotAt(2)).Clear(); // Gap at 2

            // Now: [null, elixir, null, elixir]

            _inventory.Compact();

            Assert.That(_inventory.TryGetItemAt(0)?.IsEquivalentTo(_elixir), Is.True);
            Assert.That(_inventory.TryGetItemAt(1)?.IsEquivalentTo(_elixir), Is.True);
            Assert.That(_inventory.TryGetItemAt(2), Is.Null);
            Assert.That(_inventory.TryGetItemAt(3), Is.Null);
        }

        [Test]
        public void Compact_WhenMultipleConsecutiveGapsExist_MovesAllItemsLeftInOrder()
        {
            _inventory = InventoryFactory.Create(4, true, MAX_STACK);

            _inventory.TryAddItem(_potion, 5);  // Index 0, Full
            _inventory.TryAddItem(_elixir, 5);  // Index 1, Full
            _inventory.TryAddItem(_potion, 5);  // Index 2, Full
            _inventory.TryAddItem(_elixir, 5);  // Index 3, Full

            ((InventorySlot)_inventory.TryGetSlotAt(1)).Clear(); // Gap at 1
            ((InventorySlot)_inventory.TryGetSlotAt(2)).Clear(); // Gap at 2

            // Now: [potion, null, null, elixir]

            _inventory.Compact();

            Assert.That(_inventory.TryGetItemAt(0)?.IsEquivalentTo(_potion), Is.True);
            Assert.That(_inventory.TryGetItemAt(1)?.IsEquivalentTo(_elixir), Is.True);
            Assert.That(_inventory.TryGetItemAt(2), Is.Null);
            Assert.That(_inventory.TryGetItemAt(3), Is.Null);
        }
        
        [Test]
        public void Compact_WhenAllSlotsAreEmpty_DoesNothing()
        {
            _inventory = InventoryFactory.Create(3, true, MAX_STACK);

            _inventory.Compact();

            foreach (var item in _inventory.Items)
                Assert.That(item, Is.Null);
        }

        [Test]
        public void Compact_PreservesItemOrder_AfterGapsFilled()
        {
            _inventory = InventoryFactory.Create(5, true, MAX_STACK);

            _inventory.TryAddItem(_potion, 5);  // Index 0
            _inventory.TryAddItem(_elixir,5);  // Index 1
            _inventory.TryAddItem(_potion,5);  // Index 2

            ((InventorySlot)_inventory.TryGetSlotAt(0)).Clear(); // Gap at 0

            _inventory.Compact();
            
            Assert.That(_inventory.Items.Count, Is.EqualTo(5));
            Assert.That(_inventory.TryGetItemAt(0).IsEquivalentTo(_elixir), Is.True);
            Assert.IsNotNull(_inventory.TryGetItemAt(1));
            Assert.That(_inventory.TryGetItemAt(1).IsEquivalentTo(_potion), Is.True);
        }

        #endregion

    }
}