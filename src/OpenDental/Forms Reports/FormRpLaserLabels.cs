// Laser Labels:
// Patient,Insurance Company,Custom,Birthday
//
// By Bill MacWilliams     Kapricorn Systems Inc.
//
//
// Code is broken up into Common section and a section for each Tab
//
// Some code is copied from other sections of OpenDental
//
// Report was designed in response to Client requests for more types of sheet labels
// with more selection choices.
//
// Last modification Date: 12/29/2007
//



using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Globalization;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Logic;


namespace OpenDental;

public partial class FormRpLaserLabels:FormODBase {
	private DataTable AddrTable;
	private DataTable RptAddrTable;
	private int pagesPrinted;
	private int labelsPrinted;
	private int insRange;
	private string[] colName = new string[20];
	private int iLabelStart=0;
	private System.Windows.Forms.PictureBox[] picLabel = new System.Windows.Forms.PictureBox[30];
	private List<ProviderDto> _listProviders;

	public FormRpLaserLabels() {
		InitializeComponent();
	}

	//
	// Load initial data for providers and patient status
	//
	private void FormLaserLabels_Load(object sender,System.EventArgs e) {
		_listProviders=Providers.GetDeepCopy(true);
		listProviders.Items.AddList(_listProviders,x => x.Description);
		if(listProviders.Items.Count>0) {
			listProviders.SelectedIndex=0;
		}
		checkAllProviders.Checked=true;
		listProviders.Visible=false;
		//If you change listStatus, be sure the change is reflected in BuildPatStatList.
		listStatus.Items.Add(Lan.g("enumPatientStatus","Patient"));
		listStatus.Items.Add(Lan.g("enumPatientStatus","NonPatient"));
		listStatus.Items.Add(Lan.g("enumPatientStatus","Inactive"));
		listStatus.Items.Add(Lan.g("enumPatientStatus","Archived"));
		listStatus.Items.Add(Lan.g("enumPatientStatus","Deceased"));
		listStatus.Items.Add(Lan.g("enumPatientStatus","Prospective"));
		displayLabels(iLabelStart);
		SetNextMonth();
	}

	//
	//Common Area for All Tabs in Laser Labels Report
	//

