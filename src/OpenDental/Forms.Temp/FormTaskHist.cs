using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDental.UI;

namespace OpenDental;

public partial class FormTaskHist:FormODBase {
	public long TaskNum;
	///<summary>Contains all TaskHists for the given TaskNumCur. Does not include the "current" revision of non-deleted tasks.</summary>
	private List<TaskHist> _listTaskHistsAudit;

	public FormTaskHist() {
		InitializeComponent();
	}

	private void FormTaskHist_Load(object sender,EventArgs e) {
		_listTaskHistsAudit=TaskHists.GetArchivesForTask(TaskNum);
		FillGrid();
	}

	private void FillGrid() {
		gridTaskHist.BeginUpdate();
		gridTaskHist.Columns.Clear();
		var col=new GridColumn(Lan.g("TableTaskAudit","Create Date"),140);
		gridTaskHist.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTaskAudit","Edit Date"),140);
		gridTaskHist.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTaskAudit","Editing User"),80);
		gridTaskHist.Columns.Add(col);
		col=new GridColumn(Lan.g("TableTaskAudit","Changes"),100);
		gridTaskHist.Columns.Add(col);
		gridTaskHist.ListGridRows.Clear();
		for(var i=1;i<_listTaskHistsAudit.Count;i++) {
			var taskHist=_listTaskHistsAudit[i-1];
			var taskHistNext=_listTaskHistsAudit[i];
			var row=new GridRow();//Row describes difference between current row and the Next row. Last row will be the last TaskHist compared to the current Task.
			if(taskHist.DateTimeEntry==DateTime.MinValue) {
				row.Cells.Add(_listTaskHistsAudit[i].DateTimeEntry.ToString());
			}
			else {
				row.Cells.Add(taskHist.DateTimeEntry.ToString());
			}
			row.Cells.Add(taskHist.DateTStamp.ToString());
			var userNum=taskHist.UserNumHist;
			if(userNum==0) {
				userNum=taskHist.UserNum;
			}
			row.Cells.Add(TaskHists.GetUserName(userNum));
			row.Cells.Add(TaskHists.GetChangesDescription(taskHist,taskHistNext));
			gridTaskHist.ListGridRows.Add(row);
		}
		//Compare the current task with the last hist entry (Add the "current revision" of the task if necessary.)
		if(_listTaskHistsAudit.Count<=0) {
			gridTaskHist.EndUpdate();
			return;
		}
		var taskHistLast=_listTaskHistsAudit[_listTaskHistsAudit.Count-1];
		var task=Tasks.GetOne(TaskNum);
		if(task!=null) {
			var taskHistNext=new TaskHist(task);
			var row=new GridRow();
			if(taskHistLast.DateTimeEntry==DateTime.MinValue) {
				row.Cells.Add(taskHistNext.DateTimeEntry.ToString());
			}
			else {
				row.Cells.Add(taskHistLast.DateTimeEntry.ToString());
			}
			row.Cells.Add(taskHistLast.DateTStamp.ToString());
			var userNum=taskHistLast.UserNumHist;
			if(userNum==0) {
				userNum=taskHistLast.UserNum;
			}
			row.Cells.Add(TaskHists.GetUserName(userNum));
			row.Cells.Add(TaskHists.GetChangesDescription(taskHistLast,taskHistNext));
			gridTaskHist.ListGridRows.Add(row);
		}
		gridTaskHist.EndUpdate();
	}

}