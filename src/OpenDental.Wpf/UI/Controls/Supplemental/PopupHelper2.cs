using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace WpfControls.UI {
	//=====WARNING! THERE IS A DUPLICATE OF THIS FILE OVER IN OpenDentBusiness.UI.PopupHelper.
	//=====UNTIL THAT FILE IS COMPLETELY DEPRECATED, BOTH FILES MUST BE KEPT IN SYNC.
	//=====ANY CHANGES TO ONE MUST ALSO BE MADE IN THE OTHER.
	///<summary>A helper class used add reference links to context menus</summary>
	public class PopupHelper2 {
		#region Methods - Public
		///<summary>For a given context menu item, returns a sorted list of menu item links. This supports wiki, patient, task, Job, URL, web, and file explorer.</summary>
		public static List<MenuItem> GetContextMenuItemLinks(string contextMenuItemText,bool rightClickLinks) {
			List<MenuItem> listMenuItemsLinks=new List<MenuItem>();
			List<string> listStringMatches=new List<string>();
			List<long> listNumMatches=new List<long>();
			listStringMatches=GetURLsFromText(contextMenuItemText);
			for(int i=0;i<listStringMatches.Count;i++) {
				string title=listStringMatches[i];
				if(title.Length>24) {
					title=title.Substring(0,24)+"...";
				}
				string strMatch=listStringMatches[i]; //To avoid lazy eval
				System.Windows.RoutedEventHandler eventHandler=(s,eArg)=> { OpenWebPage(strMatch); };
				listMenuItemsLinks.Add(new MenuItem("Web - "+title,eventHandler,tag:"autolink"));
			}
			listStringMatches=ODFileUtils.GetFilePathsFromText(contextMenuItemText);
			for(int i=0;i<listStringMatches.Count;i++) {
				string strMatch=listStringMatches[i]; //To avoid lazy eval
				System.Windows.RoutedEventHandler eventHandler=(s,eArg) => { OpenUNCPath(strMatch); };
				if(!false) {
					listMenuItemsLinks.Add(new MenuItem("File Explorer - "+listStringMatches[i],eventHandler));
				}
			}
			if(rightClickLinks) {
				listNumMatches=GetPatNumsFromText(contextMenuItemText);
				for(int i=0;i<listNumMatches.Count;i++) {
					long patNum=listNumMatches[i];
					System.Windows.RoutedEventHandler eventHandler=(s,eArg) => { OpenPatNum(patNum); };
					listMenuItemsLinks.Add(new MenuItem("PatNum - "+listNumMatches[i],eventHandler,tag:"autolink"));
				}
				listNumMatches=GetTaskNumsFromText(contextMenuItemText);
				for(int i=0;i<listNumMatches.Count;i++) {
					long taskNum=listNumMatches[i];
					System.Windows.RoutedEventHandler eventHandler=(s,eArg) => { OpenTaskNum(taskNum); };
					listMenuItemsLinks.Add(new MenuItem("TaskNum - "+listNumMatches[i],eventHandler,tag:"autolink"));
				}
			}
			listMenuItemsLinks=listMenuItemsLinks.OrderByDescending(x => x.Header.ToString()=="-").ThenBy(x => x.Header.ToString()).ToList();//alphabetize the link items.
			return listMenuItemsLinks;
		}

		///<summary>Returns a list of strings from the given text that are URLs.</summary>
		public static List<string> GetURLsFromText(string text) {
			//Regular expresion used to help identify URLs. This is not all encompassing.
			//There will be URLs that do not match this but this should work for 99%.
			//The url regex is generous enough to match urls fine and excludes emails well, but matches some files too.
			//These files get cleaned out though.
			string urlPattern=@"(?<!@)\b(?:https?:\/\/)?(?:www\.)?(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,4}(?:(?:\/|:)[^\s]*)?\b(?!(?:\\))";
			List<string> listStringMatches=Regex.Matches(text,urlPattern)
				.OfType<Match>()
				.Select(m => m.Groups[0].Value)
				.Distinct()
				.ToList();
			for(int i=listStringMatches.Count-1;i>=0;i--) {
				if(listStringMatches[i].StartsWith("(") && listStringMatches[i].EndsWith(")")) {
					listStringMatches[i]=listStringMatches[i].Substring(1,listStringMatches[i].Length-2);
				}
				listStringMatches[i]=listStringMatches[i].TrimEnd('.');
				Regex rgx=new Regex(@"[\\]{1}");
				if(rgx.IsMatch(listStringMatches[i])) {
					listStringMatches.RemoveAt(i);
					continue;
				}
			}
			return listStringMatches;
		}

		public static List<long> GetPatNumsFromText(string text) {
			//If this Regex pattern is ever changed, we may need to change the Select statement below.
			string strPatNum="patnum:";
			List<long> listNumMatches=Regex.Matches(text,$@"{strPatNum}\d+",RegexOptions.IgnoreCase)
				.OfType<Match>()
				.Select(x => SIn.Long(x.Groups[0].Value.Substring(strPatNum.Length),false))//Get pat num out of text.
				.Distinct()
				.ToList();
			return listNumMatches;
		}

		public static List<long> GetTaskNumsFromText(string text) {
			//If this Regex pattern is ever changed, we may need to change the Select statement below.
			string strTaskNum="tasknum:";
			List<long> listNumMatches=Regex.Matches(text,$@"{strTaskNum}\d+",RegexOptions.IgnoreCase)
				.OfType<Match>()
				.Select(x => SIn.Long(x.Groups[0].Value.Substring(strTaskNum.Length),false))//Get task num out of text.
				.Distinct()
				.ToList();
			return listNumMatches;
		}

		public static List<long> GetJobNumsFromText(string text) {
			//If this Regex pattern is ever changed, we may need to change the Select statement below.
			string strJobNum = "jobnum:";
			List<long> listNumMatches = Regex.Matches(text,$@"{strJobNum}\d+",RegexOptions.IgnoreCase)
				.OfType<Match>()
				.Select(x => SIn.Long(x.Groups[0].Value.Substring(strJobNum.Length),false))//Get Job num out of text.
				.Distinct()
				.ToList();
			return listNumMatches;
		}

		#endregion Methods - Public

		#region Methods - Private

		private static void OpenPatNum(long patNum) {
			Patient pat=Patients.GetPat(patNum);
			if(pat==null) {
				OpenDental.MsgBox.Show(Lans.g("OpenDental","Patient does not exist."));
				return;
			}
			GlobalFormOpenDental.PatientSelected(pat,true);
		}

		private static void OpenTaskNum(long taskNum) {
			if(Tasks.NavTaskDelegate!=null) {
				Tasks.NavTaskDelegate.Invoke(taskNum);
			}
		}
		
		private static void OpenWebPage(string url) {
			try {
				if(!url.ToLower().StartsWith("http")) {
					url=@"http://"+url;
				}

				Process.Start(url);
			}
			catch {
				OpenDental.MsgBox.Show(Lans.g("PopupHelper","Failed to open web browser.  Please make sure you have a default browser set and are connected to the internet then try again."),Lans.g("PopupHelper","Attention"));
			}
		}

		private static void OpenUNCPath(string folderPath) {
			//It is significantly faster to check if the directory exists before calling Process.Start() in the case that you have an invalid path.
			//Everything is a directory, scrubbed all specific files.
			bool isValidPath=Directory.Exists(folderPath);
			if(isValidPath) {
				try {
					Process.Start(folderPath);
				}
				catch(Exception e) {
					OpenDental.MsgBox.Show(e.Message);
				}
			}
			else {
				OpenDental.MsgBox.Show(Lans.g("PopupHelper","Failed to open file location. Please make sure file path is valid."));
			}
		}
		#endregion Methods - Private


	}
}
