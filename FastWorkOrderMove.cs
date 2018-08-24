using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTech;
using BattleTech.UI;
using Harmony;
using Newtonsoft.Json;
using UIWidgets;
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

    ///  [HarmonyPatch(typeof(TaskManagementWidget), "OnSortUp")]
    ///  public static class TaskManagementWidget_OnSortUp_Patch
    ///  {
    ///      public static bool Prefix(TaskManagementElement element, TaskManagementWidget __instance)
    ///      {
    ///          var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    ///          var magicThis = Traverse.Create(__instance);
    ///          var mechElements = magicThis.Field("allMechElements").GetValue<List<TaskManagementElement>>();
    ///          var initialIndex = mechElements.IndexOf(element);
    ///          if (initialIndex == 0)
    ///          {
    ///              return false;
    ///          }
    ///          var mechLabQueue = magicThis.Field("mechLabQueue").GetValue<List<WorkOrderEntry>>();
    ///          var newIndex = shiftHeld ? 0 : initialIndex - 1;
    ///          mechElements.Remove(element);
    ///          mechElements.Insert(newIndex, element);
    ///          mechLabQueue.Remove(element.Entry);
    ///          mechLabQueue.Insert(newIndex, element.Entry);
    ///          element.transform.SetSiblingIndex(newIndex);
    ///          magicThis.Field("modified").SetValue(true);
    ///          return false;
    ///      }
    ///  }

//    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortDown")]
//    public static class TaskManagementWidget_OnSortDown_Patch
//    {
//        public static bool Prefix(TaskManagementElement element, TaskManagementWidget __instance)
//        {
//            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
//            var magicThis = Traverse.Create(__instance);
//            var mechElements = magicThis.Field("allMechElements").GetValue<List<TaskManagementElement>>();
//            var initialIndex = mechElements.IndexOf(element);
//            if (initialIndex >= mechElements.Count) { return false; }
//
//            var mechLabQueue = magicThis.Field("mechLabQueue").GetValue<List<WorkOrderEntry>>();
//            var newIndex = shiftHeld ? mechElements.Count - 1 : initialIndex + 1;
//            mechElements.Remove(element);
//            mechElements.Insert(newIndex, element);
//            mechLabQueue.Remove(element.Entry);
//            mechLabQueue.Insert(newIndex, element.Entry);
//            element.transform.SetSiblingIndex(newIndex);
//            magicThis.Field("modified").SetValue(true);
//            return false;
//        }
//    }
    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortDown")]
    public static class TaskManagementWidget_OnSortDown_Patch2
    {
        // public void SetData(SimGameState sim, UnityAction closeCallback)

        public static bool Prefix(
            TaskManagementWidget __instance,
            ref List<TaskManagementElement> ___allMechElements,
            ref List<WorkOrderEntry> ___mechLabQueue,
            ref bool ___modified)
        {
            var sim = Traverse.Create(__instance).Field("Sim").GetValue<SimGameState>();
            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrlHeld) return true;

            // TaskMangementElement is available through allMechElements and contains both the days remaining and mech work queue item
            var widgetElements = ___allMechElements.OrderByDescending(m => m.cumulativeDaysRemaining).ThenBy(m => m.Entry.GUID).ToList();
            var mechLabElements = ___allMechElements.OrderByDescending(x => x.cumulativeDaysRemaining).ThenBy(x => x.Entry.GUID).Select(x => x.Entry).ToList();

            // this appears to be the only thing required in the other patches?
            ___modified = true;
            Logger.Debug("Modified");

            sim.MechLabQueue = mechLabElements;
            
            __instance.OnPooled();
            Logger.Debug("OnPooled");

            // this emulates code found in the assembly plus this class has a field closeCallback so...?
            __instance.SetData(sim, __instance.closeCallback);
            Logger.Debug("SetData");

            return false;
        }
    }

    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortUp")]
    public static class TaskManagementWidget_OnSortUp_Patch2
    {
        public static bool Prefix(
            TaskManagementWidget __instance,
            ref List<TaskManagementElement> ___allMechElements,
            ref List<WorkOrderEntry> ___mechLabQueue,
            ref bool ___modified)
        {
            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrlHeld) return true;

            var sim = Traverse.Create(__instance).Field("Sim").GetValue<SimGameState>();

            // TaskMangementElement is available through allMechElements and contains both the days remaining and mech work queue item
            sim.MechLabQueue = ___allMechElements
                .OrderByDescending(x => x.cumulativeDaysRemaining)
                .ThenBy(x => x.Entry.GUID).Select(x => x.Entry).ToList();
            Logger.Debug("Sorted");

            //sim.MechLabQueue = mechLabElements;
            
            // clears the widget
            ___modified = true;
            Logger.Debug("Modified");
            __instance.OnPooled();
            Logger.Debug("OnPooled");

            // populates the widget with the wrong? data
            __instance.SetData(sim, __instance.closeCallback);
            Logger.Debug("SetData");

            // this appears to be the only thing required in the other patches?
            return false;
        }
    }
}