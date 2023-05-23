using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DetailedTerrain.Patches {
    class TerrainManagerDataSerializationPatch {
        static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> codes) {
            foreach (var code in codes) {
                if (code.LoadsField(AccessTools.Field(typeof(TerrainManager), "m_rawHeights"))
                    || code.LoadsField(AccessTools.Field(typeof(TerrainManager), "m_blockHeights"))
                    || code.LoadsField(AccessTools.Field(typeof(TerrainManager), "m_blockHeights2"))) {
                    yield return code;
                    yield return CodeInstructionExtensions.LoadConstant((int)(1080 * GUI.ModSettings.settings.baseMeshFactor + 1));
                    yield return CodeInstructionExtensions.LoadConstant((int)(1081));
                    yield return CodeInstruction.Call(typeof(Manager.Scaler), nameof(Manager.Scaler.ResizedBuffer));
                } else {
                    yield return code;
                }
            }
        }
        static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> codesE) {
            var codes = codesE.ToList();
            int rawHeightsLI = 0;
            int blockHeightsLI = 0;
            for (int i = 0; i < codes.Count; i++) {
                var code = codes[i];
                if (code.LoadsField(AccessTools.Field(typeof(TerrainManager), "m_rawHeights"))) {
                    yield return code;
                    yield return CodeInstructionExtensions.LoadConstant((int)(1080 * GUI.ModSettings.settings.baseMeshFactor + 1));
                    yield return CodeInstructionExtensions.LoadConstant((int)(1081));
                    yield return CodeInstruction.Call(typeof(Manager.Scaler), nameof(Manager.Scaler.ResizedBuffer));
                    if (codes[i + 1].IsStloc()) {
                        rawHeightsLI = codes[i + 1].LocalIndex();
                    }
                } else if (code.LoadsField(AccessTools.Field(typeof(TerrainManager), "m_blockHeights"))) {
                    yield return code;
                    yield return CodeInstructionExtensions.LoadConstant((int)(1080 * GUI.ModSettings.settings.baseMeshFactor + 1));
                    yield return CodeInstructionExtensions.LoadConstant((int)(1081));
                    yield return CodeInstruction.Call(typeof(Manager.Scaler), nameof(Manager.Scaler.ResizedBuffer));
                    if (codes[i + 1].IsStloc()) {
                        blockHeightsLI = codes[i + 1].LocalIndex();
                    }
                } else if (code.Calls(AccessTools.Method(typeof(TerrainManager), "RefreshPatchFlatness"))) {
                    yield return CodeInstructionExtensions.LoadLocal(rawHeightsLI).MoveLabelsFrom(code);
                    yield return CodeInstructionExtensions.LoadConstant((int)(1081));
                    yield return CodeInstructionExtensions.LoadConstant((int)(1080 * GUI.ModSettings.settings.baseMeshFactor + 1));
                    yield return CodeInstruction.Call(typeof(Manager.Scaler), nameof(Manager.Scaler.ResizedBuffer));
                    yield return CodeInstruction.StoreField(typeof(TerrainManager), "m_rawHeights");
                    yield return CodeInstructionExtensions.LoadLocal(blockHeightsLI);
                    yield return CodeInstructionExtensions.LoadConstant((int)(1081));
                    yield return CodeInstructionExtensions.LoadConstant((int)(1080 * GUI.ModSettings.settings.baseMeshFactor + 1));
                    yield return CodeInstruction.Call(typeof(Manager.Scaler), nameof(Manager.Scaler.ResizedBuffer));
                    yield return CodeInstruction.StoreField(typeof(TerrainManager), "m_blockHeights");
                    yield return code;
                } else yield return code;
            }
        }
    }
}
