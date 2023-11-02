using System;

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
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        public ZilyClientSide(string protocol) : base(protocol, "Zily Side", new string[]{"zily"})
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyClientSide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        /// <param name="name">
        /// The name of this zily instance.
        /// </param>
        public ZilyClientSide(string protocol, string name) : base(protocol, name, new string[]{"zily", name})
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyClientSide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        /// <param name="name">
        /// The name of this zily instance.
        /// </param>
        /// <param name="identifiers">
        /// The identifiers of this zily instance.
        /// </param>
        public ZilyClientSide(string protocol, string name, string[] identifiers) : base(protocol, name, identifiers)
        {
            
        }

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

            zs.WriteCommand(new ZilyHeader(ZilyHeaderFlag.Connected));
            zs.IsOnline = true;
            logger.Information("Connected to the Zily server");
        }
    }
}