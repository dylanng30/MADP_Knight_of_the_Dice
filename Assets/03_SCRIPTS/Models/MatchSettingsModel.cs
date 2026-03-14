using System;
using MADP.Settings;

namespace MADP.Models
{
    public class MatchSettingsModel
    {
        public MapType SelectedMap = MapType.Desert;
        public int TimePerTurn = 30;
        public int RedCellCount = 1;
        public int YellowCellCount = 1;
        public int PurpleCellCount = 1;
        public int GreenCellCount = 1;

        public bool IsTutorial = false;
        public int TutorialStageIndex = 0;

        public LobbySlotModel[] Slots;
    }
}