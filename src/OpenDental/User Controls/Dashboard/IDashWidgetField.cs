using Imedisoft.Core.Entities;

namespace OpenDental;

internal interface IDashWidgetField
{
    void SetData(PatientDashboardDataEventArgs data, SheetField sheetField);

    void RefreshData(Patient pat, SheetField sheetField);

    void RefreshView();

    void PassLayoutManager(LayoutManagerForms layoutManager);
}