using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MADP.Models;
using UnityEngine;

namespace MADP.Settings
{
    [Serializable]
    public struct TeamStat
    {
        public RoleType Role;
        public List<UnitStat> UnitStats;
    }
    [Serializable]
    public struct UnitStat
    {
        public int Level;
        public int MaxHealth;
        public int Damage;
        public int Armor;
    }
    [CreateAssetMenu(fileName = "TeamStatDatabaseSO", menuName = "MADP/Settings/Team Stat Database")]
    public class TeamStatDatabaseSO : ScriptableObject
    {
        public List<TeamStat> TeamStats;
    }
}
