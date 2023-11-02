using System;
using System.IO;

using Serilog.Core;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyServerSide : ZilySide
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyServerSide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        public ZilyServerSide(string protocol) : base(protocol, "Zily Side", new string[]{"zily"})
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyServerSide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        /// <param name="name">
        /// The name of this zily instance.
        /// </param>
        public ZilyServerSide(string protocol, string name) : base(protocol, name, new string[]{"zily", name})
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyServerSide"/>.
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
        public ZilyServerSide(string protocol, string name, string[] identifiers) : base(protocol, name, identifiers)
        {
            
        }

        /// <summary>
        /// Waits until a client connects to the server and then establishes the Zily connection.
        /// </summary>
        public void Accept()
        {
            bool everConnected = false;

            logger.Debug("Waiting for client");
            Wait();

            while (true)
            {
                var header = ZilyHeader.Parse(zs.Stream);
                if (!everConnected)
                {
                    logger.Information("A new client connected to the pipe server");
                    logger.Information("Establishing a Zily connection");
                    everConnected = true;
                }

                ParseHeader(header);

                if (header.Flag == ZilyHeaderFlag.Connected)
                {
                    break;
                }
            }
        }

        public virtual void Wait()
        {

        }
    }
}