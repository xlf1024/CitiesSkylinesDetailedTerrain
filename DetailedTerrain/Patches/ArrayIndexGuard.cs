#if DEBUG
using HarmonyLib;
using KianCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DetailedTerrain.Patches {
    [HarmonyPatch]
    class ArrayIndexGuard {
        static T ArrayReadWrapper<T>(T[] array, int index) {
            try {
                return array[index];
            }
            catch (Exception e) {
                Log.Error("Array Index out of range: requested: " + index + ", length: " + array.Length + " in " + array.ToString());
                throw e;
            }
        }
        static ref T ArrayReadAddressWrapper<T>(T[] array, int index) {
            try {
                return ref array[index];
            }
            catch (Exception e) {
                Log.Error("Array Index out of range: requested: " + index + ", length: " + array.Length + " in " + array.ToString());
                throw e;
            }
        }
        static void ArrayWriteWrapper<T>(T[] array, int index, T value) {
            try {
                array[index] = value;
            }
            catch (Exception e) {
                Log.Error("Array Index out of range: targeted: " + index + ", length: " + array.Length + " in " + array.ToString());
                throw e;
            }
        }

        static IEnumerable<MethodBase> TargetMethods() {
            var targetMethods = new HashSet<MethodBase>();
            targetMethods.AddRange(ReplaceTerrainConstantsPatch.TargetMethods());
            targetMethods.AddRange(SurfacePainterReplaceDetailConstantsPatch.TargetMethods());
            return targetMethods;
        }
        [HarmonyPriority(Priority.Last)]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, MethodBase original, ILGenerator generator) {
            Log.Debug("Transpiling " + original.FullDescription());
            //int indexCache = generator.DeclareLocal(typeof(int)).LocalIndex;
            foreach (var code in codes) {
                if (code.opcode == OpCodes.Ldelem)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { (Type)code.operand });
                else if (code.opcode == OpCodes.Ldelema)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadAddressWrapper), null, new Type[] { (Type)code.operand });
                else if (code.opcode == OpCodes.Ldelem_Ref)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(Object).MakeByRefType() });
                else if (code.opcode == OpCodes.Ldelem_I)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(IntPtr) });
                else if (code.opcode == OpCodes.Ldelem_I1)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(sbyte) });
                else if (code.opcode == OpCodes.Ldelem_I2)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(Int16) });
                else if (code.opcode == OpCodes.Ldelem_I4)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(Int32) });
                else if (code.opcode == OpCodes.Ldelem_I8)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(Int64) });
                else if (code.opcode == OpCodes.Ldelem_U1)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(byte) });
                else if (code.opcode == OpCodes.Ldelem_U2)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(UInt16) });
                else if (code.opcode == OpCodes.Ldelem_U4)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(UInt32) });
                else if (code.opcode == OpCodes.Ldelem_R4)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(float) });
                else if (code.opcode == OpCodes.Ldelem_R8)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayReadWrapper), null, new Type[] { typeof(double) });
                else
                if (code.opcode == OpCodes.Stelem)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { (Type)code.operand });
                else if (code.opcode == OpCodes.Stelem_Ref)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(Object).MakeByRefType() });
                else if (code.opcode == OpCodes.Stelem_I)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(IntPtr) });
                else if (code.opcode == OpCodes.Stelem_I1)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(sbyte) });
                else if (code.opcode == OpCodes.Stelem_I2)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(Int16) });
                else if (code.opcode == OpCodes.Stelem_I4)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(Int32) });
                else if (code.opcode == OpCodes.Stelem_I8)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(Int64) });
                else if (code.opcode == OpCodes.Stelem_R4)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(float) });
                else if (code.opcode == OpCodes.Stelem_R8)
                    yield return CodeInstruction.Call(typeof(ArrayIndexGuard), nameof(ArrayWriteWrapper), null, new Type[] { typeof(double) });
                else
                    yield return code;
            }
        }
    }
}
#endif