using DG.Tweening;
using UnityEngine;

namespace MADP.Utilities
{
    public static class UIAnimator
    {
        public static Sequence SlideInFromTop(RectTransform rect, float dropOffset, float duration)
        {
            DOTween.Kill(rect);
            Vector2 finalPos = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(finalPos.x, finalPos.y + dropOffset);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOAnchorPosY(finalPos.y, duration).SetEase(Ease.OutBack));
            return sequence;
        }

        public static Sequence Popup(RectTransform rect, float duration)
        {
            DOTween.Kill(rect);
            rect.localScale = Vector3.zero;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOScale(Vector3.one, duration).SetEase(Ease.OutBack));
            return sequence;
        }
        public static Sequence Popdown(RectTransform rect, float duration)
        {
            DOTween.Kill(rect);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
            return sequence;
        }
    }
}