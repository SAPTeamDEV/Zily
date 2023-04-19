using System.IO;
using System.Text;
using System;
using System.Linq;
using Serilog;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides a unicode data-stream protocol.
    /// </summary>
    public partial class ZilyStream : Stream
    {
        /// <summary>
        /// Gets the protocol version.
        /// </summary>
        public static Version API = new Version(2, 0);

        /// <summary>
        /// Gets the stream protocol version.
        /// </summary>
        public Version StreamVersion { get; private set; }

        /// <summary>
        /// Gets the underlying <see cref="System.IO.Stream"/>.
        /// </summary>
        public Stream Stream { get; }

        private readonly UnicodeEncoding streamEncoding;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyStream"/>.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="System.IO.Stream"/> with ability to read, write or both.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyStream(Stream stream, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = Log.Logger;
            }

            logger.Debug("Initializing a new ZilyStream instance");

            Stream = stream;
            streamEncoding = new UnicodeEncoding();
            this.logger = logger;
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
                logger.Debug("Parsing a header with flag {flag}", flag);

                bool responseHandled = false;

                switch (flag)
                {
                    case HeaderFlag.Write:
                        var text = ReadString(length);
                        logger.Information("Writing {text} to the console", text);

                        if (Stream is MemoryStream) // Probably it is a test...
                        {
                            logger.Error("Experimental crash triggered");
                            throw new InvalidOperationException("Writing is not supported by test runners :)");
                        }
                        else
                        {
                            Console.Write(text);
                        }
                        break;
                    case HeaderFlag.Version:
                        logger.Information("Protocol version is requested");
                        VersionInfo();
                        responseHandled = true;
                        break;
                    default:
                        Fail($"The flag \"{flag}\" is not supported.");
                        responseHandled = true;
                        break;
                }

                if (!responseHandled)
                {
                    Ok();
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
        /// <see langword="true"/> if the flag is a response flag, otherwise it returns <see langword="false"/>.
        /// if the flag is <see cref="HeaderFlag.Fail"/>, it throws an <see cref="Exception"/> with the sent message.
        /// </returns>
        /// <exception cref="Exception"></exception>
        public bool ParseResponse(HeaderFlag flag, int length)
        {
            logger.Debug("Trying to parse a header with {flag} flag as a response", flag);

            bool isHandled = true;

            switch (flag)
            {
                case HeaderFlag.Ok:
                    logger.Debug("Stream response is OK");
                    break;
                case HeaderFlag.Warn:
                    logger.Warning(ReadString(length));
                    break;
                case HeaderFlag.VersionInfo:
                    StreamVersion = new Version(ReadString(length));
                    logger.Debug("Stream protocol version is {version}", StreamVersion);
                    break;
                case HeaderFlag.Fail:
                    var e = new Exception(ReadString(length));
                    logger.Fatal(e, "Stream returns an error");
                    throw e;
                default:
                    isHandled = false;
                    break;
            }

            return isHandled;
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
            logger.Information("Reading {length} bytes of data", length);
            byte[] buffer = new byte[length];
            Read(buffer, 0, length);

            return streamEncoding.GetString(buffer);
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
        public void WriteCommand(HeaderFlag flag, string text = null)
        {
            logger.Information("Writing data with flag {flag} and message \"{text}\"", flag, text);
            byte[] body = text != null ? streamEncoding.GetBytes(text) : new byte[0];
            byte[] header = CreateHeader(flag, body.Length);
            byte[] buffer = header.Concat(body).ToArray();

            foreach (var data in buffer)
            {
                WriteByte(data);
            }

            if (Stream is MemoryStream)
            {
                Seek(-buffer.Length, SeekOrigin.Current);
            }
            else
            {
                Flush();
            }

            logger.Information("Wrote {length} bytes", buffer.Length);
        }
    }
}