	private void butOK_Click(object sender,EventArgs e) {
		string patStat;
		string command;
		switch(tabLabelSetup.SelectedIndex) {
			//
			// Patient Tab Fill Address Table with selected information
			//
			case 0:
				if(!checkActiveOnly.Checked && listStatus.SelectedIndices.Count==0) {
					MsgBox.Show(this,"At least one patient status must be selected.");
					return;
				}
				if(checkAllProviders.Checked==true) {
					listProviders.SetAll(true);
				}
				string whereProv;//used as the provider portion of the where clauses.
				//each whereProv needs to be set up separately for each query
				whereProv="patient.PriProv in (";
				for(var i=0;i<listProviders.SelectedIndices.Count;i++) {
					if(i>0) {
						whereProv += ",";
					}
					whereProv += "'" + SOut.Long(_listProviders[listProviders.SelectedIndices[i]].Id) + "'";
				}
				whereProv += ") ";
				patStat = BuildPatStatList(checkActiveOnly.Checked);
				command = SetPatientBaseSelect();
				if(checkGroupByFamily.Checked) {
					command+=" INNER JOIN patient familymembers on familymembers.Guarantor=patient.Guarantor AND "+patStat.Replace("patient.","familymembers.");
					command+=" WHERE CONCAT(CONCAT(CONCAT(CONCAT(familymembers.LName,', '),familymembers.FName),' '),familymembers.MiddleI) >= ";
				}
				else {
					command+=" WHERE CONCAT(CONCAT(CONCAT(CONCAT(patient.LName,', '),patient.FName),' '),patient.MiddleI) >= ";
				}
				command+="'"+SOut.String(textStartName.Text)+"'";
				if(checkGroupByFamily.Checked) {
					command+=" AND CONCAT(CONCAT(CONCAT(CONCAT(familymembers.LName,', '),familymembers.FName),' '),familymembers.MiddleI) <= ";
				}
				else {
					command+=" AND CONCAT(CONCAT(CONCAT(CONCAT(patient.LName,', '),patient.FName),' '),patient.MiddleI) <= ";
				}
				command+="'"+SOut.String(textEndName.Text)+"'";
				if(checkGroupByFamily.Checked) {
					command+=" AND patient.Guarantor=patient.PatNum";
				}
				else {
					command+=" AND "+patStat;
				}
				command+=" AND " + whereProv;
				if(checkGroupByFamily.Checked) {
					command+=" GROUP BY patient.Guarantor ";
				}
				command+=" ORDER BY CONCAT(CONCAT(CONCAT(CONCAT(patient.LName,', '),patient.FName),' '),patient.MiddleI)";
				buildLabelTable(command);
				break;
			//
			// Insurance Company Builder
			//
			case 1:
				command = "SELECT carrier.CarrierName,carrier.Address,carrier.Address2,carrier.City,carrier.State,carrier.Zip FROM carrier";
				if(radioButSingle.Checked == true) {
					if(labInsCoStartAddr.Text=="") {
						MsgBox.Show(this,"Please use the selection button first.");
						return;
					}
					command += " WHERE " + DbHelper.Concat("carrier.CarrierName","carrier.Address") + " = '" + textInsCoStart.Text + labInsCoStartAddr.Text + "'";
					RptAddrTable = DataCore.GetTable(command);
					if(RptAddrTable.Rows.Count==0) {
						MsgBox.Show(this,"No matching carriers found.");
						return;
					}
					AddrTable = RptAddrTable.Clone();
					var numLabels=(int)numericInsCoSingle.Value;
					for(var i=0;i<numLabels;i++) {
						AddrTable.ImportRow(RptAddrTable.Rows[0]);
					}
					buildLabels();
				}
				else {
					command += " WHERE CONCAT(CONCAT(carrier.CarrierName,carrier.Address),carrier.City) >= '" + textInsCoStart.Text + labInsCoStartAddr.Text + "' AND CONCAT(CONCAT(carrier.CarrierName,carrier.Address),carrier.City) <= '" + textInsCoEnd.Text + labInsCoEndAddr + "'";
					command += " ORDER BY CONCAT(CONCAT(carrier.CarrierName,carrier.Address),carrier.City)";
					buildLabelTable(command);
				}
				break;

			//
			// Custom Label Builder
			//
			case 2:
				var CusTable = new DataTable("CustomTable");
				var CusCol = new DataColumn();
				CusCol = new DataColumn("Name",typeof(string));
				CusTable.Columns.Add(CusCol);
				CusCol = new DataColumn("Address",typeof(string));
				CusTable.Columns.Add(CusCol);
				CusCol = new DataColumn("Address2",typeof(string));
				CusTable.Columns.Add(CusCol);
				CusCol = new DataColumn("City",typeof(string));
				CusTable.Columns.Add(CusCol);
				CusCol = new DataColumn("State",typeof(string));
				CusTable.Columns.Add(CusCol);
				CusCol = new DataColumn("Zip",typeof(string));
				CusTable.Columns.Add(CusCol);
				var tmpString="";
				for(var i=0;i<listCusNames.Items.Count;i++) {
					tmpString=listCusNames.Items.GetTextShowingAt(i);
					var split=tmpString.Split('>');
					var numLabels=Convert.ToInt32(split[6]);
					for(var j=0;j<numLabels;j++) {
						var CusRow=CusTable.NewRow();
						for(var k=0;k<6;k++) {
							CusRow[k]=split[k].TrimStart(' ');
						}
						CusTable.Rows.Add(CusRow);
					}
				}
				AddrTable = CusTable.Copy();
				buildLabels();
				break;
			//
			//Birthday Labels Builder
			//
			case 3:
				if(!checkBirthdayActive.Checked && listStatus.SelectedIndices.Count==0) {
					MsgBox.Show(this,"At least one patient status must be selected.");
					return;
				}
				var dateBirthdayFrom = SIn.Date(textBirthdayFrom.Text);
				var dateBirthdayTo = SIn.Date(textBirthdayTo.Text);
				if(dateBirthdayTo < dateBirthdayFrom) {
					MsgBox.Show(this,"To date cannot be before From date.");
					return;
				}
				patStat = BuildPatStatList(checkBirthdayActive.Checked);
				command = SetPatientBaseSelect();
				command += "WHERE SUBSTRING(Birthdate,6,5) >= '" + dateBirthdayFrom.ToString("MM-dd") + "' "
				           + "AND SUBSTRING(Birthdate,6,5) <= '" + dateBirthdayTo.ToString("MM-dd") + "' "
				           + "AND SUBSTRING(Birthdate,1,4) <> '0001' AND " + patStat + " ORDER BY LName,FName";
				buildLabelTable(command);
				break;
			default:
				break;
		}
	}

