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
        /// Receives sent data and then parses it.
        /// </summary>
        public void Parse()
        {
            var header = ReadHeader();
            Parse(header.flag, header.length);
        }

        /// <summary>
        /// Parses the sent bytes and takes actions according to the header flag.
        /// </summary>
        /// <param name="header">
        /// The header data provided by the <see cref="ReadHeader()"/>.
        /// </param>
        public void Parse((HeaderFlag flag, int length) header)
        {
            Parse(header.flag, header.length);
        }

        /// <summary>
        /// Parses the sent bytes and takes actions according to the header flag.
        /// </summary>
        /// <param name="flag">
        /// The header flag. Header flags are stored in the <see cref="HeaderFlag"/>.
        /// </param>
        /// <param name="length">
        /// Length of bytes that will be sent.
        /// </param>
        public void Parse(HeaderFlag flag, int length)
        {
            if (!ParseResponse(flag, length))
            {
                switch (flag)
                {
                    case HeaderFlag.Write:
                        Console.Write(ReadString(length));
                        break;
                    default:
                        throw new ArgumentException($"The flag \"{flag}\" is not supported.");
                } 
            }
        }

        /// <summary>
        /// Parses the given header response.
        /// </summary>
        /// <param name="header">
        /// The header data provided by the <see cref="ReadHeader()"/>.
        /// </param>
        /// <returns>
        /// If the flag is <see cref="HeaderFlag.Ok"/>, it returns <see langword="true"/>.
        /// otherwise if it returns <see langword="false"/>.
        /// if the flag is <see cref="HeaderFlag.Fail"/>, it throws an <see cref="Exception"/> with the sent message.
        /// </returns>
        /// <exception cref="Exception"></exception>
        public bool ParseResponse((HeaderFlag flag, int length) header)
        {
            return ParseResponse(header.flag, header.length);
        }

        /// <summary>
        /// Parses the given header response.
        /// </summary>
        /// <param name="flag">
        /// The header flag. Header flags are stored in the <see cref="HeaderFlag"/>.
        /// </param>
        /// <param name="length">
        /// Length of bytes that will be sent.
        /// </param>
        /// <returns>
        /// If the flag is <see cref="HeaderFlag.Ok"/>, it returns <see langword="true"/>.
        /// otherwise if it returns <see langword="false"/>.
        /// if the flag is <see cref="HeaderFlag.Fail"/>, it throws an <see cref="Exception"/> with the sent message.
        /// </returns>
        /// <exception cref="Exception"></exception>
        public bool ParseResponse(HeaderFlag flag, int length)
        {
            switch (flag)
            {
                case HeaderFlag.Ok:
                    return true;;
                case HeaderFlag.Fail:
                    throw new Exception(ReadString(length));
                default:
                    return false;
            }
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
            return ReadString(length);
        }

        /// <summary>
        /// Reads the stream data as string.
        /// </summary>
        /// <param name="length">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> that contains the stream data.
        /// </returns>
        public string ReadString(int length)
        {
            byte[] buffer = new byte[length];
            Read(buffer, 0, length);

            return streamEncoding.GetString(buffer);
        }

        /// <summary>
        /// Writes a sequence of data to the stream with the <see cref="HeaderFlag.Write"/> flag for writing the text to the receiver console.
        /// </summary>
        /// <param name="text">
        /// The text that would be wrote to the stream.
        /// </param>
        public void WriteString(string text)
        {
            WriteString(HeaderFlag.Write, text);
        }

        /// <summary>
        /// Creates a header with given flag and text and writes it beside the <paramref name="text"/> to the stream.
        /// </summary>
        /// <param name="flag">
        /// The header flag. Header flags are stored in the <see cref="HeaderFlag"/>.
        /// </param>
        /// <param name="text">
        /// The text (argument) for the requested action or response to a request.
        /// </param>
        public void WriteString(HeaderFlag flag, string text = null)
        {
            byte[] body = streamEncoding.GetBytes(text);
            byte[] header = CreateHeader(flag, body.Length);
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
        }
    }
}