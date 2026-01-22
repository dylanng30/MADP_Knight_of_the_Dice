using UnityEngine;

namespace MADP.Services
{
    public class DiceService
    {
        public int Roll()
        {
            return 6;
            //return Random.Range(1, 7);
        }

        public bool CanRollAgain(int diceValue) 
        {
            return diceValue == 6;
        }

        public bool CanSpawnUnit(int diceValue)
        {
            return diceValue == 6;
        }
    }
}