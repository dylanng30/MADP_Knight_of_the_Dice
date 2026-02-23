using System;
using System.Collections.Generic;
using MADP.Models;
using UnityEngine;

namespace MADP.Settings
{
    public enum BotType
    {
        Easy, Medium, Hard
    }
    [Serializable]
    public struct BotRoleMapping
    {
        public BotType Type;
        public BotProfileSO Profile;
    }

    [CreateAssetMenu(fileName = "BotProfileDatabase", menuName = "MADP/Settings/Bot Profile Database")]
    public class BotProfileDatabaseSO : ScriptableObject
    {
        public List<BotRoleMapping> Mappings;
        
        public BotProfileSO GetProfile(BotType type)
        {
            var mapping = Mappings.Find(m => m.Type == type);
            return mapping.Profile; 
        }
    }
}