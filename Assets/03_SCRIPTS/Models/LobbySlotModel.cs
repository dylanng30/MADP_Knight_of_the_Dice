using System;
using MADP.Models.Inventory;
using MADP.Settings;

namespace MADP.Models
{
    public enum PlayerType
    {
        Empty,
        Human,
        Bot,
        MLAgent
    }

    public enum RoleType
    {
        Random,
        Attacker,
        Defender,
        Speller,
        Miner
    }

    [Serializable]
    public class LobbySlotModel
    {
        public int SlotIndex;
        public TeamColor TeamColor;
        public PlayerType PlayerType;
        public RoleType RoleType;
        public BotDifficulty BotType;
        public string PlayerName;
        public string AvatarPath;
        public bool IsHost = false;

        public PlayerInventoryModel Inventory { get; private set; }

        public bool HasPlayer => PlayerType != PlayerType.Empty;

        public LobbySlotModel(int index, TeamColor color)
        {
            SlotIndex = index;
            TeamColor = color;
            PlayerType = PlayerType.Empty;
            PlayerName = "Empty";
            RoleType = RoleType.Random;
            BotType = BotDifficulty.Medium;
            Inventory = new PlayerInventoryModel(color);
        }
    }
}