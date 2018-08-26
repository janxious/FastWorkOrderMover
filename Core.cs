using System;
using System.Reflection;
using Harmony;
using Newtonsoft.Json;

namespace FastWorkOrderMover
{
    public static class Core
    {
        public const string ModName = "FastWorkOrderMover";
        public const string ModId   = "com.joelmeador.FastWorkOrderMover";

        internal static Settings ModSettings = new Settings();
        internal static string ModDirectory;

        public static void Init(string directory, string settingsJson)
        {
            ModDirectory = directory;
            try
            {
                ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJson);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ModSettings = new Settings();
            }
            HarmonyInstance.DEBUG = ModSettings.debug;
            var harmony = HarmonyInstance.Create(ModId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}