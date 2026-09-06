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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [HarmonyPatch(typeof(BallisticSystem), nameof(BallisticSystem.Update))]
    public static class BallisticSystemHighLevelOverridePatch
    {
        // =========================================================================
        // TUNING CONFIGURATION
        // 0.5f = Exact tile boundary (bullet ray must pass within 0.5 tiles of center)
        // 0.4f = Tighter/stricter hitbox
        // 0.6f = Slightly more forgiving hitbox
        // =========================================================================
        public static float HIT_DISTANCE_THRESHOLD = Plugin.Config.Hitbox_Base_Target_Size;

        [HarmonyPrefix]
        public static bool Prefix()
        {

            if (!Plugin.Config.Hitbox_Base_Targetting) return true; // Fallback to vanilla when disabled

            // 1. Fetch static cache fields via Harmony Reflection
            var ballisticsField = AccessTools.Field(typeof(BallisticSystem), "_ballistics");
            var mapGrid = AccessTools.Field(typeof(BallisticSystem), "_cacheMapGrid")?.GetValue(null) as MapGrid;
            var creatures = AccessTools.Field(typeof(BallisticSystem), "_cacheCreatures")?.GetValue(null) as Creatures;

            if (ballisticsField == null || mapGrid == null || creatures == null) return true; // Fallback to vanilla if reflection fails

            //how static field can be handled.
            var ballistics = ballisticsField.GetValue(null) as IList;
            if (ballistics == null || ballistics.Count == 0)
                return false;

            float deltaTime = Time.deltaTime;

            // Fetch reflection methods once to avoid overhead inside loop
            var nextMethod = AccessTools.Method(typeof(BallisticSystem), "Next");
            var collisionWithCreatureMethod = AccessTools.Method(typeof(BallisticSystem), "CollisionWithCreature");
            var collisionWithWallMethod = AccessTools.Method(typeof(BallisticSystem), "CollisionWithWall");

            // 2. Iterate backwards through active ballistics (to safely handle removals)
            for (int i = ballistics.Count - 1; i >= 0; i--)
            {
                // Retrieve reference or unbox current struct
                object rawBallistic = ballistics[i];
                if (rawBallistic == null) continue;

                // Extract needed fields via Reflection
                var timeField = AccessTools.Field(rawBallistic.GetType(), "Time");
                var passTileDurationField = AccessTools.Field(rawBallistic.GetType(), "PassTileDuration");
                var previousPositionField = AccessTools.Field(rawBallistic.GetType(), "PreviousPosition");
                var currentPositionField = AccessTools.Field(rawBallistic.GetType(), "CurrentPosition");
                var reachedMaxField = AccessTools.Field(rawBallistic.GetType(), "ReachedMax");

                float time = (float)timeField.GetValue(rawBallistic);
                float passTileDuration = (float)passTileDurationField.GetValue(rawBallistic);

                time += deltaTime;

                bool removeProjectile = false;

                while (time >= passTileDuration)
                {
                    time -= passTileDuration;
                    timeField.SetValue(rawBallistic, time);

                    // Re-box struct back into the list before calling Next
                    ballistics[i] = rawBallistic;

                    // Execute native Next step (handles ray movement, walls, and ricochets)
                    object[] nextParams = new object[] { mapGrid, ballistics[i], false };
                    bool nextResult = (bool)nextMethod.Invoke(null, nextParams);

                    // Update local boxed reference with modified struct from Next
                    rawBallistic = ballistics[i];
                    bool hitWall = (bool)nextParams[2];

                    if (!nextResult)
                    {
                        // Projectile exited map bounds
                        removeProjectile = true;
                        break;
                    }

                    CellPosition prevPos = (CellPosition)previousPositionField.GetValue(rawBallistic);
                    CellPosition currPos = (CellPosition)currentPositionField.GetValue(rawBallistic);
                    bool reachedMax = (bool)reachedMaxField.GetValue(rawBallistic);

                    // 3. Custom Ray-Distance Creature Check
                    Creature hitCreature = FindCreatureOnRaySegment(prevPos, currPos, creatures);
                    bool hitDetected = false;

                    if (hitCreature != null)
                    {
                        collisionWithCreatureMethod?.Invoke(null, new object[] { rawBallistic, hitCreature });
                        hitDetected = true;
                    }

                    // 4. Handle Wall, Max Distance, or Creature Collision Termination
                    if (hitWall || reachedMax || hitDetected)
                    {
                        if (hitWall && !hitDetected)
                        {
                            collisionWithWallMethod?.Invoke(null, new object[] { rawBallistic });
                        }

                        removeProjectile = true;
                        break;
                    }
                }

                if (removeProjectile)
                {
                    ballistics.RemoveAt(i);
                }
                else
                {
                    // Write updated time back to active list
                    timeField.SetValue(rawBallistic, time);
                    ballistics[i] = rawBallistic;
                }
            }

            // Return false to skip original ProcessBallistic execution
            return false;
        }

        private static Creature FindCreatureOnRaySegment(CellPosition start, CellPosition end, Creatures creatures)
        {
            Vector2 p1 = new Vector2(start.X + 0.5f, start.Y + 0.5f);
            Vector2 p2 = new Vector2(end.X + 0.5f, end.Y + 0.5f);

            // Bounding box sweep around segment
            int minX = Math.Min(start.X, end.X) - 1;
            int maxX = Math.Max(start.X, end.X) + 1;
            int minY = Math.Min(start.Y, end.Y) - 1;
            int maxY = Math.Max(start.Y, end.Y) + 1;

            Creature closestCreature = null;
            float closestDistance = float.MaxValue;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    CellPosition candidatePos = new CellPosition(x, y);
                    Creature creature = creatures.GetCreature(candidatePos);

                    if (creature == null) continue;

                    Vector2 creatureCenter = new Vector2(x + 0.5f, y + 0.5f);
                    float distToSegment = GetDistanceToSegment(creatureCenter, p1, p2);

                    // Validate distance ratio against HIT_DISTANCE_THRESHOLD
                    if (distToSegment <= HIT_DISTANCE_THRESHOLD && distToSegment < closestDistance)
                    {
                        closestDistance = distToSegment;
                        closestCreature = creature;
                    }
                }
            }

            return closestCreature;
        }

        private static float GetDistanceToSegment(Vector2 point, Vector2 segStart, Vector2 segEnd)
        {
            Vector2 segment = segEnd - segStart;
            float lengthSq = segment.sqrMagnitude;

            if (lengthSq == 0f) return Vector2.Distance(point, segStart);

            // Clamped projection factor ensuring distance is measured along actual frame step
            float t = Mathf.Clamp01(Vector2.Dot(point - segStart, segment) / lengthSq);
            Vector2 projection = segStart + t * segment;

            return Vector2.Distance(point, projection);
        }
    }
}
