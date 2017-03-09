using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;

namespace LuminousVector.Aoba
{
	[RunInstaller(true), ComVisible(false)]
	public partial class ContextMenuInstaller : Installer
	{

		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			// Call RegistrationServices.RegisterAssembly to register the classes in 
			// the current managed assembly to enable creation from COM.
			RegistrationServices regService = new RegistrationServices();
			regService.RegisterAssembly(
				Assembly.LoadFrom(@"AobaContextMenu.dll"),
				AssemblyRegistrationFlags.SetCodeBase);
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			// Call RegistrationServices.UnregisterAssembly to unregister the classes 
			// in the current managed assembly.
			RegistrationServices regService = new RegistrationServices();
			regService.UnregisterAssembly(Assembly.LoadFrom(@"AobaContextMenu.dll"));
		}
	}
}
