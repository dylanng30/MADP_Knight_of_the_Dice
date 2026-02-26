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
        [SerializeField] private Image avatar;
        [SerializeField] private Image bgImage;
        [SerializeField] private Image frameImage;
        
        [SerializeField] private TextMeshProUGUI goldText;
        
        public void Setup(TeamColor team, Color uiColor, Color frameColor, string avatarPath)
        {
            
            if (bgImage) bgImage.color = uiColor;
            if (frameImage) frameImage.color = frameColor;
            //if (goldText) goldText.color = uiColor;
            
            //Load Avatar

            gameObject.SetActive(true);
        }
        public void SetGold(int goldAmount)
        {
            goldText.text = $"{goldAmount}$";
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
