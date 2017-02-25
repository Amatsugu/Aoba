using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProtoBuf;

namespace LuminousVector.Aoba.Keyboard
{
	[ProtoContract]
	public class KeybaordShortcut
	{
		[ProtoIgnore]
		public string Name { get { return _name; } }

		[ProtoMember(1)]
		private string _name;
		[ProtoMember(2)]
		private Keys _key;
		[ProtoMember(3)]
		private bool _shift;
		[ProtoMember(4)]
		private bool _alt;
		[ProtoMember(5)]
		private bool _ctrl;

		KeybaordShortcut()
		{

		}

		public KeybaordShortcut(string name, Keys key, bool ctrl = false, bool shift = false, bool alt = false)
		{
			_name = name;
			_key = key;
			_ctrl = ctrl;
			_shift = shift;
			_alt = alt;
		}

		internal bool IsCurrent(KeyEventArgs e) => (e.Alt == _alt && e.Shift == _shift && e.Control == e.Control && e.KeyCode == _key);
	}
}
