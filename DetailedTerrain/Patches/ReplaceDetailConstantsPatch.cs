namespace DetailedTerrain.Patches {
    using HarmonyLib;
    using JetBrains.Annotations;
    using KianCommons;
    using KianCommons.Patches;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using static KianCommons.ReflectionHelpers;
    using static DetailedTerrain.Patches.CodeInstructionExtensions;

    /* constants overview:
     * Map size: 9 x 9 tiles => center at (4.5, 4.5) tiles
     * Tile size: 1920m
     * Pixels per terrain patch: default: 120, detailed 480
     * Texture size per terrain patch: default 128, detailed 512
     * Margin pixels: default 4, detailed 16 (each side)
     * meters per pixel: default 16m, detailed 4m
     * ratio: 4, inverse 0.25
     * 
     * sometimes << 2 is used instead of *4
     * 
     * some of these are visible as constants on TerrainManager
     * 
     * note the conflict between ratio and metres per pixel (4); often this can be bypassed by distinguishing on data type
     */
    [UsedImplicitly]
    [HarmonyPatch]
#if DEBUG
    [HarmonyDebug]
#endif
    [HarmonyPriority(Priority.Last)]
    class ReplaceTerrainManagerDetailConstantsPatch {
        static T DebugValue<T>(T value) {
#if DEBUG
            UnityEngine.Debug.Log(value);
#endif
            return value;
        }
        static Dictionary<MethodBase, Dictionary<int, int>> intMap;
        static Dictionary<MethodBase, Dictionary<float, float>> floatMap;
        static void InitMaps() {
            int dp = DetailedTerrain.GUI.ModSettings.settings.detailedMeshPower;
            //int f = 1 << p
            float df = UnityEngine.Mathf.Pow(2, dp);
            intMap = new() {
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), "Awake")),
                    new() {
                        { 481 * 481, (int)((480 * df + 1) * (480 * df + 1)) },
                        { 480 * 480, (int)(480 * df * 480 * df) },
                        { 513 * 513, (int)((512 * df + 1) * (512 * df + 1)) },
                        { 4, 4 + dp }, // patch rendering levels
                        { 5, 5 + dp } // patch rendering levels
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetBlockHeight))),
                    new() {
                        { 480, (int)(480 * df) },
                        { 481, (int)(480 * df + 1) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetDetailHeight))),
                    new() {
                        { 480, (int)(480 * df) },
                        { 481, (int)(480 * df + 1) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetDetailHeightSmooth))),
                    new() {
                        { 480, (int)(480 * df) },
                        { 481, (int)(480 * df + 1) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetSurfaceCell))),
                    new() {
                        { 480, (int)(480 * df) },
                        { 479, (int)(480 * df - 1) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetZoneCell))),
                    new() {
                        { 480, (int)(480 * df) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeight), new[] { typeof(float), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() })),
                    new() {
                        { 480 * 9, (int)(480 * df * 9) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeight), new[] { typeof(float), typeof(float) })),
                    new() {
                        { 480 * 9, (int)(480 * df * 9) },
                        { 480, (int)(480 * df) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeightSmooth), new[] { typeof(float), typeof(float) })),
                    new() {
                        { 480 * 9, (int)(480 * df * 9) },
                        { 480, (int)(480 * df) }
                    }
                },
                {
                    // see also the separate patch!
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailSurface), new[] { typeof(float), typeof(float) })),
                    new() {
                        { 480 * 9, (int)(480 * df * 9) },
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SetDetailedPatch), new[] { typeof(int), typeof(int) })),
                    new() {
                        { 481 * 481, (int)((480 * df + 1) * (480 * df + 1)) },
                        { 480 * 480, (int)(480 * df * 480 * df) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.UpdateBounds))),
                    new() {
                        { 4, 4 + dp }, // patch rendering levels
                        { 144, (int)(144 * df)} //?
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainModify), nameof(TerrainModify.ApplyQuad), new[] { typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(ItemClass.Zone), typeof(bool), typeof(float), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(int), typeof(int), typeof(int), typeof(int) })),
                    new() {
                        { 2, 2 + dp }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainModify), nameof(TerrainModify.ApplyQuad), new[] { typeof(ColossalFramework.Math.Quad2), typeof(TerrainModify.Edges), typeof(TerrainModify.Surface) })),
                    new() {
                        { 2, 2 + dp }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainModify), "UpdateAreaImplementation")),
                    new() {
                        { 2, 2 + dp },
                        { 4, (int)(4 * df) },
                        { 480, (int)(480 * df) },
                        { 479, (int)(480 * df - 1) },
                        { 481, (int)(480 * df + 1) }
                    }
                },
                {
                    DebugValue(AccessTools.Constructor(typeof(TerrainPatch), new Type[] { typeof(int), typeof(int) })),
                    new() {
                        { 480, (int)(480 * df) },
                        { 512, (int)(512 * df) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainPatch), nameof(TerrainPatch.Refresh))),
                    new() { }// needs some specific handling as 16 is used both for pixel margin and color channel splitting
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainPatch), nameof(TerrainPatch.ResizeControlTextures))),
                    new() {
                        { 512, (int)(512 * df) }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainPatch), "RenderSubPatchOverlay")),
                    new() {
                        { 3, 3 + dp }
                    }
                }
            };
            floatMap = new() {
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetBlockHeight))),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetDetailHeight))),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetDetailHeightSmooth))),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetSurfaceCell))),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeight), new[] { typeof(UnityEngine.Vector3) })),
                    new() {
                        { 4f, 4f / (float)df },
                        { 480f * 4.5f, 480f * (float)df * 4.5f },
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeight), new[] { typeof(UnityEngine.Vector3), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() })),
                    new() {
                        { 4f, 4f / (float)df },
                        { 480f * 4.5f, 480f * (float)df * 4.5f },
                        { 1f / 256f, 1f / 256f * (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeight), new[] { typeof(float), typeof(float) })),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeightSmooth), new[] { typeof(UnityEngine.Vector3) })),
                    new() {
                        { 4f, 4f / (float)df },
                        { 480f * 4.5f, 480f * (float)df * 4.5f },
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailHeightSmooth), new[] { typeof(float), typeof(float) })),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailSurface), new[] { typeof(UnityEngine.Vector3) })),
                    new() {
                        { 4f, 4f / (float)df },
                        { 480f * 4.5f, 480f * (float)df * 4.5f },
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainModify), nameof(TerrainModify.ApplyQuad), new[] { typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(ItemClass.Zone), typeof(bool), typeof(float), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(UnityEngine.Vector2), typeof(int), typeof(int), typeof(int), typeof(int) })),
                    new() {
                        { 4f, 4f / (float)df },
                        { 480f * 4.5f, 480f * (float)df * 4.5f },
                        { 4f * 0.5f, 4f * 0.5f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainModify), nameof(TerrainModify.ApplyQuad), new[] { typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector3), typeof(TerrainModify.Edges), typeof(TerrainModify.Heights), typeof(float[]) })),
                    new() { }// needs some specific handling as 4f is used both for coordinate mapping and height scaling
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainModify), nameof(TerrainModify.ApplyQuad), new[] { typeof(ColossalFramework.Math.Quad2), typeof(TerrainModify.Edges), typeof(TerrainModify.Surface) })),
                    new() {
                        { 4f, 4f / (float)df },
                        { 480f * 4.5f, 480f * (float)df * 4.5f },
                        { 4f * 0.5f, 4f * 0.5f / (float)df },
                        { 5.5f, 4f * UnityEngine.Mathf.Sqrt(2) / (float)df }, // sth regarding clipping paint, TODO: investigate, other paints use 4f; maybe sth like 4*sqrt(2)?
                        { 8f, 8f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TerrainModify), "UpdateAreaImplementation")),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Constructor(typeof(TerrainPatch), new Type[] { typeof(int), typeof(int) })),
                    new() {
                        { 4f, 4f / (float)df },
                        { 1f / 512f, 1f / 512f / (float)df },
                        { 1f / 512f / 2f, 1f / 512f / 2f / (float)df },
                        { 480f, 480f * (float)df },
                        { 512f, 512f * (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(TreeInstance), nameof(TreeInstance.TerrainUpdated), new Type[] { typeof(TreeInfo), typeof(UnityEngine.Vector3) })),
                    new() {
                        { 4.5f, 0.5f + 4f / (float)df },
                        { -4.5f, -0.5f - 4f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(PropInstance), nameof(PropInstance.TerrainUpdated), new Type[] { typeof(PropInfo), typeof(ushort), typeof(UnityEngine.Vector3), typeof(float) })),
                    new() {
                        { 4.5f, 0.5f + 4f / (float)df },
                        { -4.5f, -0.5f - 4f / (float)df }
                    }
                },
                {
                    DebugValue(AccessTools.Method(typeof(Building), nameof(Building.TerrainUpdated), new Type[] { typeof(BuildingInfo), typeof(ushort), typeof(UnityEngine.Vector3), typeof(float), typeof(int), typeof(int), typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool) })),
                    new() {
                        { 4.5f, 0.5f + 4f / (float)df },
                        { -4.5f, -0.5f - 4f / (float)df }
                    }
                }
            };
        }
        internal static IEnumerable<MethodBase> TargetMethods() {
            InitMaps();
            var targetMethods = new HashSet<MethodBase>();
            targetMethods.AddRange(intMap.Keys);
            targetMethods.AddRange(floatMap.Keys);
            return targetMethods;
        }
        [HarmonyPriority(Priority.Last)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codesEnumerable, MethodBase original, ILGenerator generator) {
            Log.Debug("Transpiling " + original.FullDescription());
#if DEBUG
            var intReplacementCheck = new Dictionary<int, int>();
            if (intMap.ContainsKey(original)) foreach (var origValue in intMap[original].Keys) intReplacementCheck[origValue] = 0;
            var floatReplacementCheck = new Dictionary<float, int>();
            if (floatMap.ContainsKey(original)) foreach (var origValue in floatMap[original].Keys) floatReplacementCheck[origValue] = 0;
#endif
            var codes = codesEnumerable.ToList();
            for (int i = 0; i < codes.Count; i++) {
                var code = codes[i];
                bool replaced = false;
                if (code.LoadsConstant()) {
                    if (intMap.ContainsKey(original)) {
                        foreach (int origValue in intMap[original].Keys) {
                            if (code.LoadsConstant(origValue)) {
                                // (int) 2 only ever needs to replaced when used in a shift
                                if (origValue == 2) {
                                    if (codes[i + 1].opcode != OpCodes.Shl && codes[i + 1].opcode != OpCodes.Shr && codes[i + 1].opcode != OpCodes.Shr_Un) {
                                        break;
                                    }
                                }
#if DEBUG
                                intReplacementCheck[origValue]++;
#endif
                                yield return CodeInstructionExtensions.LoadConstant(intMap[original][origValue]);
                                replaced = true;
                                break;
                            }
                        }
                    }

                    if (replaced) continue;
                    if (floatMap.ContainsKey(original)) {
                        foreach (float origValue in floatMap[original].Keys) {
                            if (code.LoadsConstant(origValue)) {
#if DEBUG
                                floatReplacementCheck[origValue]++;
#endif
                                yield return CodeInstructionExtensions.LoadConstant(floatMap[original][origValue]);
                                replaced = true;
                                break;
                            }
                        }
                    }
                    if (replaced) continue;
                }
                yield return code;
            }
#if DEBUG
            Log.Debug("replacement stats:");
            foreach (var origValue in intReplacementCheck.Keys) {
                Log.Debug("replaced " + origValue + " with " + intMap[original][origValue] + " at " + intReplacementCheck[origValue] + " occurences.");
            }
            foreach (var origValue in floatReplacementCheck.Keys) {
                Log.Debug("replaced " + origValue + " with " + floatMap[original][origValue] + " at " + floatReplacementCheck[origValue] + " occurences.");
            }
#endif
        }
        static void LogInt(int value) {
            UnityEngine.Debug.Log(value);
        }
    }
    [UsedImplicitly]
    [HarmonyPatch]
