using System.Collections.Generic;
using MADP.Models;
using UnityEngine;

namespace MADP.Settings
{
    [System.Serializable]
    public struct TeamColorConfig
    {
        public TeamColor team;
        public Color primaryColor;
        public Color secondaryColor;
        public Color tertiaryColor;
    }

    public enum Priority
    {
        Primary, Secondary, Tertiary
    }
    
    [CreateAssetMenu(fileName = "ColorDatabase", menuName = "Game Data/Color Database")]
    public class TeamColorDatabaseSO : ScriptableObject
    {
        public List<TeamColorConfig> colorConfigs;

        public Color GetColor(TeamColor team, Priority priority)
        {
            foreach (var config in colorConfigs)
            {
                if (config.team == team)
                {
                    switch (priority)
                    {
                        case Priority.Primary: return config.primaryColor;
                        case Priority.Secondary: return config.secondaryColor;
                        case Priority.Tertiary: return config.tertiaryColor;
                        default: return config.tertiaryColor;
                    }
                }
            }
            return Color.gray;
        }
    }
}