using System;
using UnityEngine;

namespace MADP.Models
{
    [System.Serializable]
    public class StatModel
    {
        public int DMG { get; private set; }
        public int HP { get; private set; }
        public int DEF { get; private set; }

        public StatModel(int dmg, int hp, int def)
        {
            DMG = dmg;
            HP = hp;
            DEF = def;
        }

        public void TakeDamage(int amount)
        {
            int overDmg = 0;
            DEF -= Mathf.CeilToInt(DMG / 2f);
            if (DMG <= 0)
            {
                overDmg = Mathf.Abs(DEF);
            }
            HP -= Mathf.FloorToInt(DMG / 2f + overDmg);
        }
    }
}