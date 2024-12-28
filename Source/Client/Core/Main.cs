using GameClient.Core.Preferences;
using GameClient.Managers;
using GameClient.Misc;
using HarmonyLib;
using Shared;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace GameClient.Core
{
    //Class that works as an entry point for the mod

    public static class Main_
    {
        [StaticConstructorOnStartup]
        public static class RimworldTogether
        {
            static RimworldTogether()
            {
                ApplyHarmonyPathches();
                PrepareCulture();
                PreparePaths();
                CreateUnityDispatcher();

                CaravanManagerHelper.SetCaravanDefs();
                SiteManager.SetSiteDefs();

                PlayerPreferenceManager.LoadPlayerPreferences();
                CompatibilityManager.LoadAllPatchedAssemblies();
            }
        }

        private static void ApplyHarmonyPathches()
        {
            Harmony harmony = new Harmony(Master.modID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void PrepareCulture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
        }

        private static void PreparePaths()
        {
            Master.savesFolderPath = GenFilePaths.SavedGamesFolderPath;

            Master.appdataFolderPath = GenFilePaths.SaveDataFolderPath;
            Master.appdataRTFolderPath = Path.Combine(Master.appdataFolderPath, "RimWorld Together");
            Master.appdataTempFolderPath = Path.Combine(Master.appdataRTFolderPath, "Temp");

            Master.modMainFolderPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.ToString();
            Master.modAddonsFolderPath = Path.Combine(Master.modMainFolderPath, "Addons");
            Master.modAssemblyFolderPath = Path.Combine(Master.modMainFolderPath, "Current", "Assemblies");

            Master.connectionDataPath = Path.Combine(Master.appdataRTFolderPath, "ConnectionData.json");
            Master.clientPreferencesPath = Path.Combine(Master.appdataRTFolderPath, "Preferences.json");
            Master.recentServersPath = Path.Combine(Master.appdataRTFolderPath, "RecentServers.json");
            Master.loginDataPath = Path.Combine(Master.appdataRTFolderPath, "LoginData.json");

            if (!Directory.Exists(Master.appdataRTFolderPath)) Directory.CreateDirectory(Master.appdataRTFolderPath);
            if (!Directory.Exists(Master.appdataTempFolderPath)) Directory.CreateDirectory(Master.appdataTempFolderPath);
            if (!Directory.Exists(Master.modAddonsFolderPath)) Directory.CreateDirectory(Master.modAddonsFolderPath);        }

        private static void CreateUnityDispatcher()
        {
            if (Master.threadDispatcher == null)
            {
                GameObject go = new GameObject("Dispatcher");
                Master.threadDispatcher = go.AddComponent(typeof(UnityMainThreadDispatcher)) as UnityMainThreadDispatcher;
                UnityEngine.Object.Instantiate(go);

                Printer.Message($"Created dispatcher for version '{CommonValues.executableVersion}'");
            }
        }
    }
}