	private static string SetPatientBaseSelect() {
		string command;
		command = "SELECT patient.LName,patient.FName,patient.MiddleI,patient.Preferred,"//0-3
		          + "patient.Address,patient.Address2,patient.City,patient.State,patient.Zip, "//4-9
		          + "patient.Guarantor,"//10
		          + "'' FamList ";//placeholder column: 11 for patient names and dates. If empty, then only single patient will print
		command += "FROM patient ";
		return command;
	}

	private string BuildPatStatList(bool Active) {
		if(Active==true) {
			listStatus.SetAll(false);
			listStatus.SetSelected(0);
		}
		string patStat;// used as the Patient Status portion
		patStat="patient.patStatus in (";
		for(var i=0;i<listStatus.SelectedIndices.Count;i++) {
			if(i>0) {
				patStat+=",";
			}
			//patStat += "'" + listStatus.SelectedIndices[i] + "'";
			switch(listStatus.SelectedIndices[i]) {
				case 0:
					patStat+=(int)PatientStatus.Patient;
					break;
				case 1:
					patStat+=(int)PatientStatus.NonPatient;
					break;
				case 2:
					patStat+=(int)PatientStatus.Inactive;
					break;
				case 3:
					patStat+=(int)PatientStatus.Archived;
					break;
				case 4:
					patStat+=(int)PatientStatus.Deceased;
					break;
				case 5:
					patStat+=(int)PatientStatus.Prospective;
					break;
			}
		}
		patStat += ") ";
		return patStat;
	}
	private void buildLabelTable(string getData) {
		AddrTable = DataCore.GetTable(getData);
		buildLabels();
	}

	private void addBlankLabels() {
		var i = 0;
		var cnt = 0;
		var valType = "";
		while(cnt < iLabelStart) {
			var AddrRow = AddrTable.NewRow();
			foreach(DataColumn col in AddrTable.Columns) {
				colName[i] = col.ColumnName;
				valType = col.DataType.ToString();
				switch(valType) {
					case "System.String":
						AddrRow[colName[i]] = " ";
						break;
					default:
						AddrRow[colName[i]] = 0;
						break;
				}
				i += 1;
			}
			AddrTable.Rows.InsertAt(AddrRow,0);
			cnt += 1;
			i = 0;
		}
	}

