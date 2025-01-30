using System;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.Graph.Base;
using OpenDentBusiness;

namespace OpenDental.Graph.Dashboard;

public interface IDashboardDockContainer
{
    DashboardCellType GetCellType();
    string GetCellSettings();
}

public class DashboardDockContainer(Control c, IODGraphPrinter printer = null, EventHandler onEditClick = null, EventHandler onEditOk = null, EventHandler onEditCancel = null, EventHandler onDropComplete = null, EventHandler onRefreshCache = null, TableBase dbItem = null)
{
    public Control Contr { get; } = c;
    public EventHandler OnEditClick { get; } = onEditClick;
    public EventHandler OnEditOk { get; } = onEditOk;
    public EventHandler OnEditCancel { get; } = onEditCancel;
    public EventHandler OnDropComplete { get; } = onDropComplete;
    public EventHandler OnRefreshCache { get; } = onRefreshCache;
    public IODGraphPrinter Printer { get; } = printer;
    public TableBase DbItem { get; } = dbItem;
}