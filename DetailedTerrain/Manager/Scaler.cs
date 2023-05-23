using KianCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DetailedTerrain.Manager {
    class Scaler {
        internal static ushort[] ResizedBuffer(ushort[] source, int sourceSize, int targetSize){
            var target = new ushort[targetSize * targetSize];
            for (int tz = 0; tz < targetSize; tz++) {
                float szf = (float)tz / (float)targetSize * (float)sourceSize;
                int szi = (int)szf;
                int szim1 = Mathf.Clamp(szi - 1, 0, sourceSize - 1);
                int szi0 = Mathf.Clamp(szi + 0, 0, sourceSize - 1);
                int szi1 = Mathf.Clamp(szi + 1, 0, sourceSize - 1);
                int szi2 = Mathf.Clamp(szi + 2, 0, sourceSize - 1);
                szf -= szi;
                for (int tx = 0; tx < targetSize; tx++) {
                    float sxf = (float)tx / (float)targetSize * (float)sourceSize;
                    int sxi = (int)sxf;
                    int sxim1 = Mathf.Clamp(sxi - 1, 0, sourceSize - 1);
                    int sxi0 = Mathf.Clamp(sxi + 0, 0, sourceSize - 1);
                    int sxi1 = Mathf.Clamp(sxi + 1, 0, sourceSize - 1);
                    int sxi2 = Mathf.Clamp(sxi + 2, 0, sourceSize - 1);
                    sxf -= sxi;

                    var hzm1 = TerrainManager.SmoothSample(
                        source[sourceSize * szim1 + sxim1],
                        source[sourceSize * szim1 + sxi0],
                        source[sourceSize * szim1 + sxi1],
                        source[sourceSize * szim1 + sxi2],
                        sxf);
                    var hz0 = TerrainManager.SmoothSample(
                        source[sourceSize * szi0 + sxim1],
                        source[sourceSize * szi0 + sxi0],
                        source[sourceSize * szi0+ sxi1],
                        source[sourceSize * szi0 + sxi2],
                        sxf);
                    var hz1 = TerrainManager.SmoothSample(
                        source[sourceSize * szi1 + sxim1],
                        source[sourceSize * szi1 + sxi0],
                        source[sourceSize * szi1 + sxi1],
                        source[sourceSize * szi1 + sxi2],
                        sxf);
                    var hz2 = TerrainManager.SmoothSample(
                        source[sourceSize * szi2 + sxim1],
                        source[sourceSize * szi2 + sxi0],
                        source[sourceSize * szi2 + sxi1],
                        source[sourceSize * szi2 + sxi2],
                        sxf);

                    target[tz * targetSize + tx] = (ushort)TerrainManager.SmoothSample(hzm1, hz0, hz1, hz2, szf);
                }
            }
            return target;
        }
    }
}