#if DEBUG
    [HarmonyDebug]
#endif
    [HarmonyPriority(Priority.Last)]
    class ReplaceTerrainmodifyApplyQuadHeightsDetailConstantsPatch {
        static T DebugValue<T>(T value) {
#if DEBUG
            UnityEngine.Debug.Log(value);
#endif
            return value;
        }
        static IEnumerable<MethodBase> TargetMethods() {
            yield return DebugValue(AccessTools.Method(typeof(TerrainModify), nameof(TerrainModify.ApplyQuad), new[] { typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector3), typeof(UnityEngine.Vector3), typeof(TerrainModify.Edges), typeof(TerrainModify.Heights), typeof(float[]) }));
        }
        [HarmonyPriority(Priority.Last)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codesEnumerable) {
            Log.DebugCalled();
            var codes = codesEnumerable.ToList();
            int dp = DetailedTerrain.GUI.ModSettings.settings.detailedMeshPower;
            //int f = 1 << p;
            float df = UnityEngine.Mathf.Pow(2, dp);
            for (int i = 0; i < codes.Count; i++) {
                var code = codes[i];
                // / 4f : used to scale from world to detailed coords^; involved in sdf computation
                if (code.Is(OpCodes.Ldc_R4, 4f)) {
                    code.operand = 4f / (float)df;
                    // also used to scale some stuff in height modification, keep those
                    if (codes[i + 1].opcode == OpCodes.Mul) {
                        if (codes[i + 2].opcode == OpCodes.Add) {
                            if (codes[i + 3].StoresField(typeof(TerrainModify.HeightModification).GetField("m_target"))) {
                                code.operand = 4f;
                            }
                        }
                    }
                    if (codes[i + 1].opcode == OpCodes.Add) {
                        if (codes[i + 2].StoresField(typeof(TerrainModify.HeightModification).GetField("m_divider"))) {
                            code.operand = 4f;
                        }
                    }
                    if (codes[i + 1].opcode == OpCodes.Mul) {
                        if (codes[i + 2].Calls(GetMethod(typeof(UnityEngine.Mathf), "Max", ALL, new Type[] { typeof(float), typeof(float) }))) {
                            code.operand = 4f;
                        }
                    }

                }
                // 2160f : map center in detailed coords
                else if (code.Is(OpCodes.Ldc_R4, 2160f)) {
                    code.operand = 2160f * (float)df;
                }
                //4 * sqrt(2)
                else if (code.Is(OpCodes.Ldc_R4, 5.656854f)) {
                    code.operand = 5.656854f / (float)df;
                }
                // 2 : left- and right-shift used instead of *4 and /4 to translate between detailed and undetailed coords
                else if (code.opcode == OpCodes.Ldc_I4_2) {
                    if (codes[i + 1].opcode == OpCodes.Shl || codes[i + 1].opcode == OpCodes.Shr || codes[i + 1].opcode == OpCodes.Shr_Un) {
                        code = CodeInstructionExtensions.LoadConstant(2 + dp);
                    }
                }

                yield return code;
            }
        }
    }
    [UsedImplicitly]
    [HarmonyPatch]
