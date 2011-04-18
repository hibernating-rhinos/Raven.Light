//-----------------------------------------------------------------------
// <copyright file="FileBasedPersistentSource.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;

namespace Raven.Munin
{
    public class FileBasedPersistentSource : AbstractPersistentSource
    {
        private readonly string basePath;
        private readonly string logPath;

        private FileStream log;

        public FileBasedPersistentSource(string basePath, string prefix)
        {
            this.basePath = basePath;
        	EnsurePathExists(this.basePath);
            logPath = Path.Combine(basePath, prefix + ".ravendb");

            RecoverFromFailedRename(logPath);

            CreatedNew = FileExists(logPath) == false;

            log = OpenFile(logPath);
        }

    	protected virtual void EnsurePathExists(string path)
    	{
    	}

    	protected virtual bool FileExists(string path)
    	{
    		return File.Exists(path);
    	}

    	protected override Stream Log
        {
            get { return log; }
        }

        protected virtual FileStream OpenFile(string path)
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096);
        }

        protected override Stream CreateClonedStreamForReadOnlyPurposes()
        {
            return new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public override void ReplaceAtomically(Stream newNewLog)
        {
            var newLogStream = ((FileStream) newNewLog);
            string logTempName = newLogStream.Name;
            newLogStream.Flush();
            newLogStream.Dispose();

            newNewLog.Dispose();

            log.Dispose();

            string renamedLogFile = logPath + ".rename_op";

            FileMove(logPath, renamedLogFile);

            FileMove(logTempName, logPath);

            FileDelete(renamedLogFile);

            log = OpenFile(logPath);
        }

        public override Stream CreateTemporaryStream()
        {
            string tempFile = Path.Combine(basePath, Path.GetFileName(Path.GetTempFileName()));
            return OpenFile(tempFile);
        }

        public override void FlushLog()
        {
        	log.Flush(true);
        }

        private void RecoverFromFailedRename(string file)
        {
            string renamedFile = file + ".rename_op";
            if (FileExists(renamedFile) == false) // not in the middle of rename op, we are good
                return;

            if (FileExists(file))
                // we successfully renamed the new file and crashed before we could remove the old copy
            {
                //just complete the op and we are good (committed)
                FileDelete(renamedFile);
            }
            else // we successfully renamed the old file and crashed before we could remove the new file
            {
                // just undo the op and we are good (rollback)
                FileMove(renamedFile, file);
            }
        }

    	protected virtual void FileMove(string src, string dst)
    	{
    		File.Move(src, dst);
    	}

    	protected virtual void FileDelete(string path)
    	{
    		File.Delete(path);
    	}

    	public override void EnsureCapacity(int value)
    	{
			// not sure how we can reserve space on the file system
    	}

    	public override void Dispose()
        {
			Action parentDispose = base.Dispose;
            Write(_ =>
            {
				log.Dispose();
				parentDispose();
            });
        }

        public void Delete()
        {
            FileDelete(logPath);
        }
    }
}