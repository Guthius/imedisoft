using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class UserControlExperimentalPrefs:UserControl {

	#region Fields - Private
	#endregion Fields - Private

	#region Fields - Public
	public bool Changed;
	#endregion Fields - Public

	#region Constructors
	public UserControlExperimentalPrefs() {
		InitializeComponent();
		Font=new("Microsoft Sans Serif", 8.25f);
	}
	#endregion Constructors

	#region Methods - Event Handlers
	#endregion Methods - Event Handlers

	#region Methods - Private
	#endregion Methods - Private

	#region Methods - Public
	public void FillExperimentalPrefs() {
		checkAgingProcLifo.CheckState=PrefC.GetYnCheckState(PrefName.AgingProcLifo);
	}

	public bool SaveExperimentalPrefs() {
		Changed|=Prefs.UpdateYN(PrefName.AgingProcLifo,checkAgingProcLifo.CheckState);
		return true;
	}
	#endregion Methods - Public
}