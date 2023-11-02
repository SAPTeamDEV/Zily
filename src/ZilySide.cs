using System;

using Serilog;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilySide : Side
    {
        int last_request;

        /// <summary>
        /// Gets the proper flag for disconnect message.
        /// </summary>
        public virtual int DisconnectFlag => ZilyHeaderFlag.Disconnected;

        internal ZilyStream zs;
        internal ILogger logger;

        ZilyHeader OkHeader = new ZilyHeader(ZilyHeaderFlag.Ok);

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilySide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        public ZilySide(string protocol) : base(protocol, "Zily Side", new string[]{"zily"})
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilySide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        /// <param name="name">
        /// The name of this zily instance.
        /// </param>
        public ZilySide(string protocol, string name) : base(protocol, name, new string[]{"zily", name})
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilySide"/>.
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
        public ZilySide(string protocol, string name, string[] identifiers) : base(protocol, name, identifiers)
        {
            
        }

        /// <summary>
        /// Parses the given <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="header">
        /// The parsed header object.
        /// </param>
        public virtual void ParseHeader(ZilyHeader header)
        {
            switch (header.Flag)
            {
                case ZilyHeaderFlag.Ok:
                case ZilyHeaderFlag.Warn:
                    // It's ok.
                    break;
                case ZilyHeaderFlag.Fail:
                    // Throw an exception with the error message.
                    throw new ZilyException(header.Text);
                case ZilyHeaderFlag.Connected:
                    zs.IsOnline = true;
                    break;
                case ZilyHeaderFlag.Disconnected:
                    zs.IsOnline = false;
                    break;
                case ZilyHeaderFlag.Write:
                    Console.Write(header.Text);
                    Ok();
                    break;
                default:
                    // Throw an exception because the response is unknown.
                    throw new ArgumentException("Invalid response.");

            }
        }

        protected void Ok()
        {
            zs.WriteCommand(OkHeader);
        }
    }
}