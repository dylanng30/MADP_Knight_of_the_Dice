using UnityEngine;

namespace MADP.Services
{
    public class DiceService
    {
        public int Roll()
        {
            //return 6;
            /*int index = Random.Range(1, 3);
            if (index == 1)
                return 6;
            else
            {
                return 1;
            }*/
            return Random.Range(1, 7);
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