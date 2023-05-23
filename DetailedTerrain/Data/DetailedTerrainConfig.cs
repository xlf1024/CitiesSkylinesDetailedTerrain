using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DetailedTerrain.Data {

    public class DetailedTerrainConfig {
        public const string FILE_NAME = "DetailedTerrainConfig.xml";

        [DefaultValue(2)]
        public int detailedMeshPower = 2;
        public float detailedMeshFactor => UnityEngine.Mathf.Pow(2, detailedMeshPower);

        [DefaultValue(1)]
        public int baseMeshPower = 1;
        public float baseMeshFactor => UnityEngine.Mathf.Pow(2, baseMeshPower);

        // From LoadOrderMod
        public void Serialize() {
            XmlSerializer ser = new(typeof(DetailedTerrainConfig));
            using (FileStream fs = new(Path.Combine(DataLocation.localApplicationData, FILE_NAME), FileMode.Create, FileAccess.Write)) {
                ser.Serialize(fs, this);
            }
        }

        public static DetailedTerrainConfig Deserialize() {
            try {
                XmlSerializer ser = new(typeof(DetailedTerrainConfig));
                using (FileStream fs = new(Path.Combine(DataLocation.localApplicationData, FILE_NAME), FileMode.Open, FileAccess.Read)) {
                    var config = ser.Deserialize(fs) as DetailedTerrainConfig;
                    return config;
                }
            }
            catch {
                return null;
            }
        }
    }
}
