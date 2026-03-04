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

        [SerializeField] private Button rotateButton;
        
        public int _index;
        public Action<int> OnClicked;

        private void OnEnable()
        {
            if(rotateButton != null) 
                rotateButton.onClick.AddListener(() => OnClicked?.Invoke(_index));
        }

        private void OnDisable()
        {
            if(rotateButton != null) 
                rotateButton.onClick.RemoveListener(() => OnClicked?.Invoke(_index));
        }

        public void Setup(int index, Color uiColor, Color frameColor, string avatarPath)
        {
            _index = index;
            
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
