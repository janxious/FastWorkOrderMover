using System;
using System.Collections.Generic;
using System.Reflection;
using BattleTech;
using BattleTech.UI;
using Harmony;
using Newtonsoft.Json;
using UnityEngine;

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

    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortUp")]
    public static class TaskManagementWidget_OnSortUp_Patch
    {
        public static bool Prefix(TaskManagementElement element, TaskManagementWidget __instance)
        {
            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var magicThis = Traverse.Create(__instance);
            var mechElements = magicThis.Field("allMechElements").GetValue<List<TaskManagementElement>>();
            var initialIndex = mechElements.IndexOf(element);
            if (initialIndex == 0) { return false; }

            var mechLabQueue = magicThis.Field("mechLabQueue").GetValue<List<WorkOrderEntry>>();
            var newIndex = shiftHeld ? 0 : initialIndex - 1;
            mechElements.Remove(element);
            mechElements.Insert(newIndex, element);
            mechLabQueue.Remove(element.Entry);
            mechLabQueue.Insert(newIndex, element.Entry);
            element.transform.SetSiblingIndex(newIndex);
            magicThis.Field("modified").SetValue(true);
            return false;
        }
    }

    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortDown")]
    public static class TaskManagementWidget_OnSortDown_Patch
    {
        public static bool Prefix(TaskManagementElement element, TaskManagementWidget __instance)
        {
            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var magicThis = Traverse.Create(__instance);
            var mechElements = magicThis.Field("allMechElements").GetValue<List<TaskManagementElement>>();
            var initialIndex = mechElements.IndexOf(element);
            if (initialIndex >= mechElements.Count) { return false; }

            var mechLabQueue = magicThis.Field("mechLabQueue").GetValue<List<WorkOrderEntry>>();
            var newIndex = shiftHeld ? mechElements.Count - 1 : initialIndex + 1;
            mechElements.Remove(element);
            mechElements.Insert(newIndex, element);
            mechLabQueue.Remove(element.Entry);
            mechLabQueue.Insert(newIndex, element.Entry);
            element.transform.SetSiblingIndex(newIndex);
            magicThis.Field("modified").SetValue(true);
            return false;
        }
    }
}