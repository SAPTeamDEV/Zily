using System;
using System.IO;

using Serilog;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyClientSide : ZilySide
    {
        /// <summary>
        /// Gets or Sets the other side client/server information.
        /// </summary>
        protected ISide ServerSide { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyClientSide"/>.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="System.IO.Stream"/> with read and write permission.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyClientSide(Stream stream, ILogger logger = null) : base(stream, logger) { }

        /// <inheritdoc/>
        public override void ParseHeader(ZilyHeader header)
        {
            switch (header.Flag)
            {
                case ZilyHeaderFlag.Disconnected:
                    Logger.Information("Zily server did shutdown");
                    break;
            }

            base.ParseHeader(header);
        }

        public override void ParseResponse(int lastRequestFlag, string responseText)
        {
            switch (lastRequestFlag)
            {
                case ZilyHeaderFlag.SideIdentifier:
                    var otherSide = Parse(responseText);
                    if (Protocol != otherSide.Protocol || Version.Major != otherSide.Version.Major)
                    {
                        Logger.Fatal("Cannot connect to the server");
                        Close();
                    }
                    else
                    {
                        ServerSide = otherSide;
                    }
                    break;
            }
        }

        /// <summary>
        /// Establishes a Zily connection through a named pipe.
        /// </summary>
        public void Connect()
        {
            Logger.Information("Establishing a Zily connection");
            Send(new ZilyHeader(ZilyHeaderFlag.SideIdentifier));

            if (IsClosed)
            {
                return;
            }

            WriteCommand(new ZilyHeader(ZilyHeaderFlag.Connected));
            IsOnline = true;

            Logger.Information("Connected to {name}", ServerSide.Name);
        }
    }
}