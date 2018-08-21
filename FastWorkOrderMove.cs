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
            if (initialIndex == 0)
            {
                return false;
            }

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

/*
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
*/
    [HarmonyPatch(typeof(TaskManagementWidget), "OnSortDown")]
    public static class TaskManagementWidget_OnSortDown_Patch2
    {
        public static bool Prefix(TaskManagementElement element, TaskManagementWidget __instance,
            ref List<TaskManagementElement> ___allMechElements, ref List<WorkOrderEntry> ___mechLabQueue)
        {
            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var instance = Traverse.Create(__instance);
            if (!ctrlHeld) return true;        

            // zany approach using two sorted dictionaries
            var sortingDic = new SortedDictionary<int, TaskManagementElement>();
            var joinedDic = new SortedDictionary<WorkOrderEntry, TaskManagementElement>();
            var workQueue = new List<WorkOrderEntry>();

            // populate the SortedDictionary with an int for number of days, and the element as value
            foreach (var kvp in ___allMechElements)
            {
                sortingDic.Add(kvp.cumulativeDaysRemaining, kvp);

                workQueue.Sort();
                Logger.Debug($"Added {kvp.name} with {kvp.cumulativeDaysRemaining} days");
            }

            // populate another dictionary with the element and the workorder
            foreach (var kvp in sortingDic)
            {
                joinedDic.Add(kvp.Value.Entry, kvp.Value);
                Logger.Debug($"sortingDic Added\n{kvp.Value.Entry}\n{kvp.Value}");
            }

            Logger.Debug($"joinedDic count is {joinedDic.Count}");
            var elementHolder = new List<TaskManagementElement>();
            var workOrderHolder = new List<WorkOrderEntry>();

            ___allMechElements.Clear();
            ___mechLabQueue.Clear();
            Logger.Debug($"Cleared lists");

            foreach (var kvp in joinedDic)
            {
                elementHolder.Add(kvp.Value);
                Logger.Debug($"Added {kvp.Value}");

                workOrderHolder.Add(kvp.Key);
                Logger.Debug($"Added {kvp.Key}");
            }

            elementHolder.ForEach(x => Logger.Debug($"element: {x}"));
            workOrderHolder.ForEach(x => Logger.Debug($"workorder: {x}"));

            // lists seem to check out fine in the debugger
            ___allMechElements = elementHolder;
            ___mechLabQueue = workOrderHolder;

            Logger.Debug(___mechLabQueue.Count + " " + ___allMechElements);
            foreach (var entry in ___mechLabQueue)
            {
                Logger.Debug($"entry: {entry}");
            }


            // need to do something more than this perhaps?
  
            instance.Field("modified").SetValue(true);
            return false;
        }
    }
}