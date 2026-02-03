using MADP.Models;
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

        public int Cost { get; private set; }

        public UnitModel(int id, TeamColor teamOwner, UnitStatModel stat)
        {
            Id = id;
            TeamOwner = teamOwner;
            Stat = stat;
            Cost = id;
        }

        public void Reset()
        {
            State = UnitState.InNest;
        }
        public void SetState(UnitState newState)
        {
            //Debug.Log($"Unit {Id} state changed from {State} to {newState}");
            State = newState;
        }

        public void TakeDamage(int amount)
        {
            Debug.Log($"Unit {Id} takes {amount} damage");
            Stat.CurrentHealth -= amount;
        }

        public void Heal(int amount)
        {
            Stat.CurrentHealth += amount;
            if(Stat.CurrentHealth > Stat.MaxHealth)
                Stat.CurrentHealth = Stat.MaxHealth;
        }
        
        public bool IsDead() => Stat.CurrentHealth <= 0;

        public void Revive()
        {
            Stat.CurrentHealth = Stat.MaxHealth;
            Reset();
        }
    }
}