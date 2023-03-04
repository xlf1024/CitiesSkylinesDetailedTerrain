namespace DetailedTerrain.LifeCycle
{
    using System;
    using JetBrains.Annotations;
    using ICities;
    using CitiesHarmony.API;
    using KianCommons;
    using System.Diagnostics;

    public class DetailedTerrainMod : IUserMod
    {
        public static Version ModVersion => typeof(DetailedTerrainMod).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Detailed Terrain " + VersionString;
        public string Description => "increases Terrain Mesh resolution";

        [UsedImplicitly]
        public void OnEnabled()
        {
            LifeCycle.Enable();
        }

        [UsedImplicitly]
        public void OnDisabled()
        {
            LifeCycle.Disable();
        }

        [UsedImplicitly]
        public void OnSettingsUI(UIHelperBase helper) {
            GUI.ModSettings.OnSettingsUI(helper);
        }

    }
}
