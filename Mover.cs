using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace FastWorkOrderMover
{
    public static class Mover
    {
        public static void MoveWorkOrderToTop(TaskManagementWidget taskManagementWidget, TaskManagementElement workOrder)
        {
            MoveWorkOrderToEnd(taskManagementWidget, workOrder, End.Top);
        }

        public static void MoveWorkOrderToBottom(TaskManagementWidget taskManagementWidget, TaskManagementElement workOrder)
        {
            MoveWorkOrderToEnd(taskManagementWidget, workOrder, End.Bottom);
        }

        private static void MoveWorkOrderToEnd(TaskManagementWidget taskManagementWidget, TaskManagementElement workOrder, End placement)
        {
            var widgetTraversed = Traverse.Create(taskManagementWidget);
            var tasks = widgetTraversed.Field("allMechElements").GetValue<List<TaskManagementElement>>();
            var workOrders = widgetTraversed.Field("mechLabQueue").GetValue<List<WorkOrderEntry>>();
            var initialIndex = tasks.IndexOf(workOrder);
            int newIndex;
            if (placement == End.Top)
            {
                if (initialIndex == 0) return;
                newIndex = 0;
            }
            else
            {
                if (initialIndex >= tasks.Count) return;
                newIndex = tasks.Count - 1;
            }
            tasks.Remove(workOrder);
            tasks.Insert(newIndex, workOrder);
            workOrders.Remove(workOrder.Entry);
            workOrders.Insert(newIndex, workOrder.Entry);
            workOrder.transform.SetSiblingIndex(newIndex);
        }

        private enum End
        {
            Top,
            Bottom
        }
    }
}