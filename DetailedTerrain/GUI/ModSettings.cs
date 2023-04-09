using ColossalFramework;
using ICities;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using KianCommons.UI.Table;
using KianCommons;
using UnityEngine;
using DetailedTerrain.Data;
using CitiesHarmony.API;

namespace DetailedTerrain.GUI {
    class ModSettings {
        public const string FILE_NAME = nameof(DetailedTerrain);
        public static DetailedTerrainConfig settings;
        private static UIComponent settingsWindow = null;
        private static UITextField detailFactorField;
        static ModSettings() {
            settings = DetailedTerrainConfig.Deserialize() ?? new DetailedTerrainConfig();
        }
        internal static void OnSettingsUI(UIHelperBase helper) {
            //find settings window
            settingsWindow = (helper as UIHelper).self as UIComponent;
            while (settingsWindow.parent is not null && !settingsWindow.name.EndsWith("OptionsPanel")) {
                settingsWindow = settingsWindow.parent;
            }

            settingsWindow.eventVisibilityChanged += OnSettingsWindowVisibilityChanged;

            detailFactorField = helper.AddTextfield("Resolution factor. Must be a power of two. Can only be changed in the main menu.", Mathf.Pow(2, settings.detailedMeshPower).ToString(), value => { }, value => {
                value = value.Replace(",", ".");
                if (Single.TryParse(value, out float newValue)) {
                    int newPower = Mathf.RoundToInt(Mathf.Log(newValue, 2));
                    newPower = Mathf.Clamp(newPower, -2, 12);
                    if (newPower != settings.detailedMeshPower) {
                        settings.detailedMeshPower = newPower;
                        OnSettingsChanged();
                    }
                }
                if (detailFactorField is not null) detailFactorField.text = Mathf.Pow(2, settings.detailedMeshPower).ToString();
            }) as UITextField;
            detailFactorField.numericalOnly = true;
            detailFactorField.allowFloats = true;
            detailFactorField.allowNegative = false;
            if (!Helpers.InStartupMenu) {
                detailFactorField.Disable();
            }

        }

        internal static void OnSettingsWindowVisibilityChanged(UIComponent _, bool visible) {
            // only apply any changes as soon as the settings are closed
            if (!visible) {
                Log.Debug("DetailedTerrain: settings window closed");
            }
        }
        internal static void OnSettingsChanged() {
            Log.Debug("DetailedTerrain: OnSettingsChanged");
            settings.Serialize();
            LifeCycle.LifeCycle.Repatch();
            if (detailFactorField is not null) detailFactorField.text = Mathf.Pow(2, settings.detailedMeshPower).ToString();
        }

        internal static void RemoveEventListeners() {
            settingsWindow.eventVisibilityChanged -= OnSettingsWindowVisibilityChanged;
        }
    }
}
