using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MADP.Controllers;
using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views
{
    public class GoldView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Image bgImage;
        public void Setup(TeamColor team, Color uiColor)
        {
            if (teamNameText) teamNameText.text = team.ToString();
            if (bgImage) bgImage.color = uiColor;
            if (goldText) goldText.color = uiColor;

            gameObject.SetActive(true);
        }
        public void SetGold(int goldAmount)
        {
            goldText.text = $"Gold: {goldAmount}";
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