	private void buildLabels() {
		if(AddrTable.Rows.Count > 0) {
			if(iLabelStart > 0) {
				addBlankLabels();
			}
			pagesPrinted = 0;
			labelsPrinted = 0;
			PrinterL.TryPreview(pdLabels_PrintPage,
				Lan.g(this,"Laser labels printed"),
				PrintSituation.LabelSheet,
				new Margins(0,0,0,0),
				PrintoutOrigin.AtMargin,
				totalPages:(int)Math.Ceiling((double)AddrTable.Rows.Count/30)
			);
		}
		else {
			ODMessageBox.Show("No Labels to Print for Selected Criteria");
		}
	}
	public static void FitTextOld(string text,Font font,Brush brush,RectangleF rectF,StringFormat stringFormat,Graphics g) {
		var emSize=font.Size;
		var size=TextRenderer.MeasureText(text,font);
		if(size.Width>=rectF.Width) {
			emSize=emSize*(rectF.Width/size.Width);//get the ratio of the room width to font width and multiply that by the font size
			if(emSize<2) {//don't let the font be smaller than 2 point font
				emSize=2F;
			}
		}
		using(var newFont=new Font(font.FontFamily,emSize,font.Style)) {
			g.DrawString(text,newFont,brush,rectF,stringFormat);
		}
	}
	private void pdLabels_PrintPage(object sender,PrintPageEventArgs ev) {
		var totalPages = (int)Math.Ceiling((double)AddrTable.Rows.Count / 30);
		var g = ev.Graphics;
		float yPos = 75;
		float xPos = 50;
		var text = "";
		while(yPos < 1000 && labelsPrinted < AddrTable.Rows.Count) {
			switch(tabLabelSetup.SelectedIndex) {
				case 0:
				case 3:
					text = AddrTable.Rows[labelsPrinted]["FName"] + " "
					                                              + AddrTable.Rows[labelsPrinted]["MiddleI"] + " "
					                                              + AddrTable.Rows[labelsPrinted]["LName"] + "\r\n";
					break;
				case 1:
					text = AddrTable.Rows[labelsPrinted]["CarrierName"] + "\r\n";
					break;
				case 2:
					text = AddrTable.Rows[labelsPrinted]["Name"] + "\r\n";
					break;
				default:
					return;
			}
			text += AddrTable.Rows[labelsPrinted]["Address"] + "\r\n";
			if(AddrTable.Rows[labelsPrinted]["Address2"].ToString() != "") {
				text += AddrTable.Rows[labelsPrinted]["Address2"] + "\r\n";
			}
			text += AddrTable.Rows[labelsPrinted]["City"].ToString();
			if(text.Trim().Length > 0) {
				text += ", ";
			}
			text += AddrTable.Rows[labelsPrinted]["State"] + "   "
			                                               + AddrTable.Rows[labelsPrinted]["Zip"] + "\r\n";
			var rect=new Rectangle((int)xPos,(int)yPos,275,100);
			FitTextOld(text,new Font(FontFamily.GenericSansSerif,11),Brushes.Black,rect,new StringFormat(),g);
			//reposition for next label
			xPos += 275;
			if(xPos > 850) {//drop a line
				xPos = 50;
				yPos += 100;
			}
			labelsPrinted++;
		}
		pagesPrinted++;
		if(pagesPrinted == totalPages) {
			ev.HasMorePages = false;
			pagesPrinted = 0;//because it has to print again from the print preview
			labelsPrinted = 0;
		}
		else {
			ev.HasMorePages = true;
		}
		g.Dispose();
	}
	//
	// Build Label Display and Add Click to each Label
	//
	private void displayLabels(int istartingpoint) {
		var x = 0;
		var y = 3;
		var cnt = 0;
		for(var i = 0;i < 10;i++) {
			for(var j = 0;j < 3;j++) {
				if(j == 0) x = 4;
				if(j == 1) x = 60;
				if(j == 2) x = 116;
				picLabel[cnt] = new System.Windows.Forms.PictureBox();
				picLabel[cnt].Location = new Point(x,y);
				picLabel[cnt].Size = new Size(51,17);
				picLabel[cnt].SizeMode = PictureBoxSizeMode.StretchImage;
				picLabel[cnt].Click += new EventHandler(clickLabel);
				picLabel[cnt].Tag = cnt.ToString();
				if(cnt < istartingpoint) {
					picLabel[cnt].Image = global::OpenDental.Properties.Resources.DeleteX;
				}
				else {
					picLabel[cnt].Image = global::OpenDental.Properties.Resources.butLabel;
				}
				panLabels.Controls.Add(picLabel[cnt]);
				cnt += 1;
			}
			y += 23;
		}
	}
	private void clickLabel(object sender,EventArgs e) {
		var theLabel = (System.Windows.Forms.PictureBox)sender;
		iLabelStart = Convert.ToInt32(theLabel.Tag);
		for(var i=0;i<30;i++) {
			panLabels.Controls.Remove(picLabel[i]);
		}
		displayLabels(iLabelStart);
	}

