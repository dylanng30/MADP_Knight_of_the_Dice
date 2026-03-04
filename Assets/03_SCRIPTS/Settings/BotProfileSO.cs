using UnityEngine;

namespace MADP.Settings
{
    [CreateAssetMenu(fileName = "NewBotProfile", menuName = "MADP/Settings/Bot Profile")]
    public class BotProfileSO : ScriptableObject
    {
        //Điểm cộng khi đá được kẻ địch
        public float WeightKick = 500f;
        //Điểm cộng khi đứng ở ô an toàn
        public float WeightSafe = 100f;
        //Điểm cộng khi đi vào chuồng đích
        public float WeightHome = 300f;
        //Điểm cộng cho mỗi bước tiến gần về đích
        public float WeightDistance = 10f;
        //Điểm trừ khi có nguy cơ bị kẻ địch đá ở lượt sau
        public float WeightDanger = 200f;
    }
}