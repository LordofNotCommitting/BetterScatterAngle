using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Rewired.Demos.CustomPlatform.MyPlatformControllerExtension;
using static UnityEngine.GraphicsBuffer;

namespace BetterScatterAngle
{
    using HarmonyLib;
    using MGSC;
    using System.Collections;
    using UnityEngine;



    [HarmonyPatch(typeof(CreatureData), nameof(CreatureData.GetScatterAngle))]
    public static class ProjectileView_ScatterPenalty_Patch
    {
        public static float Scatter_Penalty_Perc_float;

        [HarmonyPostfix]
        public static void Postfix(BasePickupItem item, bool isShootFromCover, ref float __result)
        {
            __result = (__result * (1f + Scatter_Penalty_Perc_float)) + Plugin.Config.Scatter_Penalty_Flat;
            return;
        }


    }
}
