using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MADP.Views
{
    public class DiceView: MonoBehaviour
    {
        [SerializeField] private float rotateDuration = 0.8f;
        
        private bool _isRolling;
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
            if (_isRolling) 
                return;
            
            _isRolling = true;
            
            Vector3 targetRotation = _faceRotations[targetResult - 1];
            Quaternion finalQuat = Quaternion.Euler(targetRotation);
            transform.localRotation = finalQuat;

            _isRolling = false;
            onCompleted?.Invoke();
        }
    }
}