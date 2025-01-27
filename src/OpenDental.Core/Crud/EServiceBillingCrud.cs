
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace OpenDentBusiness.Crud{
	public class EServiceBillingCrud {
		
		public static EServiceBilling SelectOne(long eServiceBillingNum) {
			string command="SELECT * FROM eservicebilling "
				+"WHERE EServiceBillingNum = "+POut.Long(eServiceBillingNum);
			List<EServiceBilling> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}
		
		public static EServiceBilling SelectOne(string command) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<EServiceBilling> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}
		
		public static List<EServiceBilling> SelectMany(string command) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<EServiceBilling> list=TableToList(Db.GetTable(command));
			return list;
		}
		
		public static List<EServiceBilling> TableToList(DataTable table) {
			List<EServiceBilling> retVal=new List<EServiceBilling>();
			EServiceBilling eServiceBilling;
			foreach(DataRow row in table.Rows) {
				eServiceBilling=new EServiceBilling();
				eServiceBilling.EServiceBillingNum      = PIn.Long  (row["EServiceBillingNum"].ToString());
				eServiceBilling.RegistrationKeyNum      = PIn.Long  (row["RegistrationKeyNum"].ToString());
				eServiceBilling.CustPatNum              = PIn.Long  (row["CustPatNum"].ToString());
				eServiceBilling.BillingCycleDay         = PIn.Int   (row["BillingCycleDay"].ToString());
				eServiceBilling.DateTimeEntry           = PIn.DateT (row["DateTimeEntry"].ToString());
				eServiceBilling.DateTimeProceduresPosted= PIn.DateT (row["DateTimeProceduresPosted"].ToString());
				eServiceBilling.DateOfBill              = PIn.Date  (row["DateOfBill"].ToString());
				eServiceBilling.MonthOfBill             = PIn.Date  (row["MonthOfBill"].ToString());
				eServiceBilling.BillCycleStart          = PIn.Date  (row["BillCycleStart"].ToString());
				eServiceBilling.BillCycleEnd            = PIn.Date  (row["BillCycleEnd"].ToString());
				eServiceBilling.UsageCycleStart         = PIn.Date  (row["UsageCycleStart"].ToString());
				eServiceBilling.UsageCycleEnd           = PIn.Date  (row["UsageCycleEnd"].ToString());
				eServiceBilling.ProceduresJson          = PIn.String(row["ProceduresJson"].ToString());
				eServiceBilling.ChargesJson             = PIn.String(row["ChargesJson"].ToString());
				eServiceBilling.NexmoInfoJson           = PIn.String(row["NexmoInfoJson"].ToString());
				eServiceBilling.LogInfo                 = PIn.String(row["LogInfo"].ToString());
				eServiceBilling.ItemizedCharges         = PIn.String(row["ItemizedCharges"].ToString());
				retVal.Add(eServiceBilling);
			}
			return retVal;
		}
		
		public static DataTable ListToTable(List<EServiceBilling> listEServiceBillings,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="EServiceBilling";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("EServiceBillingNum");
			table.Columns.Add("RegistrationKeyNum");
			table.Columns.Add("CustPatNum");
			table.Columns.Add("BillingCycleDay");
			table.Columns.Add("DateTimeEntry");
			table.Columns.Add("DateTimeProceduresPosted");
			table.Columns.Add("DateOfBill");
			table.Columns.Add("MonthOfBill");
			table.Columns.Add("BillCycleStart");
			table.Columns.Add("BillCycleEnd");
			table.Columns.Add("UsageCycleStart");
			table.Columns.Add("UsageCycleEnd");
			table.Columns.Add("ProceduresJson");
			table.Columns.Add("ChargesJson");
			table.Columns.Add("NexmoInfoJson");
			table.Columns.Add("LogInfo");
			table.Columns.Add("ItemizedCharges");
			foreach(EServiceBilling eServiceBilling in listEServiceBillings) {
				table.Rows.Add(new object[] {
					POut.Long  (eServiceBilling.EServiceBillingNum),
					POut.Long  (eServiceBilling.RegistrationKeyNum),
					POut.Long  (eServiceBilling.CustPatNum),
					POut.Int   (eServiceBilling.BillingCycleDay),
					POut.DateT (eServiceBilling.DateTimeEntry,false),
					POut.DateT (eServiceBilling.DateTimeProceduresPosted,false),
					POut.DateT (eServiceBilling.DateOfBill,false),
					POut.DateT (eServiceBilling.MonthOfBill,false),
					POut.DateT (eServiceBilling.BillCycleStart,false),
					POut.DateT (eServiceBilling.BillCycleEnd,false),
					POut.DateT (eServiceBilling.UsageCycleStart,false),
					POut.DateT (eServiceBilling.UsageCycleEnd,false),
					            eServiceBilling.ProceduresJson,
					            eServiceBilling.ChargesJson,
					            eServiceBilling.NexmoInfoJson,
					            eServiceBilling.LogInfo,
					            eServiceBilling.ItemizedCharges,
				});
			}
			return table;
		}
		
		public static long Insert(EServiceBilling eServiceBilling) {
			return Insert(eServiceBilling,false);
		}
		
		public static long Insert(EServiceBilling eServiceBilling,bool useExistingPK) {
			if(!useExistingPK && PrefC.RandomKeys) {
				eServiceBilling.EServiceBillingNum=ReplicationServers.GetKey("eservicebilling","EServiceBillingNum");
			}
			string command="INSERT INTO eservicebilling (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="EServiceBillingNum,";
			}
			command+="RegistrationKeyNum,CustPatNum,BillingCycleDay,DateTimeEntry,DateTimeProceduresPosted,DateOfBill,MonthOfBill,BillCycleStart,BillCycleEnd,UsageCycleStart,UsageCycleEnd,ProceduresJson,ChargesJson,NexmoInfoJson,LogInfo,ItemizedCharges) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(eServiceBilling.EServiceBillingNum)+",";
			}
			command+=
				     POut.Long  (eServiceBilling.RegistrationKeyNum)+","
				+    POut.Long  (eServiceBilling.CustPatNum)+","
				+    POut.Int   (eServiceBilling.BillingCycleDay)+","
				+    DbHelper.Now()+","
				+    POut.DateT (eServiceBilling.DateTimeProceduresPosted)+","
				+    POut.Date  (eServiceBilling.DateOfBill)+","
				+    POut.Date  (eServiceBilling.MonthOfBill)+","
				+    POut.Date  (eServiceBilling.BillCycleStart)+","
				+    POut.Date  (eServiceBilling.BillCycleEnd)+","
				+    POut.Date  (eServiceBilling.UsageCycleStart)+","
				+    POut.Date  (eServiceBilling.UsageCycleEnd)+","
				+    DbHelper.ParamChar+"paramProceduresJson,"
				+    DbHelper.ParamChar+"paramChargesJson,"
				+    DbHelper.ParamChar+"paramNexmoInfoJson,"
				+    DbHelper.ParamChar+"paramLogInfo,"
				+    DbHelper.ParamChar+"paramItemizedCharges)";
			if(eServiceBilling.ProceduresJson==null) {
				eServiceBilling.ProceduresJson="";
			}
			OdSqlParameter paramProceduresJson=new OdSqlParameter("paramProceduresJson",OdDbType.Text,POut.StringParam(eServiceBilling.ProceduresJson));
			if(eServiceBilling.ChargesJson==null) {
				eServiceBilling.ChargesJson="";
			}
			OdSqlParameter paramChargesJson=new OdSqlParameter("paramChargesJson",OdDbType.Text,POut.StringParam(eServiceBilling.ChargesJson));
			if(eServiceBilling.NexmoInfoJson==null) {
				eServiceBilling.NexmoInfoJson="";
			}
			OdSqlParameter paramNexmoInfoJson=new OdSqlParameter("paramNexmoInfoJson",OdDbType.Text,POut.StringParam(eServiceBilling.NexmoInfoJson));
			if(eServiceBilling.LogInfo==null) {
				eServiceBilling.LogInfo="";
			}
			OdSqlParameter paramLogInfo=new OdSqlParameter("paramLogInfo",OdDbType.Text,POut.StringParam(eServiceBilling.LogInfo));
			if(eServiceBilling.ItemizedCharges==null) {
				eServiceBilling.ItemizedCharges="";
			}
			OdSqlParameter paramItemizedCharges=new OdSqlParameter("paramItemizedCharges",OdDbType.Text,POut.StringParam(eServiceBilling.ItemizedCharges));
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command,paramProceduresJson,paramChargesJson,paramNexmoInfoJson,paramLogInfo,paramItemizedCharges);
			}
			else {
				eServiceBilling.EServiceBillingNum=Db.NonQ(command,true,"EServiceBillingNum","eServiceBilling",paramProceduresJson,paramChargesJson,paramNexmoInfoJson,paramLogInfo,paramItemizedCharges);
			}
			return eServiceBilling.EServiceBillingNum;
		}
		
		public static long InsertNoCache(EServiceBilling eServiceBilling) {
			return InsertNoCache(eServiceBilling,false);
		}
		
		public static long InsertNoCache(EServiceBilling eServiceBilling,bool useExistingPK) {
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO eservicebilling (";
			if(!useExistingPK && isRandomKeys) {
				eServiceBilling.EServiceBillingNum=ReplicationServers.GetKeyNoCache("eservicebilling","EServiceBillingNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="EServiceBillingNum,";
			}
			command+="RegistrationKeyNum,CustPatNum,BillingCycleDay,DateTimeEntry,DateTimeProceduresPosted,DateOfBill,MonthOfBill,BillCycleStart,BillCycleEnd,UsageCycleStart,UsageCycleEnd,ProceduresJson,ChargesJson,NexmoInfoJson,LogInfo,ItemizedCharges) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(eServiceBilling.EServiceBillingNum)+",";
			}
			command+=
				     POut.Long  (eServiceBilling.RegistrationKeyNum)+","
				+    POut.Long  (eServiceBilling.CustPatNum)+","
				+    POut.Int   (eServiceBilling.BillingCycleDay)+","
				+    DbHelper.Now()+","
				+    POut.DateT (eServiceBilling.DateTimeProceduresPosted)+","
				+    POut.Date  (eServiceBilling.DateOfBill)+","
				+    POut.Date  (eServiceBilling.MonthOfBill)+","
				+    POut.Date  (eServiceBilling.BillCycleStart)+","
				+    POut.Date  (eServiceBilling.BillCycleEnd)+","
				+    POut.Date  (eServiceBilling.UsageCycleStart)+","
				+    POut.Date  (eServiceBilling.UsageCycleEnd)+","
				+    DbHelper.ParamChar+"paramProceduresJson,"
				+    DbHelper.ParamChar+"paramChargesJson,"
				+    DbHelper.ParamChar+"paramNexmoInfoJson,"
				+    DbHelper.ParamChar+"paramLogInfo,"
				+    DbHelper.ParamChar+"paramItemizedCharges)";
			if(eServiceBilling.ProceduresJson==null) {
				eServiceBilling.ProceduresJson="";
			}
			OdSqlParameter paramProceduresJson=new OdSqlParameter("paramProceduresJson",OdDbType.Text,POut.StringParam(eServiceBilling.ProceduresJson));
			if(eServiceBilling.ChargesJson==null) {
				eServiceBilling.ChargesJson="";
			}
			OdSqlParameter paramChargesJson=new OdSqlParameter("paramChargesJson",OdDbType.Text,POut.StringParam(eServiceBilling.ChargesJson));
			if(eServiceBilling.NexmoInfoJson==null) {
				eServiceBilling.NexmoInfoJson="";
			}
			OdSqlParameter paramNexmoInfoJson=new OdSqlParameter("paramNexmoInfoJson",OdDbType.Text,POut.StringParam(eServiceBilling.NexmoInfoJson));
			if(eServiceBilling.LogInfo==null) {
				eServiceBilling.LogInfo="";
			}
			OdSqlParameter paramLogInfo=new OdSqlParameter("paramLogInfo",OdDbType.Text,POut.StringParam(eServiceBilling.LogInfo));
			if(eServiceBilling.ItemizedCharges==null) {
				eServiceBilling.ItemizedCharges="";
			}
			OdSqlParameter paramItemizedCharges=new OdSqlParameter("paramItemizedCharges",OdDbType.Text,POut.StringParam(eServiceBilling.ItemizedCharges));
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command,paramProceduresJson,paramChargesJson,paramNexmoInfoJson,paramLogInfo,paramItemizedCharges);
			}
			else {
				eServiceBilling.EServiceBillingNum=Db.NonQ(command,true,"EServiceBillingNum","eServiceBilling",paramProceduresJson,paramChargesJson,paramNexmoInfoJson,paramLogInfo,paramItemizedCharges);
			}
			return eServiceBilling.EServiceBillingNum;
		}
		
		public static void Update(EServiceBilling eServiceBilling) {
			string command="UPDATE eservicebilling SET "
				+"RegistrationKeyNum      =  "+POut.Long  (eServiceBilling.RegistrationKeyNum)+", "
				+"CustPatNum              =  "+POut.Long  (eServiceBilling.CustPatNum)+", "
				+"BillingCycleDay         =  "+POut.Int   (eServiceBilling.BillingCycleDay)+", "
				//DateTimeEntry not allowed to change
				+"DateTimeProceduresPosted=  "+POut.DateT (eServiceBilling.DateTimeProceduresPosted)+", "
				+"DateOfBill              =  "+POut.Date  (eServiceBilling.DateOfBill)+", "
				+"MonthOfBill             =  "+POut.Date  (eServiceBilling.MonthOfBill)+", "
				+"BillCycleStart          =  "+POut.Date  (eServiceBilling.BillCycleStart)+", "
				+"BillCycleEnd            =  "+POut.Date  (eServiceBilling.BillCycleEnd)+", "
				+"UsageCycleStart         =  "+POut.Date  (eServiceBilling.UsageCycleStart)+", "
				+"UsageCycleEnd           =  "+POut.Date  (eServiceBilling.UsageCycleEnd)+", "
				+"ProceduresJson          =  "+DbHelper.ParamChar+"paramProceduresJson, "
				+"ChargesJson             =  "+DbHelper.ParamChar+"paramChargesJson, "
				+"NexmoInfoJson           =  "+DbHelper.ParamChar+"paramNexmoInfoJson, "
				+"LogInfo                 =  "+DbHelper.ParamChar+"paramLogInfo, "
				+"ItemizedCharges         =  "+DbHelper.ParamChar+"paramItemizedCharges "
				+"WHERE EServiceBillingNum = "+POut.Long(eServiceBilling.EServiceBillingNum);
			if(eServiceBilling.ProceduresJson==null) {
				eServiceBilling.ProceduresJson="";
			}
			OdSqlParameter paramProceduresJson=new OdSqlParameter("paramProceduresJson",OdDbType.Text,POut.StringParam(eServiceBilling.ProceduresJson));
			if(eServiceBilling.ChargesJson==null) {
				eServiceBilling.ChargesJson="";
			}
			OdSqlParameter paramChargesJson=new OdSqlParameter("paramChargesJson",OdDbType.Text,POut.StringParam(eServiceBilling.ChargesJson));
			if(eServiceBilling.NexmoInfoJson==null) {
				eServiceBilling.NexmoInfoJson="";
			}
			OdSqlParameter paramNexmoInfoJson=new OdSqlParameter("paramNexmoInfoJson",OdDbType.Text,POut.StringParam(eServiceBilling.NexmoInfoJson));
			if(eServiceBilling.LogInfo==null) {
				eServiceBilling.LogInfo="";
			}
			OdSqlParameter paramLogInfo=new OdSqlParameter("paramLogInfo",OdDbType.Text,POut.StringParam(eServiceBilling.LogInfo));
			if(eServiceBilling.ItemizedCharges==null) {
				eServiceBilling.ItemizedCharges="";
			}
			OdSqlParameter paramItemizedCharges=new OdSqlParameter("paramItemizedCharges",OdDbType.Text,POut.StringParam(eServiceBilling.ItemizedCharges));
			Db.NonQ(command,paramProceduresJson,paramChargesJson,paramNexmoInfoJson,paramLogInfo,paramItemizedCharges);
		}
		
		public static bool Update(EServiceBilling eServiceBilling,EServiceBilling oldEServiceBilling) {
			string command="";
			if(eServiceBilling.RegistrationKeyNum != oldEServiceBilling.RegistrationKeyNum) {
				if(command!="") { command+=",";}
				command+="RegistrationKeyNum = "+POut.Long(eServiceBilling.RegistrationKeyNum)+"";
			}
			if(eServiceBilling.CustPatNum != oldEServiceBilling.CustPatNum) {
				if(command!="") { command+=",";}
				command+="CustPatNum = "+POut.Long(eServiceBilling.CustPatNum)+"";
			}
			if(eServiceBilling.BillingCycleDay != oldEServiceBilling.BillingCycleDay) {
				if(command!="") { command+=",";}
				command+="BillingCycleDay = "+POut.Int(eServiceBilling.BillingCycleDay)+"";
			}
			//DateTimeEntry not allowed to change
			if(eServiceBilling.DateTimeProceduresPosted != oldEServiceBilling.DateTimeProceduresPosted) {
				if(command!="") { command+=",";}
				command+="DateTimeProceduresPosted = "+POut.DateT(eServiceBilling.DateTimeProceduresPosted)+"";
			}
			if(eServiceBilling.DateOfBill.Date != oldEServiceBilling.DateOfBill.Date) {
				if(command!="") { command+=",";}
				command+="DateOfBill = "+POut.Date(eServiceBilling.DateOfBill)+"";
			}
			if(eServiceBilling.MonthOfBill.Date != oldEServiceBilling.MonthOfBill.Date) {
				if(command!="") { command+=",";}
				command+="MonthOfBill = "+POut.Date(eServiceBilling.MonthOfBill)+"";
			}
			if(eServiceBilling.BillCycleStart.Date != oldEServiceBilling.BillCycleStart.Date) {
				if(command!="") { command+=",";}
				command+="BillCycleStart = "+POut.Date(eServiceBilling.BillCycleStart)+"";
			}
			if(eServiceBilling.BillCycleEnd.Date != oldEServiceBilling.BillCycleEnd.Date) {
				if(command!="") { command+=",";}
				command+="BillCycleEnd = "+POut.Date(eServiceBilling.BillCycleEnd)+"";
			}
			if(eServiceBilling.UsageCycleStart.Date != oldEServiceBilling.UsageCycleStart.Date) {
				if(command!="") { command+=",";}
				command+="UsageCycleStart = "+POut.Date(eServiceBilling.UsageCycleStart)+"";
			}
			if(eServiceBilling.UsageCycleEnd.Date != oldEServiceBilling.UsageCycleEnd.Date) {
				if(command!="") { command+=",";}
				command+="UsageCycleEnd = "+POut.Date(eServiceBilling.UsageCycleEnd)+"";
			}
			if(eServiceBilling.ProceduresJson != oldEServiceBilling.ProceduresJson) {
				if(command!="") { command+=",";}
				command+="ProceduresJson = "+DbHelper.ParamChar+"paramProceduresJson";
			}
			if(eServiceBilling.ChargesJson != oldEServiceBilling.ChargesJson) {
				if(command!="") { command+=",";}
				command+="ChargesJson = "+DbHelper.ParamChar+"paramChargesJson";
			}
			if(eServiceBilling.NexmoInfoJson != oldEServiceBilling.NexmoInfoJson) {
				if(command!="") { command+=",";}
				command+="NexmoInfoJson = "+DbHelper.ParamChar+"paramNexmoInfoJson";
			}
			if(eServiceBilling.LogInfo != oldEServiceBilling.LogInfo) {
				if(command!="") { command+=",";}
				command+="LogInfo = "+DbHelper.ParamChar+"paramLogInfo";
			}
			if(eServiceBilling.ItemizedCharges != oldEServiceBilling.ItemizedCharges) {
				if(command!="") { command+=",";}
				command+="ItemizedCharges = "+DbHelper.ParamChar+"paramItemizedCharges";
			}
			if(command=="") {
				return false;
			}
			if(eServiceBilling.ProceduresJson==null) {
				eServiceBilling.ProceduresJson="";
			}
			OdSqlParameter paramProceduresJson=new OdSqlParameter("paramProceduresJson",OdDbType.Text,POut.StringParam(eServiceBilling.ProceduresJson));
			if(eServiceBilling.ChargesJson==null) {
				eServiceBilling.ChargesJson="";
			}
			OdSqlParameter paramChargesJson=new OdSqlParameter("paramChargesJson",OdDbType.Text,POut.StringParam(eServiceBilling.ChargesJson));
			if(eServiceBilling.NexmoInfoJson==null) {
				eServiceBilling.NexmoInfoJson="";
			}
			OdSqlParameter paramNexmoInfoJson=new OdSqlParameter("paramNexmoInfoJson",OdDbType.Text,POut.StringParam(eServiceBilling.NexmoInfoJson));
			if(eServiceBilling.LogInfo==null) {
				eServiceBilling.LogInfo="";
			}
			OdSqlParameter paramLogInfo=new OdSqlParameter("paramLogInfo",OdDbType.Text,POut.StringParam(eServiceBilling.LogInfo));
			if(eServiceBilling.ItemizedCharges==null) {
				eServiceBilling.ItemizedCharges="";
			}
			OdSqlParameter paramItemizedCharges=new OdSqlParameter("paramItemizedCharges",OdDbType.Text,POut.StringParam(eServiceBilling.ItemizedCharges));
			command="UPDATE eservicebilling SET "+command
				+" WHERE EServiceBillingNum = "+POut.Long(eServiceBilling.EServiceBillingNum);
			Db.NonQ(command,paramProceduresJson,paramChargesJson,paramNexmoInfoJson,paramLogInfo,paramItemizedCharges);
			return true;
		}
		
		public static bool UpdateComparison(EServiceBilling eServiceBilling,EServiceBilling oldEServiceBilling) {
			if(eServiceBilling.RegistrationKeyNum != oldEServiceBilling.RegistrationKeyNum) {
				return true;
			}
			if(eServiceBilling.CustPatNum != oldEServiceBilling.CustPatNum) {
				return true;
			}
			if(eServiceBilling.BillingCycleDay != oldEServiceBilling.BillingCycleDay) {
				return true;
			}
			//DateTimeEntry not allowed to change
			if(eServiceBilling.DateTimeProceduresPosted != oldEServiceBilling.DateTimeProceduresPosted) {
				return true;
			}
			if(eServiceBilling.DateOfBill.Date != oldEServiceBilling.DateOfBill.Date) {
				return true;
			}
			if(eServiceBilling.MonthOfBill.Date != oldEServiceBilling.MonthOfBill.Date) {
				return true;
			}
			if(eServiceBilling.BillCycleStart.Date != oldEServiceBilling.BillCycleStart.Date) {
				return true;
			}
			if(eServiceBilling.BillCycleEnd.Date != oldEServiceBilling.BillCycleEnd.Date) {
				return true;
			}
			if(eServiceBilling.UsageCycleStart.Date != oldEServiceBilling.UsageCycleStart.Date) {
				return true;
			}
			if(eServiceBilling.UsageCycleEnd.Date != oldEServiceBilling.UsageCycleEnd.Date) {
				return true;
			}
			if(eServiceBilling.ProceduresJson != oldEServiceBilling.ProceduresJson) {
				return true;
			}
			if(eServiceBilling.ChargesJson != oldEServiceBilling.ChargesJson) {
				return true;
			}
			if(eServiceBilling.NexmoInfoJson != oldEServiceBilling.NexmoInfoJson) {
				return true;
			}
			if(eServiceBilling.LogInfo != oldEServiceBilling.LogInfo) {
				return true;
			}
			if(eServiceBilling.ItemizedCharges != oldEServiceBilling.ItemizedCharges) {
				return true;
			}
			return false;
		}
		
		public static void Delete(long eServiceBillingNum) {
			string command="DELETE FROM eservicebilling "
				+"WHERE EServiceBillingNum = "+POut.Long(eServiceBillingNum);
			Db.NonQ(command);
		}
		
		public static void DeleteMany(List<long> listEServiceBillingNums) {
			if(listEServiceBillingNums==null || listEServiceBillingNums.Count==0) {
				return;
			}
			string command="DELETE FROM eservicebilling "
				+"WHERE EServiceBillingNum IN("+string.Join(",",listEServiceBillingNums.Select(x => POut.Long(x)))+")";
			Db.NonQ(command);
		}
	}
}