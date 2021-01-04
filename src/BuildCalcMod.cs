using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using OutwardBuildCalc.DB;
using SideLoader;
using HarmonyLib;
using OutwardBuildCalc.UI;
using OutwardBuildCalc.CalcModel;

namespace OutwardBuildCalc
{
    [BepInPlugin(GUID, "Build Calc", "1.0.1")]
    public class BuildCalcMod : BaseUnityPlugin
    {
        const string GUID = "com.sinai.buildcalc";

        public static BuildCalcMod Instance;

        internal void Awake()
        {
            Instance = this;
            BuildCalcMenu.WINDOW_ID = Instance.GetHashCode();

            SL.OnPacksLoaded += SL_OnPacksLoaded;

            new Harmony(GUID).PatchAll();
        }

        private bool m_packsLoaded;

        private void SL_OnPacksLoaded()
        {
            Logger.LogWarning("----- Building Database -----");
            try
            {
                Database.BuildDB();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Exception building db!");
                SL.LogInnerException(ex);
                return;
            }

            m_packsLoaded = true;
            BuildCalcMenu.Init();
        }

        internal void OnGUI()
        {
            if (!m_packsLoaded)
                return;

            BuildCalcMenu.OnGUI();
        }

        [HarmonyPatch(typeof(Item), nameof(Item.HasTag), new Type[] { typeof(Tag) })]
        public class Item_HasTag
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Item __instance, Tag _tag)
            {
                if ((_tag.TagName == "Iron" || _tag.TagName == "Brutal")
                    && (__instance.Name.Contains("Damascene") 
                        || __instance.Name.Contains("Sandrose") 
                        || __instance.Name.Contains("Masterpiece"))
                    || (_tag.TagName == "Gold" && __instance.Name.Contains("Gold-Lich")))
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}
