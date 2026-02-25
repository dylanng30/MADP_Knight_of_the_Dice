using UnityEngine;

namespace MADP.Settings
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "MADP/Items/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Display")]
        public string ItemName;
        public Sprite Icon;
        [TextArea] public string Description;

        [Header("Stat Bonuses")]
        public int BonusHealth;
        public int BonusDamage;
        public int BonusArmor;
        
    }
}