using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace LuminousVector.Aoba.Keyboard
{
	public class KeybaordShortcut
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonIgnore]
		public Keys Key => _key;
		[JsonIgnore]
		public string Modifiers
		{
			get
			{
				if (modifiers != null)
					return modifiers;
				string[] m = new string[] { (_ctrl) ? "Ctrl" : null, (_shift) ? "Shift" : null, (_alt) ? "Alt" : null };
				return modifiers = string.Join(" + ", from string s in m where s != null select s);
			}
		}
		[JsonIgnore]
		private string modifiers;

		public ModifierKeys ModKeys => ((_alt ? ModifierKeys.Alt : 0) | (_shift ? ModifierKeys.Shift : 0) | (_ctrl ? ModifierKeys.Control : 0));

		[JsonProperty]
		private Keys _key;
		[JsonProperty]
		private bool _shift;
		[JsonProperty]
		private bool _alt;
		[JsonProperty]
		private bool _ctrl;

		KeybaordShortcut()
		{

		}

		public KeybaordShortcut(string name, Keys key, bool ctrl = false, bool shift = false, bool alt = false)
		{
			Name = name;
			_key = key;
			_ctrl = ctrl;
			_shift = shift;
			_alt = alt;
		}

		internal bool IsCurrent(KeyEventArgs e) => (e.Alt == _alt && e.Shift == _shift && e.Control == _ctrl && e.KeyCode == _key);

		internal bool IsCurrent(ModifierKeys modifiers, Keys key) => (ModKeys == modifiers) && (key == _key);
	}
}
