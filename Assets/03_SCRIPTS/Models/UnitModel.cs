using System;
using MADP.Models;
using MADP.Models.Inventory;
using UnityEngine;

namespace MADP.Models
{
    public enum UnitState
    {
        InNest,
        Moving,
        InHome
    }
    public class UnitModel
    {
        public int Id { get; private set; }
        public TeamColor TeamOwner { get; private set; }
        public UnitStatModel Stat { get; private set; }
        public UnitState State { get; private set; } = UnitState.InNest;
        public RoleType RoleType { get; private set; }

        public int Cost { get; private set; }
        public int StepsMoved { get; private set; }
        
        //Inventory
        public UnitInventoryModel Inventory { get; private set; }
        public Action OnStatsChanged;
        
        public int MaxHealth => Stat.MaxHealth + Inventory.GetTotalBonusHealth();
        public int CurrentHealth => Mathf.Min(Stat.CurrentHealth + Inventory.GetTotalBonusHealth(), MaxHealth);
        public int Damage => Stat.Damage + Inventory.GetTotalBonusDamage();
        public int Armor => Stat.Armor + Inventory.GetTotalBonusArmor();

        public UnitModel(int id, TeamColor teamOwner, UnitStatModel stat, RoleType roleType)
        {
            Id = id;
            TeamOwner = teamOwner;
            Stat = stat;
            Cost = id;
            RoleType = roleType;

            Inventory = new UnitInventoryModel();
            Inventory.OnInventoryUpdated += () => OnStatsChanged?.Invoke();
        }
        
        public void AddSteps(int amount)
        {
            StepsMoved += amount;
        }
        public void SetState(UnitState newState)
        {
            State = newState;
        }

        public void TakeDamage(int amount)
        {
            Stat.CurrentHealth -= amount;
            OnStatsChanged?.Invoke();
        }

        public void Heal(int amount)
        {
            Stat.CurrentHealth += amount;
            if(Stat.CurrentHealth > Stat.MaxHealth)
                Stat.CurrentHealth = Stat.MaxHealth;
            OnStatsChanged?.Invoke();
        }
        

        public void Revive()
        {
            Stat.CurrentHealth = Stat.MaxHealth;
            State = UnitState.InNest;
            StepsMoved = 0;
        }
    }
}