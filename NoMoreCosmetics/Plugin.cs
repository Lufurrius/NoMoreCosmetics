using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using MoreCompany;
using MoreCompany.Cosmetics;
using System;
using System.IO;
using UnityEngine;

namespace NoMoreCosmetics
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "lufurrius.NoMoreCosmetics";
        private const string modName = "NoMoreConsmetics";
        private const string modVersion = "1.3.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource logger;

        private static Plugin Instance;

        private ConfigFile moreCompanyConfig;

        public static string dynamicCosmeticsPath;
        public static string cosmeticSavePath;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            logger = BepInEx.Logging.Logger.CreateLogSource("NoMoreConsmetics");

            logger.LogInfo("Loading " + modGUID + " " + modVersion + @"
    ________                 ____
  _/ ______ \_   |\     |   /    \
 /O /      \ O\  | \    |  |      |
 \O \______/ O/  |  \   |  |      |
  '-/  \/  \-'   |   \  |  |      |
  .-\__/\__/-.   |    \ |  |      |
 /            \  |     \|   \____/
 ");


            string configFilePath = Path.Combine(Paths.ConfigPath, "me.swipez.melonloader.morecompany.cfg");

            if (File.Exists(configFilePath))
            {
                moreCompanyConfig = new ConfigFile(configFilePath, true);

                // Access a specific config entry
                var exampleValue = moreCompanyConfig.Bind<bool>("Cosmetics", "Per Profile Cosmetics", false, "Should the cosmetics be saved per-profile?").Value;
                logger.LogInfo($"MoreCompany \"Per Profile Cosmetics\" config value: {exampleValue}");
                if (exampleValue)
                {
                    cosmeticSavePath = $"{Application.persistentDataPath}/morecompanycosmetics-{Directory.GetParent(Paths.BepInExRootPath).Name}".Replace("/", "\\");
                }
                else
                {
                    cosmeticSavePath = $"{Application.persistentDataPath}/morecompanycosmetics".Replace("/", "\\");
                }
            }
            else
            {
                logger.LogWarning("MoreCompany config not found! Assuming it's default values");
                cosmeticSavePath = $"{Application.persistentDataPath}/morecompanycosmetics".Replace("/", "\\");
            }
            logger.LogInfo("Player cosmetics save path: " + cosmeticSavePath);

            System.IO.File.WriteAllText(cosmeticSavePath + ".txt", "");
            System.IO.File.WriteAllText(cosmeticSavePath + ".mcs", "");

            CosmeticRegistry.locallySelectedCosmetics.Clear();

            try
            {
                logger.LogInfo("Patching MoreCompany cosmetic loaders...");
                harmony.PatchAll();
            }
            catch (Exception patch)
            {
                logger.LogError("Failed applying patch: " + patch);
            }

            logger.LogInfo("Finished loading " + modName + "!");
        }
    }

    [HarmonyPatch(typeof(MainClass), "ReadCosmeticsFromFile")]
    public class CosmeticPatch
    {
        static void Postfix()
        {
            CosmeticRegistry.locallySelectedCosmetics.Clear();
        }
    }

    [HarmonyPatch(typeof(CosmeticRegistry), "LoadCosmeticsFromBundle")]
    public class CosmeticBundlePatch
    {
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(CosmeticRegistry), "LoadCosmeticsFromAssembly")]
    public class CosmeticAssemblyePatch
    {
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(CosmeticRegistry), "SpawnCosmeticGUI")]
    public class CosmeticUIPatch
    {
        static bool Prefix()
        {
            return false;
        }
    }
}