#if DEBUG
    [HarmonyDebug]
#endif
    [HarmonyPriority(Priority.Last)]
    class ReplaceTerrainPatchRefreshDetailConstantsPatch {
        static T DebugValue<T>(T value) {
#if DEBUG
            UnityEngine.Debug.Log(value);
#endif
            return value;
        }
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(TerrainPatch), "Refresh");
        }
        [HarmonyPriority(Priority.Last)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codesEnumerable) {
            Log.DebugCalled();
            var codes = codesEnumerable.ToList();
            int dp = DetailedTerrain.GUI.ModSettings.settings.detailedMeshPower;
            //int f = 1 << p;
            float df = UnityEngine.Mathf.Pow(2, dp);
            for (int i = 0; i < codes.Count; i++) {
                var code = codes[i];
                // 480: default detail mesh size per tile
                if (code.Is(OpCodes.Ldc_I4, 480)) {
                    code.operand = (int)(480 * df);
                }
                 // 0.001953125f : 1/512 somehow related to normals scaling
                 else if (code.Is(OpCodes.Ldc_R4, 0.001953125f)) {
                    code.operand = 1f / 512f * (float)df;
                }
                 // 256: half texture size
                 else if (code.Is(OpCodes.Ldc_I4, 256)) {
                    code.operand = (int)(256 * df);
                }
                 // 240: half patch size
                 else if (code.Is(OpCodes.Ldc_I4, 240)) {
                    code.operand = (int)(240 * df);
                }
                 // 2 : left- and right-shift used instead of *4 and /4 to translate between detailed and undetailed coords in TerrainPatch::Refresh
                 else if (code.opcode == OpCodes.Ldc_I4_2) {
                    if (codes[i + 1].opcode == OpCodes.Shl || codes[i + 1].opcode == OpCodes.Shr || codes[i + 1].opcode == OpCodes.Shr_Un) {
                        code = CodeInstructionExtensions.LoadConstant(2 + dp);
                    }
                }
                 // 4320: map size in detailed coords
                 else if (code.Is(OpCodes.Ldc_I4, 4320)) {
                    code.operand = (int)(4320 * df);
                } else if (code.Is(OpCodes.Ldc_I4, 4320 - 1)) {
                    code.operand = (int)(4320 * df - 1);
                }
                /*
                 * additional:
                 * - TerrainPatch::Refresh:
                 *   - num9 is padding around texture
                 *     - first set to (128 - num) >> 1 = (128 - 120) >> 1
                 *     - then (in detailed section) set to 16 = (512 - 420) >> 1
                 *     - need to replace those specific 16
                 *     - both times directly after an if that checks for m_rndDetailIndex
                 */
                else if (code.Is(OpCodes.Ldc_I4_S, 16)) {
                    if (codes[i - 1].opcode == OpCodes.Brfalse) {
                        if (codes[i - 2].LoadsField(typeof(TerrainPatch).GetField("m_rndDetailIndex"))) {
                            code.opcode = OpCodes.Ldc_I4;
                            code.operand = (int)(16 * df);
                        }
                    }
                }
                yield return code;
            }
        }
    }
    [UsedImplicitly]
    [HarmonyPatch]
