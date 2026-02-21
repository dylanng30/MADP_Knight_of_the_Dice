using System;
using System.Collections.Generic;
using UnityEngine;

namespace MADP.Settings
{
    [Serializable]
    public struct CharacterConfig
    {
        public int level;
        public GameObject modelPrefab;
    }
    public class CharacterDatabaseSO : ScriptableObject
    {
        
    }
}