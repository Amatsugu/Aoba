using System;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using SharpShell.SharpContextMenu;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using System.IO;

namespace Aoba.ShellExtensions
{
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.AllFiles)]
	class UploadExtension : SharpContextMenu
	{

		public static Image ICON;
		public static MemoryMappedFile MM_File;

		protected override bool CanShowMenu()
		{
			return true;
		}

		protected override ContextMenuStrip CreateMenu()
		{
			var menu = new ContextMenuStrip();
			var uploadItem = new ToolStripMenuItem()
			{
				Text = "Upload to Aoba",
				Image = GetIcon()
			};
			uploadItem.Click += DoUpload;
			menu.Items.Add(uploadItem);

			return menu;
		}

		private Image GetIcon()
		{
			if (ICON == null)
			{
				try
				{
					var a = Assembly.GetExecutingAssembly();
					ICON = Image.FromStream(a.GetManifestResourceStream("Aoba.ShellExtensions.res.Aoba.png"));
				}catch
				{
				}
			}
			return ICON;
		}

		private void DoUpload(object sender, EventArgs e)
		{
			try
			{
				MM_File = MemoryMappedFile.OpenExisting("Aoba.ShellUploads");
				var writer = new StreamWriter(MM_File.CreateViewStream());
				foreach (string s in SelectedItemPaths)
				{
					writer.WriteLine(s);
				}
				writer.Flush();
				writer.Dispose();
			}catch
			{
				MessageBox.Show("Aoba is not running.");
			}
		}
	}
}
