/* Receivables Breakdown Report:
 * Author: Bill MacWilliams
 * Company: Kapricorn Systems Inc.
 * 
 * This report will give you the currect calculated receivables upto the date specified.
 * This report does NOT show anything past the date specified.
 * This will allow you to do a breakdown for any month / year with totals for the month
 * at the bottom of the report and a running receivables at the far right. It also gives
 * your a breakdown by day of your receivables.  Currently this report is for the entire
 * practice.
*/
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Collections.Generic;
using OpenDental.ReportingComplex;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;

namespace OpenDental;

public partial class FormRpReceivablesBreakdown:FormODBase {
	private List<ProviderDto> _listProvs;

		
	public FormRpReceivablesBreakdown() {
		InitializeComponent();
	}

	private void FormRpReceivablesBreakdown_Load(object sender,EventArgs e) {
		_listProvs=Providers.GetListReports();
		radioWriteoffPay.Checked = true;
		listProv.Items.Add(Lan.g(this,"Practice"));
		for(var i=0;i<_listProvs.Count;i++) {
			listProv.Items.Add(_listProvs[i].Description);
		}
		listProv.SetSelected(0);
		//if(PrefC.GetBool(PrefName.EasyNoClinics")){
		listClinic.Visible = false;
		labClinic.Visible = false;
		/*}
		else{
				listClinic.Items.Add(Lan.g(this,"Unassigned"));
				listClinic.SetSelected(0);
				for(int i=0;i<Clinics.List.Length;i++) {
						listClinic.Items.Add(Clinics.List[i].Description);
						listClinic.SetSelected(i+1);
				}
		}*/
	}

