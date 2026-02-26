using System;
using UnityEngine;

namespace MADP.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed;
        
        private void Update()
        {
            if(Input.GetMouseButton(0))
                Rotate();
        }

        private void Rotate()
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, Time.deltaTime * mouseX * rotateSpeed);
        }
    }
}