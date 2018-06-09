using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Capture
{
	[StructLayout(LayoutKind.Sequential)]
	public struct WindowRect
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public Rectangle AsRectange() => new Rectangle(Left, Top, Right - Left, Bottom - Top);
	}
}
