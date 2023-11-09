using System;
using System.IO;
using System.Threading;

using Serilog;
using Serilog.Core;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilySide : Side
    {
        /// <inheritdoc/>
        public override string Protocol { get; } = "zily";

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0);

        /// <inheritdoc/>
        public override string Name { get; } = "Zily";

        protected AesEncryption aesEnncryptor = new AesEncryption();
        internal protected IEncryption Encryptor
        {
            get
            {
                if (isSecured)
                {
                    return aesEnncryptor;
                }
                else
                {
                    return Encryption.None;
                }
            }
        }

        protected bool isSecured = false;
        protected ZilyHeader okHeader;

        /// <summary>
        /// Gets or Sets the last sent request.
        /// </summary>
        protected int LastRequest { get; private set; }

        /// <summary>
        /// Gets the underlying <see cref="System.IO.Stream"/>.
        /// </summary>
        protected Stream Stream { get; }

        /// <summary>
        /// Gets or Sets the logger used by the stream.
        /// </summary>
        protected ILogger Logger;

        public ZilySideStatus Status { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilySide"/>.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="System.IO.Stream"/> with read and write permission.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilySide(Stream stream, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = Log.Logger;
            }

            logger.Debug("Initializing a new Zily session");

            Stream = stream;
            Logger = logger;
        }

        /// <summary>
        /// Parses the given <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="header">
        /// The header object.
        /// </param>
        public void Parse(ZilyHeader header)
        {
            Logger.Debug("Parsing {size} byte data with flag {flag} and message \"{text}\"", header.Length, header.Flag, header.Text != null ? header.Text.Replace("\n", "") : "");

            if (header.Flag == ZilyHeaderFlag.Ok && LastRequest > 1)
            {
                ParseResponse(header);
                LastRequest = 0;
            }
            else
            {
                ParseHeader(header);
            }
        }

        protected virtual void ParseHeader(ZilyHeader header)
        {
            switch (header.Flag)
            {
                case ZilyHeaderFlag.Ok:
                    // Do nothing
                    break;
                case ZilyHeaderFlag.Warn:
                    Logger.Warning(header.Text);
                    break;
                case ZilyHeaderFlag.Fail:
                    Logger.Error(header.Text);
                    break;
                case ZilyHeaderFlag.Connected:
                    Status = ZilySideStatus.Online;
                    break;
                case ZilyHeaderFlag.Disconnected:
                    Status = ZilySideStatus.Offline;
                    break;
                case ZilyHeaderFlag.SideIdentifier:
                    WriteCommand(CreateHeader(ZilyHeaderFlag.Ok, GetIdentifier()));
                    break;
                case ZilyHeaderFlag.Write:
                    Console.Write(header.Text);
                    Ok();
                    break;
                default:
                    Logger.Error("Flag is invalid: {flag}", header.Flag);
                    break;
            }
        }

        protected virtual void ParseResponse(ZilyHeader header)
        {

        }

        public ZilyHeader CreateHeader(int flag, string text = null)
        {
            return new ZilyHeader(Encryptor, flag, text);
        }

        /// <summary>
        /// Writes the given <see cref="ZilyHeader"/> to the stream.
        /// </summary>
        /// <param name="header">
        /// An instance of the <see cref="ZilyHeader"/> with outgoing data.
        /// </param>
        public void WriteCommand(ZilyHeader header)
        {
            if (Status != ZilySideStatus.Online && header.Flag == ZilyHeaderFlag.Write)
            {
                throw new ZilyException("Zily is not connected.");
            }

            Logger.Debug("Writing data with flag {flag} and message \"{text}\"", header.Flag, header.Text != null ? header.Text.Replace("\n", "") : "");

            byte[] buffer = header.ToByteArray();
            Stream.Write(buffer, 0, buffer.Length);

            LastRequest = header.Flag;

            if (Stream is MemoryStream)
            {
                Stream.Seek(-buffer.Length, SeekOrigin.Current);
            }
            else
            {
                Stream.Flush();
            }

            Logger.Debug("Wrote {length} bytes", buffer.Length);
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
            ZilyHeader header2 = ZilyHeader.Read(Encryptor, Stream);
            Parse(header2);
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
            if (Status != ZilySideStatus.Online)
            {
                return;
            }

            Logger.Information("Starting listener");
            ILogger _logger = null;
            if (suppressLogger)
            {
                _logger = Logger;
                Logger = Serilog.Core.Logger.None;
            }

            while (!cancellationToken.IsCancellationRequested && Status == ZilySideStatus.Online)
            {
                try
                {
                    var header = ZilyHeader.Read(Encryptor, Stream);
                    Parse(header);
                }
                catch (IOException)
                {
                    break;
                }
            }

            if (suppressLogger)
            {
                Logger = _logger;
            }

            Logger.Information("Listener has stopped");
        }

        /// <inheritdoc/>
        public void Close()
        {
            if (Status == ZilySideStatus.Online)
            {
                Logger.Information("Closing connection");
                WriteCommand(CreateHeader(ZilyHeaderFlag.Disconnected));
            }

            Status = ZilySideStatus.Offline;
        }

        /// <summary>
        /// Sends a Ok message.
        /// </summary>
        protected void Ok()
        {
            WriteCommand(okHeader);
        }
    }
}