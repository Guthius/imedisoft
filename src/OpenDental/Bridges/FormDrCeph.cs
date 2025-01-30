using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormDrCeph:FormODBase {
	public DrCephArgs Args;
	public Patient PatientCur;
	public List<ProgramProperty> ListProgramProperties= [];
		

	public FormDrCeph() {
		InitializeComponent();
	}

	private void FormDrCeph_Load(object sender,EventArgs e) {
		listBoxTxPhase.Items.Clear();
		listBoxTxPhase.Items.AddEnums<TreatmentPhaseEnum>();
		listBoxTxPhase.SetSelectedEnum(TreatmentPhaseEnum.PreTreatment);
		FillListBoxRace();
		dateXRay.Value=DateTime.Today;
	}

	private void FillListBoxRace() {
		listBoxRace.Items.Clear();
		var listRaceOptions=Enum.GetValues(typeof(CephRaceEnum)).OfType<CephRaceEnum>().Select(x => x.ToString()).ToList();
		listRaceOptions.AddRange(GetCustomRaceOptions());
		listBoxRace.Items.AddStrings(listRaceOptions);
		var patRaces=PatientRaces.GetForPatient(PatientCur.PatNum);
		var patRaceOld=PatientRaces.GetPatientRaceOldFromPatientRaces(PatientCur.PatNum,patRaces);
		var idx=-1;
		//Because our listBox is based on what is in listRaceOptions, it's safe to select and index based on this information
		switch(patRaceOld) {
			case PatientRaceOld.White:
				idx=listRaceOptions.IndexOf(CephRaceEnum.White.ToString());
				break;
			case PatientRaceOld.BlackHispanic:
			case PatientRaceOld.AfricanAmerican:
				idx=listRaceOptions.IndexOf(CephRaceEnum.Black.ToString());
				break;
			case PatientRaceOld.HispanicLatino:
				idx=listRaceOptions.IndexOf(CephRaceEnum.Hispanic.ToString());
				break;
			case PatientRaceOld.Asian:
				idx=listRaceOptions.IndexOf(CephRaceEnum.Asian.ToString());
				break;
			default:
				//we need to find a string match so now we need to make sure we remove the chance for any false negatives
				listRaceOptions=listRaceOptions.Select(x=>x.ToUpper().Replace(" ","").Trim()).ToList();
				for(var i=0;i<patRaces.Count;i++) {
					var desc=patRaces[i].Description.ToUpper().Replace(" ","").Trim();
					//IndexOf will return -1 if the item is not found in the list so once we find a value greater than that, it's safe to break out of the loop
					idx=listRaceOptions.IndexOf(desc);
					if(idx>-1) {
						break;
					}
				}
				break;
		}
		listBoxRace.SetSelected(Math.Max(0,idx));
	}

	private List<string> GetCustomRaceOptions() {
		var property=ListProgramProperties.FirstOrDefault(x=>x.PropertyDesc=="Custom Patient Race Options");
		if(property!=null && !string.IsNullOrWhiteSpace(property.PropertyValue)) {
			return property.PropertyValue.Split([','],StringSplitOptions.RemoveEmptyEntries).ToList();
		}
		return [];
	}

	///<summary>Based on options available in DrCeph</summary>
	private enum CephRaceEnum {
		Unknown,
		White,
		Black,
		Hispanic,
		Asian,
	}

	///<summary>Based on options available in DrCeph</summary>
	private enum TreatmentPhaseEnum {
		///<summary>0- </summary>
		PreTreatment,
		///<summary>1- </summary>
		Progress1,
		///<summary>2- </summary>
		Progress2,
		///<summary>3- </summary>
		Progress3,
		///<summary>4- </summary>
		PostTreatment
	}

	private void butCephBrowse_Click(object sender,EventArgs e) {
		using var folderBrowserDialog=new OpenFileDialog();
		folderBrowserDialog.FileName=textCephLocation.Text;
		if(folderBrowserDialog.ShowDialog()==DialogResult.Cancel){
			return;
		}
		textCephLocation.Text=folderBrowserDialog.FileName;
	}

	private void butPhotoBrowse_Click(object sender,EventArgs e) {
		using var folderBrowserDialog=new OpenFileDialog();
		folderBrowserDialog.FileName=textPhotoLocation.Text;
		if(folderBrowserDialog.ShowDialog()==DialogResult.Cancel){
			return;
		}
		textPhotoLocation.Text=folderBrowserDialog.FileName;
	}

	private bool IsDataValid() {
		if(listBoxRace.SelectedIndices.Count>1 
		   || listBoxRace.SelectedIndex<0 
		   || listBoxRace.SelectedIndex>=listBoxRace.Items.Count
		   || (Enum.TryParse(listBoxRace.GetStringSelectedItems(),out CephRaceEnum patRace) && patRace==CephRaceEnum.Unknown))
		{
			MsgBox.Show(this,"You must make a valid selection for patient race before continuing.");
			return false;
		}
		if(listBoxTxPhase.SelectedIndex<0 
		   || listBoxTxPhase.SelectedIndex >= Enum.GetValues(typeof(TreatmentPhaseEnum)).Length 
		   || listBoxTxPhase.SelectedIndices.Count>1) 
		{
			MsgBox.Show(this,"You must make a valid selection for treatment phase before continuing.");
			return false;
		}
		return true;
	}

	private void BuildArgs() {
		var PPCur=ProgramProperties.GetCur(ListProgramProperties, "Enter 0 to use PatientNum, or 1 to use ChartNum");;
		var patID=PatientCur.PatNum.ToString();;
		if(PPCur.PropertyValue=="1"){
			patID=PatientCur.ChartNumber;
		}
		var referalList=RefAttaches.Refresh(PatientCur.PatNum);
		var prov=Providers.GetProv(Patients.GetProvNum(PatientCur));
		var provName=prov.FName+" "+prov.MI+" "+prov.LName+" "+prov.Suffix;
		var fam=Patients.GetFamily(PatientCur.PatNum);
		var guar=fam.ListPats[0];
		var relat="UnKnown";
		if(guar.PatNum==PatientCur.PatNum){
			relat="Self";
		}
		if(guar.Gender==PatientGender.Male	&& PatientCur.Position==PatientPosition.Child){
			relat="Father";
		}
		else if(guar.Gender==PatientGender.Female	&& PatientCur.Position==PatientPosition.Child){
			relat="Mother";
		}
		Args=new DrCephArgs {
			ID=patID,
			FName=PatientCur.FName,
			LName=PatientCur.LName,
			MiddleI=PatientCur.MiddleI,
			Address1=PatientCur.Address,
			Address2=PatientCur.Address2,
			City=PatientCur.City,
			State=PatientCur.State,
			Zip=PatientCur.Zip,
			Phone=PatientCur.HmPhone,
			SSN=PatientCur.SSN,
			Sex=PatientCur.Gender.ToString(),
			Race=listBoxRace.GetStringSelectedItems(),
			Birthdate=PatientCur.Birthdate.ToString(),
			RecordsDate=DateTime.Today.ToShortDateString(),
			ReferringDr=RefAttachL.GetReferringDr(referalList),
			TreatingDr=provName,
			ResponsibleName=guar.GetNameFL(),
			ResponsibleAddress1=guar.Address,
			ResponsibleAddress2=guar.Address2,
			ResponsibleCity=guar.City,
			ResponsibleState=guar.State,
			ResponsibleZip=guar.Zip,
			ResponsiblePhone=guar.HmPhone,
			ResponsibleRelationship=relat,
			TreatmentPhase=(int)listBoxTxPhase.GetSelected<TreatmentPhaseEnum>(),
			XRayDate=dateXRay.Text,
			PhotoFileLocation=textPhotoLocation.Text,
			CephXRayLocation=textCephLocation.Text,
		};
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(IsDataValid()) {
			try {
				BuildArgs();
			}
			catch(Exception ex) {
				FriendlyException.Show(Lans.g("An error occurred while parsing arguments for ")+ProgramName.DrCeph,ex);
				DialogResult=DialogResult.Cancel;
				return;
			}
			DialogResult=DialogResult.OK;
		}
	}

}