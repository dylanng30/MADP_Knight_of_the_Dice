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
        [SerializeField] private RectTransform rect;

        [SerializeField] private float displayDuration = 1.5f;

        public void AnimateTurnNotification(TeamColor teamColor, bool playerTurn, Action onAnimationComplete)
        {
            string turnString = playerTurn ? "Your turn" : $"{teamColor.ToString()}'s turn";
            if (turnTxt) turnTxt.text = turnString;
            
            gameObject.SetActive(true);

            Sequence seq = UIAnimator.Popup(rect, 1f);
            seq.AppendInterval(displayDuration);
            seq.OnComplete(() => 
            {
                gameObject.SetActive(false);
                onAnimationComplete?.Invoke();
            });
        }
    }
}