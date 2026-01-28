using MADP.Settings;
using UnityEngine;

namespace _03_SCRIPTS.SO
{
    [CreateAssetMenu(fileName = "TeamData", menuName = "SO")]
    public class TeamData : ScriptableObject
    {
        public TeamStyle teamStyle;
        public TeamSetting[] settings = new TeamSetting[4];
    }
}