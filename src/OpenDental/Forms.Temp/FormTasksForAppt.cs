using System;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Linq;
using DataConnectionBase;

namespace OpenDental;

public partial class FormTasksForAppt:FormODBase {
	private long _aptNum;

	public FormTasksForAppt(long aptNum) {
		InitializeComponent();

		_aptNum=aptNum;
	}

	private void FormTasksForAppt_Load(object sender,EventArgs e) {
		FillGrid();
	}

	private void FillGrid() {
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		gridMain.ListGridRows.Clear();
		var gridColumn=new GridColumn("Created",70,HorizontalAlignment.Left);
		gridMain.Columns.Add(gridColumn);
		gridColumn=new GridColumn("Completed",70,HorizontalAlignment.Left);
		gridMain.Columns.Add(gridColumn);
		gridColumn=new GridColumn("Description",70);
		gridColumn.IsWidthDynamic=true;
		gridMain.Columns.Add(gridColumn);
		var listTaskNums=Tasks.GetMany(_aptNum).Select(x => x.TaskNum).ToList();
		if(listTaskNums.Count==0) {
			gridMain.EndUpdate();
			return;
		}
		var table=Tasks.GetDataSet(0, [],listTaskNums,"","","","","","",0,0,
			doIncludeTaskNote:false,doIncludeCompleted:true,doIncludeAttachments:false,reachedLimit:false);//GetDataSet orders in descending order by DateTimeOriginal if it exists, DateTimeEntry if not
		if(table==null) {
			gridMain.EndUpdate();
			return;
		}
		GridRow gridRow;
		for(var i=0;i<table.Rows.Count;i++) {
			gridRow=new GridRow();
			gridRow.Cells.Add(table.Rows[i]["dateCreate"].ToString());
			gridRow.Cells.Add(table.Rows[i]["dateComplete"].ToString());
			gridRow.Cells.Add(table.Rows[i]["description"].ToString());
			gridRow.Note=table.Rows[i]["note"].ToString();
			gridRow.ColorLborder=Color.Black;
			gridRow.ColorText=Color.FromArgb(SIn.Int(table.Rows[i]["color"].ToString()));
			gridMain.ListGridRows.Add(gridRow);
			gridRow.Tag=table.Rows[i]["TaskNum"].ToString();
		}
		gridMain.EndUpdate();
	}

	private void gridTasks_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var taskNum=SIn.Long(gridMain.ListGridRows[e.Row].Tag.ToString());
		var task=Tasks.GetOne(taskNum);
		if(task!=null) {
			using var formTaskEdit=new FormTaskEdit(task);
			formTaskEdit.ShowDialog();
			if(formTaskEdit.DialogResult==DialogResult.OK){
				FillGrid();
				return;
			}
		}
		else {
			MsgBox.Show(this,"The task no longer exists.");
		}
	}

}