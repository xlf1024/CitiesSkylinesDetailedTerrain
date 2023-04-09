using HarmonyLib;
using KianCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DetailedTerrain.Patches {
    class ResizeArrays {
        static Dictionary<FieldInfo, int> arraySizes;

        static void InitMap() {
            int dp = DetailedTerrain.GUI.ModSettings.settings.detailedMeshPower;
            //int f = 1 << p
            float df = UnityEngine.Mathf.Pow(2, dp);
            arraySizes = new() {
                {
                    AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_detailHeights)),
                    (int)((480 * df + 1) * (480 * df + 1) * GameAreaManager.instance.MaxAreaCount)
                },
                {
                    AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_detailHeights2)),
                    (int)((480 * df + 1) * (480 * df + 1) * GameAreaManager.instance.MaxAreaCount)
                },
                {
                    AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_detailSurface)),
                    (int)((480 * df) * (480 * df) * GameAreaManager.instance.MaxAreaCount)
                },
                {
                    AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_detailZones)),
                    (int)((480 * df) * (480 * df) * GameAreaManager.instance.MaxAreaCount)
                },
                {
                    AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_tempHeights)),
                    (int)((512 * df + 1) * (512 * df + 1))
                },
                {
                    AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_tempSurface)),
                    (int)((512 * df + 1) * (512 * df + 1))
                },
                {
                    AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_tempZones)),
                    (int)((512 * df + 1) * (512 * df + 1))
                }

            };
        }
        static T DebugValue<T>(T value) {
#if DEBUG
            UnityEngine.Debug.Log(value);
#endif
            return value;
        }
        internal static void Resize() {
            if (!Helpers.InStartupMenu) return;
            Log.DebugCalled();
            InitMap();
            if (TerrainManager.exists) {
                foreach (var field in arraySizes.Keys) {
                    if (field.GetValue(TerrainManager.instance) is not null) {
                        if (
                            ((int)DebugValue(AccessTools.Property(DebugValue(field.FieldType), "Length")).GetValue(field.GetValue(TerrainManager.instance), null))
                            != arraySizes[field]
                            ) {
                            Log.Debug("resizing " + field.DeclaringType + ":" + field.Name + " to size " + arraySizes[field].ToString());
                            field.SetValue(
                                TerrainManager.instance,
                                DebugValue(AccessTools.Constructor(DebugValue(field.FieldType), new Type[] { typeof(int) }))
                                .Invoke(new object[] { arraySizes[field] })
                                );
                        }
                    }
                }
                int dp = DetailedTerrain.GUI.ModSettings.settings.detailedMeshPower;
                var rawBoundsField = DebugValue(AccessTools.Field(typeof(TerrainManager), "m_rawBounds"));
                if (rawBoundsField.GetValue(TerrainManager.instance) is not null) {
                    var rawBoundsValue = rawBoundsField.GetValue(TerrainManager.instance) as TerrainManager.CellBounds[][];
                    if (rawBoundsValue.Length != 5 + dp) {
                        rawBoundsValue = new TerrainManager.CellBounds[5 + dp][];
                        int num = 81;
                        for (int i = 0; i <= 4 + dp; i++) {
                            rawBoundsValue[i] = new TerrainManager.CellBounds[num];
                            num <<= 2;
                        }
                        rawBoundsField.SetValue(TerrainManager.instance, rawBoundsValue);
                    }
                }
            }
        }
    }
}
