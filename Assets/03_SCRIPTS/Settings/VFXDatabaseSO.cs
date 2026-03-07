using System;
using System.Collections.Generic;
using MADP.Views.VFX;
using UnityEngine;

namespace MADP.Settings
{
    public enum VFXType
    {
        Heal, FloatingHealth, FloatingDamage
    }
    
    [CreateAssetMenu(fileName = "VFXDatabase", menuName = "MADP/Settings/VFX Database")]
    public class VFXDatabaseSO : ScriptableObject
    {
        [Serializable]
        public struct VFXData
        {
            public VFXType Type;
            public BaseVFXView Prefab;
        }

        public List<VFXData> vfxList;

        public BaseVFXView GetPrefab(VFXType type)
        {
            foreach (var data in vfxList)
            {
                if (data.Type == type) return data.Prefab;
            }
            return null;
        }
    }
}