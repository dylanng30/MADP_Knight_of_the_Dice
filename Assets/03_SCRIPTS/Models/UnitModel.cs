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
        
        public int StepsMoved { get; private set; }
        public int CurrentIndex { get; private set; }

        public UnitModel(int id, TeamColor teamOwner, UnitStatModel stat)
        {
            Id = id;
            TeamOwner = teamOwner;
            Stat = stat;
        }

        public void Reset()
        {
            State = UnitState.InNest;
            StepsMoved = 0;
            CurrentIndex = -1;
        }

        public void MoveTo(int cellIndex)
        {
            CurrentIndex = cellIndex;
        }
        public void SetState(UnitState newState)
        {
            State = newState;
        }

        public void TakeDamage(int amount)
        {
            Debug.Log($"Unit {Id} takes {amount} damage");
            Stat.CurrentHealth -= amount;
        }
        
        public bool IsDead() => Stat.CurrentHealth <= 0;

        public void Revive()
        {
            Stat.CurrentHealth = Stat.MaxHealth;
            Reset();
        }
    }
}