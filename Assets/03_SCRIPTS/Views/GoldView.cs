using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MADP.Controllers;
using UnityEngine;

namespace MADP.Views
{
    public class GoldView : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI goldText;

        private void Start()
        {
            if(GoldController.Instance != null)
                GoldController.Instance.OnGoldChanged += UpdateGold;

        }
        private void OnDestroy()
        {
            if(GoldController.Instance != null)
                GoldController.Instance.OnGoldChanged -= UpdateGold;
        }
        public void UpdateGold(int goldAmount)
        {
            string info = $"Gold: {goldAmount}";
            goldText.text = info;
        }
    }
}
