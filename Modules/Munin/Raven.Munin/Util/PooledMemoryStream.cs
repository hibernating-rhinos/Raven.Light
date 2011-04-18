using System;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel.Channels;

namespace Raven.Munin.Util
{
	public class PooledMemoryStream : Stream
	{
		private long pos;
		private long writtenLength;

		private byte[] internalBuffer;
		private static readonly BufferManager bufferManager = BufferManager.CreateBufferManager(1024 * 1024 * 512, 1024 * 1024 * 16);

		public static PooledMemoryStream From(Stream stream, int length)
		{
			var total = 0;
			PooledMemoryStream destination = null;
			byte[] buffer = bufferManager.TakeBuffer(4096);
			try
			{
				int read;
				destination = new PooledMemoryStream();
				while ((read = stream.Read(buffer, 0, buffer.Length)) != 0 &&
					(total += read) < length)
				{
					destination.Write(buffer, 0, read);
				}
				return destination;
			}
			catch
			{
				if(destination!=null)
					destination.Dispose();
				throw;
			}
			finally
			{
				bufferManager.ReturnBuffer(buffer);
			}
		}

		public override void Flush()
		{
			//no op in memory
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			var posCopy = pos;
			switch (origin)
			{
				case SeekOrigin.Begin:
					posCopy = offset;
					break;
				case SeekOrigin.Current:
					posCopy = posCopy + offset;
					break;
				case SeekOrigin.End:
					posCopy = Length + offset;
					break;
				default:
					throw new ArgumentOutOfRangeException("origin");
			}

			if (posCopy < 0 || posCopy > Length)
				throw new ArgumentException("Could not set position to before the file start or after file end");

			pos = posCopy;
			return pos;
		}

		public override void SetLength(long value)
		{
			var buffer = internalBuffer;
			var newBuffer = bufferManager.TakeBuffer((int)value);

			if (buffer != null)
			{
				Buffer.BlockCopy(buffer, 0, newBuffer, 0, Math.Min(buffer.Length, newBuffer.Length));
			}
			internalBuffer = newBuffer;

			if (buffer != null)
			{
				bufferManager.ReturnBuffer(buffer);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (internalBuffer == null)
				return 0;

			var byteCount = Math.Min(Length - pos, count);
			if (byteCount <= 0)
				return 0;

			Buffer.BlockCopy(internalBuffer, (int)pos, buffer, offset, (int)byteCount);
			pos += byteCount;
			return (int)byteCount;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			EnsureCapacity(count);

			Buffer.BlockCopy(buffer, offset, internalBuffer, (int)writtenLength, count);

			writtenLength += count;
		}

		private void EnsureCapacity(int count)
		{
			if (internalBuffer != null && writtenLength + count <= internalBuffer.Length)
				return;
			if (internalBuffer == null)
				SetLength(256);
			else
				SetLength(internalBuffer.Length * 2);
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override long Length
		{
			get { return writtenLength; }
		}

		public override long Position
		{
			get { return pos; }
			set { Seek(value, SeekOrigin.Begin); }
		}

		protected override void Dispose(bool disposing)
		{
			bufferManager.ReturnBuffer(internalBuffer);
			internalBuffer = null;
		}

		public byte[] ComputeHash()
		{
			using (var sha = new SHA256Managed())
			{
				return sha.ComputeHash(internalBuffer, 0, (int)writtenLength);
			}
		}
	}
}