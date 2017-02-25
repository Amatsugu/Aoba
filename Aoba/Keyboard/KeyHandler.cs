using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Diagnostics;

namespace LuminousVector.Aoba.Keyboard
{
	public class KeyHandler
	{
		public event KeyEventHandler KeyDown { add { _globalHook.KeyDown += value; } remove { _globalHook.KeyDown -= value; } }
		public event KeyEventHandler KeyUp { add { _globalHook.KeyUp += value; } remove { _globalHook.KeyUp -= value; } }
		public event EventHandler<MouseEventExtArgs> DragStart { add { _globalHook.MouseDragStartedExt += value; } remove { _globalHook.MouseDragStartedExt -= value; } }
		public event EventHandler<MouseEventExtArgs> MouseDown { add { _globalHook.MouseDownExt += value; } remove { _globalHook.MouseDownExt -= value; } }
		public event EventHandler<MouseEventExtArgs> MouseUp { add { _globalHook.MouseUpExt += value; } remove { _globalHook.MouseUpExt -= value; } }
		public event EventHandler<MouseEventExtArgs> DragEnd { add { _globalHook.MouseDragFinishedExt += value; } remove { _globalHook.MouseDragFinishedExt -= value; } }

		private IKeyboardMouseEvents _globalHook;
		//private IKeyboardMouseEvents _mouseHook;
		private Dictionary<string, Action> _eventTargets;
		private List<KeybaordShortcut> _shortcuts;
		private bool _allowNextPress = true;

		public KeyHandler()
		{
			_globalHook = Hook.GlobalEvents();
			Subscribe();
			_shortcuts = Aoba.Settings.Shortcuts;
			_eventTargets = new Dictionary<string, Action>();
		}

		public void RegisterEventTarget(string shortcutName, Action target)
		{
			if (_eventTargets.ContainsKey(shortcutName))
				_eventTargets[shortcutName] = target;
			else
				_eventTargets.Add(shortcutName, target);
		}

		public void Subscribe()
		{
			_globalHook.KeyDown += CheckKey;
			_globalHook.KeyUp += KeyRelease;
		}

		public void UnSubscribe()
		{
			_globalHook.KeyDown -= CheckKey;
			_globalHook.KeyUp -= KeyRelease;
		}

		private void KeyRelease(object sender, KeyEventArgs e) => _allowNextPress = true;

		private void CheckKey(object sender, KeyEventArgs e)
		{
			if (!_allowNextPress)
				return;
			foreach(KeybaordShortcut s in _shortcuts)
			{
				if(s.IsCurrent(e))
				{
					if (_eventTargets.ContainsKey(s.Name))
						_eventTargets[s.Name]?.Invoke();
					e.Handled = true;
					_allowNextPress = false;
				}
			}
		}
		
	}
}
