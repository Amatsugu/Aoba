using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuminousVector.Aoba.Capture
{
	public static class ScreenCapture
	{
		public static Image CapturePrimary() => CaptureScreen(Screen.PrimaryScreen);

		public static Image CaptureAll() => CaptureMultiScreen(Screen.AllScreens);

		public static Image CaptureCursor() => CaptureScreen(Screen.FromPoint(Cursor.Position));

		public static Bitmap CaptureRegion(Rectangle region)
		{
			Bitmap screenCap = new Bitmap(region.Width, region.Height);
			using (Graphics g = Graphics.FromImage(screenCap))
			{
				g.CopyFromScreen(region.Location, Point.Empty, region.Size, CopyPixelOperation.SourceCopy);
			}
			return screenCap;
		}

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hWnd, out WindowRect lpRect);

		private static Bitmap CaptureScreen(Screen screen)
		{
			Bitmap screenCap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
			using (Graphics g = Graphics.FromImage(screenCap))
			{
				g.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, screenCap.Size, CopyPixelOperation.SourceCopy);
			}
			return screenCap;
		}

		private static Bitmap CaptureMultiScreen(Screen[] screens)
		{
			Rectangle compositeRect = screens.First(x => x.Primary).Bounds;
			foreach (Screen s in screens)
			{
				if (compositeRect.X > s.Bounds.X)
					compositeRect.X = s.Bounds.X;
				if (compositeRect.Y > s.Bounds.Y)
					compositeRect.Y = s.Bounds.Y;
				if (compositeRect.Width < s.Bounds.X + s.Bounds.Width)
					compositeRect.Width = s.Bounds.X + s.Bounds.Width;
				if (compositeRect.Height < s.Bounds.Y + s.Bounds.Height)
					compositeRect.Height = s.Bounds.Y + s.Bounds.Height;
			}
			Bitmap composite = new Bitmap(Math.Abs(compositeRect.X) + compositeRect.Width, Math.Abs(compositeRect.Y) + compositeRect.Height);
			using (Graphics g = Graphics.FromImage(composite))
			{
				Point p = new Point();
				for(int s = 0; s < screens.Length; s++)
				{
					p.X = (Math.Abs(compositeRect.X) + screens[s].Bounds.X);
					p.Y = (Math.Abs(compositeRect.Y) + screens[s].Bounds.Y);
					g.CopyFromScreen(screens[s].Bounds.Location, p, screens[s].Bounds.Size, CopyPixelOperation.SourceCopy);
				}
			}
			return composite;
		}
	}
}
