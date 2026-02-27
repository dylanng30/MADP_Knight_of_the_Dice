using System.Collections.Generic;
using UnityEngine;

namespace MADP.Settings
{
    [CreateAssetMenu(fileName = "ShopDatabaseSO", menuName = "MADP/ShopDatabaseSO")]
    public class ShopDatabaseSO : ScriptableObject
    {
        public List<ItemDataSO> Items = new List<ItemDataSO>();
    }
}