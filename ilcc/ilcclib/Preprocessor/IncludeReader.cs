﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Utils.Zip;
using ilcc.Include;
using ilcc.Runtime;

namespace ilcclib.Preprocessor
{
	public interface IIncludeContainer
	{
		string GetContainerPath();
		bool Contains(string FileName);
		string Read(string FileName);
	}

	public class LocalFolderIncludeContainer : IIncludeContainer
	{
		string Path;

		public LocalFolderIncludeContainer(string Path)
		{
			this.Path = Path;
		}

		private string NormalizePath(string FileName)
		{
			// TODO: We should do security checks here.
			return FileName;
		}

		string IIncludeContainer.GetContainerPath()
		{
			return this.Path;
		}

		bool IIncludeContainer.Contains(string FileName)
		{
			return File.Exists(Path + "/" + NormalizePath(FileName));
		}

		string IIncludeContainer.Read(string FileName)
		{
			return File.ReadAllText(Path + "/" + NormalizePath(FileName), CLibUtils.DefaultEncoding);
		}
	}

	public class ZipIncludeContainer : IIncludeContainer
	{
		Stream Stream;
		ZipFile ZipFile;
		string Path;

		public ZipIncludeContainer(Stream Stream, string Path)
		{
			this.Path = Path;
			this.Stream = Stream;
			this.ZipFile = ZipFile.Read(Stream);
		}

		private ZipEntry Get(string FileName)
		{
			return this.ZipFile["include/" + FileName];
		}

		string IIncludeContainer.GetContainerPath()
		{
			return string.Format("zip://" + this.Path);
		}

		bool IIncludeContainer.Contains(string FileName)
		{
			var Item = Get(FileName);
			return Item != null;
		}

		string IIncludeContainer.Read(string FileName)
		{
			var Stream = new MemoryStream();
			var Item = Get(FileName);
			Item.Extract(Stream);
			return CLibUtils.DefaultEncoding.GetString(Stream.ToArray());
		}
	}

	public class IncludeReader : IIncludeReader
	{
		List<IIncludeContainer> IncludeContainers = new List<IIncludeContainer>();

		public IncludeReader(bool AddEmbeddedCLib = true)
		{
			if (AddEmbeddedCLib)
			{
				this.AddZip(new MemoryStream(IncludeResources.include_zip), "$include.zip");
			}
		}

		public void AddFolder(string Path)
		{
			throw(new NotImplementedException());
		}

		public void AddZip(string ZipPath)
		{
			AddZip(new MemoryStream(File.ReadAllBytes(ZipPath)), ZipPath);
		}

		public void AddZip(Stream Stream, string ZipPath)
		{
			IncludeContainers.Add(new ZipIncludeContainer(Stream, ZipPath));
		}

		public string ReadIncludeFile(string CurrentFile, string FileName, bool System, out string FullNewFileName)
		{
			CurrentFile = CurrentFile.Replace('\\', '/');
			int CurrentFileLastIndex = CurrentFile.LastIndexOf('/');
			var BaseDirectory = (CurrentFileLastIndex >= 0) ? CurrentFile.Substring(0, CurrentFileLastIndex) : CurrentFile;

			foreach (var Container in IncludeContainers)
			{
				if (Container.Contains(FileName))
				{
					if (System || Container.GetContainerPath() == BaseDirectory)
					{
						FullNewFileName = Container.GetContainerPath() + "/" + FileName;
						return Container.Read(FileName);
					}
				}
			}

			FullNewFileName = (BaseDirectory + "/" + FileName);

			//Console.WriteLine(FullNewFileName);

			if (File.Exists(FullNewFileName))
			{
				return File.ReadAllText(FullNewFileName);
			}

			throw new Exception(String.Format("Can't find file '{0}'", FileName));
		}
	}
}
