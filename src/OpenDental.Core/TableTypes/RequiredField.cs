using System;
using System.Collections.Generic;

namespace OpenDentBusiness {
	///<summary>Each row represents a field that is required to be filled out.</summary>
	[Serializable]
	public class RequiredField:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long RequiredFieldNum;
		///<summary>Enum:RequiredFieldType . The area of the program that uses this field.</summary>
		public RequiredFieldType FieldType;
		///<summary>Enum:RequiredFieldName </summary>
		[CrudColumn(SpecialType=CrudSpecialColType.EnumAsString)]
		public RequiredFieldName FieldName;
		///<summary>This is not a data column but is stored in a seperate table named RequiredFieldCondition.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		private List<RequiredFieldCondition> _listRequiredFieldConditions;

		public List<RequiredFieldCondition> ListRequiredFieldConditions {
			get {
				if(_listRequiredFieldConditions==null) {
					if(RequiredFieldNum==0) {
						_listRequiredFieldConditions=new List<RequiredFieldCondition>();
					}
					else {
						_listRequiredFieldConditions=RequiredFieldConditions.GetForRequiredField(RequiredFieldNum);
					}
				}
				return _listRequiredFieldConditions;
			}
			set {
				_listRequiredFieldConditions=value;
			}
		}

		///<summary>Refreshes the list holding the requirefieldconditions for this requiredfield.</summary>
		public void RefreshConditions() {
			_listRequiredFieldConditions=null;
			RequiredFieldConditions.RefreshCache();
		}

		
		public RequiredField Clone() {
			return (RequiredField)this.MemberwiseClone();
		}
	}

	///<summary>The part of the program where this required field is used.</summary>
	public enum RequiredFieldType {
		///<summary>0 - Edit Patient Information window and Add Family (FormPatientAddAll) window.</summary>
		PatientInfo,
		///<summary>1 - Edit Claim Payment window.</summary>
		InsPayEdit
	}

	///<summary>This enum is stored as a string, so the order of values can be rearranged.</summary>
	public enum RequiredFieldName {
		
		Address,
		
		Address2,
		
		AddressPhoneNotes,
		
		AdmitDate,
		
		AskArriveEarly,
		
		BatchNumber,
		
		BillingType,
		
		Birthdate,
		
		Carrier,
		
		ChartNumber,
		
		CheckDate,
		
		CheckNumber,
		
		City,
		
		Clinic,
		
		CollegeName,
		
		County,
		
		CreditType,
		
		DateFirstVisit,
		
		DateTimeDeceased,
		
		DepositAccountNumber,
		
		DepositDate,
		
		DischargeDate,
		
		EligibilityExceptCode,
		
		EmailAddress,
		
		EmergencyName,
		
		EmergencyPhone,
		
		Employer,
		
		Ethnicity,
		
		FeeSchedule,
		
		FirstName,
		
		Gender,
		
		GenderIdentity,
		
		GradeLevel,
		
		GroupName,
		
		GroupNum,
		
		HomePhone,
		
		InsPayEditClinic,
		
		InsurancePhone,
		
		InsuranceSubscriber,
		
		InsuranceSubscriberID,
		
		Language,
		
		LastName,
		
		PaymentAmount,
		
		PaymentType,
		
		Position,
		
		MedicaidID,
		
		MedicaidState,
		
		MiddleInitial,
		
		MothersMaidenFirstName,
		
		MothersMaidenLastName,
		
		PatientStatus,
		
		PreferConfirmMethod,
		
		PreferContactMethod,
		
		PreferRecallMethod,
		
		PreferredName,
		
		PrimaryProvider,
		
		Race,
		
		ReferredFrom,
		
		ResponsibleParty,
		
		Salutation,
		
		SecondaryProvider,
		
		SexualOrientation,
		
		Site,
		
		SocialSecurityNumber,
		
		State,
		
		StudentStatus,
		
		TextOK,
		
		Title,
		
		TreatmentUrgency,
		
		TrophyFolder,
		
		Ward,
		
		WirelessPhone,
		
		WorkPhone,
		
		Zip
	}
}