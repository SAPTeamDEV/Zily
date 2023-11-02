using System;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyClientSide : ZilySide
    {
        /// <inheritdoc/>
        public override void ParseHeader(ZilyHeader header)
        {
            switch (header.Flag)
            {
                case ZilyHeaderFlag.Disconnected:
                    Parent.logger.Information("Zily server did shutdown");
                    break;
            }

            base.ParseHeader(header);
        }

        /// <summary>
        /// Establishes a Zily connection through a named pipe.
        /// </summary>
        public void Connect()
        {
            Parent.logger.Information("Establishing a Zily connection");
            Parent.Send(new ZilyHeader(ZilyHeaderFlag.SideIdentifier));

            if (Parent.IsClosed)
            {
                return;
            }

            Parent.WriteCommand(new ZilyHeader(ZilyHeaderFlag.Connected));
            Parent.IsOnline = true;

            Parent.logger.Information("Connected to {name}", Parent.OtherSide.Name);
        }
    }
}