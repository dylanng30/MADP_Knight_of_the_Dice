using System;
using System.Collections;
using DG.Tweening;
using MADP.Models;
using MADP.Utilities;
using TMPro;
using UnityEngine;

namespace MADP.Views
{
    public class TurnView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turnTxt;
        [SerializeField] private RectTransform turnTxtRect;

        [Header("TIMER UI")] 
        [SerializeField] private TextMeshProUGUI timerTxt;

        [SerializeField] private float displayDuration = 1.5f;
        
        public void UpdateTimer(float timeLeft)
        {
            if (timerTxt != null)
            {
                timerTxt.text = Mathf.CeilToInt(timeLeft).ToString() + "s";
                timerTxt.color = timeLeft <= 5f ? Color.red : Color.white;
            }
        }

        public void AnimateTurnNotification(TeamColor teamColor, bool playerTurn, Action onAnimationCompleted)
        {
            string turnString = playerTurn ? "Your turn" : $"{teamColor.ToString()}'s turn";
            if (turnTxt) turnTxt.text = turnString;
            
            Popup(turnTxt.gameObject, onAnimationCompleted);
        }

        public void AnimateShopPhaseNotification()
        {
            if(turnTxt) turnTxt.text = $"SHOPPING PHASE";
            Popup(turnTxt.gameObject);
        }

        private void Popup(GameObject obj,Action onAnimationCompleted = null)
        {
            gameObject.SetActive(true);

            Sequence seq = UIAnimator.Popup(turnTxtRect, 1f);
            seq.AppendInterval(displayDuration);
            seq.OnComplete(() => 
            {
                obj.SetActive(false);
                onAnimationCompleted?.Invoke();
            });
        }
    }
}