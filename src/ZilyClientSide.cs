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
                    logger.Information("Zily server did shutdown");
                    break;
            }

            base.ParseHeader(header);
        }

        /// <summary>
        /// Establishes a Zily connection through a named pipe.
        /// </summary>
        public void Connect()
        {
            logger.Information("Establishing a Zily connection");
            Send(new ZilyHeader(ZilyHeaderFlag.SideIdentifier));

            if (IsClosed)
            {
                return;
            }

            WriteCommand(new ZilyHeader(ZilyHeaderFlag.Connected));
            IsOnline = true;

            logger.Information("Connected to {name}", OtherSide.Name);
        }
    }
}