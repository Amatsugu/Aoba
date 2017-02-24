using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Flurl;
using Flurl.Http;
using System.ComponentModel;

namespace LuminousVector.Aoba
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private System.Windows.Forms.NotifyIcon nIcon;


		public MainWindow()
		{
			InitializeComponent();
			Aoba.Init();
			SaveBox.IsEnabled = (bool)SaveCopy.IsChecked;
			nIcon = new System.Windows.Forms.NotifyIcon();
			nIcon.Icon = new Icon("res/Aobax32.ico");
			nIcon.Visible = false;
			nIcon.Text = "Aoba";
			var cm = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
			{
				new System.Windows.Forms.MenuItem("Restore", RestoreWindow),
				new System.Windows.Forms.MenuItem("Quit", (o, e) =>
				{
					nIcon.Visible = false;
					Aoba.Save();
					Close();
				})
			});
			nIcon.ContextMenu = cm;
			nIcon.DoubleClick += RestoreWindow;
		}

		internal void RestoreWindow(object sender, EventArgs e)
		{
			
			nIcon.Visible = false;
			Show();
			if (WindowState != WindowState.Normal)
				WindowState = WindowState.Normal;
		}

		internal void HideWindow()
		{
			Hide();
			nIcon.Visible = true;
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				HideWindow();
			}
			base.OnStateChanged(e);
		}

#if !DEBUG
		protected override void OnClosing(CancelEventArgs e)
		{
			if (WindowState == WindowState.Normal)
			{
				e.Cancel = true;
				HideWindow();
			}
			base.OnClosing(e);
		}
#endif

		private async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AccountBox.IsEnabled = false;
				Aoba.Settings.Username = Username.Text;
				Aoba.Settings.Password = Password.Password;
				await Aoba.Login();
				MessageBox.Show(Aoba.Settings.AuthToken, "Sucess");
			}catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error");
			}
			finally
			{
				AccountBox.IsEnabled = true;
			}
		}

		private void SaveCopy_Click(object sender, RoutedEventArgs e)
		{
			SaveBox.IsEnabled = (bool)SaveCopy.IsChecked;
		}

		private void SaveLocationButton_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void SoundAll_Click(object sender, RoutedEventArgs e)
		{
			SoundBox.IsEnabled = (bool)SoundAll.IsChecked;
		}

		private void ToastAll_Click(object sender, RoutedEventArgs e)
		{
			ToastBox.IsEnabled = (bool)ToastAll.IsChecked;
		}
	}
}
