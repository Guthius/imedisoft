using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace OpenDental.UI;
//Jordan is the only one allowed to edit this file.

///<summary>A replacement for the MS MonthCalendar control.</summary>
public partial class MonthCalendarOD : Control{
	#region Fields - Public
	///<summary>Set this to false to prevent user from clicking the top text to manually set date. Useful, for example, if this monthCalendar is a dropdown, so there's already a box for that.</summary>
	public bool AllowClickingTopText=true;
	
	#endregion Fields - Public

	#region Fields -Private
	///<summary>The dates that are showing in each cell.  This is a 7x6 array.</summary>
	private DateTime[,] _arrayDates;
	///<summary>Unfortunately, this has the same name as a MS event for selecting a date.  So this can't be used publicly unless I'm careful.</summary>
	private DateTime _dateSelected;
	private float _heightCell;
	///<summary>Needs to be scaled each time.</summary>
	private int _heightFooter96=21;
	///<summary>Needs to be scaled each time.</summary>
	private int _heightHeader96=34;
	///<summary>Because we can go to different months without changing the selected date. We always use the first day of the month. Can't be changed externally.</summary>
	private DateTime _dateMonthShowing;
	///<summary>Identifies the current hot hover cell as the mouse moves around.  Row 0 is never used because that's the days of week. 0,0 indicates no hot.</summary>
	private Point _pointHotHover;
	///<summary>26x33</summary>
	private Rectangle _rectangleMonthLeft;
	private Rectangle _rectangleTopText;
	private Rectangle _rectangleMonthRight;
	private Rectangle _rectangleTodayHover;
	private Rectangle _rectangleYearLeft;
	private Rectangle _rectangleYearRight;
	///<summary>x Position of left of each cell.  Always 7 columns</summary>
	private float[] _xPos;
	///<summary>y Position of top of each cell.  Top row is days of week, then always 6 rows of numbers</summary>
	private float[] _yPos;
	#endregion Fields - Private

	#region Constructor
	public MonthCalendarOD(){
		InitializeComponent();
		DoubleBuffered=true;
		_dateSelected=DateTime.Today;
		_dateMonthShowing=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
		CalculateRectangles();
	}
	#endregion Constructor

	#region Events - Public Raise
	///<summary>Occurs when user clicks to change date.  Does not fire in response to programmatic data changes.</summary>
	[Category("OD")]
	[Description("Occurs when user clicks to change date.  Does not fire in response to programmatic data changes.")]
	public event EventHandler DateChanged;
	#endregion Events - Public Raise

	#region Properties
	protected override Size DefaultSize => new Size(227, 162);
	#endregion Properties

