using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SAPTeam.Zily
{
    public partial class ZilyStream
    {
        /// <inheritdoc/>
        public override bool CanRead => Stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => Stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => Stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => Stream.Length;

        /// <inheritdoc/>
        public override long Position { get => Stream.Position; set => Stream.Position = value; }

        /// <inheritdoc/>
        public override void Flush()
        {
            Stream.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }
    }
}
