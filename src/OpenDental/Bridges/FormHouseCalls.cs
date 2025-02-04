using System;
using System.IO;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDental;

/// <summary></summary>
public partial class FormHouseCalls : FormODBase{
	public Program ProgramCur;

		
	public FormHouseCalls() {
		InitializeComponent();
	}

	private void FormHouseCalls_Load(object sender, System.EventArgs e) {
		textDateFrom.Text=DateTime.Today.AddDays(1).ToShortDateString();
		textDateTo.Text=DateTime.Today.AddDays(7).ToShortDateString();
	}

	private void but7_Click(object sender, System.EventArgs e) {
		textDateFrom.Text=DateTime.Today.AddDays(1).ToShortDateString();
		textDateTo.Text=DateTime.Today.AddDays(7).ToShortDateString();
	}

	private void butAll_Click(object sender, System.EventArgs e) {
		textDateFrom.Text=DateTime.Today.AddDays(1).ToShortDateString();
		textDateTo.Text="";
	}

	private void butOK_Click(object sender, System.EventArgs e) {
		if(!textDateFrom.IsValid() || !textDateTo.IsValid()) {
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		DateTime FromDate;
		DateTime ToDate;
		if(textDateFrom.Text==""){
			ODMessageBox.Show(Lan.g(this,"From Date cannot be left blank."));
			return;
		}
		FromDate=SIn.Date(textDateFrom.Text);
		if(textDateTo.Text=="")
			ToDate=DateTime.MaxValue.AddDays(-1);
		else
			ToDate=SIn.Date(textDateTo.Text);
		//Create the file and first row--------------------------------------------------------
		var ForProgram =ProgramProperties.GetForProgram(ProgramCur.ProgramNum);
		var PPCur=ProgramProperties.GetCur(ForProgram, "Export Path");
		var fileName=PPCur.PropertyValue+"Appt.txt";
		if(!Directory.Exists(PPCur.PropertyValue)){
			Directory.CreateDirectory(PPCur.PropertyValue);
		}
		var sr=File.CreateText(fileName);
		sr.WriteLine("\"LastName\",\"FirstName\",\"PatientNumber\",\"HomePhone\",\"WorkNumber\","
		             +"\"EmailAddress\",\"SendEmail\",\"Address\",\"Address2\",\"City\",\"State\",\"Zip\","
		             +"\"ApptDate\",\"ApptTime\",\"ApptReason\",\"DoctorNumber\",\"DoctorName\",\"IsNewPatient\",\"WirelessPhone\"");
		var table=HouseCallsQueries.GetHouseCalls(FromDate,ToDate);
		var usePatNum=false;
		PPCur=ProgramProperties.GetCur(ForProgram, "Enter 0 to use PatientNum, or 1 to use ChartNum");;
		if(PPCur.PropertyValue=="0"){
			usePatNum=true;
		}
		DateTime aptDT;
		for(var i=0;i<table.Rows.Count;i++){
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][0].ToString()))+"\",");//0-LastName
			if(table.Rows[i][2].ToString()!=""){//if Preferred Name exists
				sr.Write("\""+Dequote(SIn.String(table.Rows[i][2].ToString()))+"\",");//2-PrefName
			}
			else{
				sr.Write("\""+Dequote(SIn.String(table.Rows[i][1].ToString()))+"\",");//1-FirstName 
			}
			if(usePatNum){
				sr.Write("\""+table.Rows[i][3]+"\",");//3-PatNum
			}
			else{
				sr.Write("\""+Dequote(SIn.String(table.Rows[i][4].ToString()))+"\",");//4-ChartNumber
			}
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][5].ToString()))+"\",");//5-HomePhone
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][6].ToString()))+"\",");//6-WorkNumber
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][7].ToString()))+"\",");//7-EmailAddress
			if(table.Rows[i][7].ToString()!=""){//if an email exists
				sr.Write("\"T\",");//SendEmail
			}
			else{
				sr.Write("\"F\",");
			}
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][8].ToString()))+"\",");//8-Address
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][9].ToString()))+"\",");//9-Address2
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][10].ToString()))+"\",");//10-City
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][11].ToString()))+"\",");//11-State
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][12].ToString()))+"\",");//12-Zip
			aptDT=SIn.DateTime(table.Rows[i][13].ToString());
			sr.Write("\""+aptDT.ToString("MM/dd/yyyy")+"\",");//13-ApptDate
			sr.Write("\""+aptDT.ToString("hh:mm tt")+"\",");//13-ApptTime eg 01:30 PM
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][14].ToString()))+"\",");//14-ApptReason
			sr.Write("\""+table.Rows[i][15]+"\",");//15-DoctorNumber. might possibly be 0
			//15-DoctorName. Can handle 0 without any problem.
			sr.Write("\""+Dequote(Providers.GetLastName(SIn.Long(table.Rows[i][15].ToString())))+"\",");
			if(table.Rows[i][16].ToString()=="1"){//16-IsNewPatient
				sr.Write("\"T\",");//SendEmail
			}
			else{
				sr.Write("\"F\",");
			}
			sr.Write("\""+Dequote(SIn.String(table.Rows[i][17].ToString()))+"\"");//17-WirelessPhone
			sr.WriteLine();//Must be last.
		}
		sr.Close();
		ODMessageBox.Show("Done");
		DialogResult=DialogResult.OK;
	}

	private string Dequote(string inputStr){
		return inputStr.Replace("\"","");
	}

}