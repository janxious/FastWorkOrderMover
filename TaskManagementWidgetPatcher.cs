using BattleTech.UI;
using Harmony;

namespace FastWorkOrderMover
{
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