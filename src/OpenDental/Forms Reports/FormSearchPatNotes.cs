using System;
using System.Text;
namespace OpenDental;

/// <summary>
/// Summary description for FormSearchPatNotes.
/// </summary>
public partial class FormSearchPatNotes : FormODBase {

	public FormSearchPatNotes()
	{
		InitializeComponent();
		//
		// TODO: Add any constructor code after InitializeComponent call
		//
	}

	private void butOK_Click(object sender, System.EventArgs e)
	{
		var phrase = textPharse.Text.Replace("\t","").Replace("\n","");
		var sbSQL = new StringBuilder();
		sbSQL.AppendFormat("SELECT LName,FName,Preferred,PatStatus,Gender,Birthdate,Address,Address2,City,State,zip,HmPhone,Wkphone,",phrase);                                
		sbSQL.AppendFormat("WirelessPhone,Guarantor,PriProv,AddrNote,FamFinUrgNote,MedUrgNote,ApptModNote,DateFirstVisit",phrase);
		sbSQL.AppendFormat(" FROM patient WHERE AddrNote LIKE '%{0}%' ", phrase);
		sbSQL.AppendFormat("or FamFinUrgNote LIKE '%{0}%' ",phrase);
		sbSQL.AppendFormat("or MedUrgNote LIKE '%{0}%' ",phrase);
		sbSQL.AppendFormat("or ApptModNote LIKE '%{0}%' ",phrase);
		sbSQL.AppendFormat("or EmploymentNote LIKE '%{0}%' ",phrase);


		var report=new ReportSimpleGrid();
		report.Query= sbSQL.ToString();
		using var FormQuery2=new FormQuery(report);
		FormQuery2.IsReport=false;
		FormQuery2.SubmitQuery();
		FormQuery2.textQuery.Text=report.Query;
		FormQuery2.ShowDialog();
	}

	private void textPharse_TextChanged(object sender, System.EventArgs e)
	{
		if(textPharse.Text.Trim().Length > 0)
			butOK.Enabled=true;
		else
			butOK.Enabled = false;
	}

	private void FormSearchPatNotes_Load(object sender,EventArgs e) {

	}

}