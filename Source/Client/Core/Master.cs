using System.Reflection;
using GameClient.Core.Configs;
using GameClient.Misc;

namespace GameClient.Core
{
    // Class with all the critical variables for the client to work

    public static class Master
    {
        // Instances

        public static UnityMainThreadDispatcher threadDispatcher;

        public static ModExposer modConfigs = new ModExposer();

        // DO NOT RENAME 'loadedCompatibilityPatches'
        // IT HAS A HARDCODED REFERENCE WITH THE METHOD MANAGER

        public static Assembly[] loadedCompatibilityPatches;

        // Paths

        public static string appdataFolderPath;

        public static string appdataRTFolderPath;

        public static string appdataTempFolderPath;

        public static string modMainFolderPath;

        public static string modAssemblyFolderPath;

        public static string modAddonsFolderPath;

        public static string connectionDataPath;

        public static string loginDataPath;

        public static string clientPreferencesPath;

        public static string recentServersPath;

        public static string savesFolderPath;

        // Values

        public static readonly string modID = "RimWorld Together";
    }
}
