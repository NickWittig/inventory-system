using System;
using UnityEngine;

namespace InventorySystem.EquipmentInventory.Weapons
{
    [Serializable]
    public abstract class WeaponAttackStrategy : ScriptableObject
    {
        public abstract void Execute();
    }
}