	private void butOK_Click(object sender,System.EventArgs e) {
		if(listProv.SelectedIndices.Count==0) {
			MsgBox.Show(this,"At least one provider must be selected.");
			return;
		}
		var payPlanVersion = PrefC.GetInt(PrefName.PayPlansVersion);
		string bDate;
		string eDate;
		decimal rcvStart = 0;
		decimal rcvCharge=0;
		decimal rcvPayPlanCredit=0;
		decimal rcvProd = 0;
		decimal rcvPayPlanCharges = 0;
		decimal rcvAdj = 0;
		decimal rcvWriteoff = 0;
		decimal rcvPayment = 0;
		decimal rcvInsPayment = 0;
		decimal runningRcv = 0;
		decimal rcvDaily = 0;
		var colTotals=new decimal[11];
		string wMonth;
		string wYear;
		var wDay = "01";
		string wDate;
		// Get the year / month and instert the 1st of the month for stop point for calculated running balance
		wYear = date1.SelectionStart.Year.ToString();
		wMonth = date1.SelectionStart.Month.ToString();
		if(wMonth.Length<2) {
			wMonth = "0" + wMonth;
		}
		wDate=wYear +"-"+ wMonth +"-"+ wDay;
		var listProvNums=new List<long>();
		if(listProv.SelectedIndices[0]!=0) {
			for(var i=0;i<listProv.SelectedIndices.Count;i++) {
				//Minus 1 due to the 'Practice' option.
				listProvNums.Add(_listProvs[listProv.SelectedIndices[i]-1].Id);
			}
		}
		bool isPayPlan2;
		if(payPlanVersion==2) {
			isPayPlan2=true;
		}
		else {
			isPayPlan2=false;
		}
		var report=new ReportComplex(true,isPayPlan2);//Landscape if using PayPlan Version 2
		var TableProduction=new DataTable(); //Production
		var TablePayPlanCredit=new DataTable(); //PayPlanCredits
		var TableCharge=new DataTable();  //charges(Production-PayPlanCredits)
		var TablePayPlanCharge=new DataTable();  //PayPlanCharges
		var TableCapWriteoff=new DataTable();  //capComplete writeoffs
		var TableInsWriteoff=new DataTable();  //ins writeoffs
		var TablePay=new DataTable();  //payments - Patient
		var TableIns=new DataTable();  //payments - Ins, added SPK 
		var TableAdj=new DataTable();  //adjustments
			
		//
		// Main Loop:  This will loop twice 1st loop gets running balance to start of month selected
		//             2nd will break the numbers down by day and calculate the running balances
		//
		for(var j = 0;j <= 1;j++) {
			if(j == 0) {
				bDate = "0001-01-01";
				eDate = wDate;
			}
			else {
				bDate = wDate;
				eDate = SOut.Date(date1.SelectionStart.AddDays(1)).Substring(1,10);// Needed because all Queries are < end date to get correct Starting AR
			}
			TableProduction=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TableProduction");
			if(isPayPlan2) {
				TablePayPlanCredit=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TablePayPlanCredit");
				var TablePayPlanCreditDynamic=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TablePayPlanCreditDynamic");
				for(var i=0;i<TablePayPlanCreditDynamic.Rows.Count;i++) {
					TablePayPlanCredit.ImportRow(TablePayPlanCreditDynamic.Rows[i]);
				}
				TableCharge=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TableCharge");
				TablePayPlanCharge=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TablePayPlanCharge");
			}
			TableCapWriteoff=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TableCapWriteoff");
			TableInsWriteoff=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TableInsWriteoff");
			TablePay=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TablePay");
			TableIns=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TableIns");
			TableAdj=RpReceivablesBreakdown.GetRecvBreakdownTable(date1.SelectionStart,listProvNums,radioWriteoffPay.Checked,isPayPlan2,wDate,eDate,bDate,"TableAdj");
			//Sum up all the transactions grouped by date.
			var dictPayPlanCharges = TablePayPlanCharge.Select().GroupBy(x => SIn.Date(x["ChargeDate"].ToString()))
				.ToDictionary(y => y.Key,y => y.ToList().Sum(z => SIn.Decimal(z["Amt"].ToString())));
			//1st Loop Calculate running Accounts Receivable upto the 1st of the Month Selected
			//2nd Loop Calculate the Daily Accounts Receivable upto the Date Selected
			//Finaly Generate Report showing the breakdown upto the date specified with totals for what is on the report
			if(j == 0) {
				for(var k = 0;k < TableCharge.Rows.Count;k++) {
					rcvCharge += SIn.Decimal(TableCharge.Rows[k][1].ToString());//Production-PayPlanCredits
				}
				rcvPayPlanCharges+=dictPayPlanCharges.Sum(x => x.Value);
				for(var k = 0;k < TableCapWriteoff.Rows.Count;k++) {
					rcvWriteoff += SIn.Decimal(TableCapWriteoff.Rows[k][1].ToString());
				}
				for(var k = 0;k < TableInsWriteoff.Rows.Count;k++) {
					rcvWriteoff += SIn.Decimal(TableInsWriteoff.Rows[k][1].ToString());
				}
				for(var k = 0;k < TablePay.Rows.Count;k++) {
					rcvPayment += SIn.Decimal(TablePay.Rows[k][1].ToString());
				}
				for(var k = 0;k < TableIns.Rows.Count;k++) {
					rcvInsPayment += SIn.Decimal(TableIns.Rows[k][1].ToString());
				}
				for(var k = 0;k < TableAdj.Rows.Count;k++) {
					rcvAdj += SIn.Decimal(TableAdj.Rows[k][1].ToString());
				}
				for(var k = 0;k < TableProduction.Rows.Count;k++) {
					rcvProd += SIn.Decimal(TableProduction.Rows[k][1].ToString());
				}
				for(var k = 0;k < TablePayPlanCredit.Rows.Count;k++) {
					rcvPayPlanCredit += SIn.Decimal(TablePayPlanCredit.Rows[k][1].ToString());
				}
				TableProduction.Clear();
				TablePayPlanCredit.Clear();
				TableCharge.Clear();
				TablePayPlanCharge.Clear();
				TableCapWriteoff.Clear();
				TableInsWriteoff.Clear();
				TablePay.Clear();
				TableIns.Clear();
				TableAdj.Clear();
				rcvStart = (rcvProd - rcvPayPlanCredit + rcvPayPlanCharges + rcvAdj - rcvWriteoff) - (rcvPayment + rcvInsPayment);
			}
			else {
				rcvCharge = 0;
				rcvPayPlanCredit = 0;
				rcvAdj = 0;
				rcvInsPayment = 0;
				rcvPayment = 0;
				rcvProd = 0;
				rcvPayPlanCharges = 0;
				rcvWriteoff = 0;
				rcvDaily = 0;
				runningRcv = rcvStart;
				var TableQ = new DataTable();
				TableQ.Columns.Add("Date");
				TableQ.Columns.Add("Production");
				TableQ.Columns.Add("PayPlanCredits");
				TableQ.Columns.Add("Charges");
				TableQ.Columns.Add("PayPlanCharges");
				TableQ.Columns.Add("Adjustments");
				TableQ.Columns.Add("Writeoffs");
				TableQ.Columns.Add("Payment");
				TableQ.Columns.Add("InsPayment");
				TableQ.Columns.Add("Daily");
				TableQ.Columns.Add("Running");
				eDate = SOut.Date(date1.SelectionStart).Substring(1,10);// Reset EndDate to Selected Date
				var dates = new DateTime[(SIn.Date(eDate) - SIn.Date(bDate)).Days + 1];
				for(var i = 0;i < dates.Length;i++) {//usually 31 days in loop
					dates[i] = SIn.Date(bDate).AddDays(i);
					//create new row called 'row' based on structure of TableQ
					var row = TableQ.NewRow();
					row[0] = dates[i].ToShortDateString();
					for(var k = 0;k < TableProduction.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TableProduction.Rows[k][0].ToString()))) {
							rcvProd += SIn.Decimal(TableProduction.Rows[k][1].ToString());
						}
					}
					for(var k = 0;k < TablePayPlanCredit.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TablePayPlanCredit.Rows[k][0].ToString()))) {
							rcvPayPlanCredit += SIn.Decimal(TablePayPlanCredit.Rows[k][1].ToString());
						}
					}
					for(var k = 0;k < TableCharge.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TableCharge.Rows[k][0].ToString()))) {
							rcvCharge += SIn.Decimal(TableCharge.Rows[k][1].ToString());
						}
					}
					decimal rcvPayPlanChargesForDay = 0;
					if(dictPayPlanCharges.TryGetValue(dates[i],out rcvPayPlanChargesForDay)) {
						rcvPayPlanCharges += rcvPayPlanChargesForDay;
					}
					for(var k = 0;k < TableCapWriteoff.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TableCapWriteoff.Rows[k][0].ToString()))) {
							rcvWriteoff += SIn.Decimal(TableCapWriteoff.Rows[k][1].ToString());
						}
					}
					for(var k = 0;k < TableAdj.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TableAdj.Rows[k][0].ToString()))) {
							rcvAdj += SIn.Decimal(TableAdj.Rows[k][1].ToString());
						}
					}
					for(var k = 0;k < TableInsWriteoff.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TableInsWriteoff.Rows[k][0].ToString()))) {
							rcvWriteoff += SIn.Decimal(TableInsWriteoff.Rows[k][1].ToString());
						}
					}
					for(var k = 0;k < TablePay.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TablePay.Rows[k][0].ToString()))) {
							rcvPayment += SIn.Decimal(TablePay.Rows[k][1].ToString());
						}
					}
					for(var k = 0;k < TableIns.Rows.Count;k++) {
						if(dates[i] == (SIn.Date(TableIns.Rows[k][0].ToString()))) {
							rcvInsPayment += SIn.Decimal(TableIns.Rows[k][1].ToString());
						}
					}
					//rcvPayPlanCharges and rcvPayPlanCredit will be 0 if not on version 2.
					rcvDaily = (rcvProd - rcvPayPlanCredit + rcvPayPlanCharges + rcvAdj - rcvWriteoff) - (rcvPayment + rcvInsPayment);
					runningRcv += (rcvProd - rcvPayPlanCredit + rcvPayPlanCharges + rcvAdj - rcvWriteoff) - (rcvPayment + rcvInsPayment);
					row["Production"] = rcvProd.ToString("n");
					row["PayPlanCredits"] = rcvPayPlanCredit.ToString("n");
					row["Charges"] = rcvCharge.ToString("n");
					row["PayPlanCharges"] = rcvPayPlanCharges.ToString("n");
					row["Adjustments"] = rcvAdj.ToString("n");
					row["Writeoffs"] = rcvWriteoff.ToString("n");
					row["Payment"] = rcvPayment.ToString("n");
					row["InsPayment"] = rcvInsPayment.ToString("n");
					row["Daily"] = rcvDaily.ToString("n");
					row["Running"] = runningRcv.ToString("n");
					colTotals[1]+=rcvProd;
					colTotals[2]+=rcvPayPlanCredit;
					colTotals[3]+=rcvCharge;
					colTotals[4]+=rcvPayPlanCharges;
					colTotals[5]+=rcvAdj;
					colTotals[6]+=rcvWriteoff;
					colTotals[7]+=rcvPayment;
					colTotals[8]+=rcvInsPayment;
					colTotals[9]+=rcvDaily;
					colTotals[10]=runningRcv;
					TableQ.Rows.Add(row);  //adds row to table Q
					rcvAdj = 0;
					rcvInsPayment = 0;
					rcvPayment = 0;
					rcvProd = 0;
					rcvPayPlanCharges = 0;
					rcvWriteoff = 0;
					rcvCharge = 0;
					rcvPayPlanCredit = 0;
				}
				//Drop the PayPlan columns if not version 2
				if(!isPayPlan2) {
					TableQ.Columns.RemoveAt(2);//PayPlanCredits
					TableQ.Columns.RemoveAt(2);//Production - PayPlanCredits
					TableQ.Columns.RemoveAt(2);//PayPlanCharges
				}
				////columnCount will get incremented on the second ColTotal call so we can use it in this way
				//if(payPlanVersion==2) {
				//	report.ColTotal[1]=PIn.Decimal(colTotals[1].ToString("n")); //prod
				//	report.ColTotal[2]=PIn.Decimal(colTotals[2].ToString("n")); //payplancharges
				//	report.ColTotal[3]=PIn.Decimal(colTotals[3].ToString("n")); //adjustment
				//	report.ColTotal[4]=PIn.Decimal(colTotals[4].ToString("n")); //writeoffs
				//	report.ColTotal[5]=PIn.Decimal(colTotals[5].ToString("n")); //payment
				//	report.ColTotal[6]=PIn.Decimal(colTotals[6].ToString("n")); //inspayment
				//	report.ColTotal[7]=PIn.Decimal(colTotals[7].ToString("n")); //daily
				//	report.ColTotal[8]=PIn.Decimal(colTotals[8].ToString("n")); //running
				//}
				//else {
				//	report.ColTotal[1]=PIn.Decimal(colTotals[1].ToString("n")); //prod
				//	report.ColTotal[2]=PIn.Decimal(colTotals[3].ToString("n")); //adjustment
				//	report.ColTotal[3]=PIn.Decimal(colTotals[4].ToString("n")); //writeoffs
				//	report.ColTotal[4]=PIn.Decimal(colTotals[5].ToString("n")); //payment
				//	report.ColTotal[5]=PIn.Decimal(colTotals[6].ToString("n")); //inspayment
				//	report.ColTotal[6]=PIn.Decimal(colTotals[7].ToString("n")); //daily
				//	report.ColTotal[7]=PIn.Decimal(colTotals[8].ToString("n")); //running
				//}
				var font = new Font("Tahoma",9);
				var boldFont = new Font("Tahoma",9,FontStyle.Bold);
				var fontTitle = new Font("Tahoma",17,FontStyle.Bold);
				var fontSubTitle = new Font("Tahoma",10,FontStyle.Bold);
				report.ReportName=Lan.g(this,"Receivables Breakdown");
				report.AddTitle("Title",Lan.g(this,"Receivables Breakdown"),fontTitle);
				report.AddSubTitle("PracticeTitle",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
				report.AddSubTitle("Date SubTitle",date1.SelectionStart.ToString("d"),fontSubTitle);
				var provNames="";
				if(listProv.SelectedIndices[0]==0) {
					provNames="All Providers";
				}
				else {
					for(var i=0;i<listProv.SelectedIndices.Count;i++) {
						if(i>0) {
							provNames+=", ";
						}
						provNames+=_listProvs[listProv.SelectedIndices[i]-1].Abbr;
					}
				}
				report.AddSubTitle("Provider SubTitle",provNames);
				int[] summaryGroups1 = [1];
				var query=report.AddQuery(TableQ,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));
				query.AddColumn("Date",72,FieldValueType.Date);
				query.AddColumn("Production",80,FieldValueType.Number);
				query.GetColumnDetail("Production").ContentAlignment=ContentAlignment.MiddleRight;
				if(isPayPlan2) {
					query.AddColumn("PayPlanCredits",90,FieldValueType.Number);
					query.GetColumnDetail("PayPlanCredits").ContentAlignment=ContentAlignment.MiddleRight;
					query.AddColumn("Prod - PPCred",85,FieldValueType.Number);
					query.GetColumnDetail("Prod - PPCred").ContentAlignment=ContentAlignment.MiddleRight;
					query.AddColumn("PayPlanCharges",100,FieldValueType.Number);
					query.GetColumnDetail("PayPlanCharges").ContentAlignment=ContentAlignment.MiddleRight;
				}
				query.AddColumn("Adjustment",80,FieldValueType.Number);
				query.GetColumnDetail("Adjustment").ContentAlignment=ContentAlignment.MiddleRight;
				query.AddColumn("Write-off",80,FieldValueType.Number);
				query.GetColumnDetail("Write-off").ContentAlignment=ContentAlignment.MiddleRight;
				query.AddColumn("Payment",80,FieldValueType.Number);
				query.GetColumnDetail("Payment").ContentAlignment=ContentAlignment.MiddleRight;
				query.AddColumn("InsPayment",80,FieldValueType.Number);
				query.GetColumnDetail("InsPayment").ContentAlignment=ContentAlignment.MiddleRight;
				query.AddColumn("Daily A/R",100,FieldValueType.Number);
				query.GetColumnDetail("Daily A/R").ContentAlignment=ContentAlignment.MiddleRight;
				query.AddColumn("Ending A/R",100);
				query.GetColumnDetail("Ending A/R").ContentAlignment=ContentAlignment.MiddleRight;
				query.GetColumnDetail("Ending A/R").Font=boldFont;
				if(isPayPlan2) {
					report.AddFooterText("Desc","Receivables Calculation: (Production - PayPlanCredits + PayPlanCharges + Adjustments - Write-offs) "
					                            +"- (Payments + Insurance Payments)",font,0,ContentAlignment.MiddleCenter);
				}
				else {
					report.AddFooterText("Desc","Receivables Calculation: (Production + Adjustments - Write-offs) - (Payments + Insurance Payments)",font,0,ContentAlignment.MiddleCenter);
				}
				//report.AddText("EndingAR","Final Ending A/R: "+runningRcv.ToString(),boldFont,0,ContentAlignment.MiddleLeft);
				report.AddPageNum(font);
				if(!report.SubmitQueries()) {
					return;
				}
				using var FormR=new FormReportComplex(report);
				FormR.ShowDialog();
				DialogResult = DialogResult.OK;
			}//END If 
		}// END For Loop
	}//END OK button Clicked

}