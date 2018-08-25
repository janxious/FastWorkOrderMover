using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTech;
using BattleTech.UI;
using Harmony;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

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
        public static bool Prefix(
            TaskManagementElement element,
            TaskManagementWidget __instance,
            ref List<TaskManagementElement> ___allMechElements,
            ref List<WorkOrderEntry> ___mechLabQueue,
            ref bool ___modified,
            ref UnityAction ___closeCallback,
            ref SimGameState ___Sim)
        {
            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (ctrlHeld && shiftHeld) return true;
            if (!ctrlHeld && !shiftHeld) return true;

            if (ctrlHeld)
            {
                var sortedWorkOrders =
                    ___allMechElements
                        .OrderByDescending(el => el.cumulativeDaysRemaining)
                        .ThenBy(el => el.Entry.GUID)
                        .Select(el => el.Entry)
                        .ToList();
                AccessTools.FieldRefAccess<SimGameState, List<WorkOrderEntry>>(___Sim, "MechLabQueue") = sortedWorkOrders;
                __instance.SetData(___Sim, ___closeCallback);
                return false;
            }

            // shift was held
            var initialIndex = ___allMechElements.IndexOf(element);
            if (initialIndex >= ___allMechElements.Count) { return false; }
            var newIndex = ___allMechElements.Count - 1;
            ___allMechElements.Remove(element);
            ___allMechElements.Insert(newIndex, element);
            ___mechLabQueue.Remove(element.Entry);
            ___mechLabQueue.Insert(newIndex, element.Entry);
            element.transform.SetSiblingIndex(newIndex);
            ___modified = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortUp")]
    public static class TaskManagementWidget_OnSortUp_Patch
    {
        public static bool Prefix(
            TaskManagementWidget __instance,
            TaskManagementElement element,
            ref List<TaskManagementElement> ___allMechElements,
            ref List<WorkOrderEntry> ___mechLabQueue,
            ref bool ___modified)
        {
            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrlHeld && !shiftHeld)
            {
                var sim = Traverse.Create(__instance).Field("Sim").GetValue<SimGameState>();
                var sortedWorkOrders = ___allMechElements.OrderBy(x => x.cumulativeDaysRemaining).ThenBy(x => x.Entry.GUID).Select(x => x.Entry).ToList();
                var closeCb = Traverse.Create(__instance).Field("closeCallback").GetValue<UnityAction>();
                Traverse.Create(sim).Property("MechLabQueue").SetValue(sortedWorkOrders);
                __instance.SetData(sim, closeCb);

                return false;
            }

            if (!ctrlHeld && shiftHeld)
            {
                var initialIndex = ___allMechElements.IndexOf(element);
                if (initialIndex == 0) return false;
                var newIndex = 0;
                ___allMechElements.Remove(element);
                ___allMechElements.Insert(newIndex, element);
                ___mechLabQueue.Remove(element.Entry);
                ___mechLabQueue.Insert(newIndex, element.Entry);
                element.transform.SetSiblingIndex(newIndex);
                ___modified = true;

                return false;
            }

            // let original method run if no keypress modifiers were used
            return true;
        }
    }
}