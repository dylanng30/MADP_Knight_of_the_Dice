using System;
using System.Collections.Generic;
using MADP.Models.UnitActions;
using MADP.Systems;
using MADP.Views;
using UnityEngine;

namespace MADP.Controllers
{
    public class ActionControllerTest : MonoBehaviour
    {
        [SerializeField] private UnitView unitView;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                List<Vector3> path = new List<Vector3>
                {
                    Vector3.right, 
                    Vector3.right * 2, 
                    Vector3.right * 3, 
                    Vector3.right * 4,
                    Vector3.right * 5,
                };
                
                MoveUA moveUa = new MoveUA(unitView, path);
                ActionSystem.Instance.Perform(moveUa);
            }
        }
    }
}

