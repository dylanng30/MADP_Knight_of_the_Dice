using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MADP.Controllers;
using TMPro;
using UnityEngine;

namespace MADP.Views
{
    public class GoldView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;
        public void SetGold(int goldAmount)
        {
            string info = $"Gold: {goldAmount}";
            goldText.text = info;
        }
    }
}
