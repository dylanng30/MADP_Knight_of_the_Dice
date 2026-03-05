using System;
using System.Collections.Generic;
using MADP.Models;
using UnityEngine;

namespace MADP.Settings
{
    [Serializable]
    public struct UnitAvatarData
    {
        public TeamColor Team;
        public int UnitId;
        public Sprite Avatar;
    }

    [CreateAssetMenu(fileName = "UnitAvatarDB", menuName = "MADP/Settings/Unit Avatar Database")]
    public class UnitAvatarDatabaseSO : ScriptableObject
    {
        public List<UnitAvatarData> AvatarConfigs;
        
        public Sprite GetAvatar(TeamColor team, int unitId)
        {
            foreach (var config in AvatarConfigs)
            {
                if (config.Team == team && config.UnitId == unitId)
                {
                    return config.Avatar;
                }
            }
            Debug.LogWarning($"[UnitAvatarDatabase] Không tìm thấy Avatar cho Team: {team}, UnitID: {unitId}");
            return null;
        }
    }
}