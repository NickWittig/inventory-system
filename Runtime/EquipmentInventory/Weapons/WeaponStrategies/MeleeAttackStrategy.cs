using System;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Weapons
{
    [Serializable]
    [CreateAssetMenu(fileName = "MeleeAttackStrategy", menuName = "WeaponStrategy/MeleeAttackStrategy")]
    public class MeleeAttackStrategy : WeaponAttackStrategy
    {
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}