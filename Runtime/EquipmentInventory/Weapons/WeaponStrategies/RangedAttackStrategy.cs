using System;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Weapons
{
    [Serializable]
    [CreateAssetMenu(fileName = "RangedAttackStrategy", menuName = "InventorySystem/WeaponStrategy/RangedAttackStrategy")]
    public class RangedAttackStrategy : WeaponAttackStrategy
    {
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}