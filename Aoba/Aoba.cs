using Flurl;
using Flurl.Http;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing.Imaging;
using LuminousVector.Aoba.Net;
using LuminousVector.Aoba.Models;
using LuminousVector.Aoba.Capture;
using LuminousVector.Aoba.Keyboard;
using LuminousVector.Aoba.DataStore;

namespace LuminousVector.Aoba
{
	public static class Aoba
	{
		public static Settings Settings { get { return _settings; } }
		public static KeyHandler KeyHandler;
		public static UserStatsModel UserStats;
		public static NotifyIcon TrayIcon { get; set; }
		public static bool IsListening { get; set; } = true;


		private static string _apiUri = "https://aobacapture.com/api";
		private static string _authUri = "https://aobacapture.com/auth";
		private static Settings _settings;
		private static CookieContainer _cookies;
		private static MediaPlayer _successSound;
		private static MediaPlayer _failedSound;
		private static string _clickUri = null;
		private static bool _capturingRect
		{
			get
			{
				return (_clickCount >= 0);
			}
			set
			{
				if (value)
					_clickCount = 0;
				else
					_clickCount = -1;
			}
		}
		private static Rectangle _captureRegion;
		private static ContextMenuInstaller _inst;
		private static System.Collections.IDictionary _stateSaver;
		private static int _clickCount = -1;
		


		internal static void Init()
		{
			//Settings
			try
			{

				if (File.Exists("Settings.data"))
					_settings = Settings.Load("Settings.data");
				else
					_settings = Settings.Default;
			}catch(Exception e)
			{
				_settings = Settings.Default;
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.StackTrace);
			}
			if (Settings.HasAuth)
			{
				CreateCookie();
			}

			//Sounds
			try
			{
				_successSound = new MediaPlayer();
				_successSound.Open(new Uri("res/success.mp3", UriKind.Relative));
				_failedSound = new MediaPlayer();
				_failedSound.Open(new Uri("res/failed.mp3", UriKind.Relative));

			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			//Set Tray Icon
			TrayIcon = new NotifyIcon();
			TrayIcon.Icon = new Icon("res/Aobax32.ico");
			TrayIcon.Visible = false;
			TrayIcon.Text = "Aoba";
			TrayIcon.BalloonTipClicked += BalloonClick;


			//Shortcuts
			ResolveKeys();
			KeyHandler = new KeyHandler();
			KeyHandler.RegisterEventTarget("Capture Window", null);
			KeyHandler.RegisterEventTarget("Capture Fullscreen", CaptureFullscreenAndSave);
			KeyHandler.RegisterEventTarget("Capture Region", BeginCapture);
			KeyHandler.RegisterEventTarget("Upload File", ShowUploadWindow);

			//Region Capture
			RegisterRegionCapture();	
		}
#region Initialization

		private static void ResolveKeys()
		{
			foreach(KeybaordShortcut k in Settings.Default.Shortcuts)
			{
				if(Settings.Shortcuts.All(x => x.Name != k.Name))
				{
					Settings.Shortcuts.Add(k);
				}
			}
		}

		private static void RegisterRegionCapture()
		{
			KeyHandler.KeyDown += (_, e) =>
			{
				if (!IsListening)
					return;
				if (e.KeyCode == Keys.Escape && _capturingRect)
				{
					e.Handled = true;
					_capturingRect = false;
					_captureRegion = default(Rectangle);
					Cursor.Current = Cursors.Default;
					Debug.WriteLine("Capture Stopped");
				}
			};
			KeyHandler.MouseDown += (_, e) =>
			{
				if (!IsListening)
					return;
				if (e.Button == MouseButtons.Left && _capturingRect)
				{
					Debug.WriteLine($"Point {_clickCount + 1}");
					e.Handled = true;
					if (_clickCount == 0)
					{
						_captureRegion = new Rectangle(Cursor.Position, Size.Empty);
						_clickCount++;
					}
					else if (_clickCount == 1)
					{
						Point cPos = Cursor.Position;
						e.Handled = true;
						var size = new Size();
						size.Width = Math.Abs(Cursor.Position.X - _captureRegion.X);
						size.Height = Math.Abs(Cursor.Position.Y - _captureRegion.Y);
						if (_captureRegion.X > cPos.X)
							_captureRegion.X = cPos.X;
						if (_captureRegion.Y > cPos.Y)
							_captureRegion.Y = cPos.Y;
						_captureRegion.Size = size;
						CaptureRegion(_captureRegion);
						_capturingRect = false;
						_captureRegion = default(Rectangle);
						Cursor.Current = Cursors.Default;
					}
				}
			};
		}

		internal static void CreateCookie()
		{
			_cookies = new CookieContainer();
			_cookies.Add(new Uri(_apiUri), new Cookie("ApiKey", Settings.AuthToken));
		}

