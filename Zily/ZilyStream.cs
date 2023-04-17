using System.IO;
using System.Text;
using System;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides a unicode data-stream protocol.
    /// </summary>
    public class ZilyStream
    {
        private Stream stream;
        private UnicodeEncoding streamEncoding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyStream"/>.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="Stream"/> with ability to read, write or both.
        /// </param>
        public ZilyStream(Stream stream)
        {
            this.stream = stream;
            streamEncoding = new UnicodeEncoding();
        }

        /// <summary>
        /// Reads the stream data as string.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that contains the stream data.
        /// </returns>
        public string ReadString()
        {
            int len = 0;

            len = stream.ReadByte() * 256;
            len += stream.ReadByte();
            byte[] inBuffer = new byte[len];
            stream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        /// <summary>
        /// Writes a sequence of data to the stream.
        /// </summary>
        /// <param name="text">
        /// The text that would be wrote to the stream.
        /// </param>
        /// <returns>
        /// The length of bytes that wrote to the stream.
        /// </returns>
        public int WriteString(string text)
        {
            byte[] outBuffer = streamEncoding.GetBytes(text);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            stream.WriteByte((byte)(len / 256));
            stream.WriteByte((byte)(len & 255));
            stream.Write(outBuffer, 0, len);
            if (stream is MemoryStream)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                stream.Flush();
            }

            return outBuffer.Length + 2;
        }
    }
}