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

        public static Sequence RoleDice(Transform dice, Vector3 targetRotation, float jumpHeight = 150f, float duration = 0.8f, int spinMultiplier = 5)
        {
            DOTween.Kill(dice);
            Sequence seq = DOTween.Sequence();
            Vector3 initialPos = dice.position;
            
            seq.Append(dice.DOLocalMoveY(initialPos.y + jumpHeight, duration / 2).SetEase(Ease.OutQuad));
            seq.Join(dice.DORotate(new Vector3(360 * spinMultiplier, 360 * spinMultiplier, 0), duration / 2, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            
            seq.Append(dice.DOLocalMoveY(initialPos.y, duration / 2).SetEase(Ease.OutBounce)); 
            seq.Join(dice.DORotate(targetRotation, duration / 2).SetEase(Ease.OutQuad));
            
            return seq;
        }
    }
}