		internal static void InstallContextMenu()
		{
			_inst = new ContextMenuInstaller();
			_stateSaver = new Dictionary<object, object>();
			_inst.Install(_stateSaver);
		}
#endregion

#region Capture
		private static void BeginCapture()
		{
			if (!_capturingRect)
			{
				Debug.WriteLine("Capture Start");
				if(Settings.ShowToasts && Settings.ToastCapture)
				{
					Notify("Click two points to define region", "Ready to Capture");
				}
				_capturingRect = true;
				Cursor.Current = Cursors.Cross;
			}
			else
			{
				_capturingRect = false;
				_captureRegion = default(Rectangle);
				Cursor.Current = Cursors.Default;
			}
		}
		private static void CaptureRegion(Rectangle region)
		{ 
			if (region.IsEmpty)
				return;
			Image screenCap = ScreenCapture.CaptureRegion(region);
			if (Settings.ShowToasts && Settings.ToastCapture)
			{
				Notify("Screenshot Captured");
			}
			PublishScreen(screenCap);
		}

		private static void CaptureFullscreenAndSave()
		{
			Image screenCap = null;
			switch (Settings.FullscreenCapture)
			{
				case FullscreenCaptureMode.AllScreens:
					screenCap = ScreenCapture.CaptureAll();
					break;
				case FullscreenCaptureMode.CursorScreen:
					screenCap = ScreenCapture.CaptureCursor();
					break;
				case FullscreenCaptureMode.PrimaryScreen:
					screenCap = ScreenCapture.CapturePrimary();
					break;
			}
			if (screenCap == null)
				return;
			if(Settings.ShowToasts && Settings.ToastCapture)
			{
				Notify("Screenshot Captured");
			}
			PublishScreen(screenCap);
		}

		private static void PublishScreen(Image screenCap)
		{
			string ext = (Settings.Format == ImageFormat.Png) ? ".png" : ".jpg";
			string fileName = $"{Guid.NewGuid().ToString().Replace("-", "")}{ext}";

			if (Settings.SaveCopy)
				screenCap.Save(Settings.SaveLocation.AppendPathSegment(fileName), Settings.Format);
			if(!Settings.HasAuth)
			{
				Notify("User not logged in", "Upload Failed");
				return;
			}
			using (MemoryStream image = new MemoryStream())
			{
				try
				{
					screenCap.Save(image, Settings.Format);
					image.Position = 0;
					string uri = _apiUri.AppendPathSegment("image").Upload(image, fileName, _cookies);
					UploadSucess(uri);
				}
				catch (Exception e)
				{
					UploadFailed(e);
				}
			}
		}
#endregion

		private static void ShowUploadWindow()
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = MediaModel.GetFilterString();
				dialog.FileOk += (s, e) =>
				{
					try
					{
						String uri = _apiUri.AppendPathSegment("image").Upload(dialog.FileName, _cookies, MediaModel.GetMediaType(dialog.FileName));
						UploadSucess(uri);
					}
					catch(Exception ex)
					{
						UploadFailed(ex);
					}
				};
				DialogResult result = dialog.ShowDialog();
			}
		}

#region Post Upload
		private static void UploadSucess(string uri)
		{
			uri = $"https://{uri}";
			if (Settings.OpenLink)
				Process.Start(uri);
			if (Settings.CopyLink)
				System.Windows.Clipboard.SetText(uri);
			if (Settings.PlaySounds && Settings.SoundSuccess)
			{
				_successSound.Stop();
				_successSound.Play();
			}
			if (Settings.ShowToasts && Settings.ToastSucess)
			{
				Notify(_clickUri = uri, "Upload Sucessful");
			}
			Debug.WriteLine(uri);
		}

		private static void UploadFailed(Exception e)
		{
			if (Settings.PlaySounds && Settings.SoundFailed)
			{
				_failedSound.Stop();
				_failedSound.Play();
			}
			if (Settings.ShowToasts && Settings.ToastFailed)
			{
				Notify($"Error: {e.Message}", "Upload Failed");
			}
			Debug.WriteLine(e.Message);
		}
#endregion

		internal async static Task Login()
		{
			var token = await _authUri.AppendPathSegment("login").PostJsonAsync(new { username = Settings.Username, password = Settings.Password, authMode = "API" }).ReceiveJson<AuthToken>();
			Settings.AuthToken = token.ApiKey;
			CreateCookie();
		}

		internal async static Task UpdateStats()
		{
			try
			{
				UserStats = await _apiUri.AppendPathSegment("userStats").WithCookie("ApiKey", Settings.AuthToken).GetJsonAsync<UserStatsModel>();

			}catch(Exception e)
			{
				Notify($"Error: {e.Message}", "Connection Failed");
			}
		}

		private static void BalloonClick(object sender, EventArgs e)
		{
			if (_clickUri == null)
				return;
			Process.Start(_clickUri);
			_clickUri = null;
		}

		internal static void Notify(string message, string title = "Aoba", int timeout = 3)
		{
			TrayIcon.BalloonTipTitle = title;
			TrayIcon.BalloonTipText = message;
			TrayIcon.ShowBalloonTip(timeout);
		}

		internal static void Save()
		{
			Settings.Save("Settings.data");
			Debug.WriteLine("Saved");
		}

		public static void Dispose()
		{
			TrayIcon.Dispose();
			KeyHandler.Dispose();
			//_inst?.Uninstall(_stateSaver);
		}
	}
}
