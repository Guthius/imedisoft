namespace OpenDental.UI;

public class IconSelector
{
    public static string GetBase64(EnumIcons icon)
    {
        return icon switch
        {
            EnumIcons.Account32 => Gen_Account32.GetBase64(),
            EnumIcons.Acquire => Gen_Acquire.GetBase64(),
            EnumIcons.Add => Gen_Add.GetBase64(),
            EnumIcons.Appt32 => Gen_Appt32.GetBase64(),
            EnumIcons.ArrowLeft => Gen_ArrowLeft.GetBase64(),
            EnumIcons.ArrowRight => Gen_ArrowRight.GetBase64(),
            EnumIcons.BreakAptX => Gen_BreakAptX.GetBase64(),
            EnumIcons.Chart32 => Gen_Chart32.GetBase64(),
            EnumIcons.Chart32G => Gen_Chart32G.GetBase64(),
            EnumIcons.Chart32W => Gen_Chart32W.GetBase64(),
            EnumIcons.ChartMed32 => Gen_ChartMed32.GetBase64(),
            EnumIcons.CommLog => Gen_CommLog.GetBase64(),
            EnumIcons.Complete => Gen_Complete.GetBase64(),
            EnumIcons.DeleteX => Gen_DeleteX.GetBase64(),
            EnumIcons.Email => Gen_Email.GetBase64(),
            EnumIcons.Family32 => Gen_Family32.GetBase64(),
            EnumIcons.ImageSelectorDoc => Gen_ImageSelectorDoc.GetBase64(),
            EnumIcons.ImageSelectorFile => Gen_ImageSelectorFile.GetBase64(),
            EnumIcons.ImageSelectorFolder => Gen_ImageSelectorFolder.GetBase64(),
            EnumIcons.ImageSelectorFolderWeb => Gen_ImageSelectorFolderWeb.GetBase64(),
            EnumIcons.ImageSelectorMount => Gen_ImageSelectorMount.GetBase64(),
            EnumIcons.ImageSelectorPhoto => Gen_ImageSelectorPhoto.GetBase64(),
            EnumIcons.ImageSelectorXray => Gen_ImageSelectorXray.GetBase64(),
            EnumIcons.Imaging32 => Gen_Imaging32.GetBase64(),
            EnumIcons.Manage32 => Gen_Manage32.GetBase64(),
            EnumIcons.PatAdd => Gen_PatAdd.GetBase64(),
            EnumIcons.PatDelete => Gen_PatDelete.GetBase64(),
            EnumIcons.Patient => Gen_Patient.GetBase64(),
            EnumIcons.PatMoveFam => Gen_PatMoveFam.GetBase64(),
            EnumIcons.PatSelect => Gen_PatSelect.GetBase64(),
            EnumIcons.PatSetGuarantor => Gen_PatSetGuarantor.GetBase64(),
            EnumIcons.Probe => Gen_Probe.GetBase64(),
            EnumIcons.Recall => Gen_Recall.GetBase64(),
            EnumIcons.Text => Gen_Text.GetBase64(),
            EnumIcons.TreatPlan32 => Gen_TreatPlan32.GetBase64(),
            EnumIcons.TreatPlanMed32 => Gen_TreatPlanMed32.GetBase64(),
            EnumIcons.Video => Gen_Video.GetBase64(),
            EnumIcons.WebMail => Gen_WebMail.GetBase64(),
            _ => ""
        };
    }
}