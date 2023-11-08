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

        /// <summary>
        /// Gets the proper flag for disconnect message.
        /// </summary>
        public virtual int DisconnectFlag => ZilyHeaderFlag.Disconnected;

        /// <summary>
        /// Gets or Sets the last sent request.
        /// </summary>
        public int LastRequest { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="System.IO.Stream"/>.
        /// </summary>
        public Stream Stream { get; }

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

        /// <summary>
        /// Gets or Sets the logger used by the stream.
        /// </summary>
        protected ILogger logger;

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
            this.logger = logger;
        }

        /// <summary>
        /// Parses the given <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="header">
        /// The header object.
        /// </param>
        public virtual void ParseHeader(ZilyHeader header)
        {
            logger.Debug("Parsing data with flag {flag} and message \"{text}\"", header.Flag, header.Text != null ? header.Text.Replace("\n", "") : null);

            switch (header.Flag)
            {
                case ZilyHeaderFlag.Ok:
                    if (LastRequest > 1)
                    {
                        ParseResponse(LastRequest, header.Text);
                        LastRequest = 0;
                    }
                    break;
                case ZilyHeaderFlag.Warn:
                    logger.Warning(header.Text);
                    break;
                case ZilyHeaderFlag.Fail:
                    logger.Error(header.Text);
                    break;
                case ZilyHeaderFlag.Connected:
                    IsOnline = true;
                    break;
                case ZilyHeaderFlag.Disconnected:
                    IsOnline = false;
                    break;
                case ZilyHeaderFlag.SideIdentifier:
                    WriteCommand(new ZilyHeader(ZilyHeaderFlag.Ok, GetIdentifier()));
                    break;
                case ZilyHeaderFlag.Write:
                    Console.Write(header.Text);
                    Ok();
                    break;
                default:
                    logger.Error("Flag is invalid: {flag}", header.Flag);
                    break;
            }
        }

        public virtual void ParseResponse(int lastRequestFlag, string responseText)
        {
            switch (lastRequestFlag)
            {
                case ZilyHeaderFlag.SideIdentifier:
                    var otherSide = Parse(responseText);
                    if (Protocol != otherSide.Protocol || Version.Major != otherSide.Version.Major)
                    {
                        logger.Fatal("Cannot connect to the server");
                        Close();
                    }
                    else
                    {
                        OtherSide = otherSide;
                    }
                    break;
            }
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
            ParseHeader(header2);
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
                    ParseHeader(header);
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
        public void Close()
        {
            IsClosed = true;
            if (IsOnline)
            {
                logger.Information("Closing connection");
                WriteCommand(new ZilyHeader(DisconnectFlag));
                IsOnline = false;
            }
        }

        /// <summary>
        /// Sends a Ok message.
        /// </summary>
        protected void Ok()
        {
            WriteCommand(ZilyHeader.Ok);
        }
    }
}