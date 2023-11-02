using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents a standard text writer for interacting with the <see cref="ZilyStream"/>.
    /// </summary>
    public class ZilyTextWriter : TextWriter
    {
        StringBuilder sb;
        ZilyStream stream;

        /// <inheritdoc/>
        public override Encoding Encoding => Encoding.Unicode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyTextWriter"/>.
        /// </summary>
        /// <param name="zilyStream">
        /// The underlying Zily stream.
        /// </param>
        public ZilyTextWriter(ZilyStream zilyStream)
        {
            stream = zilyStream;
            sb = new StringBuilder();
            NewLine = "\n";
        }

        /// <inheritdoc/>
        public override void Write(char value)
        {
            sb.Append(value);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            stream.Send(new ZilyHeader(ZilyHeaderFlag.Write, sb.ToString()));
            sb.Clear();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            sb.Clear();
            sb = null;

            stream = null;
        }
    }
}
