using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;

namespace Raven.Munin
{
	public class IsolatedStoragePersistentSource : FileBasedPersistentSource
	{
		private IsolatedStorageFile isolatedStorage;

		public IsolatedStorageFile IsolatedStorage { get { return isolatedStorage ?? (isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication()); } }

		public IsolatedStoragePersistentSource(string basePath, string prefix) : base(basePath, prefix)
		{
		}

		protected override Stream CreateClonedStreamForReadOnlyPurposes()
		{
			return IsolatedStorage.OpenFile(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}

		protected override bool FileExists(string path)
		{
			return IsolatedStorage.FileExists(path);
		}

		protected override void FileDelete(string path)
		{
			IsolatedStorage.DeleteFile(path);
		}

		protected override void FileMove(string src, string dst)
		{
			IsolatedStorage.MoveFile(src, dst);
		}

		protected override FileStream OpenFile(string path)
		{
			return IsolatedStorage.OpenFile(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
		}

		protected override void EnsurePathExists(string path)
		{
			if(IsolatedStorage.DirectoryExists(path) == false)
				IsolatedStorage.CreateDirectory(path);
		}

		public override void Dispose()
		{
			if(isolatedStorage!=null)
				isolatedStorage.Dispose();
			base.Dispose();
		}
	}
}