	//
	//Add Patient Status List to Current Tab
	//
	private void displayPatStatus(int disTabIdx) {
		listStatus.Location = new System.Drawing.Point(12,190);
		listStatus.Name = "listStatus";
		listStatus.SelectionMode = OpenDental.UI.SelectionMode.MultiExtended;
		listStatus.Size = new System.Drawing.Size(125,108);
		listStatus.TabIndex = 13;
		listStatus.Visible = true;
		switch(disTabIdx) {
			case 0:
				tabPage1.Controls.Add(listStatus);
				break;
			case 3:
				tabBirthday.Controls.Add(listStatus);
				break;
			default:
				break;
		}
	}
	//
	//Patient Tab
	//
	private void checkActiveOnly_CheckedChanged(object sender,EventArgs e) {
		displayPatStatus(tabLabelSetup.SelectedIndex);
		if(checkActiveOnly.Checked) {
			listStatus.Visible=false;
		}
		else {
			listStatus.Visible=true;
		}
	}
	private void checkAllProviders_CheckedChanged(object sender,EventArgs e) {
		if(checkAllProviders.Checked) {
			listProviders.Visible=false;
		}
		else {
			listProviders.Visible=true;
		}
	}

	private void butStartName_Click(object sender,EventArgs e) {
		var frmPatientSelect = new FrmPatientSelect();
		frmPatientSelect.ShowDialog();
		if(frmPatientSelect.IsDialogCancel) {
			return;
		}
		textStartName.Text=Patients.GetPat(frmPatientSelect.PatNumSelected).GetNameLFnoPref();
	}

	private void butEndName_Click(object sender,EventArgs e) {
		var frmPatientSelect = new FrmPatientSelect();
		frmPatientSelect.ShowDialog();
		if(frmPatientSelect.IsDialogCancel) {
			return;
		}
		textEndName.Text=Patients.GetPat(frmPatientSelect.PatNumSelected).GetNameLFnoPref();
		if(string.Compare(textStartName.Text,textEndName.Text)==1) {
			textEndName.Text = textStartName.Text;

		}
	}
	//
	//Insurance Company Tab
	//
	private void radioButSingle_CheckedChanged(object sender,EventArgs e) {
		labInsCoStart.Visible = true;
		labInsCoEnd.Visible = false;
		labInsCoSingle.Visible = true;
		textInsCoStart.Visible = true;
		textInsCoEnd.Visible = false;
		butInsCoEnd.Visible = false;
		numericInsCoSingle.Visible = true;
		insRange = 0;
	}

	private void radioButRange_CheckedChanged(object sender,EventArgs e) {
		labInsCoStart.Visible = true;
		labInsCoEnd.Visible = true;
		labInsCoSingle.Visible = false;
		textInsCoStart.Visible = true;
		textInsCoEnd.Visible = true;
		numericInsCoSingle.Visible = false;
		insRange = 1;
		butInsCoEnd.Visible = true;
	}

	private string FillFromInsCoList(long carrierNum) {
		var carrier = Carriers.GetCarrier(carrierNum);
		if(insRange == 0) {
			textInsCoStart.Text = carrier.CarrierName;
			labInsCoStartAddr.Text = carrier.Address;
		}
		else {
			textInsCoEnd.Text = carrier.CarrierName;
			labInsCoEndAddr.Text = carrier.Address;
		}
		return (carrier.CarrierName);
	}
	private void textInsCoStart_Click(object sender,EventArgs e) {
		//insRange = 0;
	}
	private void textInsCoEnd_Click(object sender,EventArgs e) {
		//insRange = 1;
	}