	#region Methods Override
	protected override void OnMouseDown(MouseEventArgs e){
		/*
		if(e.Y<_yPos[1]){
			return;
		}
		if(e.Y>Height-(_heightFooter96)){
			return;
		}
		if(_pointHotHover==new Point(0,0)){
			return;
		}*/
		if(_pointHotHover!=new Point(0,0)){
			_dateSelected=_arrayDates[_pointHotHover.X,_pointHotHover.Y-1];
			DateChanged?.Invoke(this,new EventArgs());
			if(_dateSelected.Month!=_dateMonthShowing.Month){
				_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
			}
			Invalidate();
		}
		if(_rectangleTopText.Contains(e.Location) && AllowClickingTopText) {
			var frmDatePicker=new FrmDatePicker();
			frmDatePicker.DateEntered=_dateSelected;
			frmDatePicker.WidthForm=Width;
			frmDatePicker.PointStartLocation=PointToScreen(
				new Point(0,_rectangleTopText.Bottom+5));
			frmDatePicker.ShowDialog();
			if(frmDatePicker.IsDialogOK) {
				_dateSelected=frmDatePicker.DateEntered;
				if(_dateSelected.Month!=_dateMonthShowing.Month || _dateSelected.Year!=_dateMonthShowing.Year){
					_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
				}
				DateChanged?.Invoke(this,new EventArgs());
				Invalidate();
			}
		}
		if(_rectangleMonthLeft.Contains(e.Location)){
			_dateMonthShowing=_dateMonthShowing.AddMonths(-1);
			Invalidate();
		}
		if(_rectangleMonthRight.Contains(e.Location)){
			_dateMonthShowing=_dateMonthShowing.AddMonths(1);
			Invalidate();
		}
		if(_rectangleYearLeft.Contains(e.Location)){
			_dateMonthShowing=_dateMonthShowing.AddYears(-1);
			Invalidate();
		}
		if(_rectangleYearRight.Contains(e.Location)){
			_dateMonthShowing=_dateMonthShowing.AddYears(1);
			Invalidate();
		}
		if(_rectangleTodayHover.Contains(e.Location)){
			_dateSelected=DateTime.Today;
			DateChanged?.Invoke(this,new EventArgs());
			if(DateTime.Today.Month!=_dateMonthShowing.Month){
				_dateMonthShowing=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
			}
			Invalidate();
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseLeave(EventArgs e){
		_pointHotHover=new Point();
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseMove(MouseEventArgs e){
		if(_yPos is null){
			return;
		}
		var hotRow=0;
		if(e.Y>_yPos[1] && e.Y<Height-_heightFooter96){
			for(var i=2;i<_yPos.Length;i++){
				if(e.Y<_yPos[i]){
					hotRow=i-1;
					break;
				}
				hotRow=i;//last row;
			}
		}
		var hotCol=0;
		for(var i=1;i<_xPos.Length;i++){
			if(e.X<_xPos[i]){
				hotCol=i-1;
				break;
			}
			hotCol=i;//last col
		}
		if(hotRow==0){
			_pointHotHover=new Point();
		}
		else{
			_pointHotHover=new Point(hotCol,hotRow);
		}
		Invalidate();
		base.OnMouseMove(e);
	}

	protected override void OnFontChanged(EventArgs e){
		base.OnFontChanged(e);
		CalculateRectangles();
		Invalidate();
	}

	protected override void OnSizeChanged(EventArgs e){
		base.OnSizeChanged(e);
		CalculateRectangles();
		Invalidate();
	}
	#endregion Methods Overide

	#region Method OnPaint
	protected override void OnPaint(PaintEventArgs pe){
		var g=pe.Graphics;
		base.OnPaint(pe);
		g.FillRectangle(Brushes.White,ClientRectangle);
		//Calculate cell sizes========================================================================================
		var widthCell=Width/7f;
		_heightCell=(Height-_heightHeader96-_heightFooter96)/7f;
		_yPos=new float[7];
		for(var i=0;i<7;i++){
			_yPos[i]=_heightHeader96+_heightCell*(float)i;
		}
		_xPos=new float[7];
		for(var i=0;i<7;i++){
			_xPos[i]=(float)Width/7f*(float)i;
		}
		//Fill arrayDates=============================================================================================
		_arrayDates=new DateTime[7,6];
		//First day of week is not Sunday in China, for example.  So:
		var firstDayOfWeek=DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
		//Which day does the first fall on?
		var dayOfFirst=(DayOfWeek)((int)_dateMonthShowing.DayOfWeek-(int)firstDayOfWeek);
		var dateFilling=_dateMonthShowing.AddDays(-(int)dayOfFirst);
		var row=0;
		var col=0;
		while(true){
			_arrayDates[col,row]=dateFilling;
			dateFilling=dateFilling.AddDays(1);
			if(col==6){
				col=0;
				row++;
			}
			else{
				col++;
			}
			if(row==6){
				break;
			}
		}
		//Horizontal line below weekdays==============================================================================
		using var penFaint=new Pen(ColorOD.Gray(220));
		g.DrawLine(penFaint,0,_yPos[1]-1,Width,_yPos[1]-1);
		//Hover effect================================================================================================
		Rectangle rectangle;//We make these rectangles a bit narrower than the entire cell
		using var brushHover=new SolidBrush(ColorOD.Hover);
		if(_pointHotHover!=new Point()){
			rectangle=new Rectangle((int)_xPos[_pointHotHover.X]+4,(int)_yPos[_pointHotHover.Y],
				(int)widthCell-8,(int)_heightCell);
			g.FillRectangle(brushHover,rectangle);
		}
		//Date selected===============================================================================================
		//if(_monthShowing.Month==_dateSelected.Month){//date is showing. Can't do this.  Could be showing dimmed.
		for(var c=0;c<7;c++){
			for(var r=0;r<6;r++){
				if(_arrayDates[c,r]!=_dateSelected){
					continue;
				}
				rectangle=new Rectangle((int)_xPos[c]+4,(int)_yPos[r+1],
					(int)widthCell-8,(int)_heightCell);
				using var brushSelected=new SolidBrush(Color.FromArgb(70,120,180));//smokey blue to let the orange pop
				//70,120,240));Pretty good. Solid pleasant blue.
				g.FillRectangle(brushSelected,rectangle);
			}
		}
		//}
		//Today outline===============================================================================================
		var colorToday=Color.FromArgb(255,130,0);
		using var penToday=new Pen(colorToday);
		//if(_monthShowing.Month==DateTime.Today.Month){//today is showing. Can't do this. Could be dimmed
		for(var c=0;c<7;c++){
			for(var r=0;r<6;r++){
				if(_arrayDates[c,r]!=DateTime.Today){
					continue;
				}
				rectangle=new Rectangle((int)_xPos[c]+4,(int)_yPos[r+1],
					(int)widthCell-8,(int)_heightCell);
				g.DrawRectangle(penToday,rectangle);
			}
		}
		//}
		//Hover buttons================================================================================================
		var pointMouse=PointToClient(Control.MousePosition);//introducing variable for debugging
		if(_rectangleMonthLeft.Contains(pointMouse)){
			g.FillRectangle(brushHover,_rectangleMonthLeft);
		}
		if(_rectangleMonthRight.Contains(pointMouse)){
			g.FillRectangle(brushHover,_rectangleMonthRight);
		}
		if(_rectangleYearLeft.Contains(pointMouse)){
			g.FillRectangle(brushHover,_rectangleYearLeft);
		}
		if(_rectangleYearRight.Contains(pointMouse)){
			g.FillRectangle(brushHover,_rectangleYearRight);
		}
		//Header======================================================================================================
		//alignment is centered, left right, but we need to tightly control vertical because we only have a few pixels to work with.
		using var stringFormat=new StringFormat {Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Near };
		var topCellBuffer=(int)((_heightCell-Font.Height)/2f);
		var strMonth=_dateMonthShowing.ToString("MMMM yyyy");
		rectangle=new Rectangle(0,topCellBuffer+11,Width,_heightHeader96);
		//Hover top text=======================================================================================
		var sizeFStrMonth=g.MeasureString(strMonth,Font);
		//Draw the rectangle based on the text size and location with some buffer around the text
		_rectangleTopText=new Rectangle(x:Width/2-(int)sizeFStrMonth.Width/2-10,
			y:33/2-(int)sizeFStrMonth.Height/2-5,
			width:(int)sizeFStrMonth.Width+20,
			height:(int)sizeFStrMonth.Height+10);
		if(_rectangleTopText.Contains(pointMouse) && AllowClickingTopText){
			g.DrawString(strMonth,Font,Brushes.Blue,rectangle,stringFormat);
		}
		else {
			g.DrawString(strMonth,Font,Brushes.Black,rectangle,stringFormat);
		}
		//Days of week================================================================================================
		for(var i=0;i<7;i++){
			rectangle=new Rectangle((int)_xPos[i],(int)_yPos[0]+topCellBuffer,(int)widthCell,(int)_heightCell);
			string str=null;
			//First day of week is not Sunday in China, for example.  So:
			var intDay=i;
			intDay+=(int)firstDayOfWeek;
			if(intDay>6){
				intDay-=7;
			}
			str=DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName((DayOfWeek)intDay);
			g.DrawString(str,Font,Brushes.Black,rectangle,stringFormat);
		}
		//Year arrows================================================================================================
		//Left
		g.SmoothingMode=SmoothingMode.HighQuality;
		var points=new PointF[3];
		points[0]=new PointF(-3,0);//left, relative to center
		points[1]=new PointF(3f,-5f);//top
		points[2]=new PointF(3f,5f);//bottom
		var graphicsState=g.Save();
		g.TranslateTransform(11,17);//center
		g.TranslateTransform(-5,0);//move a bit to the left to make room for M
		g.ScaleTransform(1,1);
		g.FillPolygon(Brushes.Black,points);
		g.ScaleTransform(1f/1,1f/1);
		g.TranslateTransform(4,-6);//Move to the right and up for Y
		g.DrawString("Y",Font,Brushes.Black,0,0);
		g.Restore(graphicsState);
		//Right
		graphicsState=g.Save();
		g.TranslateTransform(Width-11,17);
		g.TranslateTransform(4,0);//move a bit to the right to make room for M
		g.ScaleTransform(1,1);
		g.RotateTransform(180);
		g.FillPolygon(Brushes.Black,points);
		g.RotateTransform(-180);
		g.ScaleTransform(1f/1,1f/1);
		g.TranslateTransform(-15,-6);//Move to the left and up for Y
		g.DrawString("Y",Font,Brushes.Black,0,0);
		g.Restore(graphicsState);
		//Month arrows================================================================================================
		//Left
		graphicsState=g.Save();
		g.TranslateTransform(26,0);//to UL of left month rect
		g.TranslateTransform(11,17);//center
		g.TranslateTransform(-5,0);//move a bit to the left to make room for M
		g.ScaleTransform(1,1);
		g.FillPolygon(Brushes.Black,points);
		g.ScaleTransform(1f/1,1f/1);
		g.TranslateTransform(5,-6);//Move to the right and up for M
		g.DrawString("M",Font,Brushes.Black,0,0);
		g.Restore(graphicsState);
		//Right
		graphicsState=g.Save();
		g.TranslateTransform(Width-52,0);//to UL of right month rect
		g.TranslateTransform(11,17);//center
		g.TranslateTransform(8,0);//move a bit to the right to make room for M
		g.ScaleTransform(1,1);
		g.RotateTransform(180);
		g.FillPolygon(Brushes.Black,points);
		g.RotateTransform(-180);
		g.ScaleTransform(1f/1,1f/1);
		g.TranslateTransform(-17,-6);//Move to the left and up for M
		g.DrawString("M",Font,Brushes.Black,0,0);
		g.Restore(graphicsState);
		//Dates=======================================================================================================
		using var brushDim=new SolidBrush(ColorOD.Gray(170));
		for(var c=0;c<7;c++){
			for(var r=0;r<6;r++){
				var rectangleF=new RectangleF(_xPos[c],(int)_yPos[r+1]+topCellBuffer,
					widthCell,_heightCell);//_heightCell is sort of ignored because we are controling y
				if(_arrayDates[c,r]==_dateSelected){
					g.DrawString(_arrayDates[c,r].ToString("%d"),Font,Brushes.White,rectangleF,stringFormat);
				}
				else if(_arrayDates[c,r].Month==_dateMonthShowing.Month){
					g.DrawString(_arrayDates[c,r].ToString("%d"),Font,Brushes.Black,rectangleF,stringFormat);
				}
				else{
					g.DrawString(_arrayDates[c,r].ToString("%d"),Font,brushDim,rectangleF,stringFormat);
				}
			}
		}
		//Hover footer================================================================================================
		var s=Lan.g("Calendar","Today:")+" "+DateTime.Today.ToShortDateString();
		var widthBoxAndText=(int)widthCell-15+(int)g.MeasureString(s,Font).Width;
		if(_rectangleTodayHover.Contains(pointMouse)){
			g.FillRectangle(brushHover,_rectangleTodayHover);
		}
		//Footer======================================================================================================
		var x=Width/2-widthBoxAndText/2;
		var heightFooter=_heightFooter96;
		rectangle=new Rectangle(x,
			y:Height-heightFooter+4,
			width:(int)widthCell-15,
			height:heightFooter-11);
		//(int)_heightCell);
		g.DrawRectangle(penToday,rectangle);
		x+=(int)widthCell-13;			
		g.DrawString(s,Font,Brushes.Black,x,Height-heightFooter+2);
		//Lines=======================================================================================================
		//g.DrawLine(penFaint,0,Height-heightFooter,Width,Height-heightFooter);
		using var penOutline=new Pen(ColorOD.Gray(150)); //ColorOD.Outline is too dark for this control
		g.DrawRectangle(penOutline,0,0,Width-1,Height-1);
	}
	#endregion Methods OnPaint

	#region Methods - Public
	///<summary>Use this in place of MonthCalendar.SelectionStart.</summary>
	public DateTime GetDateSelected(){
		return _dateSelected;
	}

	public Size GetDefaultSize(){
		return DefaultSize;
	}

	///<summary>Use this in place of MonthCalendar.SetDate().</summary>
	public void SetDateSelected(DateTime date){
		_dateSelected=date;
		_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
		Invalidate();
	}
	#endregion Methods - Public

	#region Methods - Private
	private void CalculateRectangles(){
		_rectangleYearLeft=new Rectangle(0,0,26,33);
		_rectangleMonthLeft=new Rectangle(26,0,26,33);
		_rectangleMonthRight=new Rectangle(Width-52,0,26,33);
		_rectangleYearRight=new Rectangle(Width-26,0,26,33);
		var widthCell=Width/7f;
		var s=Lan.g("Calendar","Today:")+" "+DateTime.Today.ToShortDateString();
		var widthBoxAndText=(int)widthCell-15+TextRenderer.MeasureText(s,Font).Width;
		var widthTodayHover=widthBoxAndText+10;
		var heightFooter=_heightFooter96;
		_rectangleTodayHover=new Rectangle(Width/2-widthTodayHover/2,
			y:Height-heightFooter,
			width:widthTodayHover,
			height:heightFooter);
	}
	#endregion Methods - Private

}