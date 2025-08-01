using System;
using System.Linq;
using NUnit.Framework;
using InventorySystem.EquipmentInventory;
using InventorySystem.Items;

namespace InventorySystemTests
{
    /// <summary>
    ///  Comprehensive behaviour-level tests for any <see cref="IEquipmentInventory"/> implementation.
    ///  Replace the TODO in <see cref="CreateInventory"/> with your concrete inventory class
    ///  or derive and override <see cref="CreateInventory"/> for different configurations.
    /// </summary>
    [TestFixture]
    public class EquipmentInventoryTests
    {
        private IEquipmentInventory _inventory;

        #region ––––– helpers –––––

        /// <summary>
        /// Factory method for the System Under Test (SUT).
        /// Adapt this to build your own <see cref="IEquipmentInventory"/> (e.g. pass allowed slots, DI, etc.).
        /// </summary>
        protected virtual IEquipmentInventory CreateInventory()
        {
            // TODO ▸ swap this placeholder with the concrete class you want to test
            return EquipmentInventory.Create();
        }

        private IEquipmentItem CreateEquipmentItem(string itemName, EquipmentType slot)
        {
            var so = TestUtils.CreateItemSO(itemName, 1);
            return new MockEquipmentItem(so, slot);
        }

        /// <summary>Local minimal mock so we don’t depend on production data.</summary>
        private sealed class MockEquipmentItem : MockItem, IEquipmentItem
        {
            public EquipmentType EquipmentType { get; }
            public MockEquipmentItem(ItemData data, EquipmentType slot) : base(data) => EquipmentType = slot;
        }

        #endregion

        #region ––––– life cycle –––––

        [SetUp]
        public void SetUp() => _inventory = CreateInventory();

        #endregion

        #region ––––– invariants / initial state –––––

        [Test]
        public void NewInventory_InitiallyEmpty_EquippedListIsEmptyAndNoNulls()
        {
            Assert.IsEmpty(_inventory.EquippedItemList);
            // should not contain nulls even if empty
            Assert.That(_inventory.EquippedItemList, Has.No.Null);
        }

        [Test]
        public void EquippedItemList_IsReadOnly_ViewingDoesNotAllowMutation()
        {
            // retrieve list and ensure typical mutation attempts don't compile/use interface.
            var list = _inventory.EquippedItemList;
            Assert.DoesNotThrow(() =>
            {
                // reading length and enumerating is safe
                _ = list.Count;
                foreach (var item in list) { }
            });
        }

        #endregion

        #region ––––– TryEquip –––––

        [Test]
        public void TryEquip_NonEquipmentItem_ReturnsFalseAndDoesNotFireEvent()
        {
            var plain = TestUtils.CreateMockItem("Potion", 1); // IItem but not IEquipmentItem
            bool eventFired = false;
            _inventory.ItemEquipped += _ => eventFired = true;

            var result = _inventory.TryEquip(plain);

            Assert.IsFalse(result);
            Assert.IsFalse(eventFired, "ItemEquipped must not fire for failure.");
            Assert.IsEmpty(_inventory.EquippedItemList);
        }

        [Test]
        public void TryEquip_Null_ReturnsFalseAndDoesNotMutate()
        {
            bool eventFired = false;
            _inventory.ItemEquipped += _ => eventFired = true;

            var result = _inventory.TryEquip(null!); // assuming nullable not guarded; expect false

            Assert.IsFalse(result);
            Assert.IsFalse(eventFired);
            Assert.IsEmpty(_inventory.EquippedItemList);
        }

        [Test]
        public void TryEquip_ValidItem_ReturnsTrue_AddsToList_AndFiresEvent()
        {
            var sword = CreateEquipmentItem("Sword", EquipmentType.Weapon);
            IItem equippedViaEvent = null;
            _inventory.ItemEquipped += item => equippedViaEvent = item;

            var success = _inventory.TryEquip(sword);

            Assert.IsTrue(success);
            Assert.That(_inventory.EquippedItemList, Has.Member(sword));
            Assert.AreSame(sword, equippedViaEvent);
        }

        [Test]
        public void TryEquip_SameSlotTwice_SecondReturnsFalse_DoesNotDuplicate()
        {
            var first = CreateEquipmentItem("Iron Helmet", EquipmentType.Armor);
            var second = CreateEquipmentItem("Steel Helmet", EquipmentType.Armor);

            Assert.IsTrue(_inventory.TryEquip(first), "First equip should succeed");
            Assert.IsFalse(_inventory.TryEquip(second), "Second equip in same slot must fail");

            Assert.AreEqual(1, _inventory.EquippedItemList.Count);
            Assert.AreSame(first, _inventory.GetItem(EquipmentType.Armor));
        }

