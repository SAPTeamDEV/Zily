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
        /// Flag and Length of sent bytes.
        /// </returns>
        public (HeaderFlag flag, int length) ReadHeader()
        {
            var flag = (HeaderFlag)ReadByte();

            int length = ReadByte() * 256;
            length += ReadByte();

            return (flag, length);
        }

        /// <summary>
        /// Creates a header.
        /// </summary>
        /// <param name="flag">
        /// The header flag. Header flags are stored in the <see cref="HeaderFlag"/>.
        /// </param>
        /// <param name="length">
        /// Length of bytes that will be sent.
        /// </param>
        /// <returns>
        /// An array of header bytes.
        /// </returns>
        public byte[] CreateHeader(HeaderFlag flag, int length)
        {
            if (length > ushort.MaxValue)
            {
                length = ushort.MaxValue;
            }

            return new byte[]
            {
                (byte)flag,
                (byte)(length / 256),
                (byte)(length & 255)
            };
        }

        /// <summary>
        /// Receives and Parses the sent data.
        /// </summary>
        public void Parse()
        {

        }

        /// <summary>
        /// Reads the stream data as string.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that contains the stream data.
        /// </returns>
        public string ReadString()
        {
            int length = ReadHeader().length;
            byte[] buffer = new byte[length];
            Read(buffer, 0, length);

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
            byte[] header = CreateHeader(HeaderFlag.Write, body.Length);
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