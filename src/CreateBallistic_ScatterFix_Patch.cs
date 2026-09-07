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
    [HarmonyPatch(typeof(BallisticSystem))]
    public static class CreateBallistic_ScatterFix_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreateBallistic", new System.Type[] {
            typeof(MapGrid),
            typeof(CellPosition),
            typeof(CellPosition),
            typeof(Vector3),
            typeof(float),
            typeof(float),
            typeof(int),
            typeof(int),
            typeof(bool)
        })]

        public static void CreateBallistic_Postfix(MapGrid mapGrid, CellPosition source, CellPosition target, Vector3 sourceWorldPos, float speed, float scatterAngle, int maxDistance, int maxRicochets, bool isPreciseTarget, ref Ballistic __result)
        {
            if (scatterAngle <= 0f) return;


            // 1. Calculate continuous direction vector
            Vector2 sourceCenter = new Vector2(source.X + 0.5f, source.Y + 0.5f);
            Vector2 targetCenter = new Vector2(target.X + 0.5f, target.Y + 0.5f);
            Vector2 direction = (targetCenter - sourceCenter).normalized;

            float angle;
            if (Plugin.Config.Scatter_Gaussian_Calculation)
            {
                // 2-1. Gaussian?
                float stdDev = scatterAngle / Plugin.Config.Scatter_Gaussian_StdDev;
                angle = NextGaussian(0f, stdDev);

                // Clamp to ensure wild outliers don't shoot behind or far past the scatter boundary
                angle = Mathf.Clamp(angle, -scatterAngle, scatterAngle);

            }
            else
            {
                // 2-2. Apply high-precision continuous float scatter angle
                angle = UnityEngine.Random.Range(-scatterAngle, scatterAngle);
            }



            Vector2 scatteredDirection = isPreciseTarget ? direction.Rotate(scatterAngle) : direction.Rotate(angle);


            // 3. Project 100 tiles out
            Vector2 farPos = sourceCenter + (scatteredDirection * (float)Plugin.Config.Scatter_Range_Sample);

            // 4. Overwrite VisionRay's TargetPosition with true continuous grid cell
            __result.Ray.TargetPosition = new CellPosition(
                Mathf.FloorToInt(farPos.x),
                Mathf.FloorToInt(farPos.y)
            );

            //for screenshot
            //__result.PassTileDuration = __result.PassTileDuration * 10;
        }

        public static float NextGaussian(float mean = 0f, float standardDeviation = 1f)
        {
            // Roll two independent uniform random variables (0, 1]
            float u1 = 1f - UnityEngine.Random.value;
            float u2 = 1f - UnityEngine.Random.value;

            // Box-Muller transform equation
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);

            // Scale by standard deviation and shift by mean
            return mean + standardDeviation * randStdNormal;
        }


    } 
}