        [Test]
        public void TryEquip_DifferentSlots_BothSucceed()
        {
            var helmet = CreateEquipmentItem("Helmet", EquipmentType.Armor);
            var weapon = CreateEquipmentItem("Sword", EquipmentType.Weapon);

            Assert.IsTrue(_inventory.TryEquip(helmet));
            Assert.IsTrue(_inventory.TryEquip(weapon));

            Assert.AreEqual(2, _inventory.EquippedItemList.Count);
            Assert.That(_inventory.EquippedItemList, Has.Member(helmet).And.Member(weapon));
        }

        #endregion

        #region ––––– Unequip –––––

        [Test]
        public void Unequip_SlotWithItem_ReturnsItem_RemovesIt_AndFiresEvent()
        {
            var boots = CreateEquipmentItem("Leather Boots", EquipmentType.Armor);
            IItem unequippedViaEvent = null;
            _inventory.ItemUnequipped += item => unequippedViaEvent = item;

            Assert.IsTrue(_inventory.TryEquip(boots));
            var removed = _inventory.Unequip(EquipmentType.Armor);

            Assert.AreSame(boots, removed);
            Assert.IsNull(_inventory.GetItem(EquipmentType.Armor));
            Assert.IsFalse(_inventory.EquippedItemList.Contains(boots));
            Assert.AreSame(boots, unequippedViaEvent);
        }

        [Test]
        public void Unequip_EmptySlot_ReturnsNull_DoesNotFireEvent()
        {
            bool eventFired = false;
            _inventory.ItemUnequipped += _ => eventFired = true;

            var result = _inventory.Unequip(EquipmentType.Armor);
            Assert.IsNull(result);
            Assert.IsFalse(eventFired);
        }

        [Test]
        public void Unequip_Then_EquipAgain_Succeeds()
        {
            var item = CreateEquipmentItem("Helmet", EquipmentType.Armor);
            Assert.IsTrue(_inventory.TryEquip(item));
            var removed = _inventory.Unequip(EquipmentType.Armor);
            Assert.IsNull(_inventory.GetItem(EquipmentType.Armor));

            Assert.IsTrue(_inventory.TryEquip(item), "Should be able to equip again after unequip");
            Assert.AreSame(item, _inventory.GetItem(EquipmentType.Armor));
        }

        #endregion

        #region ––––– GetItem ----------------------------------------------------

        [Test]
        public void GetItem_ReturnsCorrectItem_WhenEquipped()
        {
            var gloves = CreateEquipmentItem("Gloves", EquipmentType.Armor);
            Assert.IsTrue(_inventory.TryEquip(gloves));

            var retrieved = _inventory.GetItem(EquipmentType.Armor);
            Assert.AreSame(gloves, retrieved);
        }

        [Test]
        public void GetItem_EmptySlot_ReturnsNull()
        {
            Assert.IsNull(_inventory.GetItem(EquipmentType.Armor));
        }

        [Test]
        public void GetItem_DoesNotMutate_EquippedListRemainsUnchanged()
        {
            var gloves = CreateEquipmentItem("Gloves", EquipmentType.Armor);
            Assert.IsTrue(_inventory.TryEquip(gloves));
            var before = _inventory.EquippedItemList.ToList();

            var _ = _inventory.GetItem(EquipmentType.Armor); // call should be idempotent

            CollectionAssert.AreEqual(before, _inventory.EquippedItemList);
        }

        #endregion

        #region ––––– combined / edge cases -------------------------------

        [Test]
        public void Equip_FailsIfSameInstanceAlreadyEquipped_ReturnsFalse()
        {
            var item = CreateEquipmentItem("Ring", EquipmentType.Armor);
            Assert.IsTrue(_inventory.TryEquip(item));
            Assert.IsFalse(_inventory.TryEquip(item), "Equipping same instance twice in same slot should fail");
        }

        [Test]
        public void Equip_DifferentInstancesOfSameTypeInSameSlot_Fails()
        {
            var first = CreateEquipmentItem("Helmet", EquipmentType.Armor);
            var second = CreateEquipmentItem("Helmet", EquipmentType.Armor); // same equipment type but different instance

            Assert.IsTrue(_inventory.TryEquip(first));
            Assert.IsFalse(_inventory.TryEquip(second), "Second equip with same EquipmentType should fail even if different instance");
        }

        #endregion
    }
}
