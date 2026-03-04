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
        [SerializeField] private float displayDuration = 3f;
        
        public void UpdateTimer(float timeLeft)
        {
            if (timerTxt != null)
            {
                timerTxt.text = $"{Mathf.CeilToInt(timeLeft)}s";
                timerTxt.color = timeLeft <= 5f ? Color.red : Color.white;
            }
        }

        public void AnimateTurnNotification(TeamColor teamColor, bool playerTurn, Action onAnimationCompleted)
        {
            string turnString = playerTurn ? "Your turn" : $"{teamColor.ToString()}'s turn";
            if (turnTxt)
                turnTxt.text = turnString;
            
            Popup(turnTxtRect, onAnimationCompleted);
        }

        private void Popup(RectTransform rect, Action onAnimationCompleted = null)
        {
            if (rect != null)
            {
                rect.gameObject.SetActive(true);
                Sequence sequence = UIAnimator.Popup(turnTxtRect, 1f);
                sequence.AppendInterval(displayDuration);
                sequence.OnComplete(() => 
                {
                    rect.gameObject.SetActive(false);
                    onAnimationCompleted?.Invoke();
                });
            }
            else
            {
                onAnimationCompleted?.Invoke();
            }
            
        }
    }
}