using System;
using System.Collections.Generic;
using MADP.Views.VFX;
using UnityEngine;

namespace MADP.Settings
{
    public enum VFXType
    {
        Heal
    }
    
    [CreateAssetMenu(fileName = "VFXDatabase", menuName = "MADP/Settings/VFX Database")]
    public class VFXDatabaseSO : ScriptableObject
    {
        [Serializable]
        public struct VFXData
        {
            public VFXType Type;
            public VFXView Prefab;
        }

        public List<VFXData> vfxList;

        public VFXView GetPrefab(VFXType type)
        {
            foreach (var data in vfxList)
            {
                if (data.Type == type) return data.Prefab;
            }
            return null;
        }
    }
}