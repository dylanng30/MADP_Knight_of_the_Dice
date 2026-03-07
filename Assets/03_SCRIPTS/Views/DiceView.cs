using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MADP.Controllers;
using MADP.Settings;
using MADP.Utilities;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MADP.Views
{
    public class DiceView: MonoBehaviour
    {
        [SerializeField] private float rotateDuration = 0.8f;
        [SerializeField] private float jumpHeight = 150f;

        [Header("---MAP SETUP---")]
        [SerializeField] private List<TextMeshProUGUI> numbers;

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

        public void Setup(Color mapColor)
        {
            foreach (var number in numbers)
            {
                number.color = mapColor;
            }   
        }
        
        public void Roll(int targetResult, Action onCompleted)
        {
            originalPos = transform.position;
            Vector3 targetRotation = _faceRotations[targetResult - 1];
            Quaternion finalQuat = Quaternion.Euler(targetRotation);
            transform.localRotation = finalQuat;
            UIAnimator.RoleDice(transform, targetRotation, jumpHeight, rotateDuration).OnComplete(() =>
            {
                PlaySound();
                transform.position = originalPos;
                onCompleted?.Invoke();
            });
        }

        private void PlaySound()
        {
            if(AudioController.Instance != null)
                AudioController.Instance.PlaySound(SoundKey.SFX_DiceRoll);
        }
    }
}