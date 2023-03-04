namespace DetailedTerrain.LifeCycle {
    using System;
    using CitiesHarmony.API;
    using ICities;
    using KianCommons;
    using System.Diagnostics;
    using UnityEngine.SceneManagement;
    using DetailedTerrain.GUI;
    using ColossalFramework;

    public static class LifeCycle {
        public static string HARMONY_ID = "xlf1024.DetailedTerrain";
        public static bool bHotReload = false;

        public static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;
        public static LoadMode Mode => (LoadMode)UpdateMode;
        public static string Scene => SceneManager.GetActiveScene().name;

        public static bool Loaded;

        public static void Enable() {

            HarmonyHelper.EnsureHarmonyInstalled();
            HarmonyHelper.DoOnHarmonyReady(() => HarmonyUtil.InstallHarmony(HARMONY_ID, null));
            var loadingManager = Singleton<LoadingManager>.instance;
            Patches.ResizeArrays.Resize();
            if (loadingManager.m_loadingComplete && !loadingManager.m_currentlyLoading)
                HotReload();

        }

        public static void Disable() {
            Unload();
        }
        public static void Repatch() {
            Log.Info("Repatching", true);
            HarmonyHelper.EnsureHarmonyInstalled();
            HarmonyHelper.DoOnHarmonyReady(() => {
                HarmonyUtil.UninstallHarmony(HARMONY_ID);
                HarmonyUtil.InstallHarmony(HARMONY_ID, null);
                Patches.ResizeArrays.Resize();
            });
        }
        public static void HotReload() {
            bHotReload = true;
            SimulationManager.instance.AddAction(() => {
                TerrainManager.instance.UpdateBounds(0, 0, 1080, 1080);
            });
        }

        public static void OnLevelLoaded(LoadMode mode) {
        }

        public static void Unload() {
            Log.Info("LifeCycle.Unload() called");
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
            ModSettings.RemoveEventListeners();
            Loaded = false;
            Patches.ResizeArrays.Resize();
        }
    }
}
