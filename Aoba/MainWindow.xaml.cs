using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Flurl;
using Flurl.Http;
using System.ComponentModel;
using System.Diagnostics;
using LuminousVector.Aoba.Keyboard;
using IWshRuntimeLibrary;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace LuminousVector.Aoba
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDisposable
	{
#if !DEBUG
		private bool willExit = false;
#else
		private bool willExit = true;
#endif
		private System.Windows.Forms.ContextMenu _contextMenu;
		private const int WM_CLIPBOARDUPDATE = 0x031D;

		private IntPtr windowHandle;

		public event EventHandler ClipboardUpdate;

		public MainWindow()
		{
			InitializeComponent();
			Aoba.Init();
			//Start in Tray
			if (Aoba.Settings.StartInTray)
				Hide();
			if(!Aoba.Settings.HasAuth)
			{
				User.Focus();
			}
			//Set Values
			Load();
			//Tray Icon

			var pauseItem = new System.Windows.Forms.MenuItem("Pause");
			pauseItem.Click += (_, e) =>
			{
				Aoba.IsListening = !Aoba.IsListening;
				pauseItem.Text = (Aoba.IsListening) ? "Pause" : "Unpause";
				Aoba.Notify($"Key Listening {pauseItem.Text}ed");
			};

			_contextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
			{
				new System.Windows.Forms.MenuItem("Settings", RestoreWindow),
				pauseItem,
				new System.Windows.Forms.MenuItem("Quit", (o, e) =>
				{
					willExit = true;
					Aoba.Save();
					Close();
				})
			});
			Aoba.TrayIcon.ContextMenu = _contextMenu;
			Aoba.TrayIcon.DoubleClick += RestoreWindow;

			ClipboardUpdate += MainWindow_ClipboardUpdate;
		}

		private void MainWindow_ClipboardUpdate(object sender, EventArgs e)
		{
			if(Clipboard.ContainsImage() && Aoba.Settings.AutoUploadFromClipboard)
			{
				var src = Clipboard.GetImage();
				var bitmap = new Bitmap(src.PixelWidth, src.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				src.CopyPixels(Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
				bitmap.UnlockBits(bitmapData);
				Aoba.PublishScreen(bitmap);
			}
		}

		#region Clipboard Monitoring
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			windowHandle = new WindowInteropHelper(this).EnsureHandle();
			HwndSource.FromHwnd(windowHandle)?.AddHook(HwndHandler);
			Start();
		}

		public static readonly DependencyProperty ClipboardUpdateCommandProperty =
			DependencyProperty.Register("ClipboardUpdateCommand", typeof(ICommand), typeof(MainWindow), new FrameworkPropertyMetadata(null));

		public ICommand ClipboardUpdateCommand
		{
			get { return (ICommand)GetValue(ClipboardUpdateCommandProperty); }
			set { SetValue(ClipboardUpdateCommandProperty, value); }
		}

		protected virtual void OnClipboardUpdate()
		{ }

		public void Start()
		{
			NativeMethods.AddClipboardFormatListener(windowHandle);
		}

		public void Stop()
		{
			NativeMethods.RemoveClipboardFormatListener(windowHandle);
		}

		private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
		{
			if (msg == WM_CLIPBOARDUPDATE)
			{
				// fire event
				this.ClipboardUpdate?.Invoke(this, new EventArgs());
				// execute command
				if (this.ClipboardUpdateCommand?.CanExecute(null) ?? false)
				{
					this.ClipboardUpdateCommand?.Execute(null);
				}
				// call virtual method
				OnClipboardUpdate();
			}
			handled = false;
			return IntPtr.Zero;
		}


		private static class NativeMethods
		{
			[DllImport("user32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool AddClipboardFormatListener(IntPtr hwnd);

			[DllImport("user32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
		}
		#endregion

		private async void Load()
		{
			//Startup
			RunOnStartup.IsChecked = Aoba.Settings.RunAtStart;
			//After Load
			CopyLink.IsChecked = Aoba.Settings.CopyLink;
			OpenLink.IsChecked = Aoba.Settings.OpenLink;
			//Toasts
			ToastAll.IsChecked = ToastBox.IsEnabled = Aoba.Settings.ShowToasts;
			ToastCapture.IsChecked = Aoba.Settings.ToastCapture;
			ToastSuccess.IsChecked = Aoba.Settings.ToastSucess;
			ToastFailed.IsChecked = Aoba.Settings.ToastFailed;
			//Sounds
			SoundAll.IsChecked = SoundBox.IsEnabled = Aoba.Settings.PlaySounds;
			SoundCapture.IsChecked = Aoba.Settings.SoundCapure;
			SoundSuccess.IsChecked = Aoba.Settings.SoundSuccess;
			SoundFailed.IsChecked = Aoba.Settings.SoundFailed;
			//Image Format
			ImageFormat.SelectedIndex = (Aoba.Settings.Format == System.Drawing.Imaging.ImageFormat.Jpeg) ? 1 : 0;
			//Save Copy
			SaveCopy.IsChecked = SaveBox.IsEnabled = Aoba.Settings.SaveCopy;
			SaveLocation.Text = Aoba.Settings.SaveLocation;
			//Fullscreen Capture Mode
			FullscreenCaputue.SelectedIndex = (int)Aoba.Settings.FullscreenCapture;
			//Tray
			CloseToTray.IsChecked = Aoba.Settings.CloseToTray;
			StartInTray.IsChecked = Aoba.Settings.StartInTray;
			//In-Game
			GameCapture.IsChecked = Aoba.Settings.GameCapture;
			ShowFPS.IsChecked = Aoba.Settings.ShowFPS;
			//Misc
			ClipboardAutoUpload.IsChecked = Aoba.Settings.AutoUploadFromClipboard;
			//Account
			Username.Text = Aoba.Settings.Username;
			await Aoba.UpdateStats();
			if (Aoba.UserStats == null)
			{
				Aoba.Settings.AuthToken = null;
				if (Aoba.Settings.Password != null)
				{
					await Aoba.Login();
					if (Aoba.Settings.AuthToken != null)
						ShowLoggedIn();
				}
			}
			RenderKeyBinds();
		}

		private void RenderKeyBinds()
		{
			KeybindsList.ItemsSource = Aoba.Settings.Shortcuts;
		}

		private void Save()
		{
			//Startup
			Aoba.Settings.RunAtStart = (RunOnStartup.IsChecked == null) ? false : (bool)RunOnStartup.IsChecked;
			//After Load
			Aoba.Settings.CopyLink = (CopyLink.IsChecked == null) ? false : (bool)CopyLink.IsChecked;
			Aoba.Settings.OpenLink = (OpenLink.IsChecked == null) ? false : (bool)OpenLink.IsChecked;
			//Toasts
			Aoba.Settings.ShowToasts = (ToastAll.IsChecked == null) ? false : (bool)ToastAll.IsChecked;
			Aoba.Settings.ToastCapture = (ToastCapture.IsChecked == null) ? false : (bool)ToastCapture.IsChecked;
			Aoba.Settings.ToastSucess = (ToastSuccess.IsChecked == null) ? false : (bool)ToastSuccess.IsChecked;
			Aoba.Settings.ToastFailed = (ToastFailed.IsChecked == null) ? false : (bool)ToastFailed.IsChecked;
			//Sounds
			Aoba.Settings.PlaySounds = (SoundAll.IsChecked == null) ? false : (bool)SoundAll.IsChecked;
			Aoba.Settings.SoundCapure = (SoundCapture.IsChecked == null) ? false : (bool)SoundCapture.IsChecked;
			Aoba.Settings.SoundSuccess = (SoundSuccess.IsChecked == null) ? false : (bool)SoundSuccess.IsChecked;
			Aoba.Settings.SoundFailed = (SoundFailed.IsChecked == null) ? false : (bool)SoundFailed.IsChecked;
			//Image Format
			Aoba.Settings.Format = (ImageFormat.SelectedIndex == 0) ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg;
			//Save Copy
			Aoba.Settings.SaveCopy = (SaveCopy.IsChecked == null) ? false : (bool)SaveCopy.IsChecked;
			Aoba.Settings.SaveLocation = SaveLocation.Text;
			//Fullscreen Capture Mode
			Aoba.Settings.FullscreenCapture = (DataStore.FullscreenCaptureMode)FullscreenCaputue.SelectedIndex;
			//Tray
			Aoba.Settings.CloseToTray = (CloseToTray.IsChecked == null) ? false : (bool)CloseToTray.IsChecked;
			Aoba.Settings.StartInTray = (StartInTray.IsChecked == null) ? false : (bool)StartInTray.IsChecked;
			//In-Game
			Aoba.Settings.ShowFPS = (ShowFPS.IsChecked == null) ? false : (bool)ShowFPS.IsChecked;
			Aoba.Settings.GameCapture = (GameCapture.IsChecked == null) ? false : (bool)GameCapture.IsChecked;
			//Misc
			Aoba.Settings.AutoUploadFromClipboard = (ClipboardAutoUpload.IsChecked == null) ? false : (bool)ClipboardAutoUpload.IsChecked;

			Aoba.Save();
		}

		internal void RestoreWindow(object sender, EventArgs e)
		{
			Show();
			if (WindowState != WindowState.Normal)
				WindowState = WindowState.Normal;
		}

		internal void HideWindow()
		{
			Hide();
		}

		protected override void OnStateChanged(EventArgs e)
		{
			Save();
			if (WindowState == WindowState.Minimized)
			{
				HideWindow();
			}
			base.OnStateChanged(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Save();
			if (!Aoba.Settings.CloseToTray)
				willExit = true;
			if (WindowState == WindowState.Normal)
			{
				if (!willExit)
				{
					e.Cancel = true;
					HideWindow();
				}
			}
			if (willExit)
				Aoba.Dispose();
			base.OnClosing(e);
		}

		private async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AccountBox.IsEnabled = false;
				Aoba.Settings.Username = Username.Text;
				Aoba.Settings.Password = Password.Password;
				await Aoba.Login();
				AccountBox.IsEnabled = true;
				ShowLoggedIn();
			}catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
				AccountBox.IsEnabled = true;
			}
			finally
			{
				Save();
			}
		}

		private object loginForm;

		private async void ShowLoggedIn()
		{
			if (loginForm == null)
				loginForm = AccountBox.Content;
			await Aoba.UpdateStats();
			var spanel = new StackPanel();
			var logoutButton = new Button();
			logoutButton.Content = $"Logout";
			logoutButton.Click += (o, e) =>
			{
				Aoba.Settings.AuthToken = null;
				AccountBox.Content = loginForm;
				UserStatus.Content = "User not logged in...";
			};
			spanel.Children.Add(new Label { Content = $"Logged in as: {Aoba.Settings.Username}" });
			spanel.Children.Add(logoutButton);
			AccountBox.Content = spanel;
			UserStatus.Content = $"Upload Count: {Aoba.UserStats?.screenShotCount}";	
		}

		private void SaveCopy_Click(object sender, RoutedEventArgs e)
		{
			SaveBox.IsEnabled = (SaveCopy.IsChecked == null) ? false : (bool)SaveCopy.IsChecked;
			Save();
		}

		private void SaveLocationButton_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				System.Windows.Forms.DialogResult result = dialog.ShowDialog();
				SaveLocation.Text = dialog.SelectedPath;
			}
			Save();
		}

		private void SoundAll_Click(object sender, RoutedEventArgs e)
		{
			SoundBox.IsEnabled = (SoundAll.IsChecked == null) ? false : (bool)SoundAll.IsChecked;
			Save();
		}

		private void ToastAll_Click(object sender, RoutedEventArgs e)
		{
			ToastBox.IsEnabled = (ToastAll.IsChecked == null) ? false : (bool)ToastAll.IsChecked;
			Save();
		}

		private void DataUpdated(object sender, RoutedEventArgs e)
		{
			Save();
		}

		private void Username_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter || e.Key == Key.Return)
				Password.Focus();
		}

		private void Password_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
				LoginButton_Click(sender, null);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Aoba.InstallContextMenu();
			}catch(Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_contextMenu.Dispose();
				}

				disposedValue = true;
			}
		}


		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion

		private void KeybindTabFocus(object sender, RoutedEventArgs e)
		{
			Aoba.IsListening = false;
		}

		private void KeyBindTabUnfocus(object sender, RoutedEventArgs e)
		{
			Aoba.IsListening = true;
		}

		private void TrayClose_Click(object sender, RoutedEventArgs e)
		{
			Aoba.Settings.CloseToTray = (CloseToTray.IsChecked == null) ? false : (bool)CloseToTray.IsChecked;
			Save();
		}

		private void StartTray_Click(object sender, RoutedEventArgs e)
		{
			Aoba.Settings.StartInTray = (StartInTray.IsChecked == null) ? false : (bool)StartInTray.IsChecked;
			Save();
		}

		private void RunOnStartup_Click(object sender, RoutedEventArgs e)
		{
			Save();
			var startupDir = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Startup)}\Aoba";
			var shortcutDir = $@"{startupDir}\Aoba.lnk";
			if (Aoba.Settings.RunAtStart) //Create Shortcut
			{
				if(!Directory.Exists(startupDir))
					Directory.CreateDirectory(startupDir);
				if (System.IO.File.Exists(shortcutDir))
					return;
				var exeLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;

				Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
				dynamic shell = Activator.CreateInstance(t);
				try
				{
					var shortcut = shell.CreateShortcut(shortcutDir);
					try
					{
						shortcut.Description = "Aoba Capture";
						shortcut.TargetPath = exeLoc;
						shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(exeLoc);
						shortcut.Save();
					}
					finally
					{
						Marshal.FinalReleaseComObject(shortcut);
					}
				}
				finally
				{
					Marshal.FinalReleaseComObject(shell);
				}

			}else //Remove Shortcut
			{
				if (!Directory.Exists(startupDir))
					return;
				if (System.IO.File.Exists(shortcutDir))
					System.IO.File.Delete(shortcutDir);
			}
		}
	}
}