#if DEBUG
    [HarmonyDebug]
#endif
    [HarmonyPriority(Priority.Last)]
class ReplaceTerrainPatchRenderSubPatchDetailConstantsPatch {
        static T DebugValue<T>(T value) {
#if DEBUG   
            UnityEngine.Debug.Log(value);
#endif
            return value;
        }
        static IEnumerable<MethodBase> TargetMethods() {
            yield return DebugValue(AccessTools.Method(typeof(TerrainPatch), "RenderSubPatch"));
        }
        [HarmonyPriority(Priority.Last)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codesEnumerable, MethodBase method) {
            Log.DebugCalled();
            var codes = codesEnumerable.ToList();
            int dp = DetailedTerrain.GUI.ModSettings.settings.detailedMeshPower;
            //int f = 1 << p;
            float df = UnityEngine.Mathf.Pow(2, dp);
            for (int i = 0; i < codes.Count; i++) {
                var code = codes[i];
                // 480: default detail mesh size per tile
                if (code.LoadsConstant(3)) {
                   code = CodeInstructionExtensions.LoadConstant(3 + dp);
                }else if(code.LoadsConstant(4) && codes[i+1].IsLdarg(method, "level")){
                    code = CodeInstructionExtensions.LoadConstant(4 + dp);
                }
                yield return code;
            }
        }
    }
}