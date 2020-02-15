using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace LuminousVector.Aoba.Server.DataStore
{
	public class FileStreamAbstraction : TagLib.File.IFileAbstraction
	{
		public FileStreamAbstraction(string name, Stream stream)
		{
			Name = name;
			ReadStream = stream;
			WriteStream = stream;
		}

		public void CloseStream(Stream stream)
		{
			stream.Dispose();
		}

		public string Name { get; private set; }

		public Stream ReadStream { get; private set; }

		public Stream WriteStream { get; private set; }
	}
}
