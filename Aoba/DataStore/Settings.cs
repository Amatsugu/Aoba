using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ProtoBuf;
using LuminousVector.Serialization;
using System.IO;
using LuminousVector.Aoba.Keyboard;

namespace LuminousVector.Aoba.DataStore
{
	public enum FullscreenCaptureMode
	{
		CursorScreen = 0,
		AllScreens = 1,
		PrimaryScreen = 2
	}

	[ProtoContract]
	public class Settings
	{

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
		[ProtoMember(18)]
		public string Username { get; set; }
		[ProtoIgnore]
		public string Password { get; set; }
		[ProtoMember(16)]
		public string AuthToken { get; set; }
		[ProtoIgnore]
		public bool HasAuth { get { return !string.IsNullOrWhiteSpace(AuthToken); } }

		//Startup
		[ProtoMember(1)]
		public bool RunAtStart { get; set; }

		//Sounds
		[ProtoMember(2)]
		public bool PlaySounds { get; set; }
		[ProtoMember(3)]
		public bool SoundSuccess { get; set; }
		[ProtoMember(4)]
		public bool SoundFailed { get; set; }
		[ProtoMember(5)]
		public bool SoundCapure { get; set; }

		//Toasts
		[ProtoMember(6)]
		public bool ShowToasts { get; set; }
		[ProtoMember(7)]
		public bool ToastSucess { get; set; }
		[ProtoMember(8)]
		public bool ToastFailed { get; set; }
		[ProtoMember(9)]
		public bool ToastCapture { get; set; }

		//After Upload
		[ProtoMember(10)]
		public bool CopyLink { get; set; }
		[ProtoMember(11)]
		public bool OpenLink { get; set; }

		//Image Format
		[ProtoMember(12)]
		public ImageFormat Format { get; set; }

		//Saving
		[ProtoMember(13)]
		public bool SaveCopy { get; set; } = false;
		[ProtoMember(14)]
		public string SaveLocation { get; set; }

		//Fullscreen capture
		[ProtoMember(15)]
		public FullscreenCaptureMode FullscreenCapture { get; set; }

		//Keys
		[ProtoMember(17)]
		public List<KeybaordShortcut> Shortcuts { get; set; }

		public static Settings Load(string file) => DataSerializer.DeserializeData<Settings>(File.ReadAllBytes(file));

		public void Save(string file) => File.WriteAllBytes(file, DataSerializer.SerializeData(this));


	}
}
