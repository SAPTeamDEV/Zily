using System.IO;
using System.Text;
using System;
using System.Linq;
using Serilog;
using System.Threading;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides a unicode data-stream protocol.
    /// </summary>
    public partial class ZilyStream : Stream
    {
        /// <summary>
        /// Gets the underlying <see cref="System.IO.Stream"/>.
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// Gets the underlying <see cref="ISide"/> that parses the incoming responses.
        /// </summary>
        public ZilySide Side { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this connection is online
        /// </summary>
        public bool IsOnline { get; set; }

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
        /// <param name="side">
        /// The <see cref="ZilySide"/> that parses the receiving responses.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyStream(Stream stream, ZilySide side, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = Log.Logger;
            }

            logger.Debug("Initializing a new Zily session");

            Stream = stream;
            Side = side;
            Side.zs = this;
            Side.logger = logger;
            streamEncoding = new UnicodeEncoding();
            this.logger = logger;
        }

        /*
        /// <summary>
        /// Receives sent data and then parses it.
        /// </summary>
        public void Parse()
        {
            var (flag, length) = ReadHeader();
            Parse(flag, length);
        }

        /// <summary>
        /// Parses the given header.
        /// </summary>
        /// <param name="header">
        /// The header data provided by the <see cref="ReadHeader()"/>.
        /// </param>
        public void Parse((HeaderFlag flag, int length) header)
        {
            Parse(header.flag, header.length);
        }

        /// <summary>
        /// Parses the given header.
        /// </summary>
        /// <param name="flag">
        /// The header flag.
        /// </param>
        /// <param name="length">
        /// The length of the text (argument) for the header flag.
        /// </param>
        public void Parse(HeaderFlag flag, int length)
        {
            bool isHandled = false;

            if (flag.IsRequest())
            {
                isHandled = ParseRequest(flag, length);
            }
            else if (flag.IsResponse())
            {
                isHandled = ParseResponse(flag, length);
            }

            if (!isHandled)
            {
                WriteCommand(HeaderFlag.Fail, $"The flag \"{flag}\" is not supported.");
            }
        }

        /// <summary>
        /// Parses the given header request.
        /// </summary>
        /// <param name="header">
        /// The header data provided by the <see cref="ReadHeader()"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this method could handle the given flag, otherwise it returns <see langword="false"/>.
        /// </returns>
        public bool ParseRequest((HeaderFlag flag, int length) header)
        {
            return ParseRequest(header.flag, header.length);
        }

        /// <summary>
        /// Parses the given header request.
        /// </summary>
        /// <param name="flag">
        /// The header flag.
        /// </param>
        /// <param name="length">
        /// The length of the text (argument) for the header flag.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this method could handle the given flag, otherwise it returns <see langword="false"/>.
        /// </returns>
        public virtual bool ParseRequest(HeaderFlag flag, int length)
        {
            logger.Debug("Parsing a request header with {flag} flag", flag);

            if (!flag.IsRequest())
            {
                var ae = new ArgumentException($"The flag {flag} is not a request flag");
                logger.Fatal(ae, "The flag {flag} is not a request flag", flag);
                ReadString(length); // Consume sent data.
                WriteCommand(HeaderFlag.Fail, ae.Message);
                throw ae;
            }

            switch (flag)
            {
                case HeaderFlag.Write:
                    var text = ReadString(length);
                    logger.Information("Writing \"{text}\" to the console", text.Replace("\n", ""));

                    if (Stream is MemoryStream) // Probably it is a test...
                    {
                        logger.Error("Experimental crash triggered");
                        throw new InvalidOperationException("Writing is not supported by test runners :)");
                    }
                    else
                    {
                        Console.Write(text);
                    }

                    WriteCommand(HeaderFlag.Ok);
                    break;
                case HeaderFlag.Version:
                    logger.Information("Protocol version is requested");
                    WriteCommand(HeaderFlag.VersionInfo, API.ToString());
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parses the given header response.
        /// </summary>
        /// <param name="header">
        /// The header data provided by the <see cref="ReadHeader()"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this method could handle the given flag, otherwise it returns <see langword="false"/>.
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
        /// <see langword="true"/> if this method could handle the given flag, otherwise it returns <see langword="false"/>.
        /// </returns>
        /// <exception cref="ApplicationException"></exception>
        public virtual bool ParseResponse(HeaderFlag flag, int length)
        {
            logger.Debug("Parsing a response header with {flag} flag", flag);

            if (!flag.IsResponse())
            {
                var ae = new ArgumentException($"The flag {flag} is not a response flag");
                logger.Fatal(ae, "The flag {flag} is not a response flag", flag);
                ReadString(length); // Consume sent data.
                WriteCommand(HeaderFlag.Fail, ae.Message);
                throw ae;
            }

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
                    var e = new ApplicationException(ReadString(length));
                    logger.Fatal(e, "Stream returns an error");
                    throw e;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the stream data as string.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> of the stream data.
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
        /// A <see cref="string"/> of the stream data.
        /// </returns>
        public string ReadString(int length)
        {
            logger.Debug("Reading {length} bytes of data", length);
            byte[] buffer = new byte[length];
            Read(buffer, 0, length);

            return streamEncoding.GetString(buffer);
        }
        */

        /// <summary>
        /// Writes the given <see cref="ZilyHeader"/> to the stream.
        /// </summary>
        /// <param name="header">
        /// An instance of the <see cref="ZilyHeader"/> with outgoing data.
        /// </param>
        public void WriteCommand(ZilyHeader header)
        {
            if (!IsOnline && header.Flag != ZilyHeaderFlag.Connected)
            {
                throw new ZilyException("Zily is not connected.");
            }

            logger.Debug("Writing data with flag {flag} and message \"{text}\"", header.Flag, header.Text != null ? header.Text.Replace("\n", "") : null);

            Byte[] buffer = header.ToByteArray();
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
        /// Writes the given <see cref="ZilyHeader"/> to the stream, then waits for the response.
        /// </summary>
        /// <param name="header">
        /// An instance of the <see cref="ZilyHeader"/> with outgoing data.
        /// </param>
        public void Send(ZilyHeader header)
        {
            WriteCommand(header);
            ZilyHeader header2 = ZilyHeader.Parse(Stream);
            Side.ParseHeader(header2);
        }

        /// <summary>
        /// Listens to all incoming requests.
        /// </summary>
        /// <param name="suppressLogger">
        /// Determines whether the logger should be stopped during listening.
        /// </param>
        public void Listen(bool suppressLogger = true)
        {
            Listen(CancellationToken.None, suppressLogger);
        }

        /// <summary>
        /// Listens to all incoming requests.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token for terminating the listener.
        /// </param>
        /// <param name="suppressLogger">
        /// Determines whether the logger should be stopped during listening.
        /// </param>
        public void Listen(CancellationToken cancellationToken, bool suppressLogger = true)
        {
            logger.Information("Staring listener");
            ILogger _logger = null;
            if (suppressLogger)
            {
                _logger = logger;
                logger = Serilog.Core.Logger.None;
            }

            while (!cancellationToken.IsCancellationRequested && IsOnline)
            {
                try
                {
                    var header = ZilyHeader.Parse(Stream);
                    Side.ParseHeader(header);
                }
                catch (IOException)
                {
                    break;
                }
            }

            if (suppressLogger)
            {
                logger = _logger;
            }

            logger.Information("Listener has stopped");
        }

        /// <inheritdoc/>
        public override void Close()
        {
            logger.Information("Closing connection");
            if (IsOnline)
            {
                WriteCommand(new ZilyHeader(Side.DisconnectFlag));
                IsOnline = false;
            }
        }
    }
}
