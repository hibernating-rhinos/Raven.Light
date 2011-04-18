using System.IO;
using System.IO.IsolatedStorage;

namespace Raven.Munin
{
	public class IsolatedStoragePersistentSource : FileBasedPersistentSource
	{
		private readonly IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

		public IsolatedStoragePersistentSource(string basePath, string prefix) : base(basePath, prefix)
		{
		}

		protected override bool FileExists(string path)
		{
			return isolatedStorage.FileExists(path);
		}

		protected override void FileDelete(string path)
		{
			isolatedStorage.DeleteFile(path);
		}

		protected override void FileMove(string src, string dst)
		{
			isolatedStorage.MoveFile(src, dst);
		}

		protected override FileStream OpenFiles(string path)
		{
			return isolatedStorage.OpenFile(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
		}
	}
}