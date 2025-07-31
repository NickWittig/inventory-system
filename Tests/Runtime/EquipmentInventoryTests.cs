using System.Linq;
using NUnit.Framework;
using InventorySystem.EquipmentInventory;
using InventorySystem.Items;

namespace Tests.PlayerEquipment
{
    
    /// <summary>
    ///  Comprehensive behaviour‑level tests for any IEquipmentInventory implementation.
    ///  Replace the TODO in <see cref="CreateInventory"/> with your concrete inventory class
    ///  or make this class abstract and derive a type‑specific fixture.
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
        private IEquipmentInventory CreateInventory()
        {
            // TODO ▸ swap this placeholder with the concrete class you want to test
            // e.g.: return new EquipmentInventory(allowedEquipmentTypes);
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
            public EquipmentType equipmentType { get; }
            public MockEquipmentItem(ItemData data, EquipmentType slot) : base(data) => equipmentType = slot;
        }
        

        #endregion

        #region ––––– life cycle –––––

        [SetUp]
        public void SetUp() => _inventory = CreateInventory();

        #endregion

        #region ––––– TryEquip –––––

        [Test]
        public void TryEquip_NonEquipmentItem_ReturnsFalseAndDoesNotMutateList()
        {
            // Arrange
            var potion = TestUtils.CreateMockItem("Potion", 1); // plain IItem

            // Act
            var equipped = _inventory.TryEquip(potion);

            // Assert
            Assert.IsFalse(equipped, "Non‑equipment items must not be equipable.");
            Assert.IsEmpty(_inventory.EquippedItemList, "Inventory should remain unchanged when equip fails.");
        }

        [Test]
        public void TryEquip_ValidItem_ReturnsTrueAndAddsToList()
        {
            var sword = CreateEquipmentItem("Sword", EquipmentType.Weapon);

            var equipped = _inventory.TryEquip(sword);

            Assert.IsTrue(equipped);
            Assert.That(_inventory.EquippedItemList, Has.Member(sword));
        }

        [Test]
        public void TryEquip_SlotAlreadyOccupied_ReturnsFalse()
        {
            var ironHelmet  = CreateEquipmentItem("Iron Helmet",  EquipmentType.Armor);
            var steelHelmet = CreateEquipmentItem("Steel Helmet", EquipmentType.Armor);

            Assert.IsTrue(_inventory.TryEquip(ironHelmet), "first equip should succeed");
            Assert.IsFalse(_inventory.TryEquip(steelHelmet), "second equip in same slot must fail");
            Assert.AreEqual(1, _inventory.EquippedItemList.Count);
            Assert.AreSame(ironHelmet, _inventory.GetItem(EquipmentType.Armor));
        }

        #endregion

        #region ––––– Unequip –––––

        [Test]
        public void Unequip_SlotWithItem_ReturnsItemAndRemovesIt()
        {
            var boots = CreateEquipmentItem("Leather Boots", EquipmentType.Armor);
            _inventory.TryEquip(boots);

            var removed = _inventory.Unequip(EquipmentType.Armor);

            Assert.AreSame(boots, removed);
            Assert.IsNull(_inventory.GetItem(EquipmentType.Armor));
            Assert.IsFalse(_inventory.EquippedItemList.Contains(boots));
        }

        [Test]
        public void Unequip_EmptySlot_ReturnsNull()
        {
            Assert.IsNull(_inventory.Unequip(EquipmentType.Armor));
        }

        #endregion

        #region ––––– GetItem –––––

        [Test]
        public void GetItem_ReturnsCorrectItem()
        {
            var gloves = CreateEquipmentItem("Gloves", EquipmentType.Armor);
            _inventory.TryEquip(gloves);

            var retrieved = _inventory.GetItem(EquipmentType.Armor);
            Assert.AreSame(gloves, retrieved);
        }

        [Test]
        public void GetItem_EmptySlot_ReturnsNull()
        {
            Assert.IsNull(_inventory.GetItem(EquipmentType.Armor));
        }

        #endregion
    }
}
