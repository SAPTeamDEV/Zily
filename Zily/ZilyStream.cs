using System.IO;
using System.Text;
using System;
using System.Linq;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides a unicode data-stream protocol.
    /// </summary>
    public partial class ZilyStream : Stream
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
        /// Reads the header of the response.
        /// </summary>
        /// <returns>
        /// Length of sent bytes.
        /// </returns>
        public int ReadHeader()
        {
            int len = 0;

            len = ReadByte() * 256;
            len += ReadByte();

            return len;
        }

        /// <summary>
        /// Creates a header.
        /// </summary>
        /// <param name="length">
        /// Length of bytes that will be sent.
        /// </param>
        /// <returns>
        /// An array of header bytes.
        /// </returns>
        public byte[] CreateHeader(int length)
        {
            int len = length;
            if (len > ushort.MaxValue)
            {
                len = ushort.MaxValue;
            }

            return new byte[]
            {
                (byte)(len / 256),
                (byte)(len & 255)
            };
        }

        /// <summary>
        /// Reads the stream data as string.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that contains the stream data.
        /// </returns>
        public string ReadString()
        {
            int len = ReadHeader();
            byte[] buffer = new byte[len];
            Read(buffer, 0, len);

            return streamEncoding.GetString(buffer);
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
            byte[] body = streamEncoding.GetBytes(text);
            byte[] header = CreateHeader(body.Length);
            byte[] buffer = header.Concat(body).ToArray();

            foreach (var data in buffer)
            {
                WriteByte(data);
            }

            if (stream is MemoryStream)
            {
                Seek(0, SeekOrigin.Begin);
            }
            else
            {
                Flush();
            }

            return buffer.Length;
        }
    }
}