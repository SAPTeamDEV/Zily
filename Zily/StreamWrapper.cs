using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SAPTeam.Zily
{
    public partial class ZilyStream
    {
        /// <inheritdoc/>
        public override bool CanRead => stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => stream.Length;

        /// <inheritdoc/>
        public override long Position { get => stream.Position; set => stream.Position = value; }

        /// <inheritdoc/>
        public override void Flush()
        {
            stream.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }
}
