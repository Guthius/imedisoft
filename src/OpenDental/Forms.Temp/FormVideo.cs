using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CodeBase;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormVideo:FormODBase {
	private CameraFrameSource _cameraFrameSource;
	private static string _lockImg="";

	[Category("OD")]
	[Description("Fires when user clicks the Capture button.")]
	public event EventHandler<Bitmap> BitmapCaptured;

	public FormVideo() {
		InitializeComponent();
	}

	#region Methods - Event Handlers
	private void butCapture_Click(object sender,EventArgs e) {
		//tabstop had to be set to false to prevent duplicates when using the space bar.
		BitmapCaptured?.Invoke(this,(Bitmap)pictureBoxCamera.Image);
	}

	private void CameraFrameSource_NewFrame(IFrameSource iFrameSource, byte[] byteArrayData, Size size){
		var xoffsett=0;
		var xstep=1;
		//if(butFlipH.Checked){
		//	xoffsett=1;
		//	xstep=-1;
		//}
		var yoffsett=1;
		var ystep=-1;
		//if(butFlipV.Checked){
		//	yoffsett=0;
		//	ystep=1;
		//}
		lock (_lockImg){
			BitmapData bitmapData=null;
			Bitmap bitmap=null;
			try{
				bitmap = new Bitmap(size.Width, size.Height);
				bitmapData = bitmap.LockBits(new Rectangle(0, 0, size.Width, size.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
				unsafe{
					var bmpdata = (byte*)bitmapData.Scan0;
					var pos = 3 * ((xoffsett * (size.Width - 1)) + (size.Width * (yoffsett * (size.Height - 1))));
					var xstepBig = 3 * xstep;
					var offlin = ystep * 3 * size.Width;
					var imgpos = 0;
					for (var iy=0;iy<size.Height;iy++){
						var posi = pos;
						for (var ix=0;ix<size.Width;ix++){
							bmpdata[imgpos++]=byteArrayData[pos];
							bmpdata[imgpos++]=byteArrayData[pos+1];
							bmpdata[imgpos++]=byteArrayData[pos+2];
							bmpdata[imgpos++]=255;
							pos += xstepBig;
						}
						pos = posi + offlin;
					}
				}
				bitmap.UnlockBits(bitmapData);
				bitmapData = null;
				pictureBoxCamera.BeginInvoke((Action)delegate () { pictureBoxCamera.Image = bitmap; });
			}
			catch{
				if (bitmapData != null){
					bitmap.UnlockBits(bitmapData);
					bitmapData = null;
				}
			}
		}
	}

	private void comboCameras_SelectionChangeCommitted(object sender,EventArgs e) {
		Play();
	}

	private void FormVideo_FormClosing(object sender, FormClosingEventArgs e){
		TrashOldCamera();
		var rectangleConverter=new RectangleConverter();
		var str=rectangleConverter.ConvertToInvariantString(DesktopBounds);
		if(WindowState==FormWindowState.Maximized){
			str=rectangleConverter.ConvertToInvariantString(RestoreBounds);
			str+=",Max";
		}
		ComputerPrefs.LocalComputer.VideoRectangle=str;
		ComputerPrefs.Update(ComputerPrefs.LocalComputer);
	}

	private void FormVideo_KeyDown(object sender,KeyEventArgs e){
		if(e.KeyCode==Keys.Space && !butCapture.Focused) {//if focus is on butCapture, pressing space will be the same as clicking
			BitmapCaptured?.Invoke(this,(Bitmap)pictureBoxCamera.Image);
		}
	}

	private void FormVideo_SizeChanged(object sender, EventArgs e){
		var rectangle=DesktopBounds;
	}

	private void FormVideo_Load(object sender, EventArgs e){
		var str=ComputerPrefs.LocalComputer.VideoRectangle;
		if(str==""){
			var rectangleWorkingArea=System.Windows.Forms.Screen.FromHandle(this.Handle).WorkingArea;
			Location=new Point(rectangleWorkingArea.Left+rectangleWorkingArea.Width/2-Width/2,//center L/R
				rectangleWorkingArea.Bottom-Height);//bottom
		}
		else{
			var rectangleConverter=new RectangleConverter();
			if(str.EndsWith(",Max")){
				str=str.Substring(0,str.Length-4);
				var rectangle=(Rectangle)rectangleConverter.ConvertFromInvariantString(str);
				DesktopBounds=rectangle;
				//by setting max after bounds, it can restore back to the bounds we set.
				WindowState=FormWindowState.Maximized;
			}
			else{
				var rectangle=(Rectangle)rectangleConverter.ConvertFromInvariantString(str);
				DesktopBounds=rectangle;
			}
		}
		TrashOldCamera();
		comboCameras.Items.Clear();
		var cameraMethods=new CameraMethods();
		var listCameras=cameraMethods.GetListCameras();
		comboCameras.Items.AddList(listCameras);
		if(comboCameras.Items.Count==0){
			return;
		}
		comboCameras.SelectedIndex = 0;
		Play();
	}
	#endregion Methods - Event Handlers

	#region Methods
	public void Parent_KeyDown(Keys keys){
		//because FormVideo_KeyDown would not be reliable enough on its own. This form might not have focus.
		if(keys==Keys.Space){
			BitmapCaptured?.Invoke(this,(Bitmap)pictureBoxCamera.Image);
		}
	}

	///<summary>This stops any existing camera, first, then plays the new one.</summary>
	private void Play(){
		//if(_cameraFrameSource != null && _cameraFrameSource.Camera == comboCameras.SelectedItem){
		//	return;
		//}
		pictureBoxCamera.Image = null;
		try{
			TrashOldCamera();
		}
		catch (Exception ex){
			if (ex.InnerException != null){
				ODMessageBox.Show(ex.Message + "\n" + ex.InnerException.Message);
			}
			else{
				ODMessageBox.Show(ex.Message);
			}
			return;
		}
		StartVideo();
	}

	private void SetFrameSource(CameraFrameSource cameraFrameSource){
		if (_cameraFrameSource==cameraFrameSource){
			return;
		}
		_cameraFrameSource=cameraFrameSource;
	}

	private void StartVideo(){
		var camera = (Camera)comboCameras.SelectedItem;
		SetFrameSource(new CameraFrameSource(camera));
		try{
			//camera.Width = Convert.ToInt32(textWidth.Text);
			//camera.Height = Convert.ToInt32(textHeight.Text);
			_cameraFrameSource.NewFrame += CameraFrameSource_NewFrame;
			_cameraFrameSource.StartFrameCapture();
		}
		catch (Exception ex){
			ODMessageBox.Show(ex.Message);
		}
	}

	private void TrashOldCamera(){
		if(_cameraFrameSource == null){
			return;
		}
		_cameraFrameSource.ReleaseEvents();
		_cameraFrameSource.Camera.Dispose();
		SetFrameSource(null);
	}
	#endregion Methods


}