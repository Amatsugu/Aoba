using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuminousVector.Aoba
{
	public static class ScreenCapture
	{
		public static Image CapturePrimary() => CaptureScreen(Screen.PrimaryScreen);

		public static Image CaptureAll() => CaptureMultiScreen(Screen.AllScreens);

		public static Image CaptureCursor() => CaptureScreen(Screen.FromPoint(Cursor.Position));

		public static Bitmap CaptureRegion(Rectangle region)
		{
			Screen[] screens = Screen.AllScreens;
			Bitmap screenCap = new Bitmap(region.Width, region.Height);
			using (Graphics g = Graphics.FromImage(screenCap))
			{

				Point destP = new Point();
				for (int i = 0; i < screens.Length; i++)
				{
					Rectangle curRect = region;
					curRect.Intersect(screens[i].Bounds);
					if (curRect.IsEmpty)
						continue;
					destP.X = curRect.X - region.X;
					destP.Y = curRect.Y - region.Y;
					g.CopyFromScreen(curRect.Location, destP, curRect.Size, CopyPixelOperation.SourceCopy);
				}
			}
			return screenCap;
		}

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
			Screen primary = screens.First(x => x.Primary);
			Bitmap[] screenCaps = new Bitmap[screens.Length];
			Rectangle compositeRect = primary.Bounds;
			int i = 0;
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
				screenCaps[i++] = CaptureScreen(s);
			}
			Size compositeSize = new Size(Math.Abs(compositeRect.X) + compositeRect.Width, Math.Abs(compositeRect.Y) + compositeRect.Height);
			Bitmap composite = new Bitmap(compositeSize.Width, compositeSize.Height);
			using (Graphics g = Graphics.FromImage(composite))
			{
				Point p = new Point();
				for(int s = 0; s < screens.Length; s++)
				{
					p.X = (Math.Abs(compositeRect.X) + screens[s].Bounds.X);
					p.Y = (Math.Abs(compositeRect.Y) + screens[s].Bounds.Y);
					g.DrawImage(screenCaps[s], p);
				}
			}

			return composite;
			//Bitmap screenCap = new Bitmap();
		}
	}
}
