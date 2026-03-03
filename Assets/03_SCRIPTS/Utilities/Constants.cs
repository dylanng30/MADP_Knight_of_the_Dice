using UnityEngine;

namespace MADP.Utilities
{
    public class Constants
    {
        public const string PrimaryMaterial = "Primary";
        public const string SecondaryMaterial = "Secondary";
        public const string TertiaryMaterial = "Tertiary";
        
        public const string CellView = "Cell";
        public const string UnitView = "Unit";

        public const int InitialGold = 1;

        public const int AroundCellCount = 64;
        public const int CellCountPerTeam = AroundCellCount / 4;

        public const string GoldIconPath = "GoldIcon";
        public const string MythIconPath = "MythIcon";
        public const string HealIconPath = "HealIcon";
        public const string HarmIconPath = "HarmIcon";

        public const float UnitSpeed = 10f;
    }
}