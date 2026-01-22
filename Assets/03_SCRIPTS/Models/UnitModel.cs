namespace MADP.Models
{
    public class UnitModel
    {
        public int HP;
        public int DMG;

        public void TakeDamage(int amount)
        {
            HP -= amount;
        }
    }
}

