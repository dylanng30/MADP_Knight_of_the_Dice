using System;
using System.Collections;
using DG.Tweening;
using MADP.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MADP.Views
{
    public class DiceView: MonoBehaviour
    {
        [SerializeField] private float rotateDuration = 0.8f;
        [SerializeField] private float jumpHeight = 150f;

        private Vector3 originalPos;
        
        private readonly Vector3[] _faceRotations = 
        {
            new Vector3(0, 0, 0),
            new Vector3(90, 0, 0),
            new Vector3(0, 0, -90),
            new Vector3(0, 0, 90),
            new Vector3(-90, 0, 0),
            new Vector3(180, 0, 0)
        };
        
        public void Roll(int targetResult, Action onCompleted)
        {
            originalPos = transform.position;
            Vector3 targetRotation = _faceRotations[targetResult - 1];
            Quaternion finalQuat = Quaternion.Euler(targetRotation);
            transform.localRotation = finalQuat;
            UIAnimator.RoleDice(transform, targetRotation, jumpHeight, rotateDuration).OnComplete(() =>
            {
                onCompleted?.Invoke();
                transform.position = originalPos;
            });
        }
    }
}