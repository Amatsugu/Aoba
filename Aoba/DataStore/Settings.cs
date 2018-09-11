using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using LuminousVector.Aoba.Keyboard;
using Newtonsoft.Json;

namespace LuminousVector.Aoba.DataStore
{
	public enum FullscreenCaptureMode
	{
		CursorScreen = 0,
		AllScreens = 1,
		PrimaryScreen = 2
	}

	public class Settings
	{
		public Settings()
		{

		}

		public static Settings Default
		{
			get
			{
				return new Settings()
				{
					Username = null,
					AuthToken = null,
					RunAtStart = true,
					PlaySounds = true,
					SoundSuccess = true,
					SoundFailed = true,
					SoundCapure = false,
					ShowToasts = true,
					ToastSucess = true,
					ToastFailed = true,
					ToastCapture = false,
					CopyLink = true,
					OpenLink = false,
					Format = ImageFormat.Png,
					SaveCopy = false,
					CloseToTray = true,
					StartInTray = false,
					SaveLocation = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\Aoba",
					FullscreenCapture = FullscreenCaptureMode.CursorScreen,
					Shortcuts = new List<KeybaordShortcut>()
					{
						new KeybaordShortcut("Capture Window", Keys.D2, true, true),
						new KeybaordShortcut("Capture Fullscreen", Keys.D3, true, true),
						new KeybaordShortcut("Capture Region", Keys.D4, true, true),
						new KeybaordShortcut("Upload File", Keys.U, true, true),
					},
				};
			}
		}
		//User
		public string Username { get; set; }
		[JsonIgnore]
		public string Password { get; set; }
		public string AuthToken { get; set; }
		[JsonIgnore]
		public bool HasAuth { get { return !string.IsNullOrWhiteSpace(AuthToken); } }

		//Startup
		public bool RunAtStart { get; set; }

		//Sounds
		public bool PlaySounds { get; set; }
		public bool SoundSuccess { get; set; }
		public bool SoundFailed { get; set; }
		public bool SoundCapure { get; set; }

		//Toasts
		public bool ShowToasts { get; set; }
		public bool ToastSucess { get; set; }
		public bool ToastFailed { get; set; }
		public bool ToastCapture { get; set; }

		//After Upload
		public bool CopyLink { get; set; }
		public bool OpenLink { get; set; }

		//Image Format
		public ImageFormat Format { get; set; }

		//Saving
		public bool SaveCopy { get; set; } = false;
		public string SaveLocation { get; set; }

		//Fullscreen capture
		public FullscreenCaptureMode FullscreenCapture { get; set; }

		//Keys
		public List<KeybaordShortcut> Shortcuts { get; set; }

		//Tray
		public bool CloseToTray { get; set; }
		public bool StartInTray { get; set; }

		public static Settings Load(string file)
		{
			try
			{

				return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file));
			}catch
			{
				return null;
			}
		}

		public void Save(string file) => File.WriteAllText(file, JsonConvert.SerializeObject(this));


	}
}
