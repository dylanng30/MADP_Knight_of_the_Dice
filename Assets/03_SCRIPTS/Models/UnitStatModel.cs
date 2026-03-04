namespace MADP.Models
{
    public class UnitStatModel
    {
        public int MaxHealth;
        public int CurrentHealth;
        public int Damage;
        public int Armor;
        public RoleType Role;

        public UnitStatModel(int maxHealth, int damage, int armor, RoleType role)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Damage = damage;
            Armor = armor;
            Role = role;
        }
    }
}