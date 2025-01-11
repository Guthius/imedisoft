using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MedicalOrders
{
    
    public static DataTable GetOrderTable(long patNum, bool includeDiscontinued)
    {
        var table = new DataTable("orders");
        DataRow dataRow;
        table.Columns.Add("date");
        table.Columns.Add("DateTime", typeof(DateTime));
        table.Columns.Add("description");
        table.Columns.Add("MedicalOrderNum");
        table.Columns.Add("MedicationPatNum");
        table.Columns.Add("prov");
        table.Columns.Add("status");
        table.Columns.Add("type");
        var listDataRows = new List<DataRow>();
        var command = "SELECT DateTimeOrder,Description,IsDiscontinued,MedicalOrderNum,MedOrderType,ProvNum "
                      + "FROM medicalorder WHERE PatNum = " + SOut.Long(patNum);
        if (!includeDiscontinued) //only include current orders
            command += " AND IsDiscontinued=0"; //false
        var tableRaw = DataCore.GetTable(command);
        for (var i = 0; i < tableRaw.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            var dateT = SIn.DateTime(tableRaw.Rows[i]["DateTimeOrder"].ToString());
            var medicalOrderType = (MedicalOrderType) SIn.Int(tableRaw.Rows[i]["MedOrderType"].ToString());
            var medicalOrderNum = SIn.Long(tableRaw.Rows[i]["MedicalOrderNum"].ToString());
            dataRow["DateTime"] = dateT;
            dataRow["date"] = dateT.ToShortDateString();
            dataRow["description"] = SIn.String(tableRaw.Rows[i]["Description"].ToString());
            if (medicalOrderType == MedicalOrderType.Laboratory)
            {
                var listLabPanelsForOrder = LabPanels.GetPanelsForOrder(medicalOrderNum);
                for (var p = 0; p < listLabPanelsForOrder.Count; p++)
                {
                    dataRow["description"] += "\r\n     "; //new row for each panel
                    var listLabResults = LabResults.GetForPanel(listLabPanelsForOrder[p].LabPanelNum);
                    if (listLabResults.Count > 0) dataRow["description"] += listLabResults[0].DateTimeTest.ToShortDateString() + " - ";
                    dataRow["description"] += listLabPanelsForOrder[p].ServiceName;
                }
            }

            dataRow["MedicalOrderNum"] = medicalOrderNum.ToString();
            dataRow["MedicationPatNum"] = "0";
            dataRow["prov"] = Providers.GetAbbr(SIn.Long(tableRaw.Rows[i]["ProvNum"].ToString()));
            var isDiscontinued = SIn.Bool(tableRaw.Rows[i]["IsDiscontinued"].ToString());
            if (isDiscontinued)
                dataRow["status"] = "Discontinued";
            else
                dataRow["status"] = "Active";
            dataRow["type"] = medicalOrderType.ToString();
            listDataRows.Add(dataRow);
        }

        //Medications are deprecated for 2014 edition
        //MedicationPats
        //command="SELECT DateStart,DateStop,MedicationPatNum,CASE WHEN medication.MedName IS NULL THEN medicationpat.MedDescript ELSE medication.MedName END MedName,PatNote,ProvNum "
        //	+"FROM medicationpat "
        //	+"LEFT OUTER JOIN medication ON medication.MedicationNum=medicationpat.MedicationNum "
        //	+"WHERE PatNum = "+POut.Long(patNum);
        //if(!includeDiscontinued) {//exclude invalid orders
        //	command+=" AND DateStart > "+POut.Date(new DateTime(1880,1,1))+" AND PatNote !='' "
        //		+"AND (DateStop < "+POut.Date(new DateTime(1880,1,1))+" "//no date stop
        //		+"OR DateStop >= "+POut.Date(DateTime.Today)+")";//date stop hasn't happened yet
        //}
        //DataTable rawMed=DataCore.GetTable(command);
        //DateTime dateStop;
        //for(int i=0;i<rawMed.Rows.Count;i++) {
        //	row=table.NewRow();
        //	dateT=PIn.DateT(rawMed.Rows[i]["DateStart"].ToString());
        //	row["DateTime"]=dateT;
        //	if(dateT.Year<1880) {
        //		row["date"]="";
        //	}
        //	else {
        //		row["date"]=dateT.ToShortDateString();
        //	}
        //	row["description"]=PIn.String(rawMed.Rows[i]["MedName"].ToString())+", "
        //		+PIn.String(rawMed.Rows[i]["PatNote"].ToString());
        //	row["MedicalOrderNum"]="0";
        //	row["MedicationPatNum"]=rawMed.Rows[i]["MedicationPatNum"].ToString();
        //	row["prov"]=Providers.GetAbbr(PIn.Long(rawMed.Rows[i]["ProvNum"].ToString()));
        //	dateStop=PIn.Date(rawMed.Rows[i]["DateStop"].ToString());
        //	if(dateStop.Year<1880 || dateStop>DateTime.Today) {//not stopped or in the future
        //		row["status"]="Active";
        //	}
        //	else {
        //		row["status"]="Discontinued";
        //	}
        //	row["type"]="Medication";
        //	rows.Add(row);
        //}
        //Sorting-----------------------------------------------------------------------------------------
        listDataRows = listDataRows.OrderBy(x => (DateTime) x["DateTime"]).ToList();
        for (var i = 0; i < listDataRows.Count; i++) table.Rows.Add(listDataRows[i]);
        return table;
    }

    /*
    
    public static int GetCountMedical(long patNum){

        string command="SELECT COUNT(*) FROM medicalorder WHERE MedOrderType="+POut.Int((int)MedicalOrderType.Medication)+" "
            +"AND PatNUm="+POut.Long(patNum);
        return PIn.Int(Db.GetCount(command));
    }*/

    
    public static List<MedicalOrder> GetAllLabs(long patNum)
    {
        var command = "SELECT * FROM medicalorder WHERE MedOrderType=" + SOut.Int((int) MedicalOrderType.Laboratory) + " "
                      + "AND PatNum=" + SOut.Long(patNum);
        //NOT EXISTS(SELECT * FROM labpanel WHERE labpanel.MedicalOrderNum=medicalorder.MedicalOrderNum)";
        return MedicalOrderCrud.SelectMany(command);
    }

    
    public static List<MedicalOrder> GetLabsByDate(long patNum, DateTime dateStart, DateTime dateStop)
    {
        var command = "SELECT * FROM medicalorder WHERE MedOrderType=" + SOut.Int((int) MedicalOrderType.Laboratory) + " "
                      + "AND PatNum=" + SOut.Long(patNum) + " "
                      + "AND DATE(DateTimeOrder) >= " + SOut.Date(dateStart) + " "
                      + "AND DATE(DateTimeOrder) <= " + SOut.Date(dateStop);
        //NOT EXISTS(SELECT * FROM labpanel WHERE labpanel.MedicalOrderNum=medicalorder.MedicalOrderNum)";
        return MedicalOrderCrud.SelectMany(command);
    }

    
    public static bool LabHasResultsAttached(long medicalOrderNum)
    {
        var command = "SELECT COUNT(*) FROM labpanel WHERE MedicalOrderNum = " + SOut.Long(medicalOrderNum);
        if (Db.GetCount(command) == "0") return false;

        return true;
    }

    ///<summary>Gets one MedicalOrder from the db.</summary>
    public static MedicalOrder GetOne(long medicalOrderNum)
    {
        return MedicalOrderCrud.SelectOne(medicalOrderNum);
    }

    
    public static long Insert(MedicalOrder medicalOrder)
    {
        return MedicalOrderCrud.Insert(medicalOrder);
    }

    
    public static void Update(MedicalOrder medicalOrder)
    {
        MedicalOrderCrud.Update(medicalOrder);
    }

    
    public static void Delete(long medicalOrderNum)
    {
        string command;
        //validation
        command = "SELECT COUNT(*) FROM labpanel WHERE MedicalOrderNum=" + SOut.Long(medicalOrderNum);
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("MedicalOrders", "Not allowed to delete a lab order that has attached lab panels."));
        //end of validation
        command = "DELETE FROM medicalorder WHERE MedicalOrderNum = " + SOut.Long(medicalOrderNum);
        Db.NonQ(command);
    }
}