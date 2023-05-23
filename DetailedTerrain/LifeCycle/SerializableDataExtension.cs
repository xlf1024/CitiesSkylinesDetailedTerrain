using ColossalFramework.IO;
using HarmonyLib;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DetailedTerrain.LifeCycle {
    class SerializableDataExtension : SerializableDataExtensionBase {
        private const string ID = "xlf1024.DetailedTerrain.";
        public override void OnLoadData() {
            base.OnLoadData();
            if((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.GameAndMap) != 0) {
                if (serializableDataManager.EnumerateData().Contains(ID + "size")) {
                    int loadedSize = BitConverter.ToInt32(serializableDataManager.LoadData(ID + "size"), 0);
                    string[] fieldNames = new string[] { "m_blockHeights", "m_rawHeights" };
                    foreach (var fieldName in fieldNames) {
                        if(serializableDataManager.EnumerateData().Contains(ID + fieldName)) {
                            var loadedBuffer = serializableDataManager.LoadData(ID + fieldName);
                            var tempBuffer = new ushort[loadedBuffer.Length * sizeof(byte) / sizeof(ushort)];
                            Buffer.BlockCopy(loadedBuffer, 0, tempBuffer, 0, loadedBuffer.Length);
                            var resizedBuffer = Manager.Scaler.ResizedBuffer(tempBuffer, loadedSize, (int)(1080 * GUI.ModSettings.settings.baseMeshFactor + 1));
                            AccessTools.Field(typeof(TerrainManager), fieldName).SetValue(TerrainManager.instance, resizedBuffer);
                        }
                    }
                    AccessTools.Method(typeof(TerrainManager), "RefreshPatchFlatness").Invoke(TerrainManager.instance, new object[] { });
                    TerrainManager.instance.UpdateData(SimulationManager.instance.m_metaData.m_updateMode);
                }
            }
        }
        public override void OnSaveData() {
            base.OnSaveData();
            if ((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.GameAndMap) != 0) {
                serializableDataManager.SaveData(ID + "size", BitConverter.GetBytes((int)(1080 * GUI.ModSettings.settings.baseMeshFactor + 1)));
                ushort[] blockHeights = (ushort[])AccessTools.Field(typeof(TerrainManager), "m_blockHeights").GetValue(TerrainManager.instance);
                ushort[] blockHeights2 = (ushort[])AccessTools.Field(typeof(TerrainManager), "m_blockHeights2").GetValue(TerrainManager.instance);
                ushort[] rawHeights = (ushort[])AccessTools.Field(typeof(TerrainManager), "m_rawHeights").GetValue(TerrainManager.instance);

                ushort[] blockHeightsDifference = new ushort[blockHeights.Length];
                for(var i = 0; i < blockHeights.Length; i++) {
                    blockHeightsDifference[i] = (ushort)(blockHeights[i] - blockHeights2[i]);
                }
                byte[] blockHeightsStored = new byte[blockHeights.Length * sizeof(ushort) / sizeof(byte)];
                Buffer.BlockCopy(blockHeightsDifference, 0, blockHeightsStored, 0, blockHeightsStored.Length);
                serializableDataManager.SaveData(ID + "m_blockHeights", blockHeightsStored);
                byte[] rawHeightsStored = new byte[rawHeights.Length * sizeof(ushort) / sizeof(byte)];
                Buffer.BlockCopy(rawHeights, 0, rawHeightsStored, 0, rawHeightsStored.Length);
                serializableDataManager.SaveData(ID + "m_rawHeights", rawHeightsStored);
            } else {
                serializableDataManager.EraseData(ID + "size");
                serializableDataManager.EraseData(ID + "m_blockHeights");
                serializableDataManager.EraseData(ID + "m_rawHeights");
            }
        }
    }
}