	private void butInsCo_Click(object sender,EventArgs e) {
		var frmInsPlanSelect = new FrmInsPlanSelect();
		frmInsPlanSelect.ShowDialog();
		if(!frmInsPlanSelect.IsDialogOK) {
			return;
		}
		insRange=0;
		if(sender==butInsCoEnd) {
			insRange=1;
		}
		FillFromInsCoList(frmInsPlanSelect.InsPlanSelected.CarrierNum);
	}
	//
	//Custom Tab
	//
	private void butCusAdd_Click(object sender,EventArgs e) {
		var cusLabelFormat = "";
		cusLabelFormat = textCusName.Text + ">  " + textCusAddr1.Text + ">  " + textCusAddr2.Text + ">  " + textCusCity.Text + ">  " + textCusState.Text + ">  " + textCusZip.Text + ">  " + numericCusCount.Value;
		listCusNames.Items.Add(cusLabelFormat);
		textCusName.Text = "";
		textCusAddr1.Text = "";
		textCusAddr2.Text = "";
		textCusCity.Text = "";
		textCusState.Text = "";
		textCusZip.Text = "";
		numericCusCount.Value = 1;
	}

	private void butCusRemove_Click(object sender,EventArgs e) {
		if(listCusNames.SelectedIndex==-1) {
			MsgBox.Show(this,"At least one name must be selected.");
			return;
		}
		listCusNames.Items.RemoveAt(listCusNames.SelectedIndex);
		butCusRemove.Visible=false;
	}

	private void listCusNames_SelectedIndexChanged(object sender,EventArgs e) {
		butCusRemove.Visible=true;
	}
	//
	//Birthday Tab
	//
	private void checkBirthdayActive_CheckedChanged(object sender,EventArgs e) {
		displayPatStatus(tabLabelSetup.SelectedIndex);
		if(checkBirthdayActive.Checked) {
			listStatus.Visible=false;
		}
		else {
			listStatus.Visible=true;
		}
	}
	private void butBirthdayLeft_Click(object sender,EventArgs e) {
		var dateFrom=SIn.Date(textBirthdayFrom.Text);
		if(dateFrom.Year < 1880) {
			MsgBox.Show(this,"Please fix the From date first.");
			return;
		}
		var dateTo=SIn.Date(textBirthdayTo.Text);
		var toLastDay=false;
		if(CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month)==dateTo.Day) {
			toLastDay=true;
		}
		textBirthdayFrom.Text=dateFrom.AddMonths(-1).ToString(Lan.g(this,"MM/dd"));
		textBirthdayTo.Text=dateTo.AddMonths(-1).ToString(Lan.g(this,"MM/dd"));
		if(toLastDay) {
			dateTo=SIn.Date(textBirthdayTo.Text);
			textBirthdayTo.Text=new DateTime(dateTo.Year,dateTo.Month,
					CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month))
				.ToString(Lan.g(this,"MM/dd"));
		}
	}

	private void butBirthdayRight_Click(object sender,EventArgs e) {
		var dateFrom=SIn.Date(textBirthdayFrom.Text);
		var dateTo=SIn.Date(textBirthdayTo.Text);
		textBirthdayFrom.Text=dateFrom.AddMonths(-1).ToShortDateString();
		textBirthdayTo.Text=dateTo.AddMonths(-1).ToShortDateString();
		var toLastDay=false;
		if(CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month)==dateTo.Day) {
			toLastDay=true;
		}
		textBirthdayFrom.Text=dateFrom.AddMonths(1).ToString(Lan.g(this,"MM/dd"));
		textBirthdayTo.Text=dateTo.AddMonths(1).ToString(Lan.g(this,"MM/dd"));
		if(toLastDay) {
			dateTo=SIn.Date(textBirthdayTo.Text);
			textBirthdayTo.Text=new DateTime(dateTo.Year,dateTo.Month,
					CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(dateTo.Year,dateTo.Month))
				.ToString(Lan.g(this,"MM/dd"));
		}
	}
	private void SetNextMonth() {
		textBirthdayFrom.Text
			= new DateTime(DateTime.Today.AddMonths(1).Year,DateTime.Today.AddMonths(1).Month,1)
				.ToString(Lan.g(this,"MM/dd"));
		textBirthdayTo.Text
			= new DateTime(DateTime.Today.AddMonths(2).Year,DateTime.Today.AddMonths(2).Month,1).AddDays(-1)
				.ToString(Lan.g(this,"MM/dd"));
	}

	private void butBirthdayMonth_Click(object sender,EventArgs e) {
		SetNextMonth();
	}

}