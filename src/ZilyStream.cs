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
        /// Gets or Sets the last sent request.
        /// </summary>
        public int LastRequest {  get; set; }

        /// <summary>
        /// Gets the underlying <see cref="System.IO.Stream"/>.
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// Gets the underlying <see cref="ISide"/> that parses the incoming responses.
        /// </summary>
        public ZilySide Side { get; }

        /// <summary>
        /// Gets or Sets the other side client/server information.
        /// </summary>
        public ISide OtherSide { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this connection is online.
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this connection is closed.
        /// </summary>
        public bool IsClosed = false;

        private readonly UnicodeEncoding streamEncoding;

        /// <summary>
        /// Gets or Sets the logger used by the stream.
        /// </summary>
        internal ILogger logger;

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
            streamEncoding = new UnicodeEncoding();
            this.logger = logger;

            Side.SetParent(this);
        }

        /// <summary>
        /// Writes the given <see cref="ZilyHeader"/> to the stream.
        /// </summary>
        /// <param name="header">
        /// An instance of the <see cref="ZilyHeader"/> with outgoing data.
        /// </param>
        public void WriteCommand(ZilyHeader header)
        {
            if (!IsOnline && header.Flag == ZilyHeaderFlag.Write)
            {
                throw new ZilyException("Zily is not connected.");
            }

            logger.Debug("Writing data with flag {flag} and message \"{text}\"", header.Flag, header.Text != null ? header.Text.Replace("\n", "") : null);

            Byte[] buffer = header.ToByteArray();
            Write(buffer, 0, buffer.Length);

            LastRequest = header.Flag;

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
            if (!IsOnline)
            {
                return;
            }

            logger.Information("Starting listener");
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
            IsClosed = true;
            if (IsOnline)
            {
                logger.Information("Closing connection");
                WriteCommand(new ZilyHeader(Side.DisconnectFlag));
                IsOnline = false;
            }
        }
    }
}
