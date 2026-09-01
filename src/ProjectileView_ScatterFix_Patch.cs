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
    using UnityEngine;


    [HarmonyPatch(typeof(ProjectileView))]
    public static class ProjectileView_ScatterFix_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CaculateDestination", new System.Type[] {
            typeof(Ballistic),
            typeof(CellPosition),
            typeof(int),
            typeof(float),
            typeof(Vector3)
        }, new ArgumentType[] {
            ArgumentType.Normal,
            ArgumentType.Normal,
            ArgumentType.Normal,
            ArgumentType.Out,
            ArgumentType.Out
        })]
        public static void CaculateDestination_Postfix( ProjectileView __instance, Ballistic ballistic, CellPosition shooterPos, int range, ref float flyDuration, ref Vector3 destination)
        {
            //remove those cell centering logic and re-display bullet trajectory.
            VisionRay ray = ballistic.Ray;

            // 1. Calculate continuous ray direction
            Vector2 startGrid = new Vector2(ray.StartPosition.X + 0.5f, ray.StartPosition.Y + 0.5f);
            Vector2 targetGrid = new Vector2(ray.TargetPosition.X + 0.5f, ray.TargetPosition.Y + 0.5f);
            Vector3 rayDirection = new Vector3(targetGrid.x - startGrid.x, targetGrid.y - startGrid.y, 0f).normalized;

            if (rayDirection == Vector3.zero) return;

            // 2. Obtain bullet start position in world space
            Vector3 startWorldPos = SingletonMonoBehaviour<BulletFactory>.Instance.GetWorldPos(shooterPos, ray.StartPosition);

            // 3. Extrapolate destination along exact ray direction
            float distance = Vector3.Distance(startWorldPos, destination);
            destination = startWorldPos + (rayDirection * distance);
        }
    }

    /*

    [HarmonyPatch(typeof(ProjectileView))]
    public static class ProjectileView_ScatterFix_Patch
    {

        [HarmonyPostfix]
        [HarmonyPatch("CaculateDestination", new System.Type[] { typeof(Ballistic), typeof(CellPosition), typeof(int), typeof(float), typeof(Vector3) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
        public static void CaculateDestination_Postfix(
            ProjectileView __instance,
            Ballistic ballistic,
            CellPosition shooterPos,
            int range,
            ref float flyDuration,
            ref Vector3 destination)
        {
            VisionRay ray = ballistic.Ray;

            // 1. Calculate continuous start/target positions using float grid math
            Vector2 startGrid = new Vector2(ray.StartPosition.X + 0.5f, ray.StartPosition.Y + 0.5f);
            Vector2 targetGrid = new Vector2(ray.TargetPosition.X + 0.5f, ray.TargetPosition.Y + 0.5f);

            // 2. Derive true continuous direction vector
            Vector3 rayDirection = new Vector3(targetGrid.x - startGrid.x, targetGrid.y - startGrid.y, 0f).normalized;

            if (rayDirection == Vector3.zero) return;

            // 3. Get shooter's actual world position from the transform/factory
            Vector3 startWorldPos = SingletonMonoBehaviour<BulletFactory>.Instance.GetWorldPos(shooterPos, ray.StartPosition);

            // 4. Measure distance to original destination, then project along rayDirection
            float distance = Vector3.Distance(startWorldPos, destination);
            Vector3 originalDest = destination;

            destination = startWorldPos + (rayDirection * distance);

            // Debug Log to verify in BepInEx Console
            Plugin.Logger.Log($"[ScatterFix] Orig Dest: {originalDest} | New Dest: {destination} | Dir: {rayDirection}");
        }
    }
    */
}
