using System.IO;
using System.Text;
using System;
using System.Linq;
using Serilog;
using System.Threading;

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

        /// <summary>
        /// Gets or Sets the logger.
        /// </summary>
        protected ILogger logger;

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
            return ReadHeader(CancellationToken.None);
        }

        /// <summary>
        /// Reads the header of the response.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token for aborting this operation.
        /// </param>
        /// <returns>
        /// Flag and Length of sent bytes.
        /// </returns>
        public virtual (HeaderFlag flag, int length) ReadHeader(CancellationToken cancellationToken)
        {
            HeaderFlag flag = HeaderFlag.Unknown;

            while (!cancellationToken.IsCancellationRequested)
            {
                int data = ReadByte();
                if (data != -1)
                {
                    flag = (HeaderFlag)data;
                    break;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            int length = Math.Max(0, (ReadByte() * 256) + ReadByte());

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
        public virtual byte[] CreateHeader(HeaderFlag flag, int length)
        {
            if (length > ushort.MaxValue)
            {
                throw new ArgumentException("Length is too long.");
            }

            return length > 0 ? new byte[]
            {
                (byte)flag,
                (byte)(length / 256),
                (byte)(length & 255)
            } : new byte[] { (byte)flag };
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
                        logger.Information("Writing \"{text}\" to the console", text);

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
                        WriteCommand(HeaderFlag.VersionInfo, API.ToString());
                        responseHandled = true;
                        break;
                    default:
                        if (!ParseHelper(flag, length))
                        {
                            WriteCommand(HeaderFlag.Fail, $"The flag \"{flag}\" is not supported.");
                        }
                        responseHandled = true;
                        break;
                }

                if (!responseHandled)
                {
                    WriteCommand(HeaderFlag.Ok);
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, Parses header flags that is not parsed in the base class.
        /// </summary>
        /// <param name="flag">
        /// The header flag. Header flags are stored in the <see cref="HeaderFlag"/>.
        /// </param>
        /// <param name="length">
        /// Length of bytes that will be sent.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if it can parse the given header, otherwise it returns <see langword="false"/>.
        /// if you want to write your own error message, you must return <see langword="true"/> to prevent unexpected behaviors.
        /// </returns>
        protected virtual bool ParseHelper(HeaderFlag flag, int length)
        {
            return false;
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
        public virtual bool ParseResponse(HeaderFlag flag, int length)
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
                    var rawVersion = ReadString(length);
                    logger.Debug("Stream protocol version is {version}", rawVersion);
                    StreamVersion = new Version(rawVersion);
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
            logger.Debug("Reading {length} bytes of data", length);
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
            logger.Debug("Writing data with flag {flag} and message \"{text}\"", flag, text != null ? text.Replace("\n", "") : null);
            byte[] body = text != null ? streamEncoding.GetBytes(text) : new byte[0];
            byte[] header = CreateHeader(flag, body.Length);
            byte[] buffer = header.Concat(body).ToArray();

            Write(buffer, 0, buffer.Length);

            if (Stream is MemoryStream)
            {
                Seek(-buffer.Length, SeekOrigin.Current);
            }
            else
            {
                Flush();
            }

            logger.Debug("Wrote {length} bytes", buffer.Length);
        }

        /// <summary>
        /// Sends a command to the stream, then waits for receiving response and parses it.
        /// </summary>
        /// <param name="flag">
        /// The header flag. Header flags are stored in the <see cref="HeaderFlag"/>.
        /// </param>
        /// <param name="text">
        /// The text (argument) for the requested action or response to a request.
        /// </param>
        public void Send(HeaderFlag flag, string text = null)
        {
            WriteCommand(flag, text);
            var header = ReadHeader();
            Parse(header);
        }
    }
}
