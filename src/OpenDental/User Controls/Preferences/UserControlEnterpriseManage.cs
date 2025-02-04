using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class UserControlEnterpriseManage:UserControl {

	#region Fields - Private
	#endregion Fields - Private

	#region Fields - Public
	public bool Changed;
	#endregion Fields - Public

	#region Constructors
	public UserControlEnterpriseManage() {
		InitializeComponent();
		Font=new("Microsoft Sans Serif", 8.25f);
	}
	#endregion Constructors

	#region Methods - Event Handlers
	#endregion Methods - Event Handlers

	#region Methods - Private
	#endregion Methods - Private

	#region Methods - Public
	public void FillEnterpriseManage() {
		checkEraRefreshOnLoad.Checked=PrefC.GetBool(PrefName.EraRefreshOnLoad);
		checkEraStrictClaimMatching.Checked=PrefC.GetBool(PrefName.EraStrictClaimMatching);
		checkEraShowStatusAndClinic.Checked=PrefC.GetBool(PrefName.EraShowStatusAndClinic);
		checkEnterpriseManualRefreshMainTaskLists.Checked=PrefC.GetBool(PrefName.EnterpriseManualRefreshMainTaskLists);
	}

	public bool SaveEnterpriseManage() {
		Changed|=Prefs.UpdateBool(PrefName.EraRefreshOnLoad,checkEraRefreshOnLoad.Checked);
		Changed|=Prefs.UpdateBool(PrefName.EraStrictClaimMatching,checkEraStrictClaimMatching.Checked);
		Changed|=Prefs.UpdateBool(PrefName.EraShowStatusAndClinic,checkEraShowStatusAndClinic.Checked);
		Changed|=Prefs.UpdateBool(PrefName.EnterpriseManualRefreshMainTaskLists,checkEnterpriseManualRefreshMainTaskLists.Checked);
		return true;
	}
	#endregion Methods - Public
}