using System;
using System.Collections.Generic;
using DG.Tweening;
using MADP.Views;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MADP.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed;
        [SerializeField] private float mobileSensitivityMultiplier = 0.1f;
        
        [Header("---ROTATION---")]
        [SerializeField] private Vector3 defaultRotation;
        [SerializeField] private List<GoldView> goldViews;

        private int lastIndex;
        
        private bool _isInteractingWithUI = false;
        
        private void Start()
        {
            transform.eulerAngles = defaultRotation;
            
            foreach (GoldView view in goldViews)
            {
                view.OnClicked += HandleRotateRequested;
            }
        }

        private void OnDestroy()
        {
            foreach (GoldView view in goldViews)
            {
                view.OnClicked -= HandleRotateRequested;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                _isInteractingWithUI = CheckIfPointerOverUI();
            }
            if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)))
            {
                _isInteractingWithUI = false;
            }
            if (_isInteractingWithUI) return;
            
            HandleRotationInput();
        }
        private bool CheckIfPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            
            if (Input.touchCount > 0)
            {
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
            
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void HandleRotationInput()
        {
            float rotationAmount = 0;
            
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Moved)
                {
                    rotationAmount = touch.deltaPosition.x * mobileSensitivityMultiplier;
                    transform.Rotate(Vector3.up, rotationAmount * rotateSpeed * Time.deltaTime);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                rotationAmount = Input.GetAxis("Mouse X");
                transform.Rotate(Vector3.up, rotationAmount * rotateSpeed * 5f * Time.deltaTime); 
            }
        }

        private void HandleRotateRequested(int index)
        {
            float targetYAngle = defaultRotation.y - (index * 90f);
            Vector3 targetRotation = new Vector3(defaultRotation.x, targetYAngle, defaultRotation.z);
            transform.DORotate(targetRotation, 1f).SetEase(Ease.OutBack);
        }
    }
}