//using HarmonyLib;
//using MGSC;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using static Rewired.Demos.CustomPlatform.MyPlatformControllerExtension;
//using static UnityEngine.GraphicsBuffer;

//namespace BetterScatterAngle
//{
//    using HarmonyLib;
//    using MGSC;
//    using System.Reflection;
//    using System.Reflection.Emit;
//    using UnityEngine;

//    //fix fugly bullet marks on the wall
//    [HarmonyPatch(typeof(BallisticProjectileView), "Update")]
//    public static class BallisticProjectileView_DecalOffset_Transpiler
//    {
//        public static Vector3 ModifyDecalPosition(Vector3 originalPos, Vector3 startPos, Vector3 destPos)
//        {
//            Vector3 flightDir = (destPos - startPos).normalized;

//            // 1. Pull back slightly along the flight path
//            Vector3 nudgedPos = originalPos - (flightDir * 0.05f);

//            // 2. Add a tiny vertical offset (+Y) to align with wall face perspective
//            nudgedPos.y += 0.08f;

//            return nudgedPos;
//        }



//        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//        {
//            var codes = new List<CodeInstruction>(instructions);

//            FieldInfo startField = typeof(BallisticProjectileView).GetField("_start", BindingFlags.NonPublic | BindingFlags.Instance);
//            FieldInfo destField = typeof(BallisticProjectileView).GetField("_destination", BindingFlags.NonPublic | BindingFlags.Instance);
//            MethodInfo modifyMethod = typeof(BallisticProjectileView_DecalOffset_Transpiler).GetMethod(nameof(ModifyDecalPosition));

//            // Fix: Explicitly define parameter types to resolve AmbiguousMatchException
//            MethodInfo bakeSpriteMethod = AccessTools.Method(
//                typeof(GibsController),
//                "BakeSprite",
//                new Type[] { typeof(Sprite), typeof(Vector3), typeof(Vector2), typeof(bool) }
//            );

//            for (int i = 0; i < codes.Count; i++)
//            {
//                if (codes[i].Calls(bakeSpriteMethod))
//                {
//                    for (int j = i - 1; j >= 0; j--)
//                    {
//                        if (codes[j].opcode == OpCodes.Ldloc_1)
//                        {
//                            var injection = new List<CodeInstruction>
//                        {
//                            new CodeInstruction(OpCodes.Ldarg_0),
//                            new CodeInstruction(OpCodes.Ldfld, startField),
//                            new CodeInstruction(OpCodes.Ldarg_0),
//                            new CodeInstruction(OpCodes.Ldfld, destField),
//                            new CodeInstruction(OpCodes.Call, modifyMethod)
//                        };

//                            codes.InsertRange(j + 1, injection);
//                            Debug.Log("[BetterScatterAngle] Transpiler patch applied successfully.");
//                            return codes;
//                        }
//                    }
//                }
//            }

//            Debug.LogError("[BetterScatterAngle] Failed to locate target IL pattern for Decal Offset Transpiler.");
//            return codes;
//        }
//    }

//}
