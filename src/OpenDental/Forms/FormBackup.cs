using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using OpenDental.Bridges;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental
{
    public partial class FormBackup : FormODBase
    {
        //private bool usesInternalImages;
        private double _amtCopied = 0; // used for reporting amount copied for backup and restore of ODI in recursive calls

        
        public FormBackup()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            InitializeLayoutManager();
            Lan.F(this);
        }

        private void FormBackup_Load(object sender, System.EventArgs e)
        {
            #region Backup Tab

            //usesInternalImages=(PrefC.GetString(PrefName.ImageStore)=="OpenDental.Imaging.SqlStore");
            checkExcludeImages.Checked = PrefC.GetBool(PrefName.BackupExcludeImageFolder);
            checkArchiveDoBackupFirst.Checked = PrefC.GetBool(PrefName.ArchiveDoBackupFirst);
            textBackupFromPath.Text = PrefC.GetString(PrefName.BackupFromPath);
            textBackupToPath.Text = PrefC.GetString(PrefName.BackupToPath);
            textBackupRestoreFromPath.Text = PrefC.GetString(PrefName.BackupRestoreFromPath);
            textBackupRestoreToPath.Text = PrefC.GetString(PrefName.BackupRestoreToPath);
            textBackupRestoreAtoZToPath.Text = PrefC.GetString(PrefName.BackupRestoreAtoZToPath);
            textBackupRestoreAtoZToPath.Enabled = ShouldUseAtoZFolder();
            butBrowseRestoreAtoZTo.Enabled = ShouldUseAtoZFolder();
            if (ProgramProperties.IsAdvertisingDisabled(ProgramName.CentralDataStorage))
            {
                groupManagedBackups.Visible = false;
            }

            #endregion

            #region Archive Tab

            string decryptedPassword;
            checkEmailMessage.Checked = false;
            checkSecurityLog.Checked = true;
            CDT.Class1.Decrypt(PrefC.GetString(PrefName.ArchivePassHash), out decryptedPassword);
            textArchivePass.Text = decryptedPassword;
            textArchivePass.PasswordChar = (textArchivePass.Text == "" ? default(char) : '*');
            textArchiveServerName.Text = PrefC.GetString(PrefName.ArchiveServerName);
            textArchiveUser.Text = PrefC.GetString(PrefName.ArchiveUserName);
            //If pref is set, use it.  Otherwise, 3 years ago.
            if (PrefC.GetDate(PrefName.ArchiveDate) == DateTime.MinValue)
            {
                dateTimeArchive.Value = DateTime.Today.AddYears(-3);
            }
            else
            {
                dateTimeArchive.Value = PrefC.GetDate(PrefName.ArchiveDate);
            }

            ToggleBackupSettings();

            #endregion

            #region Supplemental Tab

            checkSupplementalBackupEnabled.Checked = PrefC.GetBool(PrefName.SupplementalBackupEnabled);
            if (PrefC.GetDate(PrefName.SupplementalBackupDateLastComplete).Year > 1880)
            {
                textSupplementalBackupDateLastComplete.Text = PrefC.GetDate(PrefName.SupplementalBackupDateLastComplete).ToString();
            }

            textSupplementalBackupCopyNetworkPath.Text = PrefC.GetString(PrefName.SupplementalBackupNetworkPath);

            #endregion Supplemental Tab

            if (ODEnvironment.IsCloudServer)
            {
                //OD Cloud users cannot use this tool because they're InnoDb.
                tabControl1.TabPages.Remove(tabPageBackup);
                //We don't want to allow the user to connect to another server.
                checkArchiveDoBackupFirst.Visible = false;
                checkArchiveDoBackupFirst.Checked = false;
                groupBoxBackupConnection.Visible = false;
                //We don't want the user to be able to tell if a directory exists.
                tabControl1.TabPages.Remove(tabPageSupplementalBackups);
            }
        }

        #region Backup Tab

        private bool IsBackupTabValid()
        {
            //test for trailing slashes
            if (textBackupFromPath.Text != "" && !textBackupFromPath.Text.EndsWith("" + Path.DirectorySeparatorChar))
            {
                MsgBox.Show(this, "Paths must end with " + Path.DirectorySeparatorChar + ".");
                return false;
            }

            if (textBackupToPath.Text != "" && !textBackupToPath.Text.EndsWith("" + Path.DirectorySeparatorChar))
            {
                MsgBox.Show(this, "Paths must end with " + Path.DirectorySeparatorChar + ".");
                return false;
            }

            if (textBackupRestoreFromPath.Text != "" && !textBackupRestoreFromPath.Text.EndsWith("" + Path.DirectorySeparatorChar))
            {
                MsgBox.Show(this, "Paths must end with " + Path.DirectorySeparatorChar + ".");
                return false;
            }

            if (textBackupRestoreToPath.Text != "" && !textBackupRestoreToPath.Text.EndsWith("" + Path.DirectorySeparatorChar))
            {
                MsgBox.Show(this, "Paths must end with " + Path.DirectorySeparatorChar + ".");
                return false;
            }

            if (textBackupRestoreAtoZToPath.Text != "" && !textBackupRestoreAtoZToPath.Text.EndsWith("" + Path.DirectorySeparatorChar))
            {
                MsgBox.Show(this, "Paths must end with " + Path.DirectorySeparatorChar + ".");
                return false;
            }

            return true;
        }

        private bool SaveTabPrefs()
        {
            bool hasChanged = false;
            hasChanged |= Prefs.UpdateBool(PrefName.BackupExcludeImageFolder, checkExcludeImages.Checked);
            hasChanged |= Prefs.UpdateBool(PrefName.ArchiveDoBackupFirst, checkArchiveDoBackupFirst.Checked);
            hasChanged |= Prefs.UpdateString(PrefName.BackupFromPath, textBackupFromPath.Text);
            hasChanged |= Prefs.UpdateString(PrefName.BackupToPath, textBackupToPath.Text);
            hasChanged |= Prefs.UpdateString(PrefName.BackupRestoreFromPath, textBackupRestoreFromPath.Text);
            hasChanged |= Prefs.UpdateString(PrefName.BackupRestoreToPath, textBackupRestoreToPath.Text);
            hasChanged |= Prefs.UpdateString(PrefName.BackupRestoreAtoZToPath, textBackupRestoreAtoZToPath.Text);
            hasChanged |= Prefs.UpdateString(PrefName.ArchiveServerName, textArchiveServerName.Text);
            hasChanged |= Prefs.UpdateString(PrefName.ArchiveUserName, textArchiveUser.Text);
            string encryptedPassword;
            CDT.Class1.Encrypt(textArchivePass.Text, out encryptedPassword);
            hasChanged |= Prefs.UpdateString(PrefName.ArchivePassHash, encryptedPassword);
            return hasChanged;
        }

        private bool ShouldUseAtoZFolder()
        {
            if (false)
            {
                return false;
            }

            if (checkExcludeImages.Checked)
            {
                return false;
            }

            return true;
        }

        private void butBrowseFrom_Click(object sender, System.EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = textBackupFromPath.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            textBackupFromPath.Text = ODFileUtils.CombinePaths(folderBrowserDialog.SelectedPath, ""); //Add trail slash.
        }

        private void butBrowseTo_Click(object sender, System.EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = textBackupToPath.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            textBackupToPath.Text = ODFileUtils.CombinePaths(folderBrowserDialog.SelectedPath, ""); //Add trail slash.
        }

        private void butBrowseRestoreFrom_Click(object sender, System.EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = textBackupRestoreFromPath.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            textBackupRestoreFromPath.Text = ODFileUtils.CombinePaths(folderBrowserDialog.SelectedPath, ""); //Add trail slash.
        }

        private void butBrowseRestoreTo_Click(object sender, System.EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = textBackupRestoreToPath.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            textBackupRestoreToPath.Text = ODFileUtils.CombinePaths(folderBrowserDialog.SelectedPath, ""); //Add trail slash.
        }

        private void butBrowseRestoreAtoZTo_Click(object sender, System.EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = textBackupRestoreAtoZToPath.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            textBackupRestoreAtoZToPath.Text = ODFileUtils.CombinePaths(folderBrowserDialog.SelectedPath, ""); //Add trail slash.
        }

        private void butBackup_Click(object sender, System.EventArgs e)
        {
            // if(!IsBackupTabValid()) {
            // 	return;
            // }
            // //Ensure that the backup from and backup to paths are different. This is to prevent the live database
            // //from becoming corrupt.
            // if(this.textBackupFromPath.Text.Trim().ToLower()==this.textBackupToPath.Text.Trim().ToLower()) {
            // 	MsgBox.Show(this,"The backup from path and backup to path must be different.");
            // 	return;
            // }
            // //test saving defaults
            // if(textBackupFromPath.Text!=PrefC.GetString(PrefName.BackupFromPath)
            // 	|| textBackupToPath.Text!=PrefC.GetString(PrefName.BackupToPath)
            // 	|| textBackupRestoreFromPath.Text!=PrefC.GetString(PrefName.BackupRestoreFromPath)
            // 	|| textBackupRestoreToPath.Text!=PrefC.GetString(PrefName.BackupRestoreToPath)
            // 	|| textBackupRestoreAtoZToPath.Text!=PrefC.GetString(PrefName.BackupRestoreAtoZToPath)) 
            // {
            // 	if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Set as default?") && SaveTabPrefs()) {
            // 		DataValid.SetInvalid(InvalidType.Prefs);
            // 	}
            // }
            // string dbName=MiscData.GetCurrentDatabase();
            // if(!Directory.Exists(ODFileUtils.CombinePaths(textBackupFromPath.Text,dbName))){// C:\mysql\data\opendental
            // 	MsgBox.Show(this,"Backup FROM path is invalid.");
            // 	return;
            // }
            // if(!Directory.Exists(textBackupToPath.Text)){// D:\
            // 	MsgBox.Show(this,"Backup TO path is invalid.");
            // 	return;
            // }
            // bool isInnoDb=InnoDb.HasInnoDbTables(dbName);
            // double databaseSize=GetFileSizes(textBackupFromPath.Text+dbName)/1024;
            // if(isInnoDb) {
            // 	//This is just an estimate since we will create a MyISAM backup of the InnoDB database and then copy the MYISAM database to the backup directory.
            // 	//Most likely MYISAM database will be smaller, but make sure we have enough space on disk just in case.
            // 	databaseSize*=2;
            // }
            // if(!hasDriveSpace(textBackupToPath.Text,databaseSize)) {//ensure drive space before attempting backup
            // 	MsgBox.Show(this,Lan.g(this,"Not enough free disk space available on the destination drive to backup the database."));
            // 	return;
            // }
            // string msg="";
            // //there is enough drive space, show progress bar and make backup
            // ProgressWin progressOD=new ProgressWin();
            // progressOD.IsBlocks=true;
            // progressOD.ActionMain=() => InstanceMethodDatabaseBackup(dbName,textBackupFromPath.Text,textBackupToPath.Text,isInnoDb,out msg);
            // progressOD.StartingMessage=Lan.g(this,"Preparing backup");
            // try {
            // 	progressOD.ShowDialog();
            // }
            // catch(Exception ex) {
            // 	//catch error from InstanceMethodDatabaseBackup thrown from inside the ProgressOD thread
            // 	MsgBox.Show(ex.Message);
            // 	return;
            // }
            // if(progressOD.IsCancelled) {
            // 	return;
            // }
            // SecurityLogs.MakeLogEntry(EnumPermType.Backup,0,Lan.g(this,"Database backup created at ")+textBackupToPath.Text);
            // //AtoZ folder if selected for backup.=================================================================================================================
            // if(!ShouldUseAtoZFolder()) {
            // 	MessageBox.Show(msg);
            // 	Close();
            // 	return;
            // }
            // string aToZFullPath=ODFileUtils.RemoveTrailingSeparators(ImageStore.GetPreferredAtoZpath());
            // string aToZDirectory=aToZFullPath.Substring(aToZFullPath.LastIndexOf(Path.DirectorySeparatorChar)+1);
            // double aToZSize=GetFileSizes(ODFileUtils.CombinePaths(aToZFullPath,""),
            // 	ODFileUtils.CombinePaths(new string[] { textBackupToPath.Text,aToZDirectory,"" }))/1024;
            // progressOD=new ProgressWin();
            // progressOD.IsBlocks=true;
            // progressOD.ActionMain=() => {
            // 	if(!hasDriveSpace(textBackupToPath.Text,aToZSize)) {
            // 		throw new Exception(Lan.g(this,"Not enough free disk space available on the destination drive to backup the A to Z folder."));
            // 	}
            // 	InstanceMethodAtoZBackup(textBackupToPath.Text,aToZDirectory,aToZFullPath,aToZSize);
            // };
            // progressOD.StartingMessage=Lan.g(this,"Backing up A to Z Folder");
            // try {
            // 	progressOD.ShowDialog();
            // }
            // catch(Exception ex) {
            // 	//catch error from InstanceMethodAtoZBackup thrown from inside the ProgressOD thread
            // 	MsgBox.Show(ex.Message);
            // 	return;
            // }
            // if(progressOD.IsCancelled) {
            // 	return;
            // }
            // SecurityLogs.MakeLogEntry(EnumPermType.Backup,0,Lan.g(this,"A to Z folder backup created at ")+textBackupToPath.Text);
            // MessageBox.Show(Lan.g(this,msg));
            // Close();
        }

        private void butRestore_Click(object sender, System.EventArgs e)
        {
            // if(textBackupRestoreFromPath.Text!="" && !textBackupRestoreFromPath.Text.EndsWith(""+Path.DirectorySeparatorChar)){
            // 	MessageBox.Show(Lan.g(this,"Paths must end with ")+Path.DirectorySeparatorChar+".");
            // 	return;
            // }
            // if(textBackupRestoreToPath.Text!="" && !textBackupRestoreToPath.Text.EndsWith(""+Path.DirectorySeparatorChar)){
            // 	MessageBox.Show(Lan.g(this,"Paths must end with ")+Path.DirectorySeparatorChar+".");
            // 	return;
            // }
            // if(ShouldUseAtoZFolder()) {
            // 	if(textBackupRestoreAtoZToPath.Text!="" && !textBackupRestoreAtoZToPath.Text.EndsWith(""+Path.DirectorySeparatorChar)){
            // 		MessageBox.Show(Lan.g(this,"Paths must end with ")+Path.DirectorySeparatorChar+".");
            // 		return;
            // 	}
            // }
            // if(Environment.OSVersion.Platform!=PlatformID.Unix){
            // 	//dmg This check will not work on Linux, because mapped drives exist as regular (mounted) paths. Perhaps there
            // 	//is another way to check for this on Linux.
            // 	if(textBackupRestoreToPath.Text!="" && textBackupRestoreToPath.Text.StartsWith(""+Path.DirectorySeparatorChar)){
            // 		MsgBox.Show(this,"The restore database TO folder must be on this computer.");
            // 		return;
            // 	}
            // }
            // //pointless to save defaults
            // string dbName=MiscData.GetCurrentDatabase();
            // if(InnoDb.HasInnoDbTables(dbName)) {
            // 	//Database has innodb tables. Restore tool does not work on dbs with InnoDb tables. 
            // 	MsgBox.Show(this,"InnoDb tables detected. Restore tool cannot run with InnoDb tables.");
            // 	return;
            // }
            // if(!Directory.Exists(ODFileUtils.CombinePaths(textBackupRestoreFromPath.Text,dbName))){// D:\opendental
            // 	MessageBox.Show(Lan.g(this,"Restore FROM path is invalid.  Unable to find folder named ")+dbName);
            // 	return;
            // }
            // if(!Directory.Exists(ODFileUtils.CombinePaths(textBackupRestoreToPath.Text,dbName))) {// C:\mysql\data\opendental
            // 	MessageBox.Show(Lan.g(this,"Restore TO path is invalid.  Unable to find folder named ")+dbName);
            // 	return;
            // }
            // if(ShouldUseAtoZFolder()) {
            // 	if(!Directory.Exists(textBackupRestoreAtoZToPath.Text)) {// C:\OpenDentalData\
            // 		MsgBox.Show(this,"Restore A-Z images TO path is invalid.");
            // 		return;
            // 	}
            // 	string aToZFullPath=textBackupRestoreAtoZToPath.Text;// C:\OpenDentalData\
            // 	//remove the trailing \
            // 	aToZFullPath=aToZFullPath.Substring(0,aToZFullPath.Length-1);// C:\OpenDentalData
            // 	string aToZDirectory=aToZFullPath.Substring(aToZFullPath.LastIndexOf(Path.DirectorySeparatorChar)+1);// OpenDentalData
            // 	if(!Directory.Exists(ODFileUtils.CombinePaths(textBackupRestoreFromPath.Text,aToZDirectory))){// D:\OpenDentalData
            // 		MsgBox.Show(this,"Restore A-Z images FROM path is invalid.");
            // 		return;
            // 	}
            // }
            // string fromPath=ODFileUtils.CombinePaths(new string[] {textBackupRestoreFromPath.Text,dbName,""});// D:\opendental\
            // DirectoryInfo directoryInfo=new DirectoryInfo(fromPath);//does not check to see if dir exists
            // if(MessageBox.Show(Lan.g(this,"Restore from backup created on")+"\r\n"
            // 	+directoryInfo.LastWriteTime.ToString("dddd")+"  "+directoryInfo.LastWriteTime.ToString()
            // 	,"",MessageBoxButtons.OKCancel,MessageBoxIcon.Question)==DialogResult.Cancel) {
            // 	return;
            // }
            // Cursor=Cursors.WaitCursor;
            // //stop the service--------------------------------------------------------------------------------------
            // string backupPath=textBackupRestoreToPath.Text;
            // string filePidPath=ODFileUtils.CombinePaths(backupPath,$"{Environment.MachineName}.pid");
            // if(!File.Exists(filePidPath)) {
            // 	MessageBox.Show(Lan.g(this,"Cannot find PID file in ")+backupPath+Lan.g(this," to locate SQL service name."));
            // 	return;
            // }
            // string strPid=File.ReadAllText(filePidPath).Trim();
            // string serviceName="";
            // try {
            // 	int processId=PIn.Int(strPid);
            // 	serviceName=ServicesHelper.GetProcessServiceName(processId);
            // }
            // catch(Exception) {
            // 	MsgBox.Show(this,"Cannot find service name.");
            // 	return;
            // }
            // ServiceController serviceController=new ServiceController(serviceName);
            // if(!ServicesHelper.Stop(serviceController)) {
            // 	MsgBox.Show(this,"Unable to stop MySQL service.");
            // 	Cursor=Cursors.Default;
            // 	return;
            // }
            // //rename the current database---------------------------------------------------------------------------
            // //Get a name for the new directory
            // string newDb=dbName+"backup_"+DateTime.Today.ToString("MM_dd_yyyy");
            // if(Directory.Exists(ODFileUtils.CombinePaths(textBackupRestoreToPath.Text,newDb))){//if the new database name already exists
            // 	//find a unique one
            // 	int uniqueID=1;
            // 	string originalNewDb=newDb;
            // 	do{
            // 		newDb=originalNewDb+"_"+uniqueID.ToString();
            // 		uniqueID++;
            // 	}
            // 	while(Directory.Exists(ODFileUtils.CombinePaths(textBackupRestoreToPath.Text,newDb)));
            // }
            // //move the current db (rename)
            // Directory.Move(ODFileUtils.CombinePaths(textBackupRestoreToPath.Text,dbName)
            // 	,ODFileUtils.CombinePaths(textBackupRestoreToPath.Text,newDb));
            // //Restore----------------------------------------------------------------------------------------------
            // string toPath=textBackupRestoreToPath.Text;// C:\mysql\data\
            // Directory.CreateDirectory(ODFileUtils.CombinePaths(toPath,directoryInfo.Name));
            // FileInfo[] fileInfoArray=directoryInfo.GetFiles();
            // for(int i=0;i<fileInfoArray.Length;i++){
            // 	File.Copy(fileInfoArray[i].FullName,ODFileUtils.CombinePaths(new string[] {toPath,directoryInfo.Name,fileInfoArray[i].Name}));
            // }
            // //start the service--------------------------------------------------------------------------------------
            // ServicesHelper.Start(serviceController);
            // Cursor=Cursors.Default;
            // //restore A-Z folder, and give user a chance to cancel it.
            // if(ShouldUseAtoZFolder()) {
            // 	string aToZFullPath=ODFileUtils.RemoveTrailingSeparators(ImageStore.GetPreferredAtoZpath());
            // 	ProgressWin progressOD=new ProgressWin();
            // 	progressOD.IsBlocks=true;
            // 	progressOD.ActionMain=() => InstanceMethodRestore(aToZFullPath,textBackupRestoreFromPath.Text);
            // 	progressOD.StartingMessage=Lan.g(this,"Database restored.\r\nRestoring A to Z folder.");
            // 	try {
            // 		progressOD.ShowDialog();
            // 	}
            // 	catch(Exception ex) {
            // 		//catch error from InstanceMethodRestore thrown from inside the ProgressOD thread
            // 		MsgBox.Show(ex.Message);
            // 		return;
            // 	}
            // 	if(progressOD.IsCancelled) {
            // 		return;
            // 	}
            // 	MessageBox.Show(Lan.g(this,"Restore complete."));
            // }
            // Version versionProgramVersionDb=new Version(PrefC.GetStringNoCache(PrefName.ProgramVersion));
            // Version versionProductVersion=new Version(Application.ProductVersion);
            // if(versionProgramVersionDb!=versionProductVersion) {
            // 	MsgBox.Show(this,"The restored database version is different than the version installed and requires a restart.  The program will now close.");
            // 	FormOpenDental.S_ProcessKillCommand();
            // 	return;
            // }
            // else {
            // 	DataValid.SetInvalid(Cache.GetAllCachedInvalidTypes().ToArray());
            // }
            // MsgBox.Show(this,"Done");
            // Close();
            // return;
        }

        private void butSave_Click(object sender, System.EventArgs e)
        {
            if (!IsBackupTabValid())
            {
                return;
            }

            if (SaveTabPrefs())
            {
                DataValid.SetInvalid(InvalidType.Prefs);
            }
        }

        private void checkExcludeImages_Click(object sender, EventArgs e)
        {
            textBackupRestoreAtoZToPath.Enabled = ShouldUseAtoZFolder();
            butBrowseRestoreAtoZTo.Enabled = ShouldUseAtoZFolder();
        }

        private void pictureCDS_Click(object sender, EventArgs e)
        {
            if (!Programs.IsEnabledByHq(ProgramName.CentralDataStorage, out string err))
            {
                MsgBox.Show(err);
                return;
            }

            CDS.ShowPage();
        }

        #endregion

        #region Archive Tab

        private void ToggleBackupSettings()
        {
            UIHelper.GetAllControls(groupBoxBackupConnection).ForEach(x => x.Enabled = checkArchiveDoBackupFirst.Checked);
        }

        private void checkMakeBackup_CheckedChanged(object sender, EventArgs e)
        {
            ToggleBackupSettings();
        }

        private void butArchive_Click(object sender, EventArgs e)
        {
        }

        private void butSaveArchive_Click(object sender, EventArgs e)
        {
        }

        #endregion

        #region Supplemental Tab

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPageSupplementalBackups)
            {
                if (!Security.IsAuthorized(EnumPermType.SecurityAdmin))
                {
                    tabControl1.SelectedTab = tabPageBackup;
                    return;
                }
            }
        }

        private void ButSupplementalBrowse_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = textSupplementalBackupCopyNetworkPath.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textSupplementalBackupCopyNetworkPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void ButSupplementalSaveDefaults_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textSupplementalBackupCopyNetworkPath.Text) && !Directory.Exists(textSupplementalBackupCopyNetworkPath.Text))
            {
                MsgBox.Show(this, "Invalid or inaccessible " + labelSupplementalBackupCopyNetworkPath.Text + "."); //This label text will rarely change.
                return;
            }

            if (Prefs.UpdateBool(PrefName.SupplementalBackupEnabled, checkSupplementalBackupEnabled.Checked))
            {
                try
                {
                    //Inform HQ when the supplemental backups are enabled/disabled and which security admin performed the change.
                    PayloadItem payloadItemStatus = new PayloadItem(
                        (int) (checkSupplementalBackupEnabled.Checked ? SupplementalBackupStatuses.Enabled : SupplementalBackupStatuses.Disabled),
                        "SupplementalBackupStatus");
                    PayloadItem payloadItemAdminUserName = new PayloadItem(Security.CurUser.UserName, "AdminUserName");
                    string officeData = PayloadHelper.CreatePayload(new List<PayloadItem>() {payloadItemStatus, payloadItemAdminUserName}, eServiceCode.SupplementalBackup);
                    WebServiceMainHQProxy.GetWebServiceMainHQInstance().SetSupplementalBackupStatus(officeData);
                }
                catch (Exception ex)
                {
                }

                SecurityLogs.MakeLogEntry(EnumPermType.SupplementalBackup, 0,
                    "Supplemental backup has been " + (checkSupplementalBackupEnabled.Checked ? "Enabled" : "Disabled") + ".");
            }

            if (Prefs.UpdateString(PrefName.SupplementalBackupNetworkPath, textSupplementalBackupCopyNetworkPath.Text))
            {
                SecurityLogs.MakeLogEntry(EnumPermType.SupplementalBackup, 0,
                    labelSupplementalBackupCopyNetworkPath.Text + " changed to '" + textSupplementalBackupCopyNetworkPath.Text + "'.");
            }

            MsgBox.Show(this, "Saved");
        }

        #endregion Supplemental Tab

        private void checkOptimize_Click(object sender, EventArgs e)
        {
        }
    }
}