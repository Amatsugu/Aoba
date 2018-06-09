using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LuminousVector.Aoba.Keyboard
{
	[Flags]
	public enum ModifierKeys : uint
	{
		Alt = 1,
		Control = 2,
		Shift = 4,
		Win = 8
	}
	public sealed class KeyHandler : NativeWindow, IDisposable
	{
		public event System.Windows.Forms.KeyEventHandler KeyDown { add { _globalHook.KeyDown += value; } remove { _globalHook.KeyDown -= value; } }
		public event System.Windows.Forms.KeyEventHandler KeyUp { add { _globalHook.KeyUp += value; } remove { _globalHook.KeyUp -= value; } }
		public event EventHandler<MouseEventExtArgs> DragStart { add { _globalHook.MouseDragStartedExt += value; } remove { _globalHook.MouseDragStartedExt -= value; } }
		public event EventHandler<MouseEventExtArgs> MouseDown { add { _globalHook.MouseDownExt += value; } remove { _globalHook.MouseDownExt -= value; } }
		public event EventHandler<MouseEventExtArgs> MouseUp { add { _globalHook.MouseUpExt += value; } remove { _globalHook.MouseUpExt -= value; } }
		public event EventHandler<MouseEventExtArgs> DragEnd { add { _globalHook.MouseDragFinishedExt += value; } remove { _globalHook.MouseDragFinishedExt -= value; } }
		public bool IsListening = true;

		private IKeyboardMouseEvents _globalHook;
		//private IKeyboardMouseEvents _mouseHook;
		private Dictionary<string, Action> _eventTargets;
		private List<KeybaordShortcut> _shortcuts;
		private int _curID = 0;

		private static readonly int WM_HOTKEY = 0x0312;

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		// Unregisters the hot key with Windows.
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


		public KeyHandler()
		{
			_globalHook	= Hook.GlobalEvents();
			this.CreateHandle(new CreateParams());
			_shortcuts = Aoba.Settings.Shortcuts;
			RegisterHotkeys();
			_eventTargets = new Dictionary<string, Action>();
		}

		private void RegisterHotkeys()
		{
			foreach (KeybaordShortcut s in _shortcuts)
			{
				RegisterHotKey(Handle, _curID++, (uint)s.ModKeys, (uint)s.Key);
			}
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == WM_HOTKEY)
			{
				Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
				ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

				CheckKey(modifier, key);
			}
		}

		public void RegisterEventTarget(string shortcutName, Action target)
		{
			if (_eventTargets.ContainsKey(shortcutName))
				_eventTargets[shortcutName] = target;
			else
				_eventTargets.Add(shortcutName, target);
		}


		private void CheckKey(ModifierKeys modifiers, Keys key)
		{
			if (!IsListening)
				return;
			foreach(KeybaordShortcut s in _shortcuts)
			{
				if(s.IsCurrent(modifiers, key))
				{
					if (_eventTargets.ContainsKey(s.Name))
						_eventTargets[s.Name]?.Invoke();
				}
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					for (int i = _curID; i > 0; i--)
					{
						UnregisterHotKey(Handle, i);
					}
					base.ReleaseHandle();
					_globalHook.Dispose();
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
	}
}
