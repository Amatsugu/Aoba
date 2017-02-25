using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using LuminousVector.Aoba.Net;
using LuminousVector.Aoba.DataStore;
using LuminousVector.Aoba.Keyboard;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Media;
using LuminousVector.Aoba.Models;
using System.Windows.Forms;

namespace LuminousVector.Aoba
{
	public static class Aoba
	{
		public static Settings Settings { get { return _settings; } }
		public static KeyHandler KeyHandler;
		public static UserStatsModel UserStats;
		public static System.Windows.Forms.NotifyIcon TrayIcon { get; set; }


		private static string _apiUri = "https://aoba.luminousvector.com/api";
		private static string _authUri = "https://aoba.luminousvector.com/auth";
		private static Settings _settings;
		private static CookieContainer _cookies;
		private static MediaPlayer _successSound;
		private static MediaPlayer _failedSound;
		private static string _clickUri = null;
		private static bool _capturingRect = false;
		private static Rectangle _captureRegion;

		internal static void Init()
		{
#if DEBUG
			_apiUri = "http://localhost:4321/api";
			_authUri = "http://localhost:4321/auth";
#endif
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
				_cookies = new CookieContainer();
				_cookies.Add(new Uri(_apiUri), new Cookie("ApiKey", Settings.AuthToken));
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
			KeyHandler = new KeyHandler();
			KeyHandler.RegisterEventTarget("Capture Window", null);
			KeyHandler.RegisterEventTarget("Capture Fullscreen", CaptureFullscreenAndSave);
			KeyHandler.RegisterEventTarget("Capture Region", BeginCapture);
			KeyHandler.KeyDown += (_, e) =>
			{
				if(e.KeyCode == Keys.Escape && _capturingRect)
				{
					e.Handled = true;
					_capturingRect = false;
					_captureRegion = default(Rectangle);
					Cursor.Current = Cursors.Default;
				}
			};
			KeyHandler.MouseDown += (_, e) =>
			{
				if (_capturingRect && e.Button != MouseButtons.Left)
				{ 
					_capturingRect = false;
					_captureRegion = default(Rectangle);
					//Cursor.Current = Cursors.Default;
				}
			};
			KeyHandler.DragStart += (_, e) => 
			{
				Debug.WriteLine($"Start {e.Button}");
				if (_capturingRect)
				{
					e.Handled = true;
					_captureRegion = new Rectangle(Cursor.Position, Size.Empty);
				}
			};
			KeyHandler.DragEnd += (_, e) =>
			{
				Debug.WriteLine($"Finish {e.Button}");
				if (e.Button == MouseButtons.Left && _capturingRect)
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
			};
		}

		internal async static Task Login()
		{
			var token = await _authUri.AppendPathSegment("login").PostJsonAsync(new { username = Settings.Username, password = Settings.Password, authMode = "API"}).ReceiveJson<AuthToken>();
			Settings.AuthToken = token.ApiKey;
			var r = await _apiUri.WithCookie("ApiKey", Settings.AuthToken).GetAsync();
		}

		internal static string Upload(string fileUri)
		{
			return AobaHttpRequest.Upload(fileUri, $"{_apiUri}/image", _cookies);
		}

		private static void BeginCapture()
		{
			if (!_capturingRect)
			{
				Debug.WriteLine("Capture Start");
				if(Settings.ShowToasts && Settings.ToastCapture)
				{
					TrayIcon.BalloonTipTitle = "Ready to Capture";
					TrayIcon.BalloonTipText = "Click and drag a region";
					TrayIcon.ShowBalloonTip(3);
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
				TrayIcon.BalloonTipTitle = "Screenshot Captured";
				TrayIcon.BalloonTipText = "";
				TrayIcon.ShowBalloonTip(3);
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
				TrayIcon.BalloonTipTitle = "Screenshot Captured";
				TrayIcon.BalloonTipText = "";
				TrayIcon.ShowBalloonTip(3);
			}
			PublishScreen(screenCap);
		}

		private static void PublishScreen(Image screenCap)
		{
			string ext = (Settings.Format == ImageFormat.Png) ? ".png" : ".jpg";
			string fileName = $"{Guid.NewGuid().ToString().Replace("-", "")}{ext}";

			if (Settings.SaveCopy)
				fileName = Settings.SaveLocation.AppendPathSegment(fileName);
			screenCap.Save(fileName, Settings.Format);
			try
			{
				string uri = Upload(fileName);
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
					TrayIcon.BalloonTipTitle = "Upload Successful";
					TrayIcon.BalloonTipText = _clickUri = uri;
					TrayIcon.ShowBalloonTip(3);

				}
				Debug.WriteLine(uri);
			}
			catch (Exception e)
			{
				if (Settings.PlaySounds && Settings.SoundFailed)
				{
					_failedSound.Stop();
					_failedSound.Play();
				}
				if (Settings.ShowToasts && Settings.ToastFailed)
				{
					TrayIcon.BalloonTipTitle = "Upload Failed";
					TrayIcon.BalloonTipText = $"Error: {e.Message}";
					TrayIcon.ShowBalloonTip(3);
				}
				Debug.WriteLine(e.Message);
			}
			if (!Settings.SaveCopy)
				File.Delete(fileName);
		}


		private static void BalloonClick(object sender, EventArgs e)
		{
			if (_clickUri == null)
				return;
			Process.Start(_clickUri);
			_clickUri = null;
		}

		internal async static Task UpdateStats()
		{
			try
			{
				UserStats = await _apiUri.AppendPathSegment("userStats").WithCookie("ApiKey", Settings.AuthToken).GetJsonAsync<UserStatsModel>();

			}catch(Exception e)
			{
				TrayIcon.BalloonTipTitle = "Failed to Connect";
				TrayIcon.BalloonTipText = $"Error: {e.Message}";
				TrayIcon.ShowBalloonTip(3);
			}
		}

		internal static void Save()
		{
			Settings.Save("Settings.data");
			Debug.WriteLine("Saved");
		}
	}
}
