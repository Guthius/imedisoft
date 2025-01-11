using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness;

namespace Imedisoft.Core.Features.Clinics;

public class Clinics
{
    private static long _clinicNum;

    [ThreadStatic]
    private static long _clinicNumT;

    public static long ClinicNum => _clinicNumT > 0 ? _clinicNumT : _clinicNum;

    public static void Insert(ClinicDto clinic)
    {
        throw new NotImplementedException();
    }

    public static void SetClinicNum(long clinicNum)
    {
        if (_clinicNum == clinicNum && _clinicNumT == clinicNum) return; //no change
        _clinicNum = clinicNum;
        _clinicNumT = clinicNum;
        if (Security.CurUser == null) return;
        if (PrefC.GetString(PrefName.ClinicTrackLast) != "User") return;
        var listUserOdPrefs = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.ClinicLast); //should only be one.
        if (listUserOdPrefs.Count > 0)
        {
            var isSetInvalidNeeded = false;
            for (var i = 0; i < listUserOdPrefs.Count; i++)
            {
                var userOdPrefOld = listUserOdPrefs[i].Clone();
                listUserOdPrefs[i].Fkey = clinicNum;
                if (UserOdPrefs.Update(listUserOdPrefs[i], userOdPrefOld)) isSetInvalidNeeded = true;
            }

            if (isSetInvalidNeeded)
            {
                //Only need to signal cache refresh on change.
                Signalods.SetInvalid(InvalidType.UserOdPrefs);
                UserOdPrefs.RefreshCache();
            }

            return;
        }

