using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;
using UnityEngine.Events;

namespace FastWorkOrderMover
{
    public static class Sorter
    {
        public static void SortWorkOrdersDescending(TaskManagementWidget taskManagementWidget)
        {
            OrderAndConvertTaskManagementEntriesToWorkOrderEntries(taskManagementWidget, Order.Descending);
        }

        public static void SortWorkOrdersAscending(TaskManagementWidget taskManagementWidget)
        {
            OrderAndConvertTaskManagementEntriesToWorkOrderEntries(taskManagementWidget, Order.Ascending);
        }

        private static void OrderAndConvertTaskManagementEntriesToWorkOrderEntries(TaskManagementWidget taskManagementWidget, Order order)
        {
            var widgetTraversed = Traverse.Create(taskManagementWidget);
            var tasks = widgetTraversed.Field("allMechElements").GetValue<List<TaskManagementElement>>();
            var sortedWorkOrders =
                tasks
                    .OrderBy(x => order == Order.Descending ? -x.cumulativeDaysRemaining : x.cumulativeDaysRemaining)
                    .ThenBy(x => x.Entry.GUID)
                    .Select(x => x.Entry)
                    .ToList();
            var sim = widgetTraversed.Field("Sim").GetValue<SimGameState>();
            var closeCb = widgetTraversed.Field("closeCallback").GetValue<UnityAction>();
            Traverse.Create(sim).Property("MechLabQueue").SetValue(sortedWorkOrders);
            taskManagementWidget.SetData(sim, closeCb);
        }

        private enum Order
        {
            Ascending,
            Descending
        }
    }
}