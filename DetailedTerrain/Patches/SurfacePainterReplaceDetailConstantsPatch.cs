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
    using UnityEngine;

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
    class SurfacePainterReplaceDetailConstantsPatch {
        static T DebugValue<T>(T value) {
#if DEBUG
            UnityEngine.Debug.Log(value);
#endif
            return value;
        }
        static Dictionary<MethodBase, Dictionary<int, int>> intMap;
        static Dictionary<MethodBase, Dictionary<float, float>> floatMap;
        static Dictionary<MethodBase, Dictionary<FieldInfo, int>> intFieldMap;
        static Dictionary<MethodBase, Dictionary<FieldInfo, float>> floatFieldMap;
        static void InitMaps() {
            int dp = DetailedTerrain.GUI.ModSettings.settings.detailedMeshPower;
            //int f = 1 << p
            float df = Mathf.Pow(2, dp);
            intMap = new() { };
            floatMap = new() { };
            intFieldMap = new() { };
            floatFieldMap = new() { };
            // surface painter
            try {
                intMap.Add(
                    DebugValue(AccessTools.Method("SurfacePainter.Detours.TerrainManagerDetour:GetSurfaceCell")),
                    new() {
                        { 480, (int)(480 * df) },
                        { 479, (int)(480 * df - 1) }
                    });
                floatMap.Add(
                    DebugValue(AccessTools.Method("SurfacePainter.Detours.TerrainManagerDetour:GetSurfaceCell")),
                    new() {
                        { 0.25f, 0.25f / (float)df }
                    });
                intMap.Add(DebugValue(AccessTools.Constructor(AccessTools.TypeByName("SurfacePainter.SurfaceManager"), null, true)),
                    new() {
                        { 4, (int)(4 / df) },
                        { 480, (int)(480 * df) },
                        { 480 * 9, (int)(480 * 9 * df) }
                    });
                intFieldMap.Add(DebugValue(AccessTools.Method("SurfacePainter.SurfaceManager:GetSurfaceItem")),
                    new() {
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_SIZE")), (int)(480 * 9 * df) },
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_PER_AREA")), (int)(480 * df) }
                    });
                intFieldMap.Add(DebugValue(AccessTools.Method("SurfacePainter.SurfaceManager:MigrateItems")),
                    new() {
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_PER_AREA")), (int)(480 * df) }
                    });
                intFieldMap.Add(DebugValue(AccessTools.Method("SurfacePainter.SurfaceManager:SetSurfaceItem")),
                    new() {
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_SIZE")), (int)(480 * 9 * df) },
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_PER_AREA")), (int)(480 * df) }
                    });
                intFieldMap.Add(DebugValue(AccessTools.PropertyGetter("SurfacePainter.SurfaceManager:Surfaces")),
                    new() {
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_SIZE")), (int)(480 * 9 * df) },
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_PER_AREA")), (int)(480 * df) }
                    });
                intMap.Add(DebugValue(AccessTools.Method("SurfacePainter.InGameSurfaceTool:ApplyBrush")),
                    new() {
                        { 4, (int)(4 * df) }
                    });
                intFieldMap.Add(DebugValue(AccessTools.Method("SurfacePainter.InGameSurfaceTool:ApplyBrush")),
                    new() {
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:GRID_SIZE")), (int)(480 * 9 * df) }
                    });
                floatFieldMap.Add(DebugValue(AccessTools.Method("SurfacePainter.InGameSurfaceTool:ApplyBrush")),
                    new() {
                        { DebugValue(AccessTools.Field("SurfacePainter.SurfaceManager:CELL_SIZE")), 4 / (float) df }
                    });

            }
            catch (Exception e) {
                Log.Exception(e, "failed to patch surface painter. If you don't use that mod, this can be safely ignored.", false);
            }
        }
        internal static IEnumerable<MethodBase> TargetMethods() {
            InitMaps();
            var targetMethods = new HashSet<MethodBase>();
            targetMethods.AddRange(intMap.Keys);
            targetMethods.AddRange(floatMap.Keys);
            targetMethods.AddRange(intFieldMap.Keys);
            targetMethods.AddRange(floatFieldMap.Keys);
            return targetMethods;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codesEnumerable, MethodBase original, ILGenerator generator) {
            Log.Debug("Transpiling " + original.FullDescription());
#if DEBUG
            var intReplacementCheck = new Dictionary<int, int>();
            if (intMap.ContainsKey(original)) foreach (var origValue in intMap[original].Keys) intReplacementCheck[origValue] = 0;
            var floatReplacementCheck = new Dictionary<float, int>();
            if (floatMap.ContainsKey(original)) foreach (var origValue in floatMap[original].Keys) floatReplacementCheck[origValue] = 0;
            var intFieldReplacementCheck = new Dictionary<FieldInfo, int>();
            if (intFieldMap.ContainsKey(original)) foreach (var origField in intFieldMap[original].Keys) intFieldReplacementCheck[origField] = 0;
            var floatFieldReplacementCheck = new Dictionary<FieldInfo, int>();
            if (floatFieldMap.ContainsKey(original)) foreach (var origField in floatFieldMap[original].Keys) floatFieldReplacementCheck[origField] = 0;
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
                if (code.opcode == OpCodes.Ldsfld || code.opcode == OpCodes.Ldsflda || code.opcode == OpCodes.Ldfld || code.opcode == OpCodes.Ldflda) {
                    if (intFieldMap.ContainsKey(original)) {
                        foreach (FieldInfo origField in intFieldMap[original].Keys) {
                            if (code.LoadsField(origField)) {
#if DEBUG
                                intFieldReplacementCheck[origField]++;
#endif
                                yield return CodeInstructionExtensions.LoadConstant(intFieldMap[original][origField]);
                                replaced = true;
                                break;
                            }
                        }
                    }

                    if (replaced) continue;
                    if (floatFieldMap.ContainsKey(original)) {
                        foreach (FieldInfo origField in floatFieldMap[original].Keys) {
                            if (code.LoadsField(origField)) {
#if DEBUG
                                floatFieldReplacementCheck[origField]++;
#endif
                                yield return CodeInstructionExtensions.LoadConstant(floatFieldMap[original][origField]);
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
            foreach (var origField in intFieldReplacementCheck.Keys) {
                Log.Debug("replaced " + origField.DeclaringType.FullName + ":" + origField.Name + " with " + intFieldMap[original][origField] + " at " + intFieldReplacementCheck[origField] + " occurences.");
            }
            foreach (var origField in floatFieldReplacementCheck.Keys) {
                Log.Debug("replaced " + origField.DeclaringType.FullName + ":" + origField.Name + " with " + floatFieldMap[original][origField] + " at " + floatFieldReplacementCheck[origField] + " occurences.");
            }
#endif
        }
        static void LogInt(int value) {
            Debug.Log(value);
        }
    }
}