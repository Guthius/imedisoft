using System;
using System.Windows.Forms;
using OpenDental.Graph.Base;
using OpenDentBusiness;

namespace OpenDental.Graph.Dashboard
{
    public interface IDashboardDockContainer
    {
        DashboardCellType GetCellType();
        string GetCellSettings();
    }

    public class DashboardDockContainer
    {
        public Control Contr { get; }
        public EventHandler OnEditClick { get; }
        public EventHandler OnEditOk { get; }
        public EventHandler OnEditCancel { get; }
        public EventHandler OnDropComplete { get; }
        public EventHandler OnRefreshCache { get; }
        public IODGraphPrinter Printer { get; }
        public TableBase DbItem { get; }

        public DashboardDockContainer(Control c, IODGraphPrinter printer = null, EventHandler onEditClick = null, EventHandler onEditOk = null, EventHandler onEditCancel = null, EventHandler onDropComplete = null, EventHandler onRefreshCache = null, TableBase dbItem = null)
        {
            Contr = c;
            Printer = printer;
            OnEditClick = onEditClick;
            OnEditOk = onEditOk;
            OnEditCancel = onEditCancel;
            OnDropComplete = onDropComplete;
            OnRefreshCache = onRefreshCache;
            DbItem = dbItem;
        }
    }
}