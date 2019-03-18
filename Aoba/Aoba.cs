using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Flurl;
using Flurl.Http;
using LuminousVector.Aoba.DesktopCapture;
using LuminousVector.Aoba.DataStore;
using LuminousVector.Aoba.Keyboard;
using LuminousVector.Aoba.Models;
using LuminousVector.Aoba.Net;
using Clipboard = System.Windows.Clipboard;
using Timer = System.Threading.Timer;
using Capture;
using Capture.Interface;
using System.Threading;

namespace LuminousVector.Aoba
{
	public static class Aoba
	{
		public static Settings Settings => _settings;
		public static KeyHandler KeyHandler;
		public static UserStatsModel UserStats;
		public static NotifyIcon TrayIcon { get; set; }
		public static bool IsListening
		{
			get
			{
				return _isListening;
			}
			set
			{
				_isListening = KeyHandler.IsListening = value;
			}
		}

		public static CaptureConfig d3dCapConfig;

#if DEBUG
		private const string HOST = "http://localhost:4321";
#else
		private const string HOST = "https://aoba.app";
#endif
		private static string API_URI = $"{HOST}/api";
		private static string AUTH_URI = $"{HOST}/auth";
		private static Settings _settings;
		private static CookieContainer _cookies;
		private static SoundPlayer _successSound;
		private static SoundPlayer _failedSound;
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
		private static int _clickCount = -1;
		private static bool _isListening = true;
		private static MemoryMappedFile _mFile;
		private static Timer _uploadShellWatcher;
		private static Dispatcher _aobaDispatcher;
		private static CaptureProcess _curHook;
		private static ManualResetEvent captureEvent = new ManualResetEvent(false);

		internal static void Init()
		{
			//D3D Capture
			d3dCapConfig = new CaptureConfig
			{
				Direct3DVersion = Direct3DVersion.AutoDetect,
				ShowOverlay = false
			};


			//Settings
			try
			{
				_settings = File.Exists("settings.json") ? Settings.Load("Settings.json") ?? Settings.Default : Settings.Default;
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
				var a = Assembly.GetExecutingAssembly();
				_successSound = new SoundPlayer(a.GetManifestResourceStream("LuminousVector.Aoba.res.success.wav"));
				_successSound.Load();
				_failedSound = new SoundPlayer(a.GetManifestResourceStream("LuminousVector.Aoba.res.failed.mp3"));
				_failedSound.Load();

			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			//Set Tray Icon
			TrayIcon = new NotifyIcon()
			{
				Icon = new Icon("res/Aobax32.ico"),
				Visible = true,
				Text = "Aoba"
			};
			TrayIcon.BalloonTipClicked += BalloonClick;

			//Check For Upload Shell
			_mFile = MemoryMappedFile.CreateOrOpen("Aoba.ShellUploads", 200_000);
			_aobaDispatcher = Dispatcher.CurrentDispatcher;
			_uploadShellWatcher = new Timer(async (o) =>
			{
				var s = _mFile.CreateViewStream();
				var sr = new StreamReader(s);
				var lines = sr.ReadLine().Replace("\0", "");
				s.Position = 0;
				s.Write(new byte[s.Length], 0, (int)s.Length);
				s.Flush();
				s.Dispose();
				sr.Dispose();
				foreach(string file in lines.Split('\n'))
				{
					//Upload
					if (!string.IsNullOrWhiteSpace(file))
					{
						await Upload(file);
					}
				}
			}, null, 0, 500);

			//Shortcuts
			ResolveKeys();
			KeyHandler = new KeyHandler();
			KeyHandler.RegisterEventTarget("Capture Window", CaptureWindow);
			KeyHandler.RegisterEventTarget("Capture Fullscreen", CaptureFullscreenAndSave);
			KeyHandler.RegisterEventTarget("Capture Region", BeginRegionCapture);
			KeyHandler.RegisterEventTarget("Upload File", ShowUploadWindow);
			//Region Capture
			RegisterRegionCapture();

		}
#region Initialization

		private static void ResolveKeys()
		{
			foreach(var k in Settings.Default.Shortcuts)
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
				if ((e.KeyCode == Keys.Escape && _capturingRect))
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
						var cPos = Cursor.Position;
						e.Handled = true;
						var size = new Size()
						{
							Width = Math.Abs(Cursor.Position.X - _captureRegion.X),
							Height = Math.Abs(Cursor.Position.Y - _captureRegion.Y)
						};
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
			_cookies.Add(new Uri(API_URI), new Cookie("ApiKey", Settings.AuthToken));
		}

		internal static void InstallContextMenu()
		{
			
		}
#endregion

#region Capture Region
		private static void BeginRegionCapture()
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
				Notify("Screenshot Captured");
			PublishScreen(screenCap);
		}
#endregion

#region Capture Fullscreen
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
				default:
					screenCap = ScreenCapture.CaptureAll();
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
#endregion

#region Capture Window
		public static void CaptureWindow()
		{
			var process = Process.GetProcessById(ScreenCapture.GetForgroundWindowId());
			ScreenCapture.GetWindowRect(ScreenCapture.GetForegroundWindow(), out WindowRect rect);
			if (_curHook == null || _curHook.Process.Id != process.Id)
			{
				if(_curHook != null)
					_curHook.Dispose();
				_curHook = null;
				var captureI = new CaptureInterface();
				captureI.RemoteMessage += CaptureI_RemoteMessage;
				try
				{
					_curHook = new CaptureProcess(process, d3dCapConfig, captureI);
					Thread.Sleep(10);
				}catch
				{
					_curHook = null;
				}
			}
			Bitmap screenCap;
			bool captured = false;
			if(_curHook != null)
			{
				captureEvent.Reset();
				var capture = _curHook.CaptureInterface;
				capture.BeginGetScreenshot(rect.AsRectange(), new TimeSpan(0, 0, 2), r =>
				{
					var screen = capture.EndGetScreenshot(r);

					if (screen == null)
					{
						_curHook = null;
						captureEvent.Set();
						return;
					}
					if (Settings.ShowToasts && Settings.ToastCapture)
						Notify("Screenshot Captured");
					PublishScreen(screen.ToBitmap());
					captured = true;
					captureEvent.Set();
				}, null, Capture.Interface.ImageFormat.Png);
				captureEvent.WaitOne();
				if (captured)
					return;
			}
			screenCap = ScreenCapture.CaptureRegion(rect.AsRectange());
			if (Settings.ShowToasts && Settings.ToastCapture)
				Notify("Screenshot Captured");
			PublishScreen(screenCap);
		}

		private static void CaptureI_RemoteMessage(MessageReceivedEventArgs message)
		{
			
		}
#endregion


		private static async void PublishScreen(Image screenCap)
		{
			var ext = (Settings.Format == System.Drawing.Imaging.ImageFormat.Png) ? ".png" : ".jpg";
			var fileName = $"{Guid.NewGuid().ToString().Replace("-", "")}{ext}";

			if (Settings.SaveCopy)
				screenCap.Save(Settings.SaveLocation.AppendPathSegment(fileName), Settings.Format);
			if(!Settings.HasAuth)
			{
				Notify("User not logged in", "Upload Failed");
				return;
			}
			using (var image = new MemoryStream())
			{
				try
				{
					screenCap.Save(image, Settings.Format);
					image.Position = 0;
					var uri = await API_URI.AppendPathSegment("image").Upload(image, fileName, _cookies);
					UploadSucess(uri);
				}
				catch (Exception e)
				{
					UploadFailed(e);
				}
			}
		}

		private static void ShowUploadWindow()
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.Filter = MediaModel.GetFilterString();
				dialog.RestoreDirectory = true;
				//dialog.FilterIndex = 1;
				dialog.FileOk += async (s, e) =>
				{
					await Upload(dialog.FileName);
				};
				var result = dialog.ShowDialog();
			}
		}

		private static async Task Upload(string file)
		{
			try
			{
				var uri = await API_URI.AppendPathSegment("image").Upload(file, _cookies, MediaModel.GetMediaType(file));
				UploadSucess(uri);
			}
			catch (Exception ex)
			{
				UploadFailed(ex);
			}
		}

#region Post Upload
		private static void UploadSucess(string uri)
		{
			uri = $"https://{uri}";
			if (Settings.OpenLink)
				Process.Start(uri);
			if (Settings.CopyLink)
				_aobaDispatcher.BeginInvoke(new Action(() => Clipboard.SetText(uri)));
			if (Settings.PlaySounds && Settings.SoundSuccess)
			{
				try
				{
					_successSound.Stop();
					_successSound.Play();
				}catch
				{ }
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
				try
				{
					_failedSound.Stop();
					_failedSound.Play();
				}catch
				{ }
			}
			if (Settings.ShowToasts && Settings.ToastFailed)
			{
				Notify($"Error: {e.Message}", "Upload Failed");
			}
			Debug.WriteLine(e.Message);
		}
#endregion

		internal static async Task Login()
		{
			var token = await AUTH_URI.AppendPathSegment("login").PostJsonAsync(new { Settings.Username, Settings.Password, AuthMode = "API" }).ReceiveJson<AuthToken>();
			Settings.AuthToken = token.ApiKey;
			CreateCookie();
		}

		internal static async Task UpdateStats()
		{
			try
			{
				UserStats = await API_URI.AppendPathSegment("userStats").WithCookie("ApiKey", Settings.AuthToken).GetJsonAsync<UserStatsModel>();

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

		internal static void Notify(string message, string title = "Aoba", int timeout = 1)
		{
			TrayIcon.BalloonTipTitle = title;
			TrayIcon.BalloonTipText = message;
			TrayIcon.ShowBalloonTip(timeout);
		}

		internal static void Save()
		{
			Settings.Save("Settings.json");
			Debug.WriteLine("Saved");
		}

		public static void Dispose()
		{
			TrayIcon.Dispose();
			KeyHandler.Dispose();
			_uploadShellWatcher.Dispose();
			//_inst?.Uninstall(_stateSaver);
		}
	}
}