        var userOdPref = new UserOdPref();
        userOdPref.UserNum = Security.CurUser.UserNum;
        userOdPref.FkeyType = UserOdFkeyType.ClinicLast;
        userOdPref.Fkey = clinicNum;
        UserOdPrefs.Insert(userOdPref);
        Signalods.SetInvalid(InvalidType.UserOdPrefs);
        UserOdPrefs.RefreshCache();
    }

    public static void LoadClinicNumForUser(string clinicNumCLA = "")
    {
        _clinicNum = 0; //aka headquarters clinic when clinics are enabled.
        if (Security.CurUser == null) return;
        var listClinics = GetForUserod(Security.CurUser);
        if (!long.TryParse(clinicNumCLA, out var clinicNum)) clinicNum = -1;
        if (clinicNum >= 0 && listClinics.Any(x => x.Id == clinicNum))
        {
            _clinicNum = clinicNum;
            return;
        }

        switch (PrefC.GetString(PrefName.ClinicTrackLast))
        {
            case "Workstation":
                if (Security.CurUser.ClinicIsRestricted && Security.CurUser.ClinicNum != ComputerPrefs.LocalComputer.ClinicNum)
                {
                    //The user is restricted and it's not the clinic this computer last used
                    //User's default clinic isn't the LocalComputer's clinic, see if they have access to the Localcomputer's clinic, if so, use it.
                    var clinic = listClinics.Find(x => x.Id == ComputerPrefs.LocalComputer.ClinicNum);
                    if (clinic != null)
                        _clinicNum = clinic.Id;
                    else
                        _clinicNum = Security.CurUser.ClinicNum; //Use the user's default clinic if they don't have access to LocalComputer's clinic.
                }
                else
                {
                    //The user is not restricted, just use the clinic in the ComputerPref table.
                    _clinicNum = ComputerPrefs.LocalComputer.ClinicNum;
                }

                break;
            case "User":
                var listUserOdPrefs = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.ClinicLast); //should only be one or none.
                if (listUserOdPrefs.Count == 0)
                {
                    var userOdPref = new UserOdPref();
                    userOdPref.UserNum = Security.CurUser.UserNum;
                    userOdPref.FkeyType = UserOdFkeyType.ClinicLast;
                    userOdPref.Fkey = Security.CurUser.ClinicNum; //default clinic num
                    UserOdPrefs.Insert(userOdPref);
                    listUserOdPrefs.Add(userOdPref);
                    Signalods.SetInvalid(InvalidType.UserOdPrefs);
                    UserOdPrefs.RefreshCache();
                }

                if (listClinics.All(x => x.Id != listUserOdPrefs[0].Fkey)) //user is restricted and does not have access to the computerpref clinic
                    return;
                _clinicNum = listUserOdPrefs[0].Fkey;
                return;
            case "None":
            default:
                if (listClinics.Any(x => x.Id == Security.CurUser.ClinicNum)) _clinicNum = Security.CurUser.ClinicNum;
                break;
        }
    }

    public static void LogOff()
    {
        if (!true)
        {
            _clinicNum = 0;
            return;
        }

        if (PrefC.GetString(PrefName.ClinicTrackLast) == "Workstation")
        {
            //Other two options are "None" and "User"
            //"User" is handled below.
            ComputerPrefs.LocalComputer.ClinicNum = ClinicNum;
            ComputerPrefs.Update(ComputerPrefs.LocalComputer);
        }

        //We want to always upsert a user pref for the user because we will be looking at it for MobileWeb regardless of the preference for 
        //ClinicTrackLast.
        var listUserOdPrefs = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.ClinicLast); //should only be one or none.
        if (listUserOdPrefs.Count == 0)
        {
            var userOdPref = new UserOdPref();
            userOdPref.UserNum = Security.CurUser.UserNum;
            userOdPref.FkeyType = UserOdFkeyType.ClinicLast;
            userOdPref.Fkey = ClinicNum;
            UserOdPrefs.Insert(userOdPref);
        }

        var isSetInvalidNeeded = false;
        for (var i = 0; i < listUserOdPrefs.Count; i++)
        {
            var userOdPrefOld = listUserOdPrefs[i].Clone();
            listUserOdPrefs[i].Fkey = ClinicNum;
            if (UserOdPrefs.Update(listUserOdPrefs[i], userOdPrefOld)) isSetInvalidNeeded = true;
        }

        ;
        if (!isSetInvalidNeeded)
        {
            _clinicNum = 0;
            return;
        }

        //Only need to signal cache refresh on change.
        Signalods.SetInvalid(InvalidType.UserOdPrefs);
        UserOdPrefs.RefreshCache();
        _clinicNum = 0;
    }

    public static void Update(ClinicDto clinic)
    {
        throw new NotImplementedException();
    }

    public static void Delete(ClinicDto clinic)
    {
        // #region Patients
        //
        // var command = "SELECT LName,FName FROM patient WHERE ClinicNum ="
        //               + SOut.Long(clinic.ClinicNum);
        // var table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var pats = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         pats += "\r";
        //         if (i == 15)
        //         {
        //             pats += Lans.g("Clinics", "And") + " " + (table.Rows.Count - i) + " " + Lans.g("Clinics", "others");
        //             break;
        //         }
        //
        //         pats += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"];
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because it is in use by the following patients:") + pats);
        // }
        //
        // #endregion
        //
        // #region Payments
        //
        // command = "SELECT patient.LName,patient.FName FROM patient,payment "
        //           + "WHERE payment.ClinicNum =" + SOut.Long(clinic.ClinicNum)
        //           + " AND patient.PatNum=payment.PatNum";
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var pats = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         pats += "\r";
        //         if (i == 15)
        //         {
        //             pats += Lans.g("Clinics", "And") + " " + (table.Rows.Count - i) + " " + Lans.g("Clinics", "others");
        //             break;
        //         }
        //
        //         pats += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"];
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because the following patients have payments using it:") + pats);
        // }
        //
        // #endregion
        //
        // #region ClaimPayments
        //
        // command = "SELECT patient.LName,patient.FName FROM patient,claimproc,claimpayment "
        //           + "WHERE claimpayment.ClinicNum =" + SOut.Long(clinic.ClinicNum)
        //           + " AND patient.PatNum=claimproc.PatNum"
        //           + " AND claimproc.ClaimPaymentNum=claimpayment.ClaimPaymentNum "
        //           + "GROUP BY patient.LName,patient.FName,claimpayment.ClaimPaymentNum";
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var pats = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         pats += "\r";
        //         if (i == 15)
        //         {
        //             pats += Lans.g("Clinics", "And") + " " + (table.Rows.Count - i) + " " + Lans.g("Clinics", "others");
        //             break;
        //         }
        //
        //         pats += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"];
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because the following patients have claim payments using it:") + pats);
        // }
        //
        // #endregion
        //
        // #region Appointments
        //
        // command = "SELECT patient.LName,patient.FName FROM patient,appointment "
        //           + "WHERE appointment.ClinicNum =" + SOut.Long(clinic.ClinicNum)
        //           + " AND patient.PatNum=appointment.PatNum";
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var pats = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         pats += "\r";
        //         if (i == 15)
        //         {
        //             pats += Lans.g("Clinics", "And") + " " + (table.Rows.Count - i) + " " + Lans.g("Clinics", "others");
        //             break;
        //         }
        //
        //         pats += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"];
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because the following patients have appointments using it:") + pats);
        // }
        //
        // #endregion
        //
        // #region Procedures
        //
        // //reassign procedure.ClinicNum=0 if the procs are status D.
        // command = "SELECT ProcNum FROM procedurelog WHERE ProcStatus=" + SOut.Int((int) ProcStat.D) + " AND ClinicNum=" + SOut.Long(clinic.ClinicNum);
        // var listProcNums = Db.GetListLong(command);
        // if (listProcNums.Count > 0)
        // {
        //     command = "UPDATE procedurelog SET ClinicNum=0 WHERE ProcNum IN (" + string.Join(",", listProcNums.Select(x => SOut.Long(x))) + ")";
        //     Db.NonQ(command);
        // }
        //
        // command = "SELECT patient.LName,patient.FName FROM patient,procedurelog "
        //           + "WHERE procedurelog.ClinicNum =" + SOut.Long(clinic.ClinicNum)
        //           + " AND patient.PatNum=procedurelog.PatNum";
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var pats = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         pats += "\r";
        //         if (i == 15)
        //         {
        //             pats += Lans.g("Clinics", "And") + " " + (table.Rows.Count - i) + " " + Lans.g("Clinics", "others");
        //             break;
        //         }
        //
        //         pats += table.Rows[i]["LName"] + ", " + table.Rows[i]["FName"];
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because the following patients have procedures using it:") + pats);
        // }
        //
        // #endregion
        //
        // #region Operatories
        //
        // command = "SELECT OpName FROM operatory "
        //           + "WHERE ClinicNum =" + SOut.Long(clinic.ClinicNum);
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var ops = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         ops += "\r";
        //         if (i == 15)
        //         {
        //             ops += Lans.g("Clinics", "And") + " " + (table.Rows.Count - i) + " " + Lans.g("Clinics", "others");
        //             break;
        //         }
        //
        //         ops += table.Rows[i]["OpName"].ToString();
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because the following operatories are using it:") + ops);
        // }
        //
        // #endregion
        //
        // #region Userod
        //
        // command = "SELECT UserName FROM userod "
        //           + "WHERE ClinicNum =" + SOut.Long(clinic.ClinicNum);
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var userNames = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         userNames += "\r";
        //         if (i == 15)
        //         {
        //             userNames += Lans.g("Clinics", "And") + " " + (table.Rows.Count - i) + " " + Lans.g("Clinics", "others");
        //             break;
        //         }
        //
        //         userNames += table.Rows[i]["UserName"].ToString();
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because the following Open Dental users are using it:") + userNames);
        // }
        //
        // #endregion
        //
        // #region AlertSub
        //
        // command = "SELECT DISTINCT UserNum FROM AlertSub "
        //           + "WHERE ClinicNum =" + SOut.Long(clinic.ClinicNum);
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var listUsers = new List<string>();
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         var userNum = SIn.Long(table.Rows[i]["UserNum"].ToString());
        //         var userod = Userods.GetUser(userNum);
        //         if (userod == null) //Should not happen.
        //             continue;
        //         listUsers.Add(userod.UserName);
        //     }
        //
        //     throw new Exception(Lans.g("Clinics", "Cannot delete clinic because the following Open Dental users are subscribed to it:") + "\r" + string.Join("\r", listUsers.OrderBy(x => x).ToList()));
        // }
        //
        // #endregion
        //
        // #region UserClinics
        //
        // command = "SELECT userod.UserName FROM userclinic INNER JOIN userod ON userclinic.UserNum=userod.UserNum "
        //           + "WHERE userclinic.ClinicNum=" + SOut.Long(clinic.ClinicNum);
        // table = DataCore.GetTable(command);
        // if (table.Rows.Count > 0)
        // {
        //     var userNames = "";
        //     for (var i = 0; i < table.Rows.Count; i++)
        //     {
        //         if (i > 0) userNames += ",";
        //         userNames += table.Rows[i][0].ToString();
        //     }
        //
        //     throw new Exception(
        //         Lans.g("Clinics", "Cannot delete clinic because the following users are restricted to this clinic in security setup:") + " " + userNames);
        // }
        //
        // #endregion
        //
        // //End checking for dependencies.
        // //Clinic is not being used, OK to delete.
        // //Delete clinic specific program properties.
        // command = "DELETE FROM programproperty WHERE ClinicNum=" + SOut.Long(clinic.ClinicNum) + " AND ClinicNum!=0"; //just in case a programming error tries to delete an invalid clinic.
        // Db.NonQ(command);
        // ClinicCrud.Delete(clinic.ClinicNum);
    }

    public static List<long> GetListByRegion(List<long> regionDefNums)
    {
        return GetWhere(x => regionDefNums.Contains(x.RegionId??0)).Select(x => x.Id).Distinct().ToList();
    }

    public static ClinicDto GetClinic(long clinicNum)
    {
        return Cache.GetFirstOrDefault(x => x.Id == clinicNum, true);
    }

    public static List<ClinicDto> GetClinics(List<long> clinicIds)
    {
        return Cache.GetWhere(x => clinicIds.Contains(x.Id));
    }

    public static ClinicDto GetClinicForRecall(long recallNum)
    {
        throw new NotImplementedException();

        //     if (!true) return null;
        //     var command = "SELECT patient.ClinicNum FROM patient "
        //                   + "INNER JOIN recall ON patient.PatNum=recall.PatNum "
        //                   + "WHERE recall.RecallNum=" + SOut.Long(recallNum) + " "
        //                   + DbHelper.LimitAnd(1);
        //     var clinicNumPatient = SIn.Long(DataCore.GetScalar(command));
        //     if (clinicNumPatient > 0) return GetFirstOrDefault(x => x.ClinicNum == clinicNumPatient);
        //     //Patient does not have an assigned clinic.  Grab the clinic from a scheduled or completed appointment with the largest date.
        //     command = @"SELECT appointment.ClinicNum,appointment.AptDateTime 
        // FROM appointment
        // INNER JOIN recall ON appointment.PatNum=recall.PatNum AND recall.RecallNum=" + SOut.Long(recallNum) + @"
        // WHERE appointment.AptStatus IN (" + SOut.Int((int) ApptStatus.Scheduled) + "," + SOut.Int((int) ApptStatus.Complete) + ")" + @"
        // ORDER BY AptDateTime DESC";
        //     command = DbHelper.LimitOrderBy(command, 1);
        //     var clinicNumAppt = SIn.Long(DataCore.GetScalar(command));
        //     if (clinicNumAppt > 0) return GetFirstOrDefault(x => x.ClinicNum == clinicNumAppt);
        //     return null;
    }

    public static List<ClinicDto> GetClinicsNoCache()
    {
        return GetDeepCopy();
    }

    public static string GetDesc(long clinicId, IReadOnlyList<ClinicDto> clinics = null)
    {
        if (clinics is {Count: > 0})
        {
            return clinics.Where(x => x.Id == clinicId).Select(x => x.Description).FirstOrDefault();
        }

        var clinic = GetFirstOrDefault(x => x.Id == clinicId);

        return clinic?.Description ?? string.Empty;
    }

    public static string GetAbbr(long clinicId, IReadOnlyList<ClinicDto> clinics = null)
    {
        if (clinics is {Count: > 0})
        {
            return clinics.Where(x => x.Id == clinicId).Select(x => x.Description).FirstOrDefault();
        }

        var clinic = GetFirstOrDefault(x => x.Id == clinicId);

        return clinic?.Description ?? string.Empty;
    }

    public static PlaceOfService GetPlaceService(long clinicId)
    {
        var code = GetFirstOrDefault(x => x.Id == clinicId)?.DefaultPlaceOfService ?? string.Empty;

        return PlaceOfServiceCodes.ToEnum(code);
    }

    public static long GetByDesc(string description)
    {
        var clinic =
            Cache.GetFirstOrDefault(x => string.Equals(x.Description, description, StringComparison.CurrentCultureIgnoreCase), true) ??
            Cache.GetFirstOrDefault(x => string.Equals(x.Description, description, StringComparison.CurrentCultureIgnoreCase));

        return clinic?.Id ?? 0;
    }

    public static List<ClinicDto> GetForUserod(Userod userod, bool doIncludeHQ = false, string hqClinicName = null)
    {
        var listClinics = new List<ClinicDto>();
        //Add HQ clinic if requested, even if clinics are disabled.  Counter-intuitive, but required for offices that had clinics enabled and then
        //turned them off.  If clinics are enabled and the user is restricted this will be filtered out below.
        if (doIncludeHQ) listClinics.Add(GetPracticeAsClinicZero(hqClinicName));
        listClinics.AddRange(GetDeepCopy(true)); //don't include hidden clinics
        if (!true || !userod.ClinicIsRestricted || userod.ClinicNum == 0) return listClinics;
        //If Clinics are enabled and user is restricted, then only return clinics the person has permission for.
        var listUserClinicNums = UserClinics.GetForUser(userod.UserNum).Select(x => x.ClinicNum).ToList();
        listClinics.RemoveAll(x => !listUserClinicNums.Contains(x.Id)); //Remove all clinics that are not in the list of UserClinics.
        return listClinics;
    }

    public static List<ClinicDto> GetAllForUserod(Userod userod)
    {
        var listClinics = GetDeepCopy();
        if (!userod.ClinicIsRestricted || userod.ClinicNum == 0) return listClinics;
        var listUserClinics = UserClinics.GetForUser(userod.UserNum);
        return listClinics.FindAll(x => listUserClinics.Exists(y => y.ClinicNum == x.Id)).ToList();
    }

    public static bool IsDefaultClinicProvider(long providerId)
    {
        return GetFirstOrDefault(x => x.DefaultProviderId == providerId) != null;
    }

    public static bool IsInsBillingProvider(long providerId)
    {
        return GetFirstOrDefault(x => x.BillingProviderId == providerId) != null;
    }

    public static ClinicDto GetDefaultForTexting()
    {
        return GetFirstOrDefault(x => x.Id == PrefC.GetLong(PrefName.TextingDefaultClinicNum));
    }

    public static ClinicDto GetClinicOrSmsDefaultOrPracticeClinic(long clinicId)
    {
        return GetClinic(clinicId) ?? GetDefaultForTexting() ?? GetPracticeAsClinicZero();
    }

    public static bool IsTextingEnabled(long clinicId)
    {
        return GetClinic(clinicId)?.SmsContractSignedOn is not null;
    }

    public static bool HasEmailHostingCredentials(long clinicNum)
    {
        bool hasEmailHostingCred;
        if (clinicNum == 0)
        {
            hasEmailHostingCred = !string.IsNullOrWhiteSpace(PrefC.GetString(PrefName.MassEmailGuid))
                                  && !string.IsNullOrWhiteSpace(PrefC.GetString(PrefName.MassEmailSecret));
            return hasEmailHostingCred;
        }

        hasEmailHostingCred = !string.IsNullOrWhiteSpace(ClinicPrefs.GetPref(PrefName.MassEmailGuid, clinicNum)?.ValueString)
                              && !string.IsNullOrWhiteSpace(ClinicPrefs.GetPref(PrefName.MassEmailSecret, clinicNum)?.ValueString);
        return hasEmailHostingCred;
    }

    public static bool IsMassEmailSignedUp(long clinicNum)
    {
        var hostedEmailStatus = GetEmailHostingStatus(PrefName.MassEmailStatus, clinicNum);
        var isMassEmailUp = hostedEmailStatus.HasFlag(HostedEmailStatus.SignedUp) && HasEmailHostingCredentials(clinicNum);
        return isMassEmailUp;
    }

    public static bool IsMassEmailEnabled(long clinicNum)
    {
        var isMassEmailEnabled = IsMassEmailSignedUp(clinicNum) && GetEmailHostingStatus(PrefName.MassEmailStatus, clinicNum).HasFlag(HostedEmailStatus.Enabled);
        return isMassEmailEnabled;
    }

    public static bool IsSecureEmailSignedUp(long clinicNum)
    {
        var hostedEmailStatus = GetEmailHostingStatus(PrefName.EmailSecureStatus, clinicNum);
        var isSecureEmailUp = hostedEmailStatus.HasFlag(HostedEmailStatus.SignedUp) && HasEmailHostingCredentials(clinicNum);
        return isSecureEmailUp;
    }

    public static bool IsSecureEmailEnabled(long clinicNum)
    {
        var isSecureEmailEnabled = IsSecureEmailSignedUp(clinicNum) && GetEmailHostingStatus(PrefName.EmailSecureStatus, clinicNum).HasFlag(HostedEmailStatus.Enabled);
        return isSecureEmailEnabled;
    }

    private static HostedEmailStatus GetEmailHostingStatus(PrefName prefName, long clinicNum)
    {
        HostedEmailStatus hostedEmailStatus;
        if (clinicNum == 0)
        {
            hostedEmailStatus = PrefC.GetEnum<HostedEmailStatus>(prefName);
            return hostedEmailStatus;
        }

        //Does not default to the practice preference value if not found.  Intentional.
        hostedEmailStatus = SIn.Enum<HostedEmailStatus>(ClinicPrefs.GetInt(prefName, clinicNum));
        return hostedEmailStatus;
    }

    public static bool IsMedicalPracticeOrClinic(long clinicNum)
    {
        var clinic = GetClinic(clinicNum);
        return clinic is {IsMedicalOnly: true};
    }

    public static ClinicDto GetPracticeAsClinicZero(string clinicName = null)
    {
        return GetFirstOrDefault(x => x.Id == 1);
    }

    public static string ReplaceOffice(string message, ClinicDto clinic, bool isHtmlEmail = false, bool doReplaceDisclaimer = false)
    {
        var stringBuilder = new StringBuilder(message);
        ReplaceOffice(stringBuilder, clinic, isHtmlEmail, doReplaceDisclaimer);
        return stringBuilder.ToString();
    }

    public static void ReplaceOffice(StringBuilder stringBuilder, ClinicDto clinic, bool isHtmlEmail = false, bool replaceDisclaimer = false)
    {
        var officePhone = GetOfficePhone(clinic);
        var officeName = GetOfficeName(clinic);
        var officeAddr = GetOfficeAddress(clinic);
        var officeFax = GetOfficeFax(clinic);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[OfficePhone]", officePhone, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[OfficeFax]", officeFax, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[OfficeName]", officeName, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[OfficeAddress]", officeAddr, isHtmlEmail);
        if (replaceDisclaimer) ReplaceTags.ReplaceOneTag(stringBuilder, "[EmailDisclaimer]", EmailMessages.GetEmailDisclaimer(clinic?.Id ?? 0), isHtmlEmail);
    }

    public static string GetOfficeName(ClinicDto clinic)
    {
        var officeName = clinic?.Description ?? string.Empty;
        return officeName;
    }

    public static string GetOfficeFax(ClinicDto clinic)
    {
        var officeFax = clinic?.FaxNumber ?? string.Empty;
        return TelephoneNumbers.ReFormat(officeFax);
    }

    public static string GetOfficePhone(ClinicDto clinic)
    {
        var officePhone = clinic?.PhoneNumber ?? string.Empty;
        return TelephoneNumbers.ReFormat(officePhone);
    }

    public static string GetOfficeAddress(ClinicDto clinic)
    {
        return Patients.GetAddressFull(clinic.AddressLine1, clinic.AddressLine2, clinic.City, clinic.State, clinic.Zip);
    }

    public static List<ClinicCount> GetListClinicPatientCount(bool isAllStatuses = false)
    {
        var command = "SELECT ClinicNum,COUNT(*) AS Count FROM patient ";
        if (!isAllStatuses)
            command += "WHERE PatStatus NOT IN (" + SOut.Int((int) PatientStatus.Deleted) + "," + SOut.Int((int) PatientStatus.Archived) + ","
                       + SOut.Int((int) PatientStatus.Deceased) + "," + SOut.Int((int) PatientStatus.NonPatient) + ") ";
        command += "GROUP BY ClinicNum";
        var table = DataCore.GetTable(command);
        var listClinicCounts = new List<ClinicCount>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var clinicCount = new ClinicCount();
            clinicCount.ClinicNum = SIn.Long(table.Rows[i]["ClinicNum"].ToString());
            clinicCount.Count = SIn.Int(table.Rows[i]["Count"].ToString());
            listClinicCounts.Add(clinicCount);
        }

        return listClinicCounts;
    }

    public class ClinicCount
    {
        public long ClinicNum;
        public int Count;
    }

    private class ClinicCache : ListCache<ClinicDto>
    {
        protected override List<ClinicDto> GetCacheFromDb()
        {
            return ClinicService.GetAll();
        }
        
        protected override bool InShortList(ClinicDto item)
        {
            return !item.IsHidden;
        }
    }

    private static readonly ClinicCache Cache = new();

    public static List<ClinicDto> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static int GetCount(bool shortList = false)
    {
        return Cache.GetCount(shortList);
    }

    public static ClinicDto GetFirst(bool isShort = false)
    {
        return Cache.GetFirst(isShort);
    }

    public static ClinicDto GetFirstOrDefault(Func<ClinicDto, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static List<ClinicDto> GetWhere(Predicate<ClinicDto> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        Cache.Refresh();
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}