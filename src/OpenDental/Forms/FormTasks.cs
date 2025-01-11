using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental{
	/// <summary></summary>
	public partial class FormTasks:FormODBase {
		//private System.ComponentModel.IContainer components;
		/////<summary>After closing, if this is not zero, then it will jump to the object specified in GotoKeyNum.</summary>
		//public TaskObjectType GotoType;
		/////<summary>After closing, if this is not zero, then it will jump to the specified patient.</summary>
		//public long GotoKeyNum;
		private bool _isTriage;
		private FormWindowState _formWindowStateOld;
		public static Color ColorLightRed=Color.FromArgb(247,110,110);

		public void SetUserControlTasksTab(UserControlTasksTab newTab){
			userControlTasks1.TaskTab=newTab;
		}

		
		public FormTasks()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			InitializeLayoutManager();
			//Lan.F(this);
		}

		private void FormTasks_Load(object sender,EventArgs e) {
			_formWindowStateOld=WindowState;
			userControlTasks1.InitializeOnStartup();
			splitContainer.Panel2Collapsed=true;
		}

		public override void ProcessSignalODs(List<Signalod> listSignalods) {
		}
		
		private void gridWebChatSessions_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		}

		private void userControlTasks1_Resize(object sender,EventArgs e) {
			if(WindowState==FormWindowState.Minimized) {//Form currently minimized.
				_formWindowStateOld=WindowState;
				return;//The window is invisble when minimized, so no need to refresh.
			}
			if(_formWindowStateOld==FormWindowState.Minimized) {//Form was previously minimized (invisible) and is now in normal state or maximized state.
				_formWindowStateOld=WindowState;
				return;
			}
			_formWindowStateOld=WindowState;//Set the window state after every resize.
		}

		private void UserControlTasks1_FillGridEvent(object sender,EventArgs e) {
			this.Text=userControlTasks1.TitleControlParent;
		}

		/* private void timer1_Tick(object sender,EventArgs e) {
				if(Security.CurUser!=null) {//Possible if OD auto logged a user off and they left the task window open in the background.
					userControlTasks1.RefreshTasks();
				}
				//this quick and dirty refresh is not as intelligent as the one used when tasks are docked.
				//Sound notification of new task is controlled from main form completely
				//independently of this visual refresh.
			}
		}
		*/














	}
}





















