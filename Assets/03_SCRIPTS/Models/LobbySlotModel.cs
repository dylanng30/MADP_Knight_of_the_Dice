using System;
using MADP.Settings;

namespace MADP.Models
{
    public enum PlayerType
    {
        Empty, Human, Bot
    }

    public enum RoleType
    {
        Random, Attacker, Defender, Speller, Miner
    }
    [Serializable]
    public class LobbySlotModel
    {
        public int SlotIndex;
        public TeamColor TeamColor;
        public PlayerType PlayerType;
        public RoleType RoleType;
        public BotType BotType;
        public string PlayerName;
        public string AvatarPath;
        public bool IsHost = false;
        
        public bool HasPlayer => PlayerType != PlayerType.Empty;

        public LobbySlotModel(int index, TeamColor color)
        {
            SlotIndex = index;
            TeamColor = color;
            PlayerType = PlayerType.Empty;
            PlayerName = "Empty";
            RoleType = RoleType.Random;
            BotType = SlotIndex % 2 == 0 ? BotType.Easy : BotType.Medium;
        }
    }
}