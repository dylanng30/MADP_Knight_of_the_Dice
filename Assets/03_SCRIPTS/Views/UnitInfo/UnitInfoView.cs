using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.UnitInfo
{
    public class UnitInfoView : MonoBehaviour
    {
        [SerializeField] private Image Avatar;

        [Header("HEALTH")]
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI healthRatioTxt;

        [Header("DAMAGE")]
        [SerializeField] private TextMeshProUGUI damageTxt;

        [Header("ARMOR")]
        [SerializeField] private TextMeshProUGUI armorTxt;

        private UnitModel unitModel;

        public void Setup(UnitModel model)
        {
            unitModel = model;
            healthBar.fillAmount = (float)unitModel.Stat.CurrentHealth / unitModel.Stat.MaxHealth;
            healthRatioTxt.text = $"{unitModel.Stat.CurrentHealth}/{unitModel.Stat.MaxHealth}";
            damageTxt.text = unitModel.Stat.Damage.ToString();
            armorTxt.text = unitModel.Stat.Armor.ToString();

            gameObject.SetActive(true);
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            unitModel = null;           
        }

        private void Update()
        {
           if(unitModel != null)
           {
                
           }
        }

    }
}
