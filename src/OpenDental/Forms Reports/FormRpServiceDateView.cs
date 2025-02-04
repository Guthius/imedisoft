using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.Logic;
using OpenDental.UI;
using OpenDentBusiness;
using PdfSharp.Pdf;

namespace OpenDental;

public partial class FormRpServiceDateView:FormODBase {
	#region Public Variables
	///<summary>This will be the PatNum or the Guarantor's PatNum.</summary>
	public readonly long PatNum;
	///<summary>Whether or not the window is displaying results for the entire family.</summary>
	public readonly bool IsFamily;
	#endregion
	#region Private Variables
	private bool _headingPrinted;
	private int _pagesPrinted;
	private int _headingPrintH;
	private Family _fam;
	#endregion
		
	public FormRpServiceDateView(long patNum,bool isFamily) {
		InitializeComponent();

		PatNum=patNum;
		IsFamily=isFamily;
		_fam=Patients.GetFamily(patNum);
	}

	private void FormRpServiceDate_Load(object sender,EventArgs e) {
		FillGrid();
		Text=Lans.g("Service Date View -")+" "+_fam.GetPatient(PatNum).GetNameFL()+(IsFamily ? " "+Lans.g("(Family)") : "");
	}

	private void FillGrid() {
		var table=RpServiceDateView.GetData(PatNum,IsFamily,checkDetailedView.Checked);
		gridMain.BeginUpdate();
		//Columns
		gridMain.Columns.Clear();
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"Service Date"),90));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"Trans Date"),80));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"Patient"),150));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"Reference"),220));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"Charge"),80,HorizontalAlignment.Right));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"Credit"),80,HorizontalAlignment.Right));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"Prov"),80));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"InsBal"),80,HorizontalAlignment.Right));
		gridMain.Columns.Add(new GridColumn(Lan.g(this,"AcctBal"),80,HorizontalAlignment.Right));
		//Rows
		gridMain.ListGridRows.Clear();
		var lastRow=table.Select().LastOrDefault();
		foreach(DataRow row in table.Rows) {
			var newRow=new GridRow();
			var serviceDate=SIn.Date(row["Date"].ToString());
			var transDate=SIn.Date(row["Trans Date"].ToString());
			newRow.Cells.Add((serviceDate.Year<1880) ? "" : serviceDate.ToShortDateString());
			newRow.Cells.Add((transDate.Year<1880) ? "" : transDate.ToShortDateString());
			newRow.Cells.Add(row["Patient"].ToString());
			var strReference=row["Reference"].ToString();
			var toothNumPref=PrefC.GetInt(PrefName.UseInternationalToothNumbers);
			var isProc=row["Type"].ToString().ToLower()=="proc";
			//Replace toothNum with correct nomenclature when not using American system.
			if(isProc && strReference.Contains('#') && toothNumPref!=0) {
				var toothNum=strReference.Substring(strReference.IndexOf('#')+1,strReference.IndexOf('-')-strReference.IndexOf('#')-1);
				var toothNumNom=Tooth.Display(toothNum,(ToothNumberingNomenclature)toothNumPref);
				strReference=strReference.Replace("#"+toothNum+"-","#"+toothNumNom+"-");
			}
			newRow.Cells.Add(strReference);
			var isUnallocated=strReference.ToLower().Contains("unallocated");
			newRow.Cells.Add(isUnallocated ? "" : SIn.Decimal(row["Charge"].ToString()).ToString("f"));
			newRow.Cells.Add(isUnallocated ? "" : SIn.Decimal(row["Credit"].ToString()).ToString("f"));
			newRow.Cells.Add(row["Pvdr"].ToString());
			var insBal=SIn.Decimal(row["InsBal"].ToString());
			var acctBal=SIn.Decimal(row["AcctBal"].ToString());
			var isTotalsRow=row==lastRow || strReference.ToLower().Contains("Total for Date".ToLower());
			//Show insBal and acctBal when not on totals row and detailed is checked and either of the amounts are not zero.
			var showDetailedRow=isTotalsRow || (isProc && checkDetailedView.Checked)
			                                || (checkDetailedView.Checked && (CompareDecimal.IsGreaterThanZero(Math.Abs(insBal)) || CompareDecimal.IsGreaterThanZero(Math.Abs(acctBal))));
			newRow.Cells.Add(showDetailedRow ? insBal.ToString("f") : "");
			newRow.Cells.Add(showDetailedRow ? acctBal.ToString("f") : "");
			newRow.Tag=row;
			if(isTotalsRow) {
				newRow.Bold=true;
			}
			gridMain.ListGridRows.Add(newRow);
		}
		gridMain.EndUpdate();
	}


	private void butRefresh_Click(object sender,EventArgs e) {
		FillGrid();
	}

	private void butSavePDFToImages_Click(object sender,EventArgs e) {
		if(gridMain.ListGridRows.Count==0) {
			MsgBox.Show(this,"Grid is empty.");
			return;
		}
		//Get image category to save to. First image "Statement(S)" category.
		var listImageCatDefs=Defs.GetDefsForCategory(DefCat.ImageCats,true).Where(x => x.ItemValue.Contains("S")).ToList();
		if(listImageCatDefs.IsNullOrEmpty()) {
			MsgBox.Show(this,"No image category set for Statements.");
			return;
		}
		var tempFile=PrefC.GetRandomTempFile(".pdf");
		CreatePDF(tempFile);
		var patCur=_fam.GetPatient(PatNum);
		var rawBase64="";
		var docSave=new Document();
		docSave.DocNum=Documents.Insert(docSave);
		docSave.ImgType=ImageType.Document;
		docSave.DateCreated=DateTime.Now;
		docSave.PatNum=PatNum;
		docSave.DocCategory=listImageCatDefs.FirstOrDefault().DefNum;
		docSave.Description=$"ServiceDateView"+docSave.DocNum+$"{docSave.DateCreated.Year}_{docSave.DateCreated.Month}_{docSave.DateCreated.Day}";
		docSave.RawBase64=rawBase64;//blank if using AtoZfolder
		var fileName=ODFileUtils.CleanFileName(docSave.Description);
		var filePath=ImageStore.GetPatientFolder(patCur,ImageStore.GetDataFolder());
		while(File.Exists(Path.Combine(filePath,fileName+".pdf"))) {
			fileName+="x";
		}
		File.Copy(tempFile,ODFileUtils.CombinePaths(filePath,fileName+".pdf"));
		docSave.FileName=fileName+".pdf";//file extension used for both DB images and AtoZ images
		Documents.Update(docSave);
		try {
			File.Delete(tempFile); //cleanup the temp file.
		}
		catch {
		}
		MsgBox.Show(this,"PDF saved successfully.");
	}

	private void CreatePDF(string tempFile) {
		var pdfRenderer=new MigraDoc.Rendering.PdfDocumentRenderer(true,PdfFontEmbedding.Always);
		pdfRenderer.Document=CreateDocument();
		pdfRenderer.RenderDocument();
		pdfRenderer.PdfDocument.Save(tempFile);
	}

	private MigraDoc.DocumentObjectModel.Document CreateDocument() {
		var doc= new MigraDoc.DocumentObjectModel.Document();
		doc.DefaultPageSetup.PageWidth=MigraDoc.DocumentObjectModel.Unit.FromInch(8.5);
		doc.DefaultPageSetup.PageHeight=MigraDoc.DocumentObjectModel.Unit.FromInch(11);
		doc.DefaultPageSetup.TopMargin=MigraDoc.DocumentObjectModel.Unit.FromInch(.5);
		doc.DefaultPageSetup.LeftMargin=MigraDoc.DocumentObjectModel.Unit.FromInch(.5);
		doc.DefaultPageSetup.RightMargin=MigraDoc.DocumentObjectModel.Unit.FromInch(.5);
		var section=doc.AddSection();
		var headingFont=MigraDocHelper.CreateFont(13,true);
		var subHeadingFont=MigraDocHelper.CreateFont(10,true);
		#region printHeading
		//Heading---------------------------------------------------------------------------------------------------------------
		var par=section.AddParagraph();
		var parformat=new MigraDoc.DocumentObjectModel.ParagraphFormat();
		parformat.Alignment=MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
		par.Format=parformat;
		var text=Lans.g("Service Date View");
		par.AddFormattedText(text,headingFont);
		par.AddLineBreak();
		//SubHeading---------------------------------------------------------------------------------------------------------------
		text=(IsFamily ? Lans.g("Entire Family:")+" " : "")+$"{_fam.GetNameInFamFL(PatNum)}";
		par.AddFormattedText(text,subHeadingFont);
		par.AddLineBreak();
		text=Lans.g("Date")+" "+DateTime.Now.ToShortDateString();
		par.AddFormattedText(text,subHeadingFont);
		#endregion
		MigraDocHelper.InsertSpacer(section,10);
		section.PageSetup.Orientation=MigraDoc.DocumentObjectModel.Orientation.Landscape;
		MigraDocHelper.DrawGrid(section,gridMain);
		return doc;
	}

	private void butPrint_Click(object sender,EventArgs e) {
		if(gridMain.ListGridRows.Count==0) {
			MsgBox.Show(this,"Grid is empty.");
			return;
		}
		_pagesPrinted=0;
		_headingPrinted=false;
		PrinterL.TryPrintOrDebugRpPreview(pd_PrintPage,Lan.g(this,"Service date view printed"),PrintoutOrientation.Landscape);
	}

	private void pd_PrintPage(object sender,PrintPageEventArgs e) {
		var bounds=e.MarginBounds;
		var g=e.Graphics;
		string text;
		var headingFont=new System.Drawing.Font("Arial",13,FontStyle.Bold);
		var subHeadingFont=new System.Drawing.Font("Arial",10,FontStyle.Bold);
		var yPos=bounds.Top;
		var center=bounds.X+bounds.Width/2;
		#region printHeading
		if(!_headingPrinted) {
			text=Lan.g(this,"Service Date View");
			g.DrawString(text,headingFont,Brushes.Black,center-g.MeasureString(text,headingFont).Width/2,yPos);
			yPos+=(int)g.MeasureString(text,headingFont).Height;
			text=(IsFamily ? Lans.g("Entire Family:")+" " : "")+$"{_fam.GetNameInFamFL(PatNum)}";
			g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			yPos+=(int)g.MeasureString(text,subHeadingFont).Height;
			text=DateTime.Now.ToShortDateString();
			g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			yPos+=20;
			_headingPrinted=true;
			_headingPrintH=yPos;
		}
		#endregion
		yPos=gridMain.PrintPage(g,_pagesPrinted,bounds,_headingPrintH);
		_pagesPrinted++;
		if(yPos==-1) {
			e.HasMorePages=true;
		}
		else {
			e.HasMorePages=false;
		}
		g.Dispose();
	}

}