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
        [SerializeField] private Image teamColorImage;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button teamButton;
        
        private int _index;
        public Action<int> OnClicked;

        private void OnEnable()
        {
            if(teamButton != null) 
                teamButton.onClick.AddListener(OnTeamButtonClicked);
        }

        private void OnDisable()
        {
            if(teamButton != null) 
                teamButton.onClick.RemoveListener(OnTeamButtonClicked);
        }

        private void OnTeamButtonClicked()
        {
            OnClicked?.Invoke(_index);
        }

        public void Setup(int index, Color uiColor)
        {
            _index = index;
            
            if (teamColorImage) teamColorImage.color = uiColor;
            if (goldText) goldText.color = index == 0 ? Color.white : Color.black;
            
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
