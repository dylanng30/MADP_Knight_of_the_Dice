using UnityEngine;

namespace MADP.Services
{
    public class BoardRotationService
    {
        private Vector3 _lastMousePosition;
        private bool _isDragging;
        private float _sensitivity = 0.3f;
        
        public void Update(Transform centerTransform)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _lastMousePosition = Input.mousePosition;
                _isDragging = true;
            }

            if (Input.GetMouseButton(0) && _isDragging)
            {
                Vector3 currentMousePosition = Input.mousePosition;
                float deltaX = currentMousePosition.x - _lastMousePosition.x;
                centerTransform.Rotate(Vector3.up, -deltaX * _sensitivity, Space.World);
                _lastMousePosition = currentMousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }
        }
    }
}