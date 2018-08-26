using System;
using System.Reflection;
using BattleTech.UI;
using Harmony;
using Newtonsoft.Json;

namespace FastWorkOrderMover
{
    public static class AssemblyPatch
    {
        internal static Settings ModSettings = new Settings();
        internal static string ModDirectory;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;
            try
            {
                ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ModSettings = new Settings();
            }

            var harmony = HarmonyInstance.Create($"com.joelmeador.{Settings.ModName}");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortDown")]
    public static class TaskManagementWidget_OnSortDown_Patch
    {
        public static bool Prefix(TaskManagementElement element, TaskManagementWidget __instance, ref bool ___modified)
        {
            var state = State.GetState();
            if (state.IsNothing)
                return true;
            else if (state.IsSorting)
                Sorter.SortWorkOrdersDescending(taskManagementWidget: __instance);
            else if (state.IsMoving)
                Mover.MoveWorkOrderToBottom(taskManagementWidget: __instance, workOrder: element);
            ___modified = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortUp")]
    public static class TaskManagementWidget_OnSortUp_Patch
    {
        public static bool Prefix(TaskManagementWidget __instance, TaskManagementElement element, ref bool ___modified)
        {
            var state = State.GetState();
            if (state.IsNothing)
                return true;
            else if (state.IsSorting)
                Sorter.SortWorkOrdersAscending(taskManagementWidget: __instance);
            else if (state.IsMoving)
                Mover.MoveWorkOrderToTop(taskManagementWidget: __instance, workOrder: element);
            ___modified = true;
            return false;
        }
    }
}