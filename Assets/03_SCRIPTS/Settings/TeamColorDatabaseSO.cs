using System;
using System.Collections.Generic;
using MADP.Models;
using UnityEngine;

namespace MADP.Settings
{
    [System.Serializable]
    public struct TeamColorConfig
    {
        public TeamColor Team;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public Color TertiaryColor;
    }

    public enum Priority
    {
        Primary, Secondary, Tertiary
    }
    
    public enum MapType
    {
        Desert, Snow
    }
    [Serializable]
    public struct MapSetting
    {
        public MapType MapType;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public Color TertiaryColor;
    }
    
    [CreateAssetMenu(fileName = "ColorDatabase", menuName = "Game Data/Color Database")]
    public class TeamColorDatabaseSO : ScriptableObject
    {
        public List<TeamColorConfig> ColorConfigs;
        public List<MapSetting> MapSettings;

        public Color GetTeamColor(TeamColor team, Priority priority)
        {
            foreach (var config in ColorConfigs)
            {
                if (config.Team == team)
                {
                    switch (priority)
                    {
                        case Priority.Primary: return config.PrimaryColor;
                        case Priority.Secondary: return config.SecondaryColor;
                        case Priority.Tertiary: return config.TertiaryColor;
                        default: return config.TertiaryColor;
                    }
                }
            }
            return Color.gray;
        }

        public Color GetMapColor(MapType mapType, Priority priority)
        {
            foreach (var setting in MapSettings)
            {
                if (setting.MapType == mapType)
                {
                    switch (priority)
                    {
                        case Priority.Primary: return setting.PrimaryColor;
                        case Priority.Secondary: return setting.SecondaryColor;
                        case Priority.Tertiary: return setting.TertiaryColor;
                        default: return setting.TertiaryColor;
                    }
                }
            }
            return Color.gray;
        }
    }
}