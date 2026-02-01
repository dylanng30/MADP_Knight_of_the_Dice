namespace MADP.Models
{
    public class UnitStatModel
    {
        public int MaxHealth;
        public int CurrentHealth;
        public int Damage;
        public int Armor;

        public UnitStatModel(int maxHealth, int damage, int armor)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Damage = damage;
            Armor = armor;
        }
    }
}