using System;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Weapons
{
    [Serializable]
    [CreateAssetMenu(fileName = "ThrowableAttackStrategy", menuName = "WeaponStrategy/ThrowableAttackStrategy")]
    public class ThrowableAttackStrategy : WeaponAttackStrategy
    {
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}