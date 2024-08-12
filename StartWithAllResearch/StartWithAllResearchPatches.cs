﻿using Harmony;
using Zolibrary.Logging;
using Klei.CustomSettings;

namespace StartWithAllResearch
{
    public class StartWithAllResearchPatches
    {
        private static SettingConfig StartWithAllResearch;
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                LogManager.SetModInfo("StartWithAllResearch", "1.0.2");
                LogManager.LogInit();
                StartWithAllResearchPatches.StartWithAllResearch = (SettingConfig)new ToggleSettingConfig(
                    id: "StartWithAllResearch",
                    label: "Start with all Research",
                    tooltip: "When active will start a save with all research nodes completed/researched.",
                    off_level: new SettingLevel("Disabled", "Disabled", "Unchecked: Start with all Research is turned off (Default)", 0, null),
                    on_level: new SettingLevel("Enabled", "Enabled", "Checked: Start with all Research is turned on", 0, (object)null),
                    default_level_id: "Disabled",
                    nosweat_default_level_id: "Disabled",
                    coordinate_dimension: -1,
                    coordinate_dimension_width: -1,
                    debug_only: false
                    );
            }
        }

        [HarmonyPatch(typeof(CustomGameSettings), "OnPrefabInit")]
        public class CustomGameSettings_OnPrefabInit_Patch
        {
            public static void Postfix(CustomGameSettings __instance)
            {
                __instance.AddSettingConfig(StartWithAllResearchPatches.StartWithAllResearch);
            }
        }

        [HarmonyPatch(typeof(ResearchScreen), "Update")]
        public class Game_OnSpawn_Patch
        {
            static bool inited = false;

            public static void Postfix()
            {
                if (CustomGameSettings.Instance.GetCurrentQualitySetting(StartWithAllResearchPatches.StartWithAllResearch).id != "Enabled")
                    return;

                if (Game_OnSpawn_Patch.inited) return;

                foreach (TechItem tech in Db.Get().TechItems.resources)
                {
                    if (!tech.IsComplete())
                    {
                        TechInstance ti = Research.Instance.Get(tech.ParentTech);
                        ti.Purchased();
                        Game.Instance.Trigger((int)GameHashes.ResearchComplete, (object)tech.ParentTech);
                    }
                }
                Game_OnSpawn_Patch.inited = true;
            }
        }

        [HarmonyPatch(typeof(ResearchEntry), "ResearchCompleted")]
        public class ResearchEntry_ResearchCompleted_Patch
        {
            public static void Prefix(ref bool notify)
            {
                if (CustomGameSettings.Instance.GetCurrentQualitySetting(StartWithAllResearchPatches.StartWithAllResearch).id == "Enabled")
                    notify = false;
            }
        }
    }
}
