using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MADP.Models;

namespace MADP.Utilities
{
    public static class EnumUtils
    {
        public static RoleType GetRandomRoleConcrete()
        {
            var values = Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Where(r => r != RoleType.Random)
                .ToArray();

            if (values.Length == 0) return RoleType.Attacker;

            return values[UnityEngine.Random.Range(0, values.Length)];
        }
    }
}
