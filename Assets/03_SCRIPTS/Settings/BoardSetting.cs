using System;
using UnityEngine;

namespace MADP.Settings
{
    [Serializable]
    public struct BoardSetting
    {
        [Header("--- SPEACIAL CELL SETTING ---")]
        public int RedCellCount;
        public int YellowCellCount;
        public int PurpleCellCount